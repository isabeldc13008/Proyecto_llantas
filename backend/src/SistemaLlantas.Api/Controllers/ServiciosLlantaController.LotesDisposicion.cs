using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.Controllers;
public sealed partial class ServiciosLlantaController
{
 public sealed record CrearLoteDisposicionDto(IReadOnlyList<Guid> OrdenIds,Guid CentroOrigenId,Guid CentroDestinoId,DateTimeOffset FechaSalida,string? Remision,string? Transportador,string? Observaciones,string IdempotencyKey);
 public sealed record RecibirDisposicionDto(IReadOnlyList<Guid> OrdenIds);
 public sealed record CerrarLoteDisposicionDto(IReadOnlyList<Guid> OrdenIds,Guid ProveedorId,string Observaciones);
 public sealed record ItemDisposicionDto(Guid OrdenId,Guid LlantaId,string Llanta,string Estado,string? Resultado,string? Concepto,string? Tecnico,DateTimeOffset? FechaEvaluacion,string? Aprobador,DateTimeOffset? FechaAprobacion,DateTimeOffset? FechaRecepcion,DateTimeOffset? FechaDisposicion,string? Empresa,int Evidencias);
 public sealed record LoteDisposicionDto(Guid Id,string Codigo,Guid CentroOrigenId,string Origen,Guid CentroDestinoId,string Destino,string? RelevanciaDestino,string Estado,DateTimeOffset FechaSalida,DateTimeOffset? FechaRecepcion,DateTimeOffset? FechaCierre,string Usuario,string? Receptor,string? Remision,string? Transportador,string? Observaciones,IReadOnlyList<ItemDisposicionDto> Items);

