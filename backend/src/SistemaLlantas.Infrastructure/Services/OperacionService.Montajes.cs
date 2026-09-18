using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class OperacionService
{
 public async Task ValidarAsignacionesAsync(Guid vehiculoId,IReadOnlyList<AsignacionMontajeDto> filas,AlcanceCentros alcance,Guid? grupoExcluir,CancellationToken ct)
 {
  AsignacionesMontaje.Validar(filas);
  var vehicle=await db.Vehiculos.SingleOrDefaultAsync(v=>v.Id==vehiculoId&&v.Activo&&v.Centro.Activo&&(alcance.VerTodos||alcance.CentroIds.Contains(v.CentroId)),ct)??throw new ValidacionException("Vehículo inactivo o fuera del alcance.");
  var ids=filas.Select(f=>f.PosicionId).ToArray();
  var positions=await db.PosicionesVehiculo.Where(p=>ids.Contains(p.Id)&&p.Activo&&p.EjeVehiculo.Activo&&p.EjeVehiculo.VehiculoId==vehiculoId).ToListAsync(ct);
  var active=await db.AsignacionesLlantaPosicion.Where(a=>a.EsActiva&&ids.Contains(a.PosicionVehiculoId)).ToListAsync(ct);
  var outgoing=filas.Where(f=>f.LlantaActualId.HasValue).Select(f=>f.LlantaActualId!.Value).Distinct().ToArray();
  if(await db.Llantas.CountAsync(t=>outgoing.Contains(t.Id)&&t.Activo&&t.CentroId==vehicle.CentroId,ct)!=outgoing.Length)throw new ConflictoException("Una llanta instalada no está activa en el centro del vehículo.");
  var tires=filas.Select(f=>f.LlantaId).ToArray();
  var available=await LlantasDisponibles.Consulta(db,grupoExcluir).Where(t=>tires.Contains(t.Id)&&t.CentroId==vehicle.CentroId).Select(t=>t.Id).ToListAsync(ct);
  foreach(var f in filas)
  {
   var p=positions.SingleOrDefault(p=>p.Id==f.PosicionId)??throw new ValidacionException("La posición no pertenece al vehículo activo.");
   if(p.LlantaActualId!=f.LlantaActualId||active.SingleOrDefault(a=>a.PosicionVehiculoId==p.Id)?.LlantaId!=f.LlantaActualId)throw new ConflictoException($"La llanta actual de {p.Codigo} cambió o su asignación es inconsistente. Actualiza el vehículo.");
   if(!available.Contains(f.LlantaId))throw new ConflictoException($"La llanta asignada a {p.Codigo} no está disponible en el centro o está comprometida.");
   if(await db.ActividadesProgramadas.AnyAsync(a=>a.Activo&&a.PosicionVehiculoId==p.Id&&(!grupoExcluir.HasValue||a.GrupoProgramacionId!=grupoExcluir)&&(a.TipoActividad=="Montaje"||a.TipoActividad=="Cambio de juego")&&a.Estado!=EstadoActividad.Cumplida&&a.Estado!=EstadoActividad.Cancelada,ct))throw new ConflictoException($"La posición {p.Codigo} ya tiene un montaje programado.");
   if(await db.SolicitudesOperacion.AnyAsync(s=>s.Activo&&s.PosicionDestinoId==p.Id&&(!grupoExcluir.HasValue||s.GrupoOperacionId!=grupoExcluir)&&(s.Estado==EstadoSolicitudOperacion.PENDIENTE_APROBACION||s.Estado==EstadoSolicitudOperacion.APROBADO),ct))throw new ConflictoException($"La posición {p.Codigo} tiene una solicitud pendiente.");
  }
 }
 public async Task EjecutarReemplazosAsync(IReadOnlyList<SolicitudOperacion> solicitudes,decimal kilometraje,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  if(db.Database.CurrentTransaction is null)throw new InvalidOperationException("El cambio requiere una transacción.");
  if(solicitudes.Count==0||kilometraje<0)throw new ValidacionException("Revisa las asignaciones y el kilometraje.");
  var first=solicitudes[0];
  var vehicleId=await db.PosicionesVehiculo.Where(p=>p.Id==first.PosicionDestinoId).Select(p=>p.EjeVehiculo.VehiculoId).SingleAsync(ct);
  await ValidarAsignacionesAsync(vehicleId,solicitudes.Select(s=>new AsignacionMontajeDto(s.PosicionDestinoId??Guid.Empty,s.LlantaId,s.LlantaDesplazadaId)).ToList(),alcance,first.GrupoOperacionId,ct);
  var vehicle=await db.Vehiculos.SingleAsync(v=>v.Id==vehicleId,ct);ActualizarOdometro(vehicle,kilometraje);
  foreach(var s in solicitudes)
  {
   var p=await db.PosicionesVehiculo.Include(p=>p.EjeVehiculo).ThenInclude(e=>e.Vehiculo).SingleAsync(p=>p.Id==s.PosicionDestinoId,ct);
   var tire=await db.Llantas.SingleAsync(t=>t.Id==s.LlantaId,ct);
   var movement=new Movimiento{Numero=$"MOV-{Guid.NewGuid():N}"[..28],Tipo=s.Tipo=="Cambio de juego"?"CAMBIO_JUEGO":"MONTAJE",CentroId=vehicle.CentroId,Usuario=usuario,UsuarioCreacion=usuario,Motivo=s.Motivo,Observaciones=s.Observaciones};
   if(s.LlantaDesplazadaId.HasValue)
   {
    var old=await db.Llantas.SingleAsync(t=>t.Id==s.LlantaDesplazadaId,ct);
    var assignment=await db.AsignacionesLlantaPosicion.Include(a=>a.PosicionVehiculo).ThenInclude(p=>p.EjeVehiculo).ThenInclude(e=>e.Vehiculo).SingleAsync(a=>a.EsActiva&&a.PosicionVehiculoId==p.Id,ct);
    CerrarAsignacion(assignment,kilometraje,old,usuario);await CambiarEstadoAsync(old,"DISPONIBLE",ct);old.UbicacionActual="Inventario";
    movement.Detalles.Add(new(){LlantaId=old.Id,PosicionOrigenId=p.Id,TipoDestino=TipoDestinoLlanta.Inventario,DestinoDescripcion="Inventario",UsuarioCreacion=usuario});
   }
   await CambiarEstadoAsync(tire,"MONTADA",ct);tire.UbicacionActual=$"{vehicle.Placa} / {p.Codigo}";
   movement.Detalles.Add(new(){LlantaId=tire.Id,PosicionDestinoId=p.Id,TipoDestino=TipoDestinoLlanta.Posicion,UsuarioCreacion=usuario});
   db.Movimientos.Add(movement);p.LlantaActualId=null;await db.SaveChangesAsync(ct);
   p.LlantaActualId=tire.Id;db.AsignacionesLlantaPosicion.Add(new(){LlantaId=tire.Id,PosicionVehiculoId=p.Id,MovimientoOrigenId=movement.Id,KilometrajeMontaje=kilometraje,UsuarioCreacion=usuario});
   s.KilometrajeVehiculo=kilometraje;s.MovimientoEjecutadoId=movement.Id;s.Estado=EstadoSolicitudOperacion.EJECUTADO;s.UsuarioModificacion=usuario;
   if(s.ActividadProgramadaId.HasValue){var activity=await db.ActividadesProgramadas.SingleAsync(a=>a.Id==s.ActividadProgramadaId,ct);activity.Estado=EstadoActividad.Cumplida;activity.FechaInicioReal??=DateTimeOffset.UtcNow;activity.FechaFinReal=DateTimeOffset.UtcNow;activity.UsuarioModificacion=usuario;}
   await db.SaveChangesAsync(ct);
  }
 }
}
