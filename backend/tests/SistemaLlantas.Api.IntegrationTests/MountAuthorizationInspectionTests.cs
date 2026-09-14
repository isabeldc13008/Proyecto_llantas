using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class MountAuthorizationInspectionTests(TestApplicationFactory factory):IClassFixture<TestApplicationFactory>
{
 private const string Technician="qa-mount";
 private static readonly CancellationToken Ct=CancellationToken.None;
 private static ControllerContext Context(string user,string role,Guid? center=null,bool own=false)=>new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("username",user),new Claim(ClaimTypes.Role,role),new Claim("permiso","operaciones.montar"),new Claim("permiso","operaciones.solicitar"),new Claim("permiso","inspecciones.crear"),new Claim("permiso","operaciones.aprobar")}.Concat(center.HasValue?new[]{new Claim("centro_id",center.Value.ToString())}:Array.Empty<Claim>()).Concat(own?new[]{new Claim("permiso","operaciones.aprobar_propia")}:Array.Empty<Claim>()),"test"))}};
 private static OperacionesController Operations(IServiceProvider sp,LlantasDbContext db,Guid center,string user=Technician,bool own=false)=>new(sp.GetRequiredService<IOperacionService>(),sp.GetRequiredService<ICicloVidaLlantaService>(),db){ControllerContext=Context(user,user==Technician?"TECNICO":"ADMINISTRADOR",center,own)};
 private static InspeccionesController Inspections(IServiceProvider sp,LlantasDbContext db,string user=Technician,string role="TECNICO")=>new(sp.GetRequiredService<IInspeccionService>(),db,sp.GetRequiredService<IWebHostEnvironment>()){ControllerContext=Context(user,role)};
 private static async Task<(Llanta tire,Vehiculo vehicle,PosicionVehiculo position)> Setup(LlantasDbContext db,bool otherCenter=false)
 {
  var centers=await db.Centros.Where(x=>x.Activo).Take(2).ToListAsync();var sample=await db.Llantas.AsNoTracking().FirstAsync();
  var state=await db.EstadosLlanta.FirstAsync(x=>x.Activo&&x.PermiteMontaje&&x.Codigo!="EN_TRASLADO");
  var suffix=Guid.NewGuid().ToString("N")[..10];
  var tire=new Llanta("QA-"+suffix,"SER-"+suffix){CentroId=centers[otherCenter?1:0].Id,MarcaId=sample.MarcaId,ReferenciaId=sample.ReferenciaId,DimensionId=sample.DimensionId,TipoLlantaId=sample.TipoLlantaId,EstadoLlantaId=state.Id,ProfundidadInicial=12,UbicacionActual="Inventario"};
  var vehicle=new Vehiculo{NumeroInterno="QA-"+suffix,Placa="QA"+suffix[..4],CentroId=centers[0].Id,Tipo="Camión",Kilometraje=1000};
  var axle=new EjeVehiculo{Nombre="Eje QA",Numero=1,Orden=1,TipoEje="Direccional"};var position=new PosicionVehiculo{Codigo="P1",Lado="Izquierda",Ubicacion="Externa",Orden=1};axle.Posiciones.Add(position);vehicle.Ejes.Add(axle);db.Llantas.Add(tire);db.Vehiculos.Add(vehicle);await db.SaveChangesAsync();return(tire,vehicle,position);
 }
 private static CrearSolicitudOperacionDto Request(Llanta tire,PosicionVehiculo position,Guid? activity=null)=>new(){Tipo="Montaje",LlantaId=tire.Id,PosicionDestinoId=position.Id,TipoDestino="Posicion",Motivo="Montaje QA",KilometrajeVehiculo=1000,ActividadProgramadaId=activity};
 private static SolicitudOperacionDto Created(ActionResult<SolicitudOperacionDto> result)=>Assert.IsType<SolicitudOperacionDto>(Assert.IsType<CreatedResult>(result.Result).Value);
 [Fact]public async Task SinProgramacion_PendienteVisible_RechazarNoModificaInventario()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
  var request=Created(await Operations(sp,db,v.CentroId).Solicitar(Request(t,p),Ct));Assert.Equal("PENDIENTE_APROBACION",request.Estado);Assert.False(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==t.Id));
  var approver=Operations(sp,db,v.CentroId,"qa-admin");var pending=Assert.IsType<OkObjectResult>(await approver.Autorizaciones(null,null,null,null,null,null,null,1,Ct));Assert.Contains(request.Id.ToString(),JsonSerializer.Serialize(pending.Value));
  await Assert.ThrowsAsync<ValidacionException>(()=>approver.Resolver(request.Id,new(false,""),Ct));
  var rejected=await approver.Resolver(request.Id,new(false,"Rechazo QA"),Ct);Assert.Equal("RECHAZADO",rejected.Estado);Assert.Equal("qa-admin",rejected.Aprobador);
  Assert.False(await db.Movimientos.AnyAsync(x=>x.Detalles.Any(d=>d.LlantaId==t.Id)));Assert.Null((await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);Assert.Equal(t.EstadoLlantaId,(await db.Llantas.AsNoTracking().SingleAsync(x=>x.Id==t.Id)).EstadoLlantaId);
 }
 [Theory][InlineData("ADMINISTRADOR")][InlineData("SUPERVISOR_ADMINISTRADOR")]
 public async Task Aprobar_CreaAsignacionYMovimiento(string role)
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);var request=Created(await Operations(sp,db,v.CentroId).Solicitar(Request(t,p),Ct));
  var approver=Operations(sp,db,v.CentroId,"qa-admin");approver.ControllerContext=Context("qa-admin",role,v.CentroId);Assert.Equal("EJECUTADO",(await approver.Resolver(request.Id,new(true,null),Ct)).Estado);
  var assignment=await db.AsignacionesLlantaPosicion.SingleAsync(x=>x.LlantaId==t.Id&&x.EsActiva);Assert.Equal(p.Id,assignment.PosicionVehiculoId);Assert.Equal(1000,assignment.KilometrajeMontaje);Assert.True(await db.Movimientos.AnyAsync(x=>x.Id==assignment.MovimientoOrigenId));
 }
 [Fact]public async Task Propia_RequierePermisoExplicito()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);var controller=Operations(sp,db,v.CentroId);var request=Created(await controller.Solicitar(Request(t,p),Ct));await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>controller.Resolver(request.Id,new(true,null),Ct));
  var explicitPermission=Operations(sp,db,v.CentroId,Technician,true);Assert.Equal("EJECUTADO",(await explicitPermission.Resolver(request.Id,new(true,null),Ct)).Estado);
 }
 [Fact]public async Task ProgramacionValida_EjecutaSinSegundaAutorizacion_YNoSeReutiliza()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
  var activity=new ActividadProgramada{TipoActividad="Montaje",CentroId=v.CentroId,VehiculoId=v.Id,PosicionVehiculoId=p.Id,LlantaId=t.Id,TecnicoId=Technician,Estado=EstadoActividad.Pendiente,FechaProgramada=DateTimeOffset.UtcNow};db.ActividadesProgramadas.Add(activity);await db.SaveChangesAsync();
  var controller=Operations(sp,db,v.CentroId);var request=Created(await controller.Solicitar(Request(t,p,activity.Id),Ct));Assert.Equal("EJECUTADO",request.Estado);Assert.Equal(activity.Id,(await db.SolicitudesOperacion.SingleAsync(x=>x.Id==request.Id)).ActividadProgramadaId);Assert.True(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==t.Id&&x.EsActiva));await Assert.ThrowsAsync<ConflictoException>(()=>controller.Solicitar(Request(t,p,activity.Id),Ct));
 }
 [Theory][InlineData(true)][InlineData(false)]public async Task Aprobar_RevalidaPosicionYLlanta(bool occupiedPosition)
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);var request=Created(await Operations(sp,db,v.CentroId).Solicitar(Request(t,p),Ct));var(other,_,otherPosition)=await Setup(db);
  await sp.GetRequiredService<IOperacionService>().MoverAsync(new(){LlantaId=occupiedPosition?other.Id:t.Id,PosicionDestinoId=occupiedPosition?p.Id:otherPosition.Id,TipoDestino="Posicion",Motivo="Otra operación",KilometrajeVehiculo=1000},"qa-other",new(true,[]),Ct);
  await Assert.ThrowsAsync<ConflictoException>(()=>Operations(sp,db,v.CentroId,"qa-admin").Resolver(request.Id,new(true,null),Ct));db.ChangeTracker.Clear();Assert.Equal(EstadoSolicitudOperacion.PENDIENTE_APROBACION,(await db.SolicitudesOperacion.SingleAsync(x=>x.Id==request.Id)).Estado);
 }
 [Theory][InlineData(false)][InlineData(true)]public async Task Inspeccion_AsignaLlantaGlobal_ConservaCentroYSoloRegistraMontaje(bool otherCenter)
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db,otherCenter);var originalCenter=t.CentroId;var controller=Inspections(sp,db);
  var created=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await controller.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
  var found=Assert.IsType<OkObjectResult>(await controller.BuscarLlanta(t.Codigo,Ct,created.Id));Assert.Contains(t.Id.ToString(),JsonSerializer.Serialize(found.Value));
  await controller.Asignar(created.Id,p.Id,new(t.Id,"Posición vacía"),sp.GetRequiredService<IOperacionService>(),Ct);
  await controller.Detalle(created.Id,p.Id,new(){ProfundidadExterior=10,ProfundidadCentro=10,ProfundidadInterior=10},Ct);db.ChangeTracker.Clear();
  Assert.Equal(originalCenter,(await db.Llantas.SingleAsync(x=>x.Id==t.Id)).CentroId);var movement=Assert.Single(await db.Movimientos.Where(x=>x.InspeccionId==created.Id).ToListAsync());Assert.Equal("MONTAJE",movement.Tipo);Assert.Equal(v.CentroId,movement.CentroId);
  var detail=await db.InspeccionesDetalle.SingleAsync(x=>x.InspeccionId==created.Id&&x.PosicionVehiculoId==p.Id);Assert.Equal(t.Id,detail.LlantaId);Assert.Equal(10,detail.ProfundidadExterior);
  await Assert.ThrowsAsync<ConflictoException>(()=>controller.Asignar(created.Id,p.Id,new(t.Id,"Duplicado"),sp.GetRequiredService<IOperacionService>(),Ct));
 }
 [Fact]public async Task InspeccionAjena_NoPermiteAsignacion()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db,true);var owner=Inspections(sp,db);var created=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await owner.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
  await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Inspections(sp,db,"otro-tecnico").Asignar(created.Id,p.Id,new(t.Id,"Ajena"),sp.GetRequiredService<IOperacionService>(),Ct));
 }
 [Fact]public async Task ProgramacionCancelada_NoCreaSolicitudNiMontaje()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
  var activity=new ActividadProgramada{TipoActividad="Montaje",CentroId=v.CentroId,VehiculoId=v.Id,TecnicoId=Technician,Estado=EstadoActividad.Cancelada};db.ActividadesProgramadas.Add(activity);await db.SaveChangesAsync();
  await Assert.ThrowsAsync<ValidacionException>(()=>Operations(sp,db,v.CentroId).Solicitar(Request(t,p,activity.Id),Ct));Assert.False(await db.SolicitudesOperacion.AnyAsync(x=>x.LlantaId==t.Id));
 }
 [Fact]public async Task Inspeccion_RechazaLlantaQueFueMontadaDespuesDeBuscar()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);var controller=Inspections(sp,db);
  var created=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await controller.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
  var(_,_,otherPosition)=await Setup(db);await sp.GetRequiredService<IOperacionService>().MoverAsync(new(){LlantaId=t.Id,PosicionDestinoId=otherPosition.Id,TipoDestino="Posicion",Motivo="Otro montaje",KilometrajeVehiculo=1000},"otro",new(true,[]),Ct);
  await Assert.ThrowsAsync<ConflictoException>(()=>controller.Asignar(created.Id,p.Id,new(t.Id,"Posición vacía"),sp.GetRequiredService<IOperacionService>(),Ct));db.ChangeTracker.Clear();Assert.Null((await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
 }
 [Fact]public async Task Alertas_SoloEvaluaReglasActivasDelCentroOGlobales()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db,true);var prefix="RULE-"+Guid.NewGuid().ToString("N")[..10];
  var global=new ParametroAlerta{Codigo=prefix,Tipo="PROFUNDIDAD_MINIMA",Operador="<=",Valor=3};
  db.ParametrosAlerta.AddRange(global,new(){Codigo=prefix+"-OTHER",CentroId=t.CentroId,Tipo="PROFUNDIDAD_MINIMA",Operador="<=",Valor=3},new(){Codigo=prefix+"-OFF",CentroId=v.CentroId,Tipo="PROFUNDIDAD_MINIMA",Operador="<=",Valor=3,Activo=false});await db.SaveChangesAsync();
  var service=sp.GetRequiredService<IInspeccionService>();var inspection=await service.CrearAsync(new(){VehiculoId=v.Id,Kilometraje=1000},Technician,new(true,[]),Ct);
  await service.GuardarDetalleAsync(inspection.Id,p.Id,new(){ProfundidadExterior=2,ProfundidadCentro=2,ProfundidadInterior=2},Technician,Ct);
  var alerts=await db.AlertasInspeccion.Where(x=>x.InspeccionId==inspection.Id&&x.Tipo.StartsWith(prefix)).ToListAsync();Assert.Equal(prefix,Assert.Single(alerts).Tipo);
 }
 [Fact]public async Task Montajes_BusquedaEncuentraMasAllaPrimeraPagina()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,_)=await Setup(db);
  for(var n=0;n<55;n++)db.Llantas.Add(new Llanta("AAA-"+Guid.NewGuid().ToString("N")[..16],"PAGE-"+Guid.NewGuid().ToString("N")){CentroId=t.CentroId,MarcaId=t.MarcaId,ReferenciaId=t.ReferenciaId,DimensionId=t.DimensionId,TipoLlantaId=t.TipoLlantaId,EstadoLlantaId=t.EstadoLlantaId});await db.SaveChangesAsync();
  var found=Assert.IsType<OkObjectResult>(await Operations(sp,db,v.CentroId).Disponibles(v.Id,t.Codigo,Ct));Assert.Contains(t.Id.ToString(),JsonSerializer.Serialize(found.Value));
 }
}
