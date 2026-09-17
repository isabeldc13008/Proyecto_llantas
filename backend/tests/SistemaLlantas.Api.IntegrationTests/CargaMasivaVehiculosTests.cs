using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Api.Middleware;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Vehiculos;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class CargaMasivaVehiculosTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    private static readonly string[] Headers = ["Interno", "Placa", "Centro", "TipoVehiculo", "ConfiguracionEjes", "Kilometraje", "Estado"];

    private async Task<Scenario> Setup()
    {
        _ = factory.CreateClient();
        var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var suffix = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        var center = new Centro { Codigo = "CE" + suffix, Nombre = "Centro " + suffix };
        var configuration = new ConfiguracionVehiculo {
            Codigo = "CFG" + suffix, Nombre = "Configuración " + suffix, TipoVehiculo = "Tractocamión",
            Ejes = [
                new ConfiguracionEje { Orden = 1, Nombre = "Delantero", TipoEje = "Direccional", Posiciones = [
                    new ConfiguracionPosicion { Codigo = "P1", Orden = 1, Lado = "Izquierda", Ubicacion = "Externa" },
                    new ConfiguracionPosicion { Codigo = "P2", Orden = 2, Lado = "Derecha", Ubicacion = "Externa" }
                ]}
            ]
        };
        db.AddRange(center, configuration);
        await db.SaveChangesAsync();
        var service = scope.ServiceProvider.GetRequiredService<IVehiculoService>();
        var controller = new CargaMasivaController(db, service) { ControllerContext = Context("vehicle-" + suffix) };
        return new(scope, db, service, controller, center, configuration, suffix);
    }

    private static ControllerContext Context(string username) => new() {
        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("username", username), new Claim("permiso", "centros.ver_todos")
        ], "test")) }
    };

    [Theory]
    [InlineData(1, false, false)]
    [InlineData(1, true, true)]
    [InlineData(12, true, false)]
    public async Task ValidPreview_ConfirmsAllRows_UsingCodeOrName(int count, bool xlsx, bool names)
    {
        await using var s = await Setup();
        var rows = Enumerable.Range(0, count).Select(s.Row).ToArray();
        foreach (var row in rows)
        {
            row[0] = "  " + row[0].ToLowerInvariant() + "  ";
            row[2] = "  " + (names ? s.Center.Nombre : s.Center.Codigo).ToLowerInvariant() + "  ";
            row[3] = "  tractocamión  ";
            row[4] = "  " + (names ? s.Configuration.Nombre : s.Configuration.Codigo).ToLowerInvariant() + "  ";
        }
        var preview = await Preview(s, rows, xlsx);
        Assert.Equal(count, preview.Validas);
        Assert.Empty(preview.Errores);
        Assert.Equal(count, (await s.Controller.Confirm(preview.Id, default)).Procesadas);
        s.Db.ChangeTracker.Clear();
        var vehicles = await s.Db.Vehiculos.Include(x => x.Ejes).ThenInclude(x => x.Posiciones)
            .Where(x => x.NumeroInterno.StartsWith("V" + s.Suffix)).ToListAsync();
        Assert.Equal(count, vehicles.Count);
        Assert.All(vehicles, v => {
            Assert.Equal(s.Center.Id, v.CentroId);
            Assert.Equal(s.Configuration.Id, v.ConfiguracionVehiculoId);
            Assert.Equal("Tractocamión", v.Tipo);
            Assert.Equal(1000m, v.Kilometraje);
            Assert.Equal("Activo", v.Estado);
            Assert.Single(v.Ejes);
            Assert.Equal(2, v.Ejes.Single().Posiciones.Count);
        });
    }

    [Theory]
    [InlineData("Interno", "")]
    [InlineData("Placa", "")]
    [InlineData("Centro", "missing")]
    [InlineData("ConfiguracionEjes", "missing")]
    [InlineData("ConfiguracionEjes", "")]
    [InlineData("TipoVehiculo", "")]
    [InlineData("TipoVehiculo", "Camión")]
    [InlineData("Estado", "")]
    [InlineData("Kilometraje", "")]
    [InlineData("Kilometraje", "ABC")]
    [InlineData("Kilometraje", "-100")]
    [InlineData("Kilometraje", "10.000 km")]
    [InlineData("Kilometraje", "10000000000000000")]
    [InlineData("Kilometraje", "999999999999999999999999999999999")]
    [InlineData("Interno", "long")]
    [InlineData("Placa", "long")]
    [InlineData("TipoVehiculo", "long")]
    [InlineData("Estado", "long")]
    public async Task InvalidRow_IsRejectedBeforeConfirm(string field, string value)
    {
        await using var s = await Setup();
        var row = s.Row(0);
        row[Array.IndexOf(Headers, field)] = value == "long" ? new string('X', 151) : value;
        var preview = await Preview(s, [row]);
        Assert.Equal(0, preview.Validas);
        Assert.Equal(1, preview.ConError);
        var errorField = field == "TipoVehiculo" && value == "Camión" ? "ConfiguracionEjes" : field;
        Assert.Contains(preview.Errores, e => e.Fila == 2 && e.Campo == errorField);
        Assert.False(await s.Db.Vehiculos.AnyAsync(x => x.NumeroInterno == s.Row(0)[0]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1000.25")]
    public async Task ValidMileage_IsStored(string text)
    {
        await using var s = await Setup();
        var row = s.Row(0); row[5] = text;
        var preview = await Preview(s, [row]);
        Assert.Equal(1, preview.Validas);
        await s.Controller.Confirm(preview.Id, default);
        s.Db.ChangeTracker.Clear();
        Assert.Equal(decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
            (await s.Db.Vehiculos.SingleAsync(x => x.NumeroInterno == row[0])).Kilometraje);
    }

    [Fact]
    public async Task DuplicateInternalInExcel_IsDetected_ForTheReportedExample()
    {
        await using var s = await Setup();
        // A numeric value is a catalog code, never an axle count or an implicit ID.
        s.Configuration.Codigo = "5";
        await s.Db.SaveChangesAsync();
        var first = s.Row(0); first[0] = "DEMO-1542"; first[1] = "LKO234";
        var second = s.Row(1); second[0] = "  demo-1542  ";
        var preview = await Preview(s, [first, second], true);
        Assert.Equal(1, preview.Validas);
        Assert.Equal(1, preview.ConError);
        Assert.Contains(preview.Errores, e => e.Fila == 3 && e.Campo == "Interno" && e.Error.Contains("duplicado"));
        Assert.DoesNotContain(preview.Errores, e => e.Campo == "ConfiguracionEjes");
        Assert.Equal(1, (await s.Controller.Confirm(preview.Id, default)).Procesadas);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingInternal_IsRejected_EvenWhenInactive(bool active)
    {
        await using var s = await Setup();
        var row = s.Row(0);
        await s.Service.CrearAsync(new(row[0], row[1], row[3], s.Center.Id, s.Configuration.Id, 1000, "Activo", null),
            "test", new(true, []), default);
        var vehicle = await s.Db.Vehiculos.SingleAsync(x => x.NumeroInterno == row[0]);
        vehicle.Activo = active; await s.Db.SaveChangesAsync();
        row[0] = " " + row[0].ToLowerInvariant() + " ";
        var preview = await Preview(s, [row]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == "Interno" && e.Error.Contains("duplicado"));
    }

    [Theory]
    [InlineData("Centro")]
    [InlineData("ConfiguracionEjes")]
    public async Task InactiveCatalog_IsRejected(string field)
    {
        await using var s = await Setup();
        if (field == "Centro") s.Center.Activo = false; else s.Configuration.Activo = false;
        await s.Db.SaveChangesAsync();
        var preview = await Preview(s, [s.Row(0)]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == field && e.Error.Contains("activ"));
    }

    [Fact]
    public async Task CenterOutsideScope_IsRejected()
    {
        await using var s = await Setup();
        s.Controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("username", "restricted")], "test"));
        var preview = await Preview(s, [s.Row(0)]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == "Centro" && e.Error.Contains("alcance"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AmbiguousConfigurationNameOrCode_IsRejected(bool codeCollision)
    {
        await using var s = await Setup();
        s.Db.ConfiguracionesVehiculo.Add(new ConfiguracionVehiculo {
            Codigo = "OTHER" + s.Suffix,
            Nombre = codeCollision ? s.Configuration.Codigo : s.Configuration.Nombre,
            TipoVehiculo = "Tractocamión"
        });
        await s.Db.SaveChangesAsync();
        var row = s.Row(0); row[4] = codeCollision ? s.Configuration.Codigo : s.Configuration.Nombre;
        var preview = await Preview(s, [row]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == "ConfiguracionEjes" && e.Error.Contains("varias"));
    }

    [Fact]
    public async Task WrongConfigurationType_ReportsBothTypesAndReceivedValue()
    {
        await using var s = await Setup();
        s.Configuration.TipoVehiculo = "Camión"; await s.Db.SaveChangesAsync();
        var row = s.Row(0); row[3] = "Tractocamión";
        var preview = await Preview(s, [row]);
        var error = Assert.Single(preview.Errores);
        Assert.Equal("ConfiguracionEjes", error.Campo);
        Assert.Contains(s.Configuration.Codigo, error.Error);
        Assert.Contains("Camión", error.Error);
        Assert.Contains("Tractocamión", error.Error);
    }

    [Fact]
    public async Task RepeatedPlateAndFreeTextState_RemainAllowed()
    {
        await using var s = await Setup();
        var rows = new[] { s.Row(0), s.Row(1) };
        rows[1][1] = rows[0][1];
        rows[0][6] = rows[1][6] = "En mantenimiento";
        var preview = await Preview(s, rows);
        Assert.Equal(2, preview.Validas);
        Assert.Equal(2, (await s.Controller.Confirm(preview.Id, default)).Procesadas);
    }

    [Theory]
    [InlineData("configuration")]
    [InlineData("center")]
    [InlineData("scope")]
    [InlineData("type")]
    [InlineData("internal")]
    public async Task ChangeAfterPreview_Returns400WithOriginalRowAndNoPartialImport(string change)
    {
        await using var s = await Setup();
        var invalid = s.Row(0); invalid[5] = "ABC";
        var preview = await Preview(s, [invalid, s.Row(1), s.Row(2)]);
        Assert.Equal(2, preview.Validas);
        if (change == "configuration") s.Configuration.Activo = false;
        if (change == "center") s.Center.Activo = false;
        if (change == "type") s.Configuration.TipoVehiculo = "Camión";
        if (change == "internal")
            await s.Service.CrearAsync(new(s.Row(1)[0], "NEW", "Tractocamión", s.Center.Id, s.Configuration.Id, 100, "Activo", null),
                "other", new(true, []), default);
        await s.Db.SaveChangesAsync();
        if (change == "scope")
            s.Controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("username", "vehicle-" + s.Suffix)], "test"));
        var context = new DefaultHttpContext(); context.Response.Body = new MemoryStream();
        var middleware = new ApiExceptionMiddleware(async _ => { await s.Controller.Confirm(preview.Id, default); },
            NullLogger<ApiExceptionMiddleware>.Instance);
        await middleware.InvokeAsync(context);
        Assert.Equal(400, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("VALIDATION_ERROR", response.RootElement.GetProperty("code").GetString());
        Assert.Contains("Fila 3", response.RootElement.GetProperty("message").GetString());
        s.Db.ChangeTracker.Clear();
        Assert.Equal(change == "internal" ? 1 : 0,
            await s.Db.Vehiculos.CountAsync(x => x.NumeroInterno.StartsWith("V" + s.Suffix)));
        Assert.Equal("PREVISUALIZADA", (await s.Db.CargasMasivas.SingleAsync(x => x.Id == preview.Id)).Estado);
    }

    [Fact]
    public async Task LaterServiceFailure_RollsBackPreviouslySavedVehicle()
    {
        await using var s = await Setup();
        var preview = await Preview(s, [s.Row(0), s.Row(1)]);
        var controller = new CargaMasivaController(s.Db, new FailSecondCreation(s.Service)) {
            ControllerContext = s.Controller.ControllerContext
        };
        var error = await Assert.ThrowsAsync<ValidacionException>(() => controller.Confirm(preview.Id, default));
        Assert.Contains("Fila 3", error.Message);
        Assert.Contains("conflicto simulado", error.Message);
        s.Db.ChangeTracker.Clear();
        Assert.False(await s.Db.Vehiculos.AnyAsync(x => x.NumeroInterno.StartsWith("V" + s.Suffix)));
        Assert.Equal("PREVISUALIZADA", (await s.Db.CargasMasivas.SingleAsync(x => x.Id == preview.Id)).Estado);
    }

    private static async Task<CargaMasivaController.PreviewDto> Preview(Scenario s, string[][] rows, bool xlsx = false)
    {
        using var stream = new MemoryStream();
        if (xlsx)
        {
            using var book = new XLWorkbook();
            var sheet = book.AddWorksheet("Vehículos");
            for (var c = 0; c < Headers.Length; c++) sheet.Cell(1, c + 1).Value = Headers[c].ToLowerInvariant();
            for (var r = 0; r < rows.Length; r++)
                for (var c = 0; c < rows[r].Length; c++) sheet.Cell(r + 2, c + 1).Value = rows[r][c];
            book.SaveAs(stream);
        }
        else
        {
            string Escape(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
            var csv = string.Join(',', Headers) + "\n" + string.Join("\n", rows.Select(r => string.Join(',', r.Select(Escape))));
            stream.Write(Encoding.UTF8.GetBytes(csv));
        }
        stream.Position = 0;
        return (await s.Controller.Preview("vehiculos",
            new FormFile(stream, 0, stream.Length, "archivo", xlsx ? "vehicles.xlsx" : "vehicles.csv"), default)).Value!;
    }

    private sealed record Scenario(AsyncServiceScope Scope, LlantasDbContext Db, IVehiculoService Service,
        CargaMasivaController Controller, Centro Center, ConfiguracionVehiculo Configuration, string Suffix) : IAsyncDisposable
    {
        public string[] Row(int index) => ["V" + Suffix + index, "LKO234", Center.Codigo,
            Configuration.TipoVehiculo, Configuration.Codigo, "1000", "Activo"];
        public ValueTask DisposeAsync() => Scope.DisposeAsync();
    }

    private sealed class FailSecondCreation(IVehiculoService inner) : IVehiculoService
    {
        private int count;
        public Task<VehiculoDetalleDto> CrearAsync(GuardarVehiculoDto dto, string user, AlcanceCentros scope, CancellationToken ct) =>
            ++count == 2 ? throw new ConflictoException("conflicto simulado") : inner.CrearAsync(dto, user, scope, ct);
        public Task<Pagina<VehiculoResumenDto>> ConsultarAsync(ConsultaPaginada c, AlcanceCentros a, CancellationToken ct) => inner.ConsultarAsync(c, a, ct);
        public Task<VehiculoDetalleDto?> ObtenerAsync(Guid id, AlcanceCentros a, CancellationToken ct) => inner.ObtenerAsync(id, a, ct);
        public Task<VehiculoDetalleDto?> ActualizarAsync(Guid id, GuardarVehiculoDto d, string u, AlcanceCentros a, CancellationToken ct) => inner.ActualizarAsync(id, d, u, a, ct);
        public Task<IReadOnlyList<ConfiguracionVehiculoDto>> ConfiguracionesAsync(CancellationToken ct) => inner.ConfiguracionesAsync(ct);
        public Task<ConfiguracionVehiculoDto> CrearConfiguracionAsync(GuardarConfiguracionVehiculoDto d, string u, CancellationToken ct) => inner.CrearConfiguracionAsync(d, u, ct);
    }
}
