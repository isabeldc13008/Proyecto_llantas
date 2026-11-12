using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class OperacionService
{
 public async Task ValidarDestinosMontajeAsync(IReadOnlyList<SolicitudOperacion> rows,AlcanceCentros scope,CancellationToken ct)
 {
  var targets=rows.Select(s=>s.PosicionDestinoId).ToHashSet();var retirementTargets=new HashSet<Guid>();
  foreach(var s in rows){var d=RevisionPosicionMontaje.Destino(s);RevisionPosicionMontaje.ValidarDestino(d,s.LlantaDesplazadaId.HasValue);
   if(!s.LlantaDesplazadaId.HasValue)continue;
   if(await db.OrdenesServicioLlanta.AnyAsync(o=>o.Activo&&o.LlantaId==s.LlantaDesplazadaId&&!new[]{"CERRADA","RECHAZADA","RETORNADA_INVENTARIO"}.Contains(o.Estado),ct))throw new ConflictoException("La retirada ya tiene un proceso de servicio activo.");
   if(d.Tipo=="Traslado"&&(!scope.Autoriza(d.CentroId!.Value)||d.CentroId==s.CentroId||!await db.Centros.AnyAsync(c=>c.Id==d.CentroId&&c.Activo,ct)))throw new ValidacionException("Centro destino inactivo, igual al origen o no autorizado.");
   if(d.Tipo=="Posicion"){
    if(targets.Contains(d.PosicionId)||!retirementTargets.Add(d.PosicionId!.Value))throw new ValidacionException("Destino de retirada repetido o reservado para una entrante del mismo trabajo.");
    var vehicle=await db.PosicionesVehiculo.Where(p=>p.Id==s.PosicionDestinoId).Select(p=>p.EjeVehiculo.VehiculoId).SingleAsync(ct);
    var p=await db.PosicionesVehiculo.SingleOrDefaultAsync(p=>p.Id==d.PosicionId&&p.Activo&&p.EjeVehiculo.Activo&&p.EjeVehiculo.VehiculoId==vehicle,ct)??throw new ValidacionException("La retirada requiere otra posición activa del mismo vehículo.");
    if(p.LlantaActualId.HasValue||await db.AsignacionesLlantaPosicion.AnyAsync(a=>a.EsActiva&&a.PosicionVehiculoId==p.Id,ct))throw new ConflictoException("La posición de retirada debe estar libre.");
    if(await db.SolicitudesOperacion.AnyAsync(x=>x.Activo&&x.Id!=s.Id&&(x.Estado==EstadoSolicitudOperacion.APROBADO||x.Estado==EstadoSolicitudOperacion.PENDIENTE_APROBACION)&&(x.PosicionDestinoId==p.Id||x.PosicionDestinoDesplazadaId==p.Id),ct)||await db.ActividadesProgramadas.AnyAsync(x=>x.Activo&&x.PosicionVehiculoId==p.Id&&x.Estado!=EstadoActividad.Cumplida&&x.Estado!=EstadoActividad.Cancelada,ct))throw new ConflictoException("La posición de retirada está comprometida por otro movimiento.");
   }
  }
 }
 private async Task DestinarRetirada(SolicitudOperacion s,Movimiento movement,Llanta old,decimal km,string user,EstadoLlanta mounted,CancellationToken ct)
 {
  var d=RevisionPosicionMontaje.Destino(s);
  if(d.Tipo=="Posicion"){
   var p=await db.PosicionesVehiculo.Include(p=>p.EjeVehiculo).ThenInclude(e=>e.Vehiculo).SingleAsync(p=>p.Id==d.PosicionId,ct);
   p.LlantaActualId=old.Id;old.EstadoLlanta=mounted;old.EstadoLlantaId=mounted.Id;old.UbicacionActual=$"{p.EjeVehiculo.Vehiculo.Placa} / {p.Codigo}";
   var detail=movement.Detalles.Single(x=>x.LlantaId==old.Id);detail.PosicionDestinoId=p.Id;detail.TipoDestino=TipoDestinoLlanta.Posicion;detail.DestinoDescripcion="Posicion";
   db.AsignacionesLlantaPosicion.Add(new(){LlantaId=old.Id,PosicionVehiculoId=p.Id,MovimientoOrigenId=movement.Id,KilometrajeMontaje=km,UsuarioCreacion=user});
  }else if(d.Tipo is "Reparacion" or "Reencauche"){
   var eligible=true;string? criterion=null;
   if(d.Tipo=="Reencauche"){
    var today=DateOnly.FromDateTime(DateTime.UtcNow);var param=await db.ParametrosReencauche.Where(x=>x.Activo&&(x.DimensionId==null||x.DimensionId==old.DimensionId)&&x.VigenteDesde<=today&&(x.VigenteHasta==null||x.VigenteHasta>=today)).OrderByDescending(x=>x.DimensionId.HasValue).FirstOrDefaultAsync(ct);
    eligible=param is not null&&old.NumeroReencauches<param.MaximoReencauches;criterion=param is null?"Sin parámetro vigente para la dimensión.":$"Máximo parametrizado: {param.MaximoReencauches}; realizados: {old.NumeroReencauches}.";
   }
   db.OrdenesServicioLlanta.Add(new(){Tipo=d.Tipo=="Reparacion"?TipoServicioLlanta.Reparacion:TipoServicioLlanta.Reencauche,Estado=d.Tipo=="Reparacion"?"OPCIONADA":eligible?"PENDIENTE_APROBACION":"NO_ELEGIBLE",LlantaId=old.Id,CentroOrigenId=s.CentroId,Motivo=s.Motivo,Observaciones=s.Observaciones,Elegible=eligible,CriterioElegibilidad=criterion,Solicitante=s.Solicitante,UsuarioOpciona=user,OrigenTipo="MONTAJE",OrigenEntidadId=movement.Id,PosicionOrigenId=s.PosicionDestinoId,FechaOpcionada=DateTimeOffset.UtcNow,UsuarioCreacion=user});
   old.UbicacionActual=$"Inventario / pendiente {d.Tipo}";
  }else if(d.Tipo=="Traslado"){
   // The specialized transfer approval and reception remain mandatory.
   db.SolicitudesOperacion.Add(new(){Tipo="Traslado",TipoDestino="Traslado",Estado=EstadoSolicitudOperacion.PENDIENTE_APROBACION,CentroId=s.CentroId,CentroDestinoId=d.CentroId,LlantaId=old.Id,Motivo=s.Motivo,Observaciones=s.Observaciones,HistorialRevisionJson=System.Text.Json.JsonSerializer.Serialize(new{OrigenMontaje=s.Id,MovimientoId=movement.Id}),Solicitante=s.Solicitante,UsuarioCreacion=user});
   old.UbicacionActual="Inventario / traslado pendiente de autorización";
  }
 }
}
