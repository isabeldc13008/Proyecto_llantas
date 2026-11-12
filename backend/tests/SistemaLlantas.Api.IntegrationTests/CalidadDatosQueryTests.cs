using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

// Query translation only. No SQL connection is opened and no database is created.
public sealed class CalidadDatosQueryTests
{
    private static LlantasDbContext Db() => new(new DbContextOptionsBuilder<LlantasDbContext>()
        .UseSqlServer("Server=localhost;Database=QueryOnly;Integrated Security=True;TrustServerCertificate=True").Options);

    [Fact] public void CountsAndSharedFiltersAreAggregatedInSqlWithAuthorizedEvidence()
    {
        using var db=Db();var service=new AnaliticaService(db);var center=Guid.NewGuid();var vehicle=Guid.NewGuid();
        var state=Guid.NewGuid();var brand=Guid.NewGuid();var reference=Guid.NewGuid();
        var f=new FiltroMantenimiento {Montaje="TODAS",CentroId=center,VehiculoId=vehicle,EstadoId=state,MarcaId=brand,ReferenciaId=reference};
        var sql=service.TotalesCalidadQuery(f,new(false,[center])).ToQueryString();
        foreach(var id in new[]{center,vehicle,state,brand,reference})Assert.Contains(id.ToString(),sql);
        Assert.Contains("COUNT",sql);Assert.Contains("Inspeccion",sql);Assert.Contains("Movimiento",sql);
        Assert.Contains("EsActiva",sql);Assert.Contains("ProfundidadExterior",sql);Assert.Contains(">= 0",sql);
        Assert.DoesNotContain("DEMO",sql);Assert.DoesNotContain("5000",sql);Assert.Empty(db.ChangeTracker.Entries());
    }
    [Fact] public async Task RejectsForeignCenterAndUnknownCategoryBeforeDatabaseAccess()
    {
        using var db=Db();var service=new AnaliticaService(db);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CalidadDatosAsync(new(){CentroId=Guid.NewGuid()},"todas",new(false,[]),default));
        await Assert.ThrowsAsync<ValidacionException>(()=>service.CalidadDatosAsync(new(),"criticas",new(true,[]),default));
        await Assert.ThrowsAsync<ValidacionException>(()=>service.CalidadDatosAsync(new(){Tamano=101},"todas",new(true,[]),default));
    }
    [Fact] public void NewEndpointKeepsAnalyticsAuthorizationAndCancellation()
    {
        var auth=Assert.Single(typeof(AnaliticaController).GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>());
        Assert.Equal("Analitica.Consultar",auth.Policy);
        Assert.Contains(typeof(AnaliticaController).GetMethod("CalidadDatos")!.GetParameters(),p=>p.ParameterType==typeof(CancellationToken));
    }
}
