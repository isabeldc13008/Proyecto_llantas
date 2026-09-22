using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class ProgramacionService
{
 private static bool EsMontaje(string tipo)=>tipo.Trim().Equals("Montaje",StringComparison.OrdinalIgnoreCase)||tipo.Trim().Equals("Cambio de juego",StringComparison.OrdinalIgnoreCase)||tipo.Trim().Equals("Reemplazar llanta",StringComparison.OrdinalIgnoreCase);
 private async Task<T> Transaccion<T>(Func<Task<T>> action,CancellationToken ct)
 {
  if(db.Database.CurrentTransaction is not null)return await action();
  return await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var result=await action();await tx.CommitAsync(ct);return result;});
 }
 private async Task<ActividadProgramada> CrearTrabajo(GuardarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  if(!EsMontaje(dto.Tipo)){var regular=await Construir(dto,usuario,ct);db.ActividadesProgramadas.Add(regular);await db.SaveChangesAsync(ct);return regular;}
  await new OperacionService(db).ValidarAsignacionesAsync(dto.VehiculoId??Guid.Empty,dto.Asignaciones!,alcance,null,ct);
  var group=Guid.NewGuid();ActividadProgramada? first=null;
  foreach(var fila in dto.Asignaciones!)
  {
   var activity=await Construir(dto,usuario,ct);activity.TipoActividad=dto.Tipo.Trim().Equals("Montaje",StringComparison.OrdinalIgnoreCase)?"Montaje":dto.Tipo.Trim().Equals("Reemplazar llanta",StringComparison.OrdinalIgnoreCase)?"Reemplazar llanta":"Cambio de juego";activity.GrupoProgramacionId=group;activity.LlantaId=fila.LlantaId;activity.PosicionVehiculoId=fila.PosicionId;
   if(first is not null)activity.IdempotencyKey=null;
   first??=activity;db.ActividadesProgramadas.Add(activity);
   db.SolicitudesOperacion.Add(new(){Tipo=activity.TipoActividad,GrupoOperacionId=group,ActividadProgramadaId=activity.Id,CentroId=activity.CentroId,LlantaId=fila.LlantaId,PosicionDestinoId=fila.PosicionId,LlantaDesplazadaId=fila.LlantaActualId,TipoDestino="Posicion",DestinoDesplazada="Inventario",Motivo=dto.Motivo!.Trim(),Observaciones=dto.Observaciones,Estado=EstadoSolicitudOperacion.APROBADO,Solicitante=usuario,Aprobador=usuario,FechaDecision=DateTimeOffset.UtcNow,UsuarioCreacion=usuario});
  }
  await db.SaveChangesAsync(ct);return first!;
 }
 private async Task CancelarTrabajo(Guid id,string motivo,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  var item=await db.ActividadesProgramadas.SingleOrDefaultAsync(a=>a.Id==id&&a.Activo&&(alcance.VerTodos||alcance.CentroIds.Contains(a.CentroId)),ct)??throw new KeyNotFoundException("Programación no encontrada.");
  var rows=EsMontaje(item.TipoActividad)&&item.GrupoProgramacionId.HasValue?await db.ActividadesProgramadas.Where(a=>a.GrupoProgramacionId==item.GrupoProgramacionId&&a.Activo).ToListAsync(ct):[item];
  if(rows.Any(a=>!alcance.Autoriza(a.CentroId)))throw new UnauthorizedAccessException();
  if(rows.Any(a=>a.Estado==EstadoActividad.Cumplida))throw new ConflictoException("Una actividad cumplida no puede cancelarse.");
  var ids=rows.Select(a=>a.Id).ToArray();
  foreach(var a in rows){a.Estado=EstadoActividad.Cancelada;a.MotivoCancelacion=motivo.Trim();a.UsuarioModificacion=usuario;a.FechaModificacion=DateTimeOffset.UtcNow;}
  foreach(var s in await db.SolicitudesOperacion.Where(s=>s.ActividadProgramadaId.HasValue&&ids.Contains(s.ActividadProgramadaId.Value)&&s.Estado==EstadoSolicitudOperacion.APROBADO).ToListAsync(ct)){s.Estado=EstadoSolicitudOperacion.RECHAZADO;s.MotivoRechazo=motivo.Trim();s.UsuarioModificacion=usuario;}
  await db.SaveChangesAsync(ct);
 }
}
