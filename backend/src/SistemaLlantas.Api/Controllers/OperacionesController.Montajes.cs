using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.Controllers;
public sealed partial class OperacionesController
{
 private async Task<ActionResult<SolicitudOperacionDto>> SolicitarJuego(CrearSolicitudOperacionDto dto,CancellationToken ct)
 {
  if(!User.HasClaim("permiso","operaciones.montar"))return Forbid();
  if(dto.ActividadProgramadaId.HasValue)throw new ValidacionException("Ejecuta la asignación guardada desde su programación.");
  if(!dto.Tipo.Equals("Reemplazar llanta",StringComparison.OrdinalIgnoreCase)&&!dto.Tipo.Equals("Cambio de juego",StringComparison.OrdinalIgnoreCase))throw new ValidacionException("Usa Montaje para una posición libre o Reemplazar llanta para una ocupada.");
  AsignacionesMontaje.ValidarNueva(dto.Tipo,dto.Asignaciones);
  if(!dto.VehiculoId.HasValue||dto.VehiculoId==Guid.Empty)throw new ValidacionException("Selecciona un vehículo válido.");
  if(!dto.KilometrajeVehiculo.HasValue||dto.KilometrajeVehiculo<0||string.IsNullOrWhiteSpace(dto.Motivo)||dto.Motivo.Length>500||(dto.Observaciones?.Length??0)>1000)throw new ValidacionException("Ingresa kilometraje, motivo (máximo 500 caracteres) y observaciones válidas.");
  var a=User.AlcanceCentros();Guid first=Guid.Empty;
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
   db.ChangeTracker.Clear();first=Guid.Empty;await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
   await service.ValidarAsignacionesAsync(dto.VehiculoId??Guid.Empty,dto.Asignaciones!,a,null,ct);
   var vehicle=await db.Vehiculos.SingleOrDefaultAsync(v=>v.Id==dto.VehiculoId,ct)??throw new ConflictoException("El vehículo ya no está disponible. Actualiza la solicitud.");
   var positions=dto.Asignaciones!.Select(f=>f.PosicionId).ToArray();var outgoing=await db.AsignacionesLlantaPosicion.Where(x=>x.EsActiva&&positions.Contains(x.PosicionVehiculoId)).Select(x=>new SalidaKilometraje(x.Llanta.Codigo,x.PosicionVehiculo.Codigo,x.KilometrajeMontaje)).ToListAsync(ct);
   KilometrajeOperacion.Validar(dto.KilometrajeVehiculo,vehicle.Kilometraje,outgoing);
   var group=Guid.NewGuid();
   foreach(var f in dto.Asignaciones!){var s=new SolicitudOperacion{GrupoOperacionId=group,Tipo=dto.Tipo.Equals("Reemplazar llanta",StringComparison.OrdinalIgnoreCase)?"Reemplazar llanta":"Cambio de juego",Estado=EstadoSolicitudOperacion.PENDIENTE_APROBACION,CentroId=vehicle.CentroId,LlantaId=f.LlantaId,LlantaDesplazadaId=f.LlantaActualId,PosicionDestinoId=f.PosicionId,TipoDestino="Posicion",DestinoDesplazada="Inventario",Motivo=dto.Motivo.Trim(),Observaciones=dto.Observaciones,KilometrajeVehiculo=dto.KilometrajeVehiculo,Solicitante=Usuario(),UsuarioCreacion=Usuario()};db.SolicitudesOperacion.Add(s);if(first==Guid.Empty)first=s.Id;}
   await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
  });return Created(string.Empty,await ObtenerSolicitud(first,a,ct));
 }
 private async Task ResolverGrupo(SolicitudOperacion item,ResolverSolicitudDto dto,AlcanceCentros a,CancellationToken ct)
 {
  var rows=await db.SolicitudesOperacion.Where(s=>s.GrupoOperacionId==item.GrupoOperacionId&&s.Activo).ToListAsync(ct);
  if(rows.Any(s=>!a.Autoriza(s.CentroId)))throw new UnauthorizedAccessException();
  if(rows.Any(s=>s.Estado!=EstadoSolicitudOperacion.PENDIENTE_APROBACION))throw new ConflictoException("El grupo ya fue procesado.");
  if(rows.Any(s=>s.Solicitante==Usuario())&&!User.HasClaim("permiso","operaciones.aprobar_propia"))throw new UnauthorizedAccessException("No puede resolver su propia solicitud.");
  if(!dto.Aprobar&&(string.IsNullOrWhiteSpace(dto.Motivo)||dto.Motivo.Length>500))throw new ValidacionException("El motivo del rechazo es obligatorio (máximo 500 caracteres).");
  foreach(var s in rows){s.Aprobador=Usuario();s.FechaDecision=DateTimeOffset.UtcNow;if(!dto.Aprobar){s.Estado=EstadoSolicitudOperacion.RECHAZADO;s.MotivoRechazo=dto.Motivo;}}
  if(dto.Aprobar)await service.EjecutarReemplazosAsync(rows,item.KilometrajeVehiculo??throw new ValidacionException("Falta kilometraje."),Usuario(),a,ct);
  await db.SaveChangesAsync(ct);
 }
 [HttpGet("api/actividades/{id:guid}/montaje"),Authorize(Policy="Operaciones.Montar")]
 public async Task<TrabajoMontajeDto> TrabajoMontaje(Guid id,CancellationToken ct)
 {
  var (activity,rows)=await Trabajo(id,ct);
  var ids=rows.Select(s=>s.LlantaId).ToArray();var labels=await db.Llantas.Where(t=>ids.Contains(t.Id)).Select(t=>new{t.Id,t.Codigo,t.Serial,MarcaReferencia=t.Marca.Nombre+" · "+t.Referencia.Nombre,Dimension=t.Dimension.Nombre}).ToDictionaryAsync(t=>t.Id,ct);
  return new(id,activity.GrupoProgramacionId,activity.VehiculoId!.Value,activity.TipoActividad,rows[0].Motivo,rows[0].Observaciones,rows.Select(s=>new AsignacionMontajeDto(s.PosicionDestinoId!.Value,s.LlantaId,s.LlantaDesplazadaId,labels.GetValueOrDefault(s.LlantaId)?.Codigo,labels.GetValueOrDefault(s.LlantaId)?.Serial,labels.GetValueOrDefault(s.LlantaId)?.MarcaReferencia,labels.GetValueOrDefault(s.LlantaId)?.Dimension)).ToList());
 }
 [HttpPost("api/actividades/{id:guid}/montaje"),Authorize(Policy="Operaciones.Montar"),Authorize(Policy="Operaciones.Solicitar")]
 public async Task<IActionResult> EjecutarTrabajo(Guid id,EjecutarTrabajoMontajeDto dto,CancellationToken ct)
 {
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var (_,rows)=await Trabajo(id,ct);await service.EjecutarReemplazosAsync(rows,dto.Kilometraje,Usuario(),User.AlcanceCentros(),ct);await tx.CommitAsync(ct);});return Ok(new{estado="EJECUTADO"});
 }
 private async Task<(ActividadProgramada,List<SolicitudOperacion>)> Trabajo(Guid id,CancellationToken ct)
 {
  var a=User.AlcanceCentros();var item=await db.ActividadesProgramadas.Include(x=>x.TecnicoUsuario).SingleOrDefaultAsync(x=>x.Id==id&&x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Programación no encontrada.");
  var activities=item.GrupoProgramacionId.HasValue?await db.ActividadesProgramadas.Include(x=>x.TecnicoUsuario).Where(x=>x.GrupoProgramacionId==item.GrupoProgramacionId&&x.Activo).ToListAsync(ct):[item];
  if(activities.Any(x=>!a.Autoriza(x.CentroId)||(x.TecnicoId!=Usuario()&&x.TecnicoId!=Usuario()+".local"&&x.TecnicoUsuario?.Username!=Usuario())))throw new UnauthorizedAccessException("La programación pertenece a otro técnico o centro.");
  if(activities.Any(x=>x.Estado==EstadoActividad.Cancelada||x.Estado==EstadoActividad.Cumplida))throw new ConflictoException("La programación ya está cancelada o cumplida.");
  var ids=activities.Select(x=>x.Id).ToArray();var rows=await db.SolicitudesOperacion.Where(s=>s.Activo&&s.ActividadProgramadaId.HasValue&&ids.Contains(s.ActividadProgramadaId.Value)&&s.Estado==EstadoSolicitudOperacion.APROBADO).ToListAsync(ct);
  if(rows.Count!=activities.Count||rows.Count==0||!item.VehiculoId.HasValue)throw new ConflictoException("La programación no tiene una asignación completa. Solicita al planificador cancelarla y crearla nuevamente.");
  return(item,rows);
 }
}
