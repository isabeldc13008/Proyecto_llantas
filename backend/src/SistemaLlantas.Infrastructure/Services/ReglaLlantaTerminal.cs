using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Infrastructure.Services;
public static class ReglaLlantaTerminal
{
 public const string Mensaje="La llanta ya se encuentra en disposición final y no admite nuevas operaciones.";
 public static async Task ValidarAsync(LlantasDbContext db,IEnumerable<Guid> ids,CancellationToken ct=default){var selected=ids.Distinct().ToArray();if(selected.Length>0&&await db.Llantas.IgnoreQueryFilters().AnyAsync(t=>selected.Contains(t.Id)&&t.EstadoLlanta.EsDisposicionFinal,ct))throw new ConflictoException(Mensaje);}
 public static async Task ValidarCambiosAsync(LlantasDbContext db,CancellationToken ct)
 {
  var ids=new HashSet<Guid>();
  foreach(var e in db.ChangeTracker.Entries().Where(e=>e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
  {
   if(e.Entity is Llanta t && e.State!=EntityState.Added)ids.Add(t.Id);
   if(e.Entity is OrdenServicioLlanta o && e.State==EntityState.Modified && o.Estado=="DISPOSICION_FINAL" && (string?)e.OriginalValues[nameof(o.Estado)]=="PENDIENTE_DISPOSICION")continue;
   var property=e.Metadata.FindProperty("LlantaId");if(property is not null){if(e.CurrentValues[property] is Guid id)ids.Add(id);if(e.State!=EntityState.Added&&e.OriginalValues[property] is Guid old)ids.Add(old);}
   if(e.Entity is PosicionVehiculo p){if(p.LlantaActualId is Guid id)ids.Add(id);if(e.State!=EntityState.Added&&e.OriginalValues[nameof(p.LlantaActualId)] is Guid old)ids.Add(old);}
  }
  await ValidarAsync(db,ids,ct);
 }
}
