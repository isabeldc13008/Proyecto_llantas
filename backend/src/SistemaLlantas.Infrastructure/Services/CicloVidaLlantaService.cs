using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class CicloVidaLlantaService(LlantasDbContext db,ILlantaService llantas):ICicloVidaLlantaService
{
 public async Task<LlantaDetalleDto?> ObtenerDetalleAsync(Guid id,AlcanceCentros alcance,CancellationToken ct)
 {
  var tire=await llantas.ObtenerAsync(id,alcance,ct);if(tire is null)return null;
  var assignments=await db.AsignacionesLlantaPosicion.AsNoTracking().Where(x=>x.LlantaId==id).Include(x=>x.PosicionVehiculo).ThenInclude(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).ThenInclude(x=>x.Centro).OrderBy(x=>x.FechaInicio).ToListAsync(ct);
  var movements=await db.MovimientosDetalle.AsNoTracking().Where(x=>x.LlantaId==id).Include(x=>x.Movimiento).ThenInclude(x=>x.Centro).ToListAsync(ct);
  var destinationIds=movements.Where(x=>x.CentroDestinoId.HasValue).Select(x=>x.CentroDestinoId!.Value).Distinct().ToArray();var destinationNames=await db.Centros.IgnoreQueryFilters().Where(x=>destinationIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.Nombre,ct);
  var inspections=await db.InspeccionesDetalle.AsNoTracking().Where(x=>x.LlantaId==id).Include(x=>x.Inspeccion).ThenInclude(x=>x.Centro).Include(x=>x.PosicionVehiculo).ThenInclude(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).ToListAsync(ct);
  var services=await db.OrdenesServicioLlanta.AsNoTracking().Where(x=>x.LlantaId==id&&x.Activo).Include(x=>x.Proveedor).Include(x=>x.CentroOrigen).OrderByDescending(x=>x.FechaCreacion).ToListAsync(ct);
  var events=new List<EventoVidaLlantaDto>();
  foreach(var a in assignments){var vehicle=a.PosicionVehiculo.EjeVehiculo.Vehiculo;events.Add(new(a.FechaInicio,"Montaje",$"Montaje en {vehicle.NumeroInterno}",a.UsuarioCreacion,tire.Centro,vehicle.NumeroInterno,a.PosicionVehiculo.Codigo,a.KilometrajeMontaje,null));if(a.FechaFin.HasValue)events.Add(new(a.FechaFin.Value,"Desmontaje",$"Desmontaje de {vehicle.NumeroInterno}",a.UsuarioModificacion??a.UsuarioCreacion,tire.Centro,vehicle.NumeroInterno,a.PosicionVehiculo.Codigo,a.KilometrajeDesmontaje,a.KilometrajeRecorrido));}
  foreach(var d in movements){var route=d.CentroDestinoId.HasValue&&destinationNames.TryGetValue(d.CentroDestinoId.Value,out var destination)?$" · {d.Movimiento.Centro.Nombre} → {destination}":string.Empty;events.Add(new(d.Movimiento.FechaCreacion,d.Movimiento.Tipo,$"{d.Movimiento.Motivo}{route}{(string.IsNullOrWhiteSpace(d.Movimiento.Observaciones)?string.Empty:$" · {d.Movimiento.Observaciones}")}",d.Movimiento.Usuario,d.Movimiento.Centro.Nombre,null,null,null,null));}
  foreach(var d in inspections)events.Add(new(d.Inspeccion.FechaCreacion,"Inspección",d.Observaciones??"Inspección de llanta",d.Inspeccion.TecnicoId,d.Inspeccion.Centro.Nombre,d.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno,d.PosicionVehiculo.Codigo,d.Inspeccion.Kilometraje,null));
  foreach(var s in services)events.Add(new(s.FechaCreacion,s.Tipo.ToString(),s.Motivo,s.Solicitante,s.CentroOrigen.Nombre,null,null,null,null));
  var active=assignments.SingleOrDefault(x=>x.EsActiva);var cacheIds=await db.PosicionesVehiculo.AsNoTracking().Where(x=>x.LlantaActualId==id).Select(x=>x.Id).ToListAsync(ct);var discrepancy=active is null?cacheIds.Count>0:cacheIds.Count!=1||cacheIds[0]!=active.PosicionVehiculoId;
  var entity=await db.Llantas.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
  var mounts=assignments.OrderByDescending(x=>x.FechaInicio).Select(a=>{var vehicle=a.PosicionVehiculo.EjeVehiculo.Vehiculo;var end=a.EsActiva?(decimal?)vehicle.Kilometraje:a.KilometrajeDesmontaje;var inconsistent=a.KilometrajeMontaje.HasValue&&end.HasValue&&end.Value<a.KilometrajeMontaje.Value;var distance=inconsistent?null:a.EsActiva&&a.KilometrajeMontaje.HasValue&&end.HasValue?end-a.KilometrajeMontaje:a.KilometrajeRecorrido;return new MontajeVidaDto(a.FechaInicio,a.FechaFin,vehicle.NumeroInterno,vehicle.Placa,vehicle.Centro.Nombre,a.PosicionVehiculo.Codigo,a.KilometrajeMontaje,end,distance,a.EsActiva,inconsistent);}).ToList();
  var inspectionDtos=inspections.OrderByDescending(x=>x.Inspeccion.FechaCreacion).Select(d=>new InspeccionVidaDto(d.InspeccionId,d.Inspeccion.FechaCreacion,d.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno,d.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa,d.PosicionVehiculo.Codigo,d.Inspeccion.Centro.Nombre,d.ProfundidadExterior,d.ProfundidadCentro,d.ProfundidadInterior,new[]{d.ProfundidadExterior,d.ProfundidadCentro,d.ProfundidadInterior}.Where(v=>v.HasValue).Min(),d.Inspeccion.Estado.ToString())).ToList();
  var serviceDtos=services.Select(s=>new ServicioVidaDto(s.Id,s.Tipo.ToString(),s.Estado,s.FechaCreacion,s.Proveedor?.Nombre,s.Motivo,s.FechaEnvio,s.FechaRecepcion)).ToList();
  var movementDtos=movements.OrderByDescending(x=>x.Movimiento.FechaCreacion).Select(d=>new MovimientoVidaDto(d.MovimientoId,d.Movimiento.FechaCreacion,d.Movimiento.Tipo,d.Movimiento.Motivo,d.Movimiento.Centro.Nombre,d.Movimiento.Usuario,d.PosicionOrigenId?.ToString(),d.PosicionDestinoId?.ToString()??d.DestinoDescripcion)).ToList();
  var repairs=services.Where(x=>x.Tipo==TipoServicioLlanta.Reparacion).ToList();var retreads=services.Where(x=>x.Tipo==TipoServicioLlanta.Reencauche).ToList();
  var summary=new ResumenCicloDto(entity.FechaIngreso,mounts.Count,repairs.Count,retreads.Count,repairs.MaxBy(x=>x.FechaCreacion)?.FechaCreacion,retreads.MaxBy(x=>x.FechaCreacion)?.FechaCreacion);
  return new(tire,summary,mounts,inspectionDtos,serviceDtos,movementDtos,events.OrderByDescending(x=>x.Fecha).ToList(),discrepancy);
 }

 public Task TrasladarCentroAsync(Guid id,TrasladarLlantaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  if(db.Database.CurrentTransaction is not null)return TrasladarCentroCoreAsync(id,dto,usuario,alcance,ct);
  var strategy=db.Database.CreateExecutionStrategy();
  return strategy.ExecuteAsync(()=>TrasladarCentroCoreAsync(id,dto,usuario,alcance,ct));
 }
 private async Task TrasladarCentroCoreAsync(Guid id,TrasladarLlantaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  if(!alcance.Autoriza(dto.CentroDestinoId))throw new UnauthorizedAccessException("El centro destino no está autorizado.");if(string.IsNullOrWhiteSpace(dto.Motivo))throw new ValidacionException("El motivo es obligatorio.");
  await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;var tire=await db.Llantas.SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Llanta no encontrada.");if(tire.CentroId==dto.CentroDestinoId)throw new ValidacionException("El centro destino debe ser diferente.");if(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==id&&x.EsActiva,ct))throw new ConflictoException("Debe desmontar la llanta antes de trasladarla entre centros.");if(!await db.Centros.AnyAsync(x=>x.Id==dto.CentroDestinoId,ct))throw new ValidacionException("El centro destino no existe o está inactivo.");
  var movement=new Movimiento{Numero=$"TRS-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..28],Tipo="Traslado centro",Motivo=dto.Motivo.Trim(),Observaciones=dto.Observaciones?.Trim(),CentroId=tire.CentroId,Usuario=usuario,UsuarioCreacion=usuario};movement.Detalles.Add(new(){LlantaId=tire.Id,TipoDestino=TipoDestinoLlanta.Traslado,CentroDestinoId=dto.CentroDestinoId,DestinoDescripcion="Traslado entre centros",UsuarioCreacion=usuario});db.Movimientos.Add(movement);tire.CentroId=dto.CentroDestinoId;tire.UbicacionActual="En traslado";tire.EstadoLlantaId=await db.EstadosLlanta.Where(x=>x.Codigo=="EN_TRASLADO").Select(x=>x.Id).SingleAsync(ct);tire.FechaModificacion=DateTimeOffset.UtcNow;tire.UsuarioModificacion=usuario;await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);
 }
 public async Task ConciliarMontajeAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct)
 {
  if(!await db.Llantas.AnyAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct))throw new KeyNotFoundException("Llanta no encontrada.");var activePosition=await db.AsignacionesLlantaPosicion.Where(x=>x.LlantaId==id&&x.EsActiva).Select(x=>(Guid?)x.PosicionVehiculoId).SingleOrDefaultAsync(ct);var positions=await db.PosicionesVehiculo.Where(x=>x.LlantaActualId==id||x.Id==activePosition).ToListAsync(ct);foreach(var position in positions){position.LlantaActualId=position.Id==activePosition?id:null;position.FechaModificacion=DateTimeOffset.UtcNow;position.UsuarioModificacion=usuario;}await db.SaveChangesAsync(ct);
 }
}
