using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class ScheduledMountSetTests(TestApplicationFactory factory):IClassFixture<TestApplicationFactory>
{
 [Theory][InlineData("success")][InlineData("rollback")][InlineData("duplicate")][InlineData("cancel")]
 public async Task AsignacionReservaEjecucionAtomica(string scenario)
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var operations=sp.GetRequiredService<IOperacionService>();var scheduling=sp.GetRequiredService<IProgramacionService>();var ct=CancellationToken.None;
  var tech=await db.UsuariosSistema.Include(t=>t.Centros).FirstAsync(t=>t.Activo&&t.Rol.Codigo=="TECNICO"&&t.Centros.Any(c=>c.Activo));var center=tech.Centros.First(c=>c.Activo).CentroId;var access=new AlcanceCentros(false,[center]);
  var sample=await db.Llantas.FirstAsync();var state=await db.EstadosLlanta.FirstAsync(s=>s.Activo&&s.PermiteMontaje&&!s.EsDisposicionFinal&&s.Codigo!="EN_TRASLADO");var suffix=Guid.NewGuid().ToString("N")[..10];
  var vehicle=new Vehiculo{CentroId=center,NumeroInterno="CJ-"+suffix,Placa=suffix,Tipo="Camión",Kilometraje=1000};var axle=new EjeVehiculo{Numero=1,Orden=1,Nombre="Eje",TipoEje="Direccional"};var positions=new[]{new PosicionVehiculo{Codigo="P1",Lado="Izquierda",Orden=1},new PosicionVehiculo{Codigo="P2",Lado="Derecha",Orden=2}};foreach(var p in positions)axle.Posiciones.Add(p);vehicle.Ejes.Add(axle);db.Vehiculos.Add(vehicle);
  var tires=Enumerable.Range(0,4).Select(i=>new Llanta($"CJ-{suffix}-{i}",$"SER-{suffix}-{i}"){CentroId=center,MarcaId=sample.MarcaId,ReferenciaId=sample.ReferenciaId,DimensionId=sample.DimensionId,TipoLlantaId=sample.TipoLlantaId,EstadoLlantaId=state.Id,ProfundidadInicial=12}).ToArray();db.Llantas.AddRange(tires);await db.SaveChangesAsync();
  for(var i=0;i<2;i++)await operations.MoverAsync(new(){LlantaId=tires[i].Id,PosicionDestinoId=positions[i].Id,TipoDestino="Posicion",Motivo="Preparar prueba",KilometrajeVehiculo=1000},tech.Username,access,ct);
  var start=DateTimeOffset.UtcNow.AddDays(5);var input=new GuardarProgramacionDto{Tipo="Cambio de juego",VehiculoId=vehicle.Id,CentroId=center,TecnicoUsuarioId=tech.Id,Inicio=start,Fin=start.AddHours(1),Motivo="Renovar juego",Asignaciones=[new(positions[0].Id,tires[2].Id,tires[0].Id),new(positions[1].Id,tires[3].Id,tires[1].Id)]};
  var plan=await scheduling.CrearAsync(input,"planner",access,ct);var requests=await db.SolicitudesOperacion.Where(s=>s.GrupoOperacionId==plan.GrupoProgramacionId).ToListAsync(ct);Assert.Equal(2,requests.Count);Assert.All(requests,s=>Assert.Equal(EstadoSolicitudOperacion.APROBADO,s.Estado));
  Assert.False(await SistemaLlantas.Infrastructure.Services.LlantasDisponibles.Consulta(db).AnyAsync(t=>t.Id==tires[2].Id));
  if(scenario=="duplicate"){await Assert.ThrowsAsync<ConflictoException>(()=>operations.ValidarAsignacionesAsync(vehicle.Id,input.Asignaciones!,access,null,ct));return;}
  if(scenario=="cancel"){await scheduling.CancelarAsync(plan.Id,new("Cancelar grupo"),"planner",access,ct);Assert.True(await SistemaLlantas.Infrastructure.Services.LlantasDisponibles.Consulta(db).AnyAsync(t=>t.Id==tires[2].Id));Assert.Equal(2,await db.ActividadesProgramadas.CountAsync(a=>a.GrupoProgramacionId==plan.GrupoProgramacionId&&a.Estado==EstadoActividad.Cancelada));return;}
  var controller=new OperacionesController(operations,sp.GetRequiredService<ICicloVidaLlantaService>(),db){ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim("username",tech.Username),new Claim("centro_id",center.ToString()),new Claim("permiso","operaciones.montar"),new Claim("permiso","operaciones.solicitar")],"test"))}}};
  var work=await controller.TrabajoMontaje(plan.Id,ct);Assert.Equal(tires[2].Id,work.Asignaciones[0].LlantaId);Assert.Equal(2,work.Asignaciones.Count);
  if(scenario=="rollback")
  {
   var second=await db.AsignacionesLlantaPosicion.SingleAsync(a=>a.LlantaId==tires[1].Id&&a.EsActiva);second.KilometrajeMontaje=2000;await db.SaveChangesAsync();
   await Assert.ThrowsAsync<ValidacionException>(()=>controller.EjecutarTrabajo(plan.Id,new(1500),ct));db.ChangeTracker.Clear();
   Assert.Equal(tires[0].Id,(await db.PosicionesVehiculo.SingleAsync(p=>p.Id==positions[0].Id)).LlantaActualId);Assert.Equal(tires[1].Id,(await db.PosicionesVehiculo.SingleAsync(p=>p.Id==positions[1].Id)).LlantaActualId);Assert.False(await db.SolicitudesOperacion.AnyAsync(s=>s.GrupoOperacionId==plan.GrupoProgramacionId&&s.MovimientoEjecutadoId.HasValue));return;
  }
  await controller.EjecutarTrabajo(plan.Id,new(1500),ct);db.ChangeTracker.Clear();
  Assert.Equal(2,await db.ActividadesProgramadas.CountAsync(a=>a.GrupoProgramacionId==plan.GrupoProgramacionId&&a.Estado==EstadoActividad.Cumplida));
  var executed=await db.SolicitudesOperacion.Where(s=>s.GrupoOperacionId==plan.GrupoProgramacionId).ToListAsync();Assert.All(executed,s=>Assert.Equal(EstadoSolicitudOperacion.EJECUTADO,s.Estado));var moves=executed.Select(s=>s.MovimientoEjecutadoId).ToArray();Assert.Equal(4,await db.MovimientosDetalle.CountAsync(d=>moves.Contains(d.MovimientoId)));
  Assert.Equal(2,await db.AsignacionesLlantaPosicion.CountAsync(a=>a.EsActiva&&(a.LlantaId==tires[2].Id||a.LlantaId==tires[3].Id)));await Assert.ThrowsAsync<ConflictoException>(()=>controller.EjecutarTrabajo(plan.Id,new(1500),ct));
 }
}
