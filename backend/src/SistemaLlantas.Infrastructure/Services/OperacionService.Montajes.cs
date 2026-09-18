using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class OperacionService
{
 public Task ValidarAsignacionesAsync(Guid vehiculoId,IReadOnlyList<AsignacionMontajeDto> filas,AlcanceCentros alcance,Guid? grupoExcluir,CancellationToken ct)=>ValidarAsignacionesCoreAsync(vehiculoId,filas,alcance,grupoExcluir,ct);
 private async Task ValidarAsignacionesCoreAsync(Guid vehiculoId,IReadOnlyList<AsignacionMontajeDto> filas,AlcanceCentros alcance,Guid? grupoExcluir,CancellationToken ct,Guid? solicitudExcluir=null,Guid? actividadExcluir=null)
 {
  AsignacionesMontaje.Validar(filas);
  var vehicle=await db.Vehiculos.SingleOrDefaultAsync(v=>v.Id==vehiculoId&&v.Activo&&v.Centro.Activo&&(alcance.VerTodos||alcance.CentroIds.Contains(v.CentroId)),ct)??throw new ValidacionException("Vehículo inactivo o fuera del alcance.");
  var ids=filas.Select(f=>f.PosicionId).ToArray();
  var positions=await db.PosicionesVehiculo.Where(p=>ids.Contains(p.Id)&&p.Activo&&p.EjeVehiculo.Activo&&p.EjeVehiculo.VehiculoId==vehiculoId).ToListAsync(ct);
  var active=await db.AsignacionesLlantaPosicion.Where(a=>a.EsActiva&&ids.Contains(a.PosicionVehiculoId)).ToListAsync(ct);
  var outgoing=filas.Where(f=>f.LlantaActualId.HasValue).Select(f=>f.LlantaActualId!.Value).Distinct().ToArray();
  if(await db.Llantas.CountAsync(t=>outgoing.Contains(t.Id)&&t.Activo&&t.CentroId==vehicle.CentroId,ct)!=outgoing.Length)throw new ConflictoException("Una llanta instalada no está activa en el centro del vehículo.");
  var tires=filas.Select(f=>f.LlantaId).ToArray();
  var available=await LlantasDisponibles.Consulta(db,grupoExcluir,solicitudExcluir,actividadExcluir).Where(t=>tires.Contains(t.Id)&&t.CentroId==vehicle.CentroId).Select(t=>t.Id).ToListAsync(ct);
  foreach(var f in filas)
  {
   var p=positions.SingleOrDefault(p=>p.Id==f.PosicionId)??throw new ValidacionException("La posición no pertenece al vehículo activo.");
   AsignacionConsistente(p,f.LlantaActualId,active.Where(a=>a.PosicionVehiculoId==p.Id));
   if(!available.Contains(f.LlantaId))throw new ConflictoException($"La llanta asignada a {p.Codigo} no está disponible en el centro o está comprometida.");
   if(await db.ActividadesProgramadas.AnyAsync(a=>a.Activo&&a.Id!=actividadExcluir&&a.PosicionVehiculoId==p.Id&&(!grupoExcluir.HasValue||a.GrupoProgramacionId!=grupoExcluir)&&(a.TipoActividad=="Montaje"||a.TipoActividad=="Cambio de juego")&&a.Estado!=EstadoActividad.Cumplida&&a.Estado!=EstadoActividad.Cancelada,ct))throw new ConflictoException($"La posición {p.Codigo} ya tiene un montaje programado.");
   if(await db.SolicitudesOperacion.AnyAsync(s=>s.Activo&&s.Id!=solicitudExcluir&&s.PosicionDestinoId==p.Id&&(!grupoExcluir.HasValue||s.GrupoOperacionId!=grupoExcluir)&&(s.Estado==EstadoSolicitudOperacion.PENDIENTE_APROBACION||s.Estado==EstadoSolicitudOperacion.APROBADO),ct))throw new ConflictoException($"La posición {p.Codigo} tiene una solicitud pendiente.");
  }
 }
 public async Task EjecutarReemplazosAsync(IReadOnlyList<SolicitudOperacion> solicitudes,decimal kilometraje,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  if(db.Database.CurrentTransaction is null)throw new InvalidOperationException("El cambio requiere una transacción.");
  if(solicitudes.Count==0)throw new ValidacionException("Asigna al menos una llanta.");
  var group=solicitudes[0].GrupoOperacionId;
  if((!group.HasValue&&solicitudes.Count!=1)||solicitudes.Any(s=>s.GrupoOperacionId!=group))throw new ConflictoException("Las solicitudes no pertenecen al mismo grupo de montaje.");
  var vehicleId=await db.PosicionesVehiculo.Where(p=>p.Id==solicitudes[0].PosicionDestinoId).Select(p=>(Guid?)p.EjeVehiculo.VehiculoId).SingleOrDefaultAsync(ct)??throw new ConflictoException("La posición programada ya no existe.");
  await ValidarAsignacionesCoreAsync(vehicleId,solicitudes.Select(s=>new AsignacionMontajeDto(s.PosicionDestinoId??Guid.Empty,s.LlantaId,s.LlantaDesplazadaId)).ToList(),alcance,group,ct,solicitudes.Count==1?solicitudes[0].Id:null,solicitudes.Count==1?solicitudes[0].ActividadProgramadaId:null);
  var vehicle=await db.Vehiculos.SingleOrDefaultAsync(v=>v.Id==vehicleId,ct)??throw new ConflictoException("El vehículo ya no existe.");
  if(solicitudes.Any(s=>!s.Activo||s.CentroId!=vehicle.CentroId||s.Estado is not (EstadoSolicitudOperacion.APROBADO or EstadoSolicitudOperacion.PENDIENTE_APROBACION)))throw new ConflictoException("Una solicitud ya fue procesada o no pertenece al centro del vehículo.");
  var positionIds=solicitudes.Select(s=>s.PosicionDestinoId!.Value).ToArray();
  var positions=await db.PosicionesVehiculo.Include(p=>p.EjeVehiculo).ThenInclude(e=>e.Vehiculo).Where(p=>positionIds.Contains(p.Id)).ToDictionaryAsync(p=>p.Id,ct);
  var active=await db.AsignacionesLlantaPosicion.Include(a=>a.Llanta).Include(a=>a.PosicionVehiculo).ThenInclude(p=>p.EjeVehiculo).ThenInclude(e=>e.Vehiculo).Where(a=>a.EsActiva&&positionIds.Contains(a.PosicionVehiculoId)).ToListAsync(ct);
  KilometrajeOperacion.Validar(kilometraje,vehicle.Kilometraje,active.Select(a=>new SalidaKilometraje(a.Llanta.Codigo,a.PosicionVehiculo.Codigo,a.KilometrajeMontaje)));
  var tireIds=solicitudes.Select(s=>s.LlantaId).ToArray();var tires=await db.Llantas.Where(t=>tireIds.Contains(t.Id)).ToDictionaryAsync(t=>t.Id,ct);
  if(tires.Count!=tireIds.Length)throw new ConflictoException("Una llanta asignada ya no está disponible.");
  var activityIds=solicitudes.Where(s=>s.ActividadProgramadaId.HasValue).Select(s=>s.ActividadProgramadaId!.Value).ToArray();
  var activities=await db.ActividadesProgramadas.Where(a=>activityIds.Contains(a.Id)).ToDictionaryAsync(a=>a.Id,ct);
  foreach(var s in solicitudes.Where(s=>s.ActividadProgramadaId.HasValue))
  {
   if(!activities.TryGetValue(s.ActividadProgramadaId!.Value,out var a)||!a.Activo||a.GrupoProgramacionId!=group||a.VehiculoId!=vehicle.Id||a.CentroId!=vehicle.CentroId||a.LlantaId!=s.LlantaId||a.PosicionVehiculoId!=s.PosicionDestinoId||a.Estado is EstadoActividad.Cancelada or EstadoActividad.Cumplida)throw new ConflictoException("La asignación o el grupo de la programación cambió. Actualiza el trabajo.");
  }
  // Resolve both destination states before closing any assignment.
  var mounted=await ObtenerEstadoAsync("MONTADA",ct);
  var available=active.Count>0?await ObtenerEstadoAsync("DISPONIBLE",ct):null;
  foreach(var assignment in active)
  {
   var old=assignment.Llanta;CerrarAsignacion(assignment,kilometraje,old,usuario);old.EstadoLlanta=available!;old.EstadoLlantaId=available!.Id;old.UbicacionActual="Inventario";assignment.PosicionVehiculo.LlantaActualId=null;
  }
  // Release filtered unique indexes on active tire/position before inserting replacements.
  // One intermediate flush for the whole set, protected by the caller's transaction.
  if(active.Count>0)await db.SaveChangesAsync(ct);
  foreach(var s in solicitudes)
  {
   var p=positions[s.PosicionDestinoId!.Value];var tire=tires[s.LlantaId];
   var movement=new Movimiento{Numero=$"MOV-{Guid.NewGuid():N}"[..28],Tipo=s.Tipo=="Cambio de juego"?"CAMBIO_JUEGO":"MONTAJE",CentroId=vehicle.CentroId,Usuario=usuario,UsuarioCreacion=usuario,Motivo=s.Motivo,Observaciones=s.Observaciones};
   if(s.LlantaDesplazadaId.HasValue)movement.Detalles.Add(new(){LlantaId=s.LlantaDesplazadaId.Value,PosicionOrigenId=p.Id,TipoDestino=TipoDestinoLlanta.Inventario,DestinoDescripcion="Inventario",UsuarioCreacion=usuario});
   tire.EstadoLlanta=mounted;tire.EstadoLlantaId=mounted.Id;tire.UbicacionActual=$"{vehicle.Placa} / {p.Codigo}";
   movement.Detalles.Add(new(){LlantaId=tire.Id,PosicionDestinoId=p.Id,TipoDestino=TipoDestinoLlanta.Posicion,UsuarioCreacion=usuario});db.Movimientos.Add(movement);
   p.LlantaActualId=tire.Id;db.AsignacionesLlantaPosicion.Add(new(){LlantaId=tire.Id,PosicionVehiculoId=p.Id,MovimientoOrigenId=movement.Id,KilometrajeMontaje=kilometraje,UsuarioCreacion=usuario});
   s.KilometrajeVehiculo=kilometraje;s.MovimientoEjecutadoId=movement.Id;s.Estado=EstadoSolicitudOperacion.EJECUTADO;s.UsuarioModificacion=usuario;
   if(s.ActividadProgramadaId.HasValue){var activity=activities[s.ActividadProgramadaId.Value];activity.Estado=EstadoActividad.Cumplida;activity.FechaInicioReal??=DateTimeOffset.UtcNow;activity.FechaFinReal=DateTimeOffset.UtcNow;activity.UsuarioModificacion=usuario;}
  }
  ActualizarOdometro(vehicle,kilometraje);await db.SaveChangesAsync(ct);
 }
}
