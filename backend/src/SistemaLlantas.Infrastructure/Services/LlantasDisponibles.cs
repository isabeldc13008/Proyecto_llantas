using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Infrastructure.Services;
public static class LlantasDisponibles
{
    public static IQueryable<Llanta> Consulta(LlantasDbContext db,Guid? grupoExcluir=null,Guid? solicitudExcluir=null,Guid? actividadExcluir=null) => db.Llantas.AsNoTracking().Where(x=>x.Activo && x.Centro.Activo && !x.EstadoLlanta.EsDisposicionFinal && x.EstadoLlanta.Activo && x.EstadoLlanta.PermiteMontaje && x.EstadoLlanta.Codigo!="EN_TRASLADO"
        && !db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.EsActiva)
        && !db.PosicionesVehiculo.Any(p=>p.LlantaActualId==x.Id)
        && !db.ActividadesProgramadas.Any(a=>a.Activo&&a.Id!=actividadExcluir&&(!grupoExcluir.HasValue||a.GrupoProgramacionId!=grupoExcluir)&&a.LlantaId==x.Id&&(a.TipoActividad=="Montaje"||a.TipoActividad=="Cambio de juego"||a.TipoActividad=="Reemplazar llanta")&&a.Estado!=EstadoActividad.Cancelada&&a.Estado!=EstadoActividad.Cumplida)
        && !db.SolicitudesOperacion.Any(s=>s.Activo&&s.Id!=solicitudExcluir&&(!grupoExcluir.HasValue||s.GrupoOperacionId!=grupoExcluir)&&s.LlantaId==x.Id&&(s.Estado==EstadoSolicitudOperacion.PENDIENTE_APROBACION||s.Estado==EstadoSolicitudOperacion.APROBADO))
        && !db.SolicitudesOperacion.Any(s=>s.Activo&&s.LlantaId==x.Id&&s.Tipo=="RESERVA"&&s.Estado==EstadoSolicitudOperacion.EJECUTADO)
        && !db.OrdenesServicioLlanta.Any(o=>o.Activo&&o.LlantaId==x.Id&&o.Estado!="CERRADA"&&o.Estado!="RECHAZADA"));
}
