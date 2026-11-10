using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Api.Controllers;
public sealed partial class OperacionesController
{
 private async Task<SolicitudOperacion> PosicionRevision(Guid id,CancellationToken ct){var s=await db.SolicitudesOperacion.SingleOrDefaultAsync(s=>s.Id==id&&s.Activo,ct)??throw new SolicitudNoEncontradaException();if(!User.AlcanceCentros().Autoriza(s.CentroId))throw new UnauthorizedAccessException();if(!RevisionPosicionMontaje.EsMontaje(s))throw new ValidacionException("No es una solicitud de montaje.");return s;}
 private async Task<List<SolicitudOperacion>> GrupoRevision(SolicitudOperacion s,CancellationToken ct)=>s.GrupoOperacionId.HasValue?await db.SolicitudesOperacion.Where(x=>x.Activo&&x.GrupoOperacionId==s.GrupoOperacionId).ToListAsync(ct):[s];
 private async Task ValidarRevision(SolicitudOperacion s,CancellationToken ct){
  var vehicle=await db.PosicionesVehiculo.Where(p=>p.Id==s.PosicionDestinoId).Select(p=>p.EjeVehiculo.VehiculoId).SingleAsync(ct);
  var peers=await GrupoRevision(s,ct);if(peers.Any(x=>!User.AlcanceCentros().Autoriza(x.CentroId)))throw new UnauthorizedAccessException();var live=peers.Where(x=>x.Estado!=EstadoSolicitudOperacion.EJECUTADO&&x.Estado!=EstadoSolicitudOperacion.RECHAZADO).ToList();
  AsignacionesMontaje.Validar(live.Select(x=>new AsignacionMontajeDto(x.PosicionDestinoId!.Value,x.LlantaId,x.LlantaDesplazadaId)).ToList());
  await service.ValidarAsignacionesAsync(vehicle,[new(s.PosicionDestinoId!.Value,s.LlantaId,s.LlantaDesplazadaId)],User.AlcanceCentros(),s.GrupoOperacionId,ct,s.Id,s.ActividadProgramadaId);
  await service.ValidarDestinosMontajeAsync(live,User.AlcanceCentros(),ct);
 }
 [HttpPost("api/operaciones/solicitudes/{id:guid}/decision-posicion"),Authorize(Policy="Operaciones.Aprobar")]
 public async Task<IActionResult> DecidirPosicion(Guid id,DecisionPosicionDto dto,CancellationToken ct){
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var s=await PosicionRevision(id,ct);RevisionPosicionMontaje.Decidir(s,dto,Usuario(),User.HasClaim("permiso","operaciones.aprobar_propia"));if(s.Estado==EstadoSolicitudOperacion.APROBADO)await ValidarRevision(s,ct);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);});return Ok(new{estado="REVISADO"});
 }
 [HttpPost("api/operaciones/solicitudes/{id:guid}/corregir-posicion"),Authorize(Policy="Operaciones.Solicitar"),Authorize(Policy="Operaciones.Montar")]
 public async Task<IActionResult> CorregirPosicion(Guid id,CorregirPosicionDto dto,CancellationToken ct){
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var s=await PosicionRevision(id,ct);RevisionPosicionMontaje.Corregir(s,dto,Usuario());await ValidarRevision(s,ct);await ActualizarActividad(s,ct);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);});return Ok(new{estado="PENDIENTE_APROBACION"});
 }
 private async Task ActualizarActividad(SolicitudOperacion s,CancellationToken ct){if(!s.ActividadProgramadaId.HasValue)return;var a=await db.ActividadesProgramadas.SingleAsync(x=>x.Id==s.ActividadProgramadaId,ct);a.LlantaId=s.LlantaId;a.UsuarioModificacion=Usuario();}
 private static List<SolicitudOperacion> SeleccionarEjecucion(List<SolicitudOperacion> rows,IReadOnlyList<VersionPosicion>? versions){
  if(versions is null||versions.Count==0||versions.Count>100||versions.Select(x=>x.Id).Distinct().Count()!=versions.Count)throw new ValidacionException("Selecciona posiciones aprobadas con sus versiones.");
  var chosen=new List<SolicitudOperacion>();foreach(var v in versions){var row=rows.SingleOrDefault(s=>s.Id==v.Id)??throw new ValidacionException("Posición ajena al trabajo.");RevisionPosicionMontaje.Version(row,v.RowVersion);chosen.Add(row);}RevisionPosicionMontaje.ValidarEjecucion(chosen);return chosen;
 }
 [HttpGet("api/operaciones/trabajos-montaje"),Authorize(Policy="Operaciones.Solicitar"),Authorize(Policy="Operaciones.Montar")]
 public async Task<IActionResult> TrabajosMontaje(CancellationToken ct){var a=User.AlcanceCentros();var rows=await db.SolicitudesOperacion.AsNoTracking().Where(x=>x.Activo&&(x.Tipo=="Montaje"||x.Tipo=="Cambio de juego"||x.Tipo=="Reemplazar llanta")&&x.Solicitante==Usuario()&&(a.VerTodos||a.CentroIds.Contains(x.CentroId))).OrderByDescending(x=>x.FechaCreacion).Select(x=>new{x.Id,x.GrupoOperacionId,x.ActividadProgramadaId,x.Tipo,x.LlantaId,Llanta=x.Llanta.Codigo,x.LlantaDesplazadaId,x.PosicionDestinoId,Posicion=db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionDestinoId).Select(p=>p.Codigo).FirstOrDefault(),VehiculoId=db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionDestinoId).Select(p=>p.EjeVehiculo.VehiculoId).FirstOrDefault(),Estado=x.Estado.ToString(),x.DestinoDesplazada,x.PosicionDestinoDesplazadaId,x.CentroDestinoId,x.Motivo,x.Observaciones,x.MotivoRechazo,x.KilometrajeVehiculo,RowVersion=Convert.ToBase64String(x.RowVersion)}).ToListAsync(ct);return Ok(rows.GroupBy(x=>x.GrupoOperacionId??x.Id).Select(g=>new{id=g.Key,posiciones=g.ToList()}));}
 [HttpPost("api/operaciones/trabajos-montaje/{id:guid}/ejecutar"),Authorize(Policy="Operaciones.Montar"),Authorize(Policy="Operaciones.Solicitar")]
 public async Task<IActionResult> EjecutarPosiciones(Guid id,EjecutarPosicionesDto dto,CancellationToken ct){
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var rows=await db.SolicitudesOperacion.Where(x=>x.Activo&&(x.GrupoOperacionId==id||x.Id==id)).ToListAsync(ct);if(rows.Any(x=>x.Solicitante!=Usuario()||!User.AlcanceCentros().Autoriza(x.CentroId)))throw new UnauthorizedAccessException("Solo el responsable del trabajo puede ejecutarlo.");if(rows.Any(x=>x.ActividadProgramadaId.HasValue))throw new ValidacionException("Ejecuta el trabajo desde su actividad programada.");var selected=SeleccionarEjecucion(rows,dto.Posiciones);await service.EjecutarReemplazosAsync(selected,dto.Kilometraje,Usuario(),User.AlcanceCentros(),ct);await tx.CommitAsync(ct);});return Ok(new{estado="EJECUTADO"});
 }
 [HttpPost("api/actividades/{id:guid}/montaje/cambios"),Authorize(Policy="Operaciones.Montar"),Authorize(Policy="Operaciones.Solicitar")]
 public async Task<IActionResult> CambiarProgramacion(Guid id,IReadOnlyList<AsignacionMontajeDto> cambios,CancellationToken ct){
  AsignacionesMontaje.Validar(cambios);
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var (_,rows)=await Trabajo(id,ct);
   foreach(var f in cambios){var s=rows.SingleOrDefault(s=>s.PosicionDestinoId==f.PosicionId)??throw new ValidacionException("La posición no pertenece a la programación.");RevisionPosicionMontaje.Version(s,f.RowVersion??"");var d=new DestinoRetirada(f.DestinoRetirada??"Inventario",f.PosicionRetiradaId,f.CentroRetiradaId);if(s.LlantaId==f.LlantaId&&s.LlantaDesplazadaId==f.LlantaActualId&&RevisionPosicionMontaje.Destino(s)==d)continue;
    if(s.Estado is not (EstadoSolicitudOperacion.APROBADO or EstadoSolicitudOperacion.DEVUELTO))throw new ConflictoException("La posición está ejecutada o en revisión.");var before=RevisionPosicionMontaje.Foto(s);s.DestinoSolicitadoJson??=System.Text.Json.JsonSerializer.Serialize(RevisionPosicionMontaje.Destino(s));s.LlantaId=f.LlantaId;s.LlantaDesplazadaId=f.LlantaActualId;RevisionPosicionMontaje.AplicarDestino(s,d);s.Solicitante=Usuario();s.Estado=EstadoSolicitudOperacion.PENDIENTE_APROBACION;s.Aprobador=null;s.FechaDecision=null;RevisionPosicionMontaje.Registrar(s,"CAMBIO_PROGRAMADO",Usuario(),"Cambio de asignación programada",before);await ValidarRevision(s,ct);await ActualizarActividad(s,ct);
   }await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
  });return Ok(new{estado="CAMBIOS_ENVIADOS"});
 }
}
