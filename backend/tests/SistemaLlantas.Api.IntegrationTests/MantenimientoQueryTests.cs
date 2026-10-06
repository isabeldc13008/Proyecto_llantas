using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class MantenimientoQueryTests
{
    private static LlantasDbContext Db() => new(new DbContextOptionsBuilder<LlantasDbContext>()
        .UseSqlServer("Server=localhost;Database=QueryOnly;Integrated Security=True;TrustServerCertificate=True").Options);

    [Fact] public void QueueFiltersAndPaginationTranslateToSql()
    {
        using var db = Db(); var service = new AnaliticaService(db); var center = Guid.NewGuid(); var vehicle = Guid.NewGuid();
        var query = service.ConsultaMantenimiento(new() { CentroId = center, VehiculoId = vehicle }, new(false, [center]));
        var sql = service.OrdenarMantenimiento(query.Where(x => x.Alertas > 0 || !x.UltimaCompleta))
            .Skip(20).Take(20).ToQueryString();
        Assert.Contains("OFFSET", sql); Assert.Contains("FETCH NEXT", sql);
        Assert.Contains("EsActiva", sql); Assert.Contains("MovimientoOrigenId", sql);
        Assert.Contains("Inspeccion", sql); Assert.Contains("Vehiculo", sql);
        Assert.Contains(center.ToString(), sql); Assert.Contains(vehicle.ToString(), sql);
        Assert.Empty(db.ChangeTracker.Entries());
    }
    [Fact] public void UnsupportedClassificationAndForeignCenterFailBeforeQuery()
    {
        using var db = Db(); var service = new AnaliticaService(db);
        Assert.Throws<ValidacionException>(() => service.ConsultaMantenimiento(new() { Clasificacion = "ATENCION_INMEDIATA" }, new(true, [])));
        Assert.Throws<UnauthorizedAccessException>(() => service.ConsultaMantenimiento(new() { CentroId = Guid.NewGuid() }, new(false, [])));
    }
    [Fact] public void AllScopeDoesNotRequireAnActiveAssignment()
    {
        using var db = Db(); var service = new AnaliticaService(db);
        var all = service.ConsultaMantenimiento(new() { Montaje = "TODAS" }, new(true, [])).ToQueryString();
        var mounted = service.ConsultaMantenimiento(new(), new(true, [])).ToQueryString();
        Assert.NotEqual(all, mounted);
        Assert.Contains("EsActiva", mounted);
    }
}
