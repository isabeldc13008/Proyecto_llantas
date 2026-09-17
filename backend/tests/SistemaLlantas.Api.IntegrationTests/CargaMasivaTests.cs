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
using SistemaLlantas.Application.Vehiculos;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class CargaMasivaTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    private static readonly string[] Headers = ["CodigoLlanta", "Serial", "Marca", "Referencia", "Dimension", "TipoLlanta", "Estado", "Centro", "Ubicacion", "ProfundidadInicial"];

    private async Task<Scenario> Setup()
    {
        _ = factory.CreateClient();
        var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var brand = new Marca { Codigo = "BM" + suffix, Nombre = "Marca " + suffix };
        var other = new Marca { Codigo = "BO" + suffix, Nombre = "Otra " + suffix };
        var reference = new Referencia { Codigo = "RF" + suffix, Nombre = "Referencia " + suffix, MarcaId = brand.Id };
        var center = new Centro { Codigo = "CE" + suffix, Nombre = "Centro " + suffix };
        var dimension = new Dimension { Codigo = "DM" + suffix, Nombre = "Dimension " + suffix };
        var type = new TipoLlanta { Codigo = "TP" + suffix, Nombre = "Tipo " + suffix };
        var state = new EstadoLlanta { Codigo = "ES" + suffix, Nombre = "Estado " + suffix };
        db.AddRange(brand, other, reference, center, dimension, type, state);
        await db.SaveChangesAsync();
        var controller = new CargaMasivaController(db, scope.ServiceProvider.GetRequiredService<IVehiculoService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim("username", "bulk-" + suffix), new Claim("permiso", "centros.ver_todos")], "test"))
            }}
        };
        return new Scenario(scope, db, controller, brand, other, reference, center, dimension, type, state, suffix);
    }

    [Theory]
    [InlineData(1, false, false)]
    [InlineData(1, true, true)]
    [InlineData(12, true, false)]
    public async Task ValidRows_PreviewAndConfirm_WithOptionalFieldsAbsent(int count, bool xlsx, bool names)
    {
        await using var s = await Setup();
        var rows = Enumerable.Range(0, count).Select(s.Row).ToArray();
        if (names)
        {
            rows[0][2] = s.Brand.Nombre;
            rows[0][3] = s.Reference.Nombre;
            rows[0][4] = s.Dimension.Nombre;
            rows[0][5] = s.Type.Nombre;
            rows[0][6] = s.State.Nombre;
            rows[0][7] = s.Center.Nombre;
        }
        foreach (var row in rows)
            for (var i = 2; i <= 7; i++) row[i] = "  " + row[i].ToLowerInvariant() + "  ";
        var preview = await Preview(s, rows, xlsx);
        Assert.Equal(count, preview.Validas);
        Assert.Equal(0, preview.ConError);
        var result = await s.Controller.Confirm(preview.Id, default);
        Assert.Equal(count, result.Procesadas);
        var tires = await s.Db.Llantas.Where(x => x.Codigo.StartsWith("T" + s.Suffix)).ToListAsync();
        Assert.Equal(count, tires.Count);
        Assert.All(tires, tire => {
            Assert.Equal(s.Brand.Id, tire.MarcaId);
            Assert.Equal(s.Reference.Id, tire.ReferenciaId);
            Assert.Equal("BODEGA", tire.UbicacionActual);
            Assert.Null(tire.FechaCompra);
            Assert.Null(tire.Costo);
            Assert.Null(tire.Observaciones);
            Assert.NotEqual(default, tire.FechaIngreso);
        });
    }

    [Theory]
    [InlineData("Marca", "missing")]
    [InlineData("Referencia", "missing")]
    [InlineData("Centro", "missing")]
    [InlineData("Dimension", "missing")]
    [InlineData("TipoLlanta", "missing")]
    [InlineData("Estado", "missing")]
    [InlineData("ProfundidadInicial", "-1")]
    [InlineData("ProfundidadInicial", "100.01")]
    [InlineData("ProfundidadInicial", "abc")]
    [InlineData("ProfundidadInicial", "")]
    [InlineData("CodigoLlanta", "long")]
    [InlineData("Serial", "long")]
    [InlineData("Ubicacion", "long")]
    public async Task InvalidValue_IsRejectedInPreview(string field, string value)
    {
        await using var s = await Setup();
        var row = s.Row(0);
        row[Array.IndexOf(Headers, field)] = value == "long" ? new string('X', 151) : value;
        var preview = await Preview(s, [row]);
        Assert.Equal(0, preview.Validas);
        Assert.Equal(1, preview.ConError);
        Assert.Contains(preview.Errores, e => e.Fila == 2 && e.Campo == field);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100")]
    public async Task DepthBoundaries_AreAccepted(string depth)
    {
        await using var s = await Setup();
        var row = s.Row(0); row[9] = depth;
        var preview = await Preview(s, [row]);
        Assert.Equal(1, preview.Validas);
        Assert.Equal(1, (await s.Controller.Confirm(preview.Id, default)).Procesadas);
    }

    [Fact]
    public async Task ReferenceFromOtherBrand_IsRejectedInPreview()
    {
        await using var s = await Setup();
        var row = s.Row(0); row[2] = s.Other.Codigo;
        var preview = await Preview(s, [row]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == "Referencia" && e.Error.Contains(s.Other.Codigo));
    }

    [Fact]
    public async Task UnauthorizedCenter_IsRejectedInPreview()
    {
        await using var s = await Setup();
        s.Controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("username", "restricted")], "test"));
        var preview = await Preview(s, [s.Row(0)]);
        Assert.Contains(preview.Errores, e => e.Campo == "Centro" && e.Error.Contains("alcance"));
        Assert.Equal(0, preview.Validas);
    }

    [Theory]
    [InlineData("Marca")]
    [InlineData("Referencia")]
    [InlineData("Centro")]
    [InlineData("Dimension")]
    [InlineData("TipoLlanta")]
    [InlineData("Estado")]
    public async Task InactiveCatalog_IsRejected(string field)
    {
        await using var s = await Setup();
        CatalogoBase entity = field switch {
            "Marca" => s.Brand, "Referencia" => s.Reference, "Centro" => s.Center,
            "Dimension" => s.Dimension, "TipoLlanta" => s.Type, _ => s.State
        };
        entity.Activo = false;
        await s.Db.SaveChangesAsync();
        var preview = await Preview(s, [s.Row(0)]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == field);
    }

    [Fact]
    public async Task AmbiguousName_IsRejectedInPreview()
    {
        await using var s = await Setup();
        s.Other.Nombre = s.Brand.Nombre;
        await s.Db.SaveChangesAsync();
        var row = s.Row(0); row[2] = s.Brand.Nombre;
        var preview = await Preview(s, [row]);
        Assert.Equal(0, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == "Marca" && e.Error.Contains("varios"));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public async Task DuplicateCodeOrSerial_IsRejected(int column, bool existing)
    {
        await using var s = await Setup();
        var first = s.Row(0);
        var second = s.Row(1);
        second[column] = " " + first[column].ToLowerInvariant() + " ";
        if (existing)
        {
            var initial = await Preview(s, [first]);
            await s.Controller.Confirm(initial.Id, default);
            // Inactive tires still reserve their unique identifiers.
            var tire = await s.Db.Llantas.SingleAsync(x => x.Codigo == first[0]);
            tire.Activo = false;
            await s.Db.SaveChangesAsync();
        }
        var preview = await Preview(s, existing ? [second] : [first, second]);
        Assert.Equal(existing ? 0 : 1, preview.Validas);
        Assert.Contains(preview.Errores, e => e.Campo == Headers[column] && e.Error.Contains("duplicado"));
    }

    [Fact]
    public async Task ConfirmValidation_ReturnsUseful400_OriginalRow_AndLeavesLoadUnprocessed()
    {
        await using var s = await Setup();
        var invalid = s.Row(0); invalid[9] = "-1";
        var preview = await Preview(s, [invalid, s.Row(1), s.Row(2)]);
        Assert.Equal(2, preview.Validas);
        s.Reference.MarcaId = s.Other.Id;
        await s.Db.SaveChangesAsync();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ApiExceptionMiddleware(async _ => { await s.Controller.Confirm(preview.Id, default); },
            NullLogger<ApiExceptionMiddleware>.Instance);
        await middleware.InvokeAsync(context);
        Assert.Equal(400, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var message = response.RootElement.GetProperty("message").GetString()!;
        Assert.Contains("Fila 3", message);
        Assert.Contains(s.Reference.Codigo, message);
        Assert.Contains(s.Brand.Codigo, message);
        Assert.Equal("VALIDATION_ERROR", response.RootElement.GetProperty("code").GetString());
        s.Db.ChangeTracker.Clear();
        Assert.False(await s.Db.Llantas.AnyAsync(x => x.Codigo.StartsWith("T" + s.Suffix)));
        Assert.Equal("PREVISUALIZADA", (await s.Db.CargasMasivas.SingleAsync(x => x.Id == preview.Id)).Estado);
    }

    [Fact]
    public async Task Vehicles_StillPreviewAndConfirm()
    {
        await using var s = await Setup();
        var config = await s.Db.ConfiguracionesVehiculo.FirstAsync(x => x.Activo);
        var csv = "Interno,Placa,Centro,TipoVehiculo,ConfiguracionEjes,Kilometraje,Estado\n"
            + $"V{s.Suffix},QA123,{s.Center.Codigo},Camión,{config.Codigo},100,Activo";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var file = new FormFile(stream, 0, stream.Length, "archivo", "vehicles.csv");
        var preview = (await s.Controller.Preview("vehiculos", file, default)).Value!;
        Assert.Equal(1, preview.Validas);
        Assert.Equal(1, (await s.Controller.Confirm(preview.Id, default)).Procesadas);
        Assert.True(await s.Db.Vehiculos.AnyAsync(x => x.NumeroInterno == "V" + s.Suffix));
    }

    private static async Task<CargaMasivaController.PreviewDto> Preview(Scenario s, string[][] rows, bool xlsx = false)
    {
        using var stream = new MemoryStream();
        if (xlsx)
        {
            using var book = new XLWorkbook();
            var sheet = book.AddWorksheet("Llantas");
            for (var c = 0; c < Headers.Length; c++) sheet.Cell(1, c + 1).Value = Headers[c].ToLowerInvariant();
            for (var r = 0; r < rows.Length; r++)
                for (var c = 0; c < rows[r].Length; c++) sheet.Cell(r + 2, c + 1).Value = rows[r][c];
            book.SaveAs(stream);
        }
        else
        {
            var csv = string.Join(',', Headers) + "\n" + string.Join("\n", rows.Select(r => string.Join(',', r)));
            stream.Write(Encoding.UTF8.GetBytes(csv));
        }
        stream.Position = 0;
        return (await s.Controller.Preview("llantas", new FormFile(stream, 0, stream.Length, "archivo", xlsx ? "tires.xlsx" : "tires.csv"), default)).Value!;
    }

    private sealed record Scenario(AsyncServiceScope Scope, LlantasDbContext Db, CargaMasivaController Controller,
        Marca Brand, Marca Other, Referencia Reference, Centro Center, Dimension Dimension, TipoLlanta Type,
        EstadoLlanta State, string Suffix) : IAsyncDisposable
    {
        public string[] Row(int index) => ["T" + Suffix + index, "S" + Suffix + index, Brand.Codigo, Reference.Codigo,
            Dimension.Codigo, Type.Codigo, State.Codigo, Center.Codigo, "BODEGA", "12.5"];
        public ValueTask DisposeAsync() => Scope.DisposeAsync();
    }
}
