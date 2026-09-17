using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class InventoryDashboardTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AdministratorCanReadInventoryMetricsAndDashboardSummary()
    {
        var client = factory.CreateClient();
        await Login(client, "administrador", "admin123");

        var metrics = await client.GetFromJsonAsync<InventoryMetrics>("/api/inventario/metricas");
        Assert.NotNull(metrics);
        Assert.True(metrics!.Disponibles >= 0);
        Assert.True(metrics.Bloqueadas >= 0);
        Assert.True(metrics.ConAtencion >= 0);

        var dashboard = await client.GetFromJsonAsync<DashboardSummary>("/api/dashboard/resumen");
        Assert.NotNull(dashboard);
        Assert.NotNull(dashboard!.Metrics);
        Assert.NotNull(dashboard.Centers);
        Assert.NotNull(dashboard.Attention);
        Assert.NotNull(dashboard.Today);
    }

    [Fact]
    public async Task TechnicianCannotReadInventoryModuleWithoutItsPermission()
    {
        var client = factory.CreateClient();
        await Login(client, "tecnico", "tec123");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/inventario/metricas")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/inventario/reservas")).StatusCode);
    }

    [Fact]
    public async Task ReservationCanBeCreatedAndReleasedWithoutChangingTheTireMaster()
    {
        var client = factory.CreateClient();
        await Login(client, "administrador", "admin123");

        Guid tireId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
            var tire = await db.Llantas.AsNoTracking().Where(x => x.EstadoLlanta.PermiteMontaje)
                .Where(x => !db.AsignacionesLlantaPosicion.Any(a => a.LlantaId == x.Id && a.EsActiva))
                .Where(x => !db.SolicitudesOperacion.Any(r => r.LlantaId == x.Id && r.Activo && r.Tipo == "RESERVA" && r.Estado == EstadoSolicitudOperacion.EJECUTADO))
                .OrderBy(x => x.Codigo).FirstOrDefaultAsync();
            if (tire is null) throw new Xunit.Sdk.XunitException("El seed debe contener al menos una llanta disponible para reservar.");
            tireId = tire.Id;
        }

        var reserve = await client.PostAsJsonAsync($"/api/inventario/{tireId}/reservar", new { motivo = "Prueba de reserva" });
        Assert.Equal(HttpStatusCode.NoContent, reserve.StatusCode);

        var reservations = await client.GetFromJsonAsync<Reservation[]>("/api/inventario/reservas");
        Assert.Contains(reservations!, x => x.LlantaId == tireId && x.Motivo == "Prueba de reserva");

        var duplicate = await client.PostAsJsonAsync($"/api/inventario/{tireId}/reservar", new { motivo = "Duplicada" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var release = await client.PostAsync($"/api/inventario/{tireId}/liberar-reserva", null);
        Assert.Equal(HttpStatusCode.NoContent, release.StatusCode);

        await using var cleanup = factory.Services.CreateAsyncScope();
        var cleanupDb = cleanup.ServiceProvider.GetRequiredService<LlantasDbContext>();
        Assert.Empty(await cleanupDb.SolicitudesOperacion.AsNoTracking().Where(x => x.LlantaId == tireId && x.Activo && x.Tipo == "RESERVA").ToListAsync());
    }

    private static async Task Login(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }

    private sealed record LoginResponse(string AccessToken);
    private sealed record InventoryMetrics(int Disponibles, int EnReparacion, int EnReencauche, int EnTraslado, int Bloqueadas, int ConAtencion);
    private sealed record DashboardSummary(DashboardMetrics Metrics, object[] Attention, object[] Today, object Fleet, object TireDistribution, object[] Centers);
    private sealed record DashboardMetrics(int TotalLlantas, int Montadas, int Disponibles, int AtencionRequerida, int InspeccionesVencidas, int VehiculosIncompletos, int EnReparacion, int EnReencauche, int DisposicionFinal, int ProgramacionesPendientes);
    private sealed record Reservation(Guid Id, Guid LlantaId, string Llanta, Guid? PosicionId, string VehiculoId, string Motivo, string Solicitante, DateTimeOffset Fecha);
}
