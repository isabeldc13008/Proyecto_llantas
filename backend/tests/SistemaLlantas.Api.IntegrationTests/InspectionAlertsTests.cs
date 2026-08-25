using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class InspectionAlertsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;
    public InspectionAlertsTests(WebApplicationFactory<Program> factory) => this.factory = factory;

    [Fact]
    public async Task ReglaParametrizada_GeneraAlertaYPreservaHistorial()
    {
        _ = factory.CreateClient();
        Guid inspectionId = Guid.Empty;
        try
        {
            Guid alertId;
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
                var vehicle = await db.Vehiculos.AsNoTracking().FirstAsync(x => x.Ejes.Any(e => e.Posiciones.Any()));
                var service = scope.ServiceProvider.GetRequiredService<IInspeccionService>();
                var inspection = await service.CrearAsync(new() { VehiculoId = vehicle.Id, Kilometraje = 1000 }, "qa", new(true, []), CancellationToken.None);
                inspectionId = inspection.Id;
                var position = Assert.Single(inspection.Detalles.Take(1)).PosicionId;
                await service.GuardarDetalleAsync(inspection.Id, position, new() { ProfundidadExterior = 10, ProfundidadCentro = 9, ProfundidadInterior = 7 }, "qa", CancellationToken.None);
                var alert = Assert.Single(await service.AlertasAsync(new(true, []), CancellationToken.None), x => x.InspeccionId == inspection.Id);
                alertId = alert.Id;
                Assert.Equal("ABIERTA", alert.Estado);
                Assert.Equal("DIFERENCIA_HOMBROS", alert.Tipo);
            }
            await using var manageScope = factory.Services.CreateAsyncScope();
            var manageService = manageScope.ServiceProvider.GetRequiredService<IInspeccionService>();
            var managed = await manageService.CambiarAlertaAsync(alertId, new("EN_PROCESO", "Revisión iniciada"), "supervisor", new(true, []), CancellationToken.None);
            Assert.Equal("EN_PROCESO", managed.Estado);
            Assert.Equal(2, managed.Historial.Count);
            Assert.Equal("supervisor", managed.Historial.Last().Usuario);
        }
        finally
        {
            if (inspectionId != Guid.Empty)
            {
                await using var cleanup = factory.Services.CreateAsyncScope();
                var db = cleanup.ServiceProvider.GetRequiredService<LlantasDbContext>();
                await db.AlertasHistorial.Where(x => x.Alerta.InspeccionId == inspectionId).ExecuteDeleteAsync();
                await db.AlertasInspeccion.Where(x => x.InspeccionId == inspectionId).ExecuteDeleteAsync();
                await db.InspeccionesDetalle.Where(x => x.InspeccionId == inspectionId).ExecuteDeleteAsync();
                await db.Inspecciones.Where(x => x.Id == inspectionId).ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task EvidenciaPdf_SeValidaDescargaYEliminaLogicamente()
    {
        _ = factory.CreateClient();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            var vehicle = await db.Vehiculos.AsNoTracking().FirstAsync(x => x.Ejes.Any(e => e.Posiciones.Any()));
            var service = scope.ServiceProvider.GetRequiredService<IInspeccionService>();
            var inspection = await service.CrearAsync(new() { VehiculoId = vehicle.Id, Kilometraje = 1000 }, "qa", new(true, []), CancellationToken.None);
            var root = Path.Combine(Path.GetTempPath(), "llantas-evidence-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var controller = new InspeccionesController(service, db, new TestEnvironment(root))
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("username", "qa"), new Claim("permiso", "centros.ver_todos")], "Test")) } }
            };
            await using var stream = new MemoryStream("%PDF-1.4\n%%EOF"u8.ToArray());
            var file = new FormFile(stream, 0, stream.Length, "archivo", "evidencia-qa.pdf") { Headers = new HeaderDictionary(), ContentType = "application/pdf" };
            var upload = await controller.AdjuntarEvidencia(inspection.Id, file, CancellationToken.None);
            Assert.IsType<CreatedResult>(upload.Result);
            var evidence = await db.EvidenciasInspeccion.SingleAsync(x => x.InspeccionId == inspection.Id);
            Assert.Equal("application/pdf", evidence.MimeType);
            Assert.Equal(stream.Length, evidence.TamanoBytes);
            Assert.Equal(64, evidence.Hash.Length);
            Assert.True(File.Exists(Path.Combine(root, evidence.Ubicacion)));
            var listed = await controller.Evidencias(inspection.Id, CancellationToken.None);
            Assert.Contains(Assert.IsAssignableFrom<IReadOnlyList<EvidenciaDto>>(listed.Value), x => x.Id == evidence.Id && x.Activo);
            var download = Assert.IsType<PhysicalFileResult>(await controller.Descargar(evidence.Id, CancellationToken.None));
            Assert.Equal("application/pdf", download.ContentType);
            Assert.IsType<NoContentResult>(await controller.Eliminar(evidence.Id, CancellationToken.None));
            db.ChangeTracker.Clear();
            Assert.False((await db.EvidenciasInspeccion.IgnoreQueryFilters().SingleAsync(x => x.Id == evidence.Id)).Activo);
            File.Delete(Path.Combine(root, evidence.Ubicacion));
            Directory.Delete(Path.Combine(root, "App_Data", "evidencias"));
            Directory.Delete(Path.Combine(root, "App_Data"));
            Directory.Delete(root);
            await tx.RollbackAsync();
        });
    }

    [Fact]
    public async Task AutocompleteVehiculos_RespetaBusquedaYCentroAutorizado()
    {
        _ = factory.CreateClient();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var vehicle = await db.Vehiculos.AsNoTracking().FirstAsync();
        var service = scope.ServiceProvider.GetRequiredService<IInspeccionService>();
        var allowed = await service.ObtenerVehiculosAsync("qa", false, vehicle.Placa, new(false, [vehicle.CentroId]), CancellationToken.None);
        Assert.Contains(allowed, x => x.Id == vehicle.Id);
        Assert.All(allowed, x => Assert.Equal(vehicle.CentroId, x.CentroId));
        var denied = await service.ObtenerVehiculosAsync("qa", false, vehicle.Placa, new(false, [Guid.NewGuid()]), CancellationToken.None);
        Assert.Empty(denied);
        var contextual = await service.ObtenerVehiculosAsync("tecnico", true, vehicle.Placa, new(false, [Guid.NewGuid()]), CancellationToken.None, true);
        Assert.Contains(contextual, x => x.Id == vehicle.Id);
    }

    private sealed class TestEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "SistemaLlantas.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRoot;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
