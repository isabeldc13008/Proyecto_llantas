using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class SchedulingPhase5Tests:IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;
    public SchedulingPhase5Tests(WebApplicationFactory<Program> factory)=>this.factory=factory;

    [Fact]
    public async Task Programacion_FiltraAdvierteSolapamientoYAlimentaMisActividades()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>
        {
            await using var tx=await db.Database.BeginTransactionAsync();var vehicle=await db.Vehiculos.AsNoTracking().FirstAsync();var tech=await db.UsuariosSistema.Include(x=>x.Rol).Include(x=>x.Centros).FirstAsync(x=>x.Rol.Codigo=="TECNICO");if(tech.Centros.All(x=>x.CentroId!=vehicle.CentroId))db.UsuariosCentros.Add(new(){UsuarioId=tech.Id,CentroId=vehicle.CentroId,UsuarioCreacion="qa"});await db.SaveChangesAsync();var scheduling=scope.ServiceProvider.GetRequiredService<IProgramacionService>();var operations=scope.ServiceProvider.GetRequiredService<IOperacionService>();var start=DateTimeOffset.UtcNow.AddDays(2);var first=await scheduling.CrearAsync(Input("Inspección",start,start.AddHours(1),vehicle,tech),"qa",new(false,[vehicle.CentroId]),CancellationToken.None);var second=await scheduling.CrearAsync(Input("Movimiento",start.AddMinutes(30),start.AddHours(2),vehicle,tech),"qa",new(false,[vehicle.CentroId]),CancellationToken.None);
            var filtered=await scheduling.ListarAsync(new(vehicle.CentroId,vehicle.Id,tech.Id,"Inspe",null,start.AddMinutes(-1),start.AddHours(3),"Alta"),new(false,[vehicle.CentroId]),CancellationToken.None);Assert.Single(filtered,x=>x.Id==first.Id);var overlap=Assert.Single(await scheduling.ListarAsync(new(null,null,null,"Movimiento",null,null,null,null),new(false,[vehicle.CentroId]),CancellationToken.None),x=>x.Id==second.Id);Assert.True(overlap.TieneSolapamiento);var mine=await operations.MisActividadesAsync(tech.Username,new(false,[vehicle.CentroId]),CancellationToken.None);Assert.Contains(mine,x=>x.Id==first.Id&&x.RutaInicio.Contains(vehicle.Id.ToString()));var running=await operations.IniciarActividadAsync(first.Id,tech.Username,new(false,[vehicle.CentroId]),CancellationToken.None);Assert.Equal("EnEjecucion",running.Estado);var done=await operations.CompletarActividadAsync(first.Id,tech.Username,new(false,[vehicle.CentroId]),CancellationToken.None);Assert.Equal("Cumplida",done.Estado);await tx.RollbackAsync();
        });
    }

    [Fact]
    public async Task ProgramacionMasiva_RechazaDuplicadosYCentroNoAutorizado()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var vehicle=await db.Vehiculos.AsNoTracking().FirstAsync();var tech=await db.UsuariosSistema.Include(x=>x.Rol).FirstAsync(x=>x.Rol.Codigo=="TECNICO");var input=Input("Inspección",DateTimeOffset.UtcNow.AddDays(5),DateTimeOffset.UtcNow.AddDays(5).AddHours(1),vehicle,tech);var service=scope.ServiceProvider.GetRequiredService<IProgramacionService>();await Assert.ThrowsAsync<ConflictoException>(()=>service.CrearMasivaAsync(new([input,input]),"qa",new(true,[]),CancellationToken.None));await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CrearAsync(input,"qa",new(false,[Guid.NewGuid()]),CancellationToken.None));
    }

    private static GuardarProgramacionDto Input(string type,DateTimeOffset start,DateTimeOffset end,Vehiculo vehicle,UsuarioSistema tech)=>new(){Tipo=type,Inicio=start,Fin=end,CentroId=vehicle.CentroId,VehiculoId=vehicle.Id,TecnicoUsuarioId=tech.Id,Prioridad="Alta"};
}
