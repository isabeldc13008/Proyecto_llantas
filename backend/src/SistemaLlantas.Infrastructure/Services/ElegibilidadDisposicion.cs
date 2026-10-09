using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Infrastructure.Services;
public static class ElegibilidadDisposicion
{
 public static IQueryable<Llanta> Consulta(LlantasDbContext db)=>db.Llantas.Where(t=>t.Activo&&t.Centro.Activo&&t.EstadoLlanta.Activo&&!t.EstadoLlanta.EsDisposicionFinal&&t.EstadoLlanta.Codigo!="EN_TRASLADO"
 &&!db.OrdenesServicioLlanta.Any(o=>o.LlantaId==t.Id&&o.Activo&&!new[]{"CERRADA","RECHAZADA","NO_REPARABLE","DISPOSICION_FINAL","RETORNADA_INVENTARIO"}.Contains(o.Estado))
 &&!db.SolicitudesOperacion.Any(o=>o.LlantaId==t.Id&&o.Activo&&(o.Estado==EstadoSolicitudOperacion.PENDIENTE_APROBACION||o.Estado==EstadoSolicitudOperacion.APROBADO))
 &&!db.ActividadesProgramadas.Any(o=>o.LlantaId==t.Id&&o.Activo&&o.Estado!=EstadoActividad.Cumplida&&o.Estado!=EstadoActividad.Cancelada));
}
