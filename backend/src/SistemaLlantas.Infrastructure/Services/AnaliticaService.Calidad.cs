using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Infrastructure.Services;

public sealed partial class AnaliticaService
{
    // These descriptive aggregates share the authorized evidence rules used by maintenance.
    // Group/count in SQL, not an in-memory sample or the legacy 5,000-tire cohort.
    public IQueryable<TotalesCalidad> TotalesCalidadQuery(FiltroMantenimiento f, AlcanceCentros a) => ConsultaMantenimiento(f, a)
        .GroupBy(x => 1).Select(g => new TotalesCalidad(g.Count(), g.Count(x => x.Montada),
            g.Count(x => x.Disponible), g.Count(x => x.Alertas > 0), g.Count(x => x.LecturaId != null), g.Count(x => x.UltimaCompleta)));

    public async Task<CalidadDatosAnalitica> CalidadDatosAsync(FiltroMantenimiento f, string categoria, AlcanceCentros a, CancellationToken ct)
    {
        if (categoria is not ("todas" or "sin-inspeccion" or "incompleta" or "completa"))
            throw new ValidacionException("Categoría de calidad no admitida.");
        var query = ConsultaMantenimiento(f, a);
        var totals = await TotalesCalidadQuery(f, a).SingleOrDefaultAsync(ct) ?? new(0, 0, 0, 0, 0, 0);
        if (totals.Total == 0)
            return new(DateTimeOffset.UtcNow, ProcedenciaDescriptiva, totals, [], [], new([], f.Pagina, f.Tamano, 0));
        var states = await query.GroupBy(x => new { x.EstadoId, x.Estado }).OrderBy(x => x.Key.Estado).ThenBy(x => x.Key.EstadoId)
            .Select(g => new GrupoCalidad(g.Key.EstadoId, g.Key.Estado, g.Count(), g.Count(x => x.LecturaId != null))).ToListAsync(ct);
        var centers = await query.GroupBy(x => new { x.CentroId, x.Centro }).OrderBy(x => x.Key.Centro).ThenBy(x => x.Key.CentroId)
            .Select(g => new GrupoCalidad(g.Key.CentroId, g.Key.Centro, g.Count(), g.Count(x => x.LecturaId != null))).ToListAsync(ct);
        var detail = categoria switch {
            "sin-inspeccion" => query.Where(x => x.LecturaId == null),
            "incompleta" => query.Where(x => x.LecturaId != null && !x.UltimaCompleta),
            "completa" => query.Where(x => x.UltimaCompleta), _ => query
        };
        var count = categoria switch { "sin-inspeccion" => totals.SinInspeccion, "incompleta" => totals.UltimaIncompleta,
            "completa" => totals.UltimaCompleta, _ => totals.Total };
        var rows = await detail.OrderBy(x => x.Codigo).ThenBy(x => x.Id).Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano)
            .Select(x => new LlantaCalidad(x.Id, x.Codigo, x.Centro, x.Estado, x.InspeccionId, x.FechaLectura, x.UltimaCompleta, x.Alertas)).ToListAsync(ct);
        return new(DateTimeOffset.UtcNow, ProcedenciaDescriptiva, totals, states, centers, new(rows, f.Pagina, f.Tamano, count));
    }

    private const string ProcedenciaDescriptiva = "Datos del entorno actual · Procedencia no verificada";
}
