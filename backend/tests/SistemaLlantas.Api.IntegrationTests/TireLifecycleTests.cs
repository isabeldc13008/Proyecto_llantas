using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class TireLifecycleTests:IClassFixture<TestApplicationFactory>
{
 private readonly TestApplicationFactory factory;public TireLifecycleTests(TestApplicationFactory factory)=>this.factory=factory;
 [Fact] public async Task TrasladoCentro_GeneraMovimientoYNoEsEdicionSilenciosa()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>{await using var tx=await db.Database.BeginTransactionAsync();var centers=await db.Centros.Take(2).ToListAsync();Assert.Equal(2,centers.Count);var tire=await NuevaLlanta(db,centers[0].Id);var llantaService=scope.ServiceProvider.GetRequiredService<ILlantaService>();await Assert.ThrowsAsync<ConflictoException>(()=>llantaService.ActualizarAsync(tire.Id,new(){CentroId=centers[1].Id},"qa",new(true,[]),CancellationToken.None));var lifecycle=scope.ServiceProvider.GetRequiredService<ICicloVidaLlantaService>();await lifecycle.TrasladarCentroAsync(tire.Id,new(centers[1].Id,"Traslado de prueba","Trazabilidad QA"),"qa",new(true,[]),CancellationToken.None);db.ChangeTracker.Clear();var moved=await db.Llantas.SingleAsync(x=>x.Id==tire.Id);var movement=await db.Movimientos.Include(x=>x.Detalles).SingleAsync(x=>x.Detalles.Any(d=>d.LlantaId==tire.Id));Assert.Equal(centers[1].Id,moved.CentroId);Assert.Equal(centers[0].Id,movement.CentroId);Assert.Equal(centers[1].Id,Assert.Single(movement.Detalles).CentroDestinoId);Assert.Equal("qa",movement.Usuario);await tx.RollbackAsync();});
 }
 [Fact] public async Task MontajeYDesmontaje_CalculanRecorridoYConservanUnicidadActiva()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>{await using var tx=await db.Database.BeginTransactionAsync();var center=await db.Centros.FirstAsync();var tire=await NuevaLlanta(db,center.Id,true);var vehicle=new Vehiculo{NumeroInterno=$"KM-{Guid.NewGuid():N}"[..11],Placa="KM001",Tipo="Camión",CentroId=center.Id,Kilometraje=1000,UsuarioCreacion="qa"};var axle=new EjeVehiculo{Numero=1,Orden=1,Nombre="Eje 1",TipoEje="Direccional",UsuarioCreacion="qa"};var position=new PosicionVehiculo{Codigo="P1",Lado="Izquierda",Ubicacion="Externa",Orden=1,UsuarioCreacion="qa"};axle.Posiciones.Add(position);vehicle.Ejes.Add(axle);db.Vehiculos.Add(vehicle);await db.SaveChangesAsync();var operations=scope.ServiceProvider.GetRequiredService<IOperacionService>();var scopeAll=new AlcanceCentros(true,[]);await operations.MoverAsync(new(){LlantaId=tire.Id,PosicionDestinoId=position.Id,TipoDestino="Posicion",Motivo="Montaje QA",KilometrajeVehiculo=1000},"qa",scopeAll,CancellationToken.None);Assert.Single(await db.AsignacionesLlantaPosicion.Where(x=>x.LlantaId==tire.Id&&x.EsActiva).ToListAsync());await operations.DesmontarAsync(new(){PosicionId=position.Id,Destino="Inventario",Motivo="Desmontaje QA",KilometrajeVehiculo=1250},"qa",scopeAll,CancellationToken.None);db.ChangeTracker.Clear();var assignment=await db.AsignacionesLlantaPosicion.SingleAsync(x=>x.LlantaId==tire.Id);var updated=await db.Llantas.SingleAsync(x=>x.Id==tire.Id);Assert.False(assignment.EsActiva);Assert.Equal(1000,assignment.KilometrajeMontaje);Assert.Equal(1250,assignment.KilometrajeDesmontaje);Assert.Equal(250,assignment.KilometrajeRecorrido);Assert.Equal(250,updated.KilometrajeAcumulado);Assert.Empty(await db.AsignacionesLlantaPosicion.Where(x=>x.LlantaId==tire.Id&&x.EsActiva).ToListAsync());await tx.RollbackAsync();});
 }
 private static async Task<Llanta> NuevaLlanta(LlantasDbContext db,Guid centerId,bool mountingState=false){var sample=await db.Llantas.AsNoTracking().FirstAsync();var stateId=mountingState?await db.EstadosLlanta.Where(x=>x.PermiteMontaje).Select(x=>x.Id).FirstAsync():sample.EstadoLlantaId;var suffix=Guid.NewGuid().ToString("N")[..10];var tire=new Llanta($"LC-{suffix}",$"LCS-{suffix}"){MarcaId=sample.MarcaId,ReferenciaId=sample.ReferenciaId,DimensionId=sample.DimensionId,TipoLlantaId=sample.TipoLlantaId,EstadoLlantaId=stateId,CentroId=centerId,UbicacionActual="QA",ProfundidadInicial=12,UsuarioCreacion="qa"};db.Llantas.Add(tire);await db.SaveChangesAsync();return tire;}
}