 [HttpGet("disposicion/lotes")]
 public async Task<IReadOnlyList<LoteDisposicionDto>> LotesDisposicion(CancellationToken ct)
 {
  var a=User.AlcanceCentros();
  return await db.LotesDisposicionFinal.AsNoTracking().Where(l=>l.Activo&&(a.VerTodos||a.CentroIds.Contains(l.CentroOrigenId)||a.CentroIds.Contains(l.CentroDestinoId))).OrderByDescending(l=>l.FechaSalida)
   .Select(l=>new LoteDisposicionDto(l.Id,l.Codigo,l.CentroOrigenId,l.CentroOrigen.Codigo+" · "+l.CentroOrigen.Nombre,l.CentroDestinoId,l.CentroDestino.Codigo+" · "+l.CentroDestino.Nombre,l.RelevanciaDestino,l.Estado,l.FechaSalida,l.FechaRecepcion,l.FechaCierre,l.UsuarioCreacion,l.Receptor,l.Remision,l.Transportador,l.Observaciones,l.Ordenes.Where(o=>o.Activo).Select(o=>new ItemDisposicionDto(o.Id,o.LlantaId,o.Llanta.Codigo,o.Estado,o.Resultado,o.Observaciones,o.EvaluadoPor,o.FechaEvaluacion,o.Aprobador,o.FechaAprobacion,o.FechaRecepcion,o.FechaDisposicion,o.Proveedor!=null?o.Proveedor.Nombre:null,o.Evidencias.Count(e=>e.Activo))).ToList())).ToListAsync(ct);
 }
 [HttpPost("disposicion/lotes"),Authorize(Policy="ServiciosLlanta.Gestionar")]
 public async Task<ActionResult<LoteDisposicionDto>> CrearLoteDisposicion(CrearLoteDisposicionDto dto,CancellationToken ct)
 {
  ValidarSeleccionDisposicion(dto.OrdenIds);
  if(dto.CentroOrigenId==Guid.Empty||dto.CentroDestinoId==Guid.Empty||dto.FechaSalida==default||string.IsNullOrWhiteSpace(dto.IdempotencyKey)||dto.IdempotencyKey.Length>100)throw new ValidacionException("Selecciona origen, destino, fecha de salida y clave de operación válidos.");
  if((dto.Remision?.Length??0)>100||(dto.Transportador?.Length??0)>150||(dto.Observaciones?.Length??0)>1000)throw new ValidacionException("Remisión: máximo 100 caracteres; transportador: 150; observaciones: 1000.");
  var a=User.AlcanceCentros();if(!a.Autoriza(dto.CentroOrigenId))throw new UnauthorizedAccessException("Centro origen fuera de alcance.");
  var id=await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>
  {
   db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
   var existing=await db.LotesDisposicionFinal.Include(l=>l.Ordenes).SingleOrDefaultAsync(l=>l.IdempotencyKey==dto.IdempotencyKey,ct);
   if(existing is not null)
   {
    if(!existing.Activo||existing.CentroOrigenId!=dto.CentroOrigenId||existing.CentroDestinoId!=dto.CentroDestinoId||existing.FechaSalida!=dto.FechaSalida||existing.Remision!=dto.Remision||existing.Transportador!=dto.Transportador||existing.Observaciones!=dto.Observaciones||!existing.Ordenes.Select(o=>o.Id).ToHashSet().SetEquals(dto.OrdenIds))throw new ConflictoException("La clave de operación ya corresponde a otro envío.");
    await tx.CommitAsync(ct);return existing.Id;
   }
   var destino=await db.Centros.SingleOrDefaultAsync(c=>c.Id==dto.CentroDestinoId&&c.Activo,ct)??throw new ValidacionException("El centro destino no existe o está inactivo.");
   if(!await db.Centros.AnyAsync(c=>c.Id==dto.CentroOrigenId&&c.Activo,ct))throw new ValidacionException("El centro origen no está activo.");
   var orders=await db.OrdenesServicioLlanta.IgnoreQueryFilters().Include(o=>o.Llanta).ThenInclude(t=>t.EstadoLlanta).Include(o=>o.LoteDisposicionFinal).Where(o=>dto.OrdenIds.Contains(o.Id)).ToListAsync(ct);
   if(orders.Count!=dto.OrdenIds.Count)throw new ValidacionException("No existe la orden: "+string.Join(", ",dto.OrdenIds.Except(orders.Select(o=>o.Id))));
   if(orders.Select(o=>o.LlantaId).Distinct().Count()!=orders.Count)throw new ConflictoException("Una llanta no puede repetirse en el lote.");
   var tireIds=orders.Select(o=>o.LlantaId).ToArray();
   var mounted=await db.AsignacionesLlantaPosicion.Where(m=>m.EsActiva&&tireIds.Contains(m.LlantaId)).Select(m=>m.LlantaId).ToListAsync(ct);
   var occupied=await db.PosicionesVehiculo.Where(p=>p.LlantaActualId.HasValue&&tireIds.Contains(p.LlantaActualId.Value)).Select(p=>p.LlantaActualId!.Value).ToListAsync(ct);
   foreach(var o in orders)
   {
    if(!a.Autoriza(o.CentroOrigenId)||!a.Autoriza(o.Llanta.CentroId))throw new UnauthorizedAccessException("Llanta fuera de los centros autorizados.");
    if(o.LoteDisposicionFinalId.HasValue)throw new ConflictoException($"{o.Llanta.Codigo} ya pertenece al lote {o.LoteDisposicionFinal?.Codigo}.");
    if(!o.Activo||!o.Llanta.Activo||o.Tipo!=TipoServicioLlanta.DisposicionFinal||o.Resultado!="DISPOSICION"||o.Estado!="APROBADA")throw new ConflictoException($"{o.Llanta.Codigo}: requiere orden activa, evaluación DISPOSICION y aprobación.");
    if(mounted.Contains(o.LlantaId)||occupied.Contains(o.LlantaId))throw new ConflictoException($"{o.Llanta.Codigo} continúa montada. Debe desmontarse primero.");
    if(o.Llanta.EstadoLlanta.Codigo=="EN_TRASLADO"||o.Llanta.EstadoLlanta.EsDisposicionFinal)throw new ConflictoException($"{o.Llanta.Codigo}: está en traslado o disposición final.");
    if(o.CentroOrigenId!=dto.CentroOrigenId||o.Llanta.CentroId!=dto.CentroOrigenId)throw new ConflictoException($"{o.Llanta.Codigo}: las llantas seleccionadas pertenecen a diferentes centros. Cree un lote por centro de origen.");
   }
   var prefix=$"DSP-{DateTimeOffset.UtcNow.Year}-";var last=await db.LotesDisposicionFinal.Where(l=>l.Codigo.StartsWith(prefix)).OrderByDescending(l=>l.Codigo.Length).ThenByDescending(l=>l.Codigo).Select(l=>l.Codigo).FirstOrDefaultAsync(ct);
   var sequence=last is null?1:int.Parse(last[prefix.Length..])+1;
   var lot=new LoteDisposicionFinal{Codigo=$"{prefix}{sequence:0000}",CentroOrigenId=dto.CentroOrigenId,CentroDestinoId=destino.Id,RelevanciaDestino=destino.Relevancia,FechaSalida=dto.FechaSalida,Remision=dto.Remision,Transportador=dto.Transportador,Observaciones=dto.Observaciones,IdempotencyKey=dto.IdempotencyKey,UsuarioCreacion=User.Username()};db.LotesDisposicionFinal.Add(lot);
   var traslado=new AlcanceCentros(false,new[]{dto.CentroOrigenId,dto.CentroDestinoId});
   foreach(var o in orders)
   {
    if(dto.CentroOrigenId!=dto.CentroDestinoId)await ciclo.TrasladarCentroAsync(o.LlantaId,new(destino.Id,$"Envío disposición {lot.Codigo}",dto.Observaciones),User.Username(),traslado,ct);
    else await operaciones.MoverAsync(new(){LlantaId=o.LlantaId,TipoDestino="Traslado",Motivo=$"Envío disposición {lot.Codigo}",Observaciones=dto.Observaciones},User.Username(),a,ct);
    o.LoteDisposicionFinal=lot;o.Estado="EN_TRANSITO_DISPOSICION";o.FechaEnvio=dto.FechaSalida;o.UsuarioModificacion=User.Username();o.FechaModificacion=DateTimeOffset.UtcNow;
   }
   await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return lot.Id;
  });
  return Created(string.Empty,(await LotesDisposicion(ct)).Single(l=>l.Id==id));
 }
 [HttpPost("disposicion/lotes/{id:guid}/recibir"),Authorize(Policy="ServiciosLlanta.Gestionar")]
 public Task<LoteDisposicionDto> RecibirLoteDisposicion(Guid id,RecibirDisposicionDto dto,CancellationToken ct)
 {
  ValidarSeleccionDisposicion(dto.OrdenIds);
  return CambiarLoteDisposicion(id,async lot=>
  {
   var orders=SeleccionarOrdenes(lot,dto.OrdenIds);
   foreach(var o in orders){if(o.Estado!="EN_TRANSITO_DISPOSICION"||o.FechaRecepcion.HasValue)throw new ConflictoException($"{o.Llanta.Codigo}: ya fue recibida o no está en tránsito.");await ValidarDesmontada(o,ct);if(o.Llanta.CentroId!=lot.CentroDestinoId)throw new ConflictoException($"{o.Llanta.Codigo}: ubicación incompatible con el destino del lote.");}
   foreach(var o in orders){await operaciones.MoverAsync(new(){LlantaId=o.LlantaId,TipoDestino="Inventario",Motivo=$"Recepción disposición {lot.Codigo}",Observaciones=lot.Observaciones},User.Username(),User.AlcanceCentros(),ct);o.Estado="PENDIENTE_DISPOSICION";o.FechaRecepcion=DateTimeOffset.UtcNow;o.UsuarioModificacion=User.Username();o.FechaModificacion=DateTimeOffset.UtcNow;}
   lot.Receptor=User.Username();ActualizarEstadoLote(lot);
  },ct);
 }
 [HttpPost("disposicion/lotes/{id:guid}/cerrar"),Authorize(Policy="ServiciosLlanta.Gestionar")]
 public Task<LoteDisposicionDto> CerrarLoteDisposicion(Guid id,CerrarLoteDisposicionDto dto,CancellationToken ct)
 {
  ValidarSeleccionDisposicion(dto.OrdenIds);
  if(string.IsNullOrWhiteSpace(dto.Observaciones)||dto.Observaciones.Length>1000)throw new ValidacionException("El concepto de cierre es obligatorio (máximo 1000 caracteres).");
  return CambiarLoteDisposicion(id,async lot=>
  {
   if(!await db.ProveedoresServicio.AnyAsync(p=>p.Id==dto.ProveedorId&&p.Activo&&(p.Tipo=="DisposicionFinal"||p.Tipo==""),ct))throw new ValidacionException("Selecciona una empresa receptora activa de disposición final.");
   var orders=SeleccionarOrdenes(lot,dto.OrdenIds);
   foreach(var o in orders){if(o.Estado!="PENDIENTE_DISPOSICION"||o.Resultado!="DISPOSICION"||!o.FechaRecepcion.HasValue)throw new ConflictoException($"{o.Llanta.Codigo}: no está recibida y pendiente de disposición.");if(o.Llanta.CentroId!=lot.CentroDestinoId)throw new ConflictoException($"{o.Llanta.Codigo}: no está en el destino del lote.");await ValidarDesmontada(o,ct);if(!o.Evidencias.Any(e=>e.Activo&&e.MimeType.StartsWith("image/")))throw new ValidacionException($"{o.Llanta.Codigo}: falta evidencia fotográfica.");}
   foreach(var o in orders){await operaciones.MoverAsync(new(){LlantaId=o.LlantaId,TipoDestino="DisposicionFinal",Motivo=$"Disposición final {lot.Codigo}",Observaciones=dto.Observaciones},User.Username(),User.AlcanceCentros(),ct);o.Estado="DISPOSICION_FINAL";o.ProveedorId=dto.ProveedorId;o.FechaDisposicion=DateTimeOffset.UtcNow;o.UsuarioModificacion=User.Username();o.FechaModificacion=DateTimeOffset.UtcNow;}
   ActualizarEstadoLote(lot);
  },ct);
 }
 public static void ValidarSeleccionDisposicion(IReadOnlyList<Guid>? ids){if(ids is null||ids.Count==0||ids.Count>500||ids.Contains(Guid.Empty)||ids.Distinct().Count()!=ids.Count)throw new ValidacionException("Selecciona entre 1 y 500 órdenes válidas, sin repetir.");}
 private static List<OrdenServicioLlanta> SeleccionarOrdenes(LoteDisposicionFinal lot,IReadOnlyList<Guid> ids){var orders=lot.Ordenes.Where(o=>o.Activo&&o.Llanta.Activo&&ids.Contains(o.Id)).ToList();if(orders.Count!=ids.Count)throw new ConflictoException("Una orden no está activa o no pertenece al lote.");return orders;}
 private static void ActualizarEstadoLote(LoteDisposicionFinal lot)
 {
  if(lot.Ordenes.All(o=>o.FechaRecepcion.HasValue))lot.FechaRecepcion??=DateTimeOffset.UtcNow;
  if(lot.Ordenes.All(o=>o.Estado=="DISPOSICION_FINAL")){lot.Estado="CERRADO";lot.FechaCierre=DateTimeOffset.UtcNow;}
  else if(lot.Ordenes.Any(o=>o.Estado=="DISPOSICION_FINAL"))lot.Estado="EN_DISPOSICION";
  else lot.Estado=lot.Ordenes.All(o=>o.FechaRecepcion.HasValue)?"RECIBIDO":"RECEPCION_PARCIAL";
 }
 private Task<LoteDisposicionDto> CambiarLoteDisposicion(Guid id,Func<LoteDisposicionFinal,Task> cambio,CancellationToken ct)=>db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>
 {
  db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  var lot=await db.LotesDisposicionFinal.IgnoreQueryFilters().Include(l=>l.Ordenes).ThenInclude(o=>o.Llanta).Include(l=>l.Ordenes).ThenInclude(o=>o.Evidencias).SingleOrDefaultAsync(l=>l.Id==id&&l.Activo,ct)??throw new KeyNotFoundException("Lote no encontrado.");
  if(!User.AlcanceCentros().Autoriza(lot.CentroDestinoId))throw new UnauthorizedAccessException("Solo el centro destino puede recibir o cerrar este lote.");
  if(lot.Estado=="CERRADO")throw new ConflictoException("El lote ya está cerrado.");
  await cambio(lot);lot.UsuarioModificacion=User.Username();lot.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);
  var result=(await LotesDisposicion(ct)).Single(l=>l.Id==id);await tx.CommitAsync(ct);return result;
 });
}
