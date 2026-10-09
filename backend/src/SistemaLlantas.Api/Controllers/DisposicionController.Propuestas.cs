using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.Controllers;
public sealed partial class DisposicionController
{
 [HttpPost("propuestas"),Authorize(Policy="ServiciosLlanta.Gestionar")]
 public async Task<IActionResult> Propuestas(CrearPropuestasDisposicion dto,CancellationToken ct)
 {
  if(dto.Items is null||dto.Items.Count is <1 or >100||dto.Items.Any(i=>i.OrdenId==Guid.Empty||i.LlantaId==Guid.Empty)||dto.Items.Select(i=>i.OrdenId).Distinct().Count()!=dto.Items.Count||dto.Items.Select(i=>i.LlantaId).Distinct().Count()!=dto.Items.Count)throw new ValidacionException("Selecciona entre 1 y 100 llantas diferentes.");
  if(string.IsNullOrWhiteSpace(dto.Motivo)||dto.Motivo.Length>1000||dto.Observacion?.Length>1000)throw new ValidacionException("Ingresa motivo y observación de máximo 1000 caracteres.");
  if(dto.Origen is not "PROPUESTA" and not "MONTAJE")throw new ValidacionException("La disposición solo admite origen PROPUESTA o MONTAJE.");
  var a=User.AlcanceCentros();var user=User.Username();var ids=dto.Items.Select(i=>i.LlantaId).ToArray();var orderIds=dto.Items.Select(i=>i.OrdenId).ToArray();
  await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
   await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
   var existing=await db.OrdenesServicioLlanta.Where(o=>orderIds.Contains(o.Id)).ToListAsync(ct);
   if(existing.Count>0){if(existing.Count!=dto.Items.Count||existing.Any(o=>o.UsuarioCreacion!=user||o.Tipo!=TipoServicioLlanta.DisposicionFinal||o.OrigenTipo!=dto.Origen||o.Motivo!=dto.Motivo.Trim()||o.Observaciones!=dto.Observacion?.Trim()||!dto.Items.Any(i=>i.OrdenId==o.Id&&i.LlantaId==o.LlantaId)||!a.Autoriza(o.CentroOrigenId)))throw new ConflictoException("La propuesta ya fue procesada o cambió. Actualiza antes de continuar.");await tx.CommitAsync(ct);return;}
   if(dto.Origen=="MONTAJE"&&(dto.Items.Count!=1||!dto.PosicionOrigenId.HasValue||!await db.PosicionesVehiculo.AnyAsync(p=>p.Id==dto.PosicionOrigenId&&p.LlantaActualId==ids[0]&&(a.VerTodos||a.CentroIds.Contains(p.EjeVehiculo.Vehiculo.CentroId)),ct)))throw new ValidacionException("La propuesta desde montaje requiere una posición vigente de la llanta.");
   var tires=await ElegibilidadDisposicion.Consulta(db).Where(t=>ids.Contains(t.Id)&&(a.VerTodos||a.CentroIds.Contains(t.CentroId))).ToListAsync(ct);
   if(tires.Count!=ids.Length)throw new ConflictoException("Una o más llantas dejaron de ser elegibles o están fuera de tu alcance. Actualiza la selección; no se creó ninguna propuesta.");
   foreach(var i in dto.Items){var t=tires.Single(t=>t.Id==i.LlantaId);var o=new OrdenServicioLlanta{Tipo=TipoServicioLlanta.DisposicionFinal,Estado="PENDIENTE_EVALUACION_TECNICA",LlantaId=t.Id,CentroOrigenId=t.CentroId,Motivo=dto.Motivo.Trim(),Observaciones=dto.Observacion?.Trim(),Elegible=true,OrigenTipo=dto.Origen,PosicionOrigenId=dto.PosicionOrigenId,OrigenEntidadId=dto.PosicionOrigenId,Solicitante=user,UsuarioOpciona=user,FechaOpcionada=DateTimeOffset.UtcNow,UsuarioCreacion=user};db.OrdenesServicioLlanta.Add(o);db.Entry(o).Property(x=>x.Id).CurrentValue=i.OrdenId;}
   await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
  });
  return Ok(dto.Items);
 }
}

