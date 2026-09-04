using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Vehiculos;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class VehiclePhase2Tests:IClassFixture<TestApplicationFactory>
{
 private readonly TestApplicationFactory factory;
 public VehiclePhase2Tests(TestApplicationFactory factory)=>this.factory=factory;

 [Fact]
 public async Task CrearVehiculo_ClonaConfiguracionDinamicaSinLimiteFijo()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>{await using var tx=await db.Database.BeginTransactionAsync();var service=scope.ServiceProvider.GetRequiredService<IVehiculoService>();var center=await db.Centros.AsNoTracking().FirstAsync();var suffix=Guid.NewGuid().ToString("N")[..8];
  var cfg=await service.CrearConfiguracionAsync(new($"QA-{suffix}","Configuración variable QA","Camión",[new(1,"Eje delantero","Direccional",[new("P1","Izquierda","Externa",1),new("P2","Derecha","Externa",2)]),new(2,"Eje libre","Remolque",[new("P3","Izquierda","Externa",1),new("P4","Izquierda","Interna",2),new("P5","Derecha","Interna",3),new("P6","Derecha","Externa",4)])]),"qa",CancellationToken.None);
  var vehicle=await service.CrearAsync(new($"QA-{suffix}","QA000","Camión",center.Id,cfg.Id,1200,"Activo",null),"qa",new(true,[]),CancellationToken.None);
  Assert.Equal(2,vehicle.Ejes.Count);Assert.Equal(6,vehicle.Ejes.Sum(x=>x.Posiciones.Count));Assert.Equal(["P1","P2","P3","P4","P5","P6"],vehicle.Ejes.SelectMany(x=>x.Posiciones).Select(x=>x.Codigo));await tx.RollbackAsync();});
 }

 [Fact]
 public async Task CambiarConfiguracion_ConMontajeActivo_EsBloqueado()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>{await using var tx=await db.Database.BeginTransactionAsync();var service=scope.ServiceProvider.GetRequiredService<IVehiculoService>();var center=await db.Centros.AsNoTracking().FirstAsync();var sample=await db.Llantas.AsNoTracking().FirstAsync();var suffix=Guid.NewGuid().ToString("N")[..8];var tire=new Llanta($"QB-{suffix}",$"SER-{suffix}"){MarcaId=sample.MarcaId,ReferenciaId=sample.ReferenciaId,DimensionId=sample.DimensionId,TipoLlantaId=sample.TipoLlantaId,EstadoLlantaId=sample.EstadoLlantaId,CentroId=center.Id,UbicacionActual="QA",ProfundidadInicial=10,UsuarioCreacion="qa"};db.Llantas.Add(tire);await db.SaveChangesAsync();
  var cfg=await service.CrearConfiguracionAsync(new($"QB-{suffix}","Configuración bloqueo QA","Camión",[new(1,"Eje delantero","Direccional",[new("P1","Izquierda","Externa",1)])]),"qa",CancellationToken.None);var vehicle=await service.CrearAsync(new($"QB-{suffix}","QB000","Camión",center.Id,cfg.Id,0,"Activo",null),"qa",new(true,[]),CancellationToken.None);var position=await db.PosicionesVehiculo.SingleAsync(x=>x.EjeVehiculo.VehiculoId==vehicle.Id);db.AsignacionesLlantaPosicion.Add(new(){LlantaId=tire.Id,PosicionVehiculoId=position.Id,MovimientoOrigenId=Guid.NewGuid(),UsuarioCreacion="qa"});await db.SaveChangesAsync();
  var error=await Assert.ThrowsAsync<ConflictoException>(()=>service.ActualizarAsync(vehicle.Id,new(vehicle.NumeroInterno,vehicle.Placa,vehicle.Tipo,center.Id,null,0,"Activo",vehicle.RowVersion),"qa",new(true,[]),CancellationToken.None));Assert.Contains("desmontar",error.Message,StringComparison.OrdinalIgnoreCase);await tx.RollbackAsync();});
 }
}
