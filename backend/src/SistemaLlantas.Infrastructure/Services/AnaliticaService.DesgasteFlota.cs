using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Infrastructure.Services;

public sealed partial class AnaliticaService
{
    public sealed class HechoDesgaste
    {
        public Guid Id { get; init; }
        public string Codigo { get; init; } = "";
        public string GrupoId { get; init; } = "";
        public string Grupo { get; init; } = "";
        public decimal? Profundidad { get; init; }
        public decimal? Km { get; init; }
    }

    public IQueryable<HechoDesgaste> ConsultaDesgaste(FiltroMantenimiento f, string agrupar, AlcanceCentros a)
    {
        if (agrupar is not ("marca" or "referencia" or "dimension" or "centro" or "vehiculo" or "posicion"))
            throw new ValidacionException("Agrupación de desgaste no admitida.");
        var cohort = ConsultaMantenimiento(f, a);
        var assignments = Asignaciones(new(), a);
        var mounted = assignments.Where(x => x.EsActiva);
        var unique = mounted.Where(x => mounted.Count(s => s.LlantaId == x.LlantaId) == 1);
        var closed = assignments.Where(x => !x.EsActiva);
        var distances = closed.Select(x => new {
            x.LlantaId, x.KilometrajeRecorrido,
            Valido = x.FechaInicio.HasValue && x.FechaFin.HasValue && x.FechaFin >= x.FechaInicio
                && x.KilometrajeMontaje >= 0 && x.KilometrajeDesmontaje >= x.KilometrajeMontaje
                && x.KilometrajeRecorrido >= 0 && x.KilometrajeRecorrido == x.KilometrajeDesmontaje - x.KilometrajeMontaje
                && !assignments.Any(s => s.LlantaId == x.LlantaId && s.Id != x.Id
                    && s.FechaInicio < x.FechaFin && (!s.FechaFin.HasValue || s.FechaFin > x.FechaInicio))
        }).GroupBy(x => x.LlantaId).Select(g => new {
            Id = g.Key, Total = g.Count(), Validos = g.Count(x => x.Valido), Km = g.Sum(x => x.KilometrajeRecorrido)
        });
        // SQL-side joins and aggregation; no inventory materialization, no fallback to older complete readings.
        return from c in cohort
               join t in Llantas(f.FiltroBase(), a) on c.Id equals t.Id
               join reading in LecturasMantenimiento(a) on c.LecturaId equals (Guid?)reading.Id into readings
               from r in readings.DefaultIfEmpty()
               join distance in distances on c.Id equals distance.Id into kms
               from k in kms.DefaultIfEmpty()
               join location in unique on c.Id equals location.LlantaId into locations
               from s in locations.DefaultIfEmpty()
               select new HechoDesgaste {
                   Id = t.Id, Codigo = t.Codigo,
                   GrupoId = agrupar == "marca" ? t.MarcaId.ToString() : agrupar == "referencia" ? t.ReferenciaId.ToString()
                       : agrupar == "dimension" ? t.DimensionId.ToString() : agrupar == "centro" ? t.CentroId.ToString()
                       : s == null ? "sin-asignacion-unica" : agrupar == "vehiculo" ? s.PosicionVehiculo.EjeVehiculo.VehiculoId.ToString() : s.PosicionVehiculoId.ToString(),
                   Grupo = agrupar == "marca" ? t.Marca.Nombre : agrupar == "referencia" ? t.Marca.Nombre + " · " + t.Referencia.Nombre
                       : agrupar == "dimension" ? t.Dimension.Nombre : agrupar == "centro" ? t.Centro.Nombre
                       : s == null ? "Sin asignación vigente única visible"
                       : agrupar == "vehiculo" ? s.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno + " · " + s.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa
                       : s.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno + " · " + s.PosicionVehiculo.Codigo,
                   Profundidad = r != null && r.ProfundidadExterior >= 0 && r.ProfundidadCentro >= 0 && r.ProfundidadInterior >= 0
                       ? (r.ProfundidadExterior < r.ProfundidadCentro
                           ? (r.ProfundidadExterior < r.ProfundidadInterior ? r.ProfundidadExterior : r.ProfundidadInterior)
                           : (r.ProfundidadCentro < r.ProfundidadInterior ? r.ProfundidadCentro : r.ProfundidadInterior)) : null,
                   Km = k != null && k.Total == k.Validos ? k.Km : null
               };
    }

    public async Task<DesgasteFlota> DesgasteFlotaAsync(FiltroMantenimiento f, string agrupar, int paginaLlantas, AlcanceCentros a, CancellationToken ct)
    {
        if (paginaLlantas is < 1 or > 100000) throw new ValidacionException("Página de llantas fuera de rango.");
        var query = ConsultaDesgaste(f, agrupar, a);
        var totals = await query.GroupBy(x => 1).Select(g => new GrupoDesgaste("total", "Alcance filtrado", g.Count(),
            g.Count(x => x.Profundidad != null), g.Average(x => x.Profundidad), g.Count(x => x.Km != null), g.Average(x => x.Km))).SingleOrDefaultAsync(ct)
            ?? new("total", "Alcance filtrado", 0, 0, null, 0, null);
        var groups = query.GroupBy(x => new { x.GrupoId, x.Grupo });
        var count = await groups.CountAsync(ct);
        var rows = await groups.OrderBy(g => g.Key.Grupo).ThenBy(g => g.Key.GrupoId).Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano)
            .Select(g => new GrupoDesgaste(g.Key.GrupoId, g.Key.Grupo, g.Count(), g.Count(x => x.Profundidad != null),
                g.Average(x => x.Profundidad), g.Count(x => x.Km != null), g.Average(x => x.Km))).ToListAsync(ct);
        var tires = await query.OrderBy(x => x.Codigo).ThenBy(x => x.Id).Skip((paginaLlantas - 1) * f.Tamano).Take(f.Tamano)
            .Select(x => new LlantaDesgaste(x.Id, x.Codigo, x.Profundidad, x.Km)).ToListAsync(ct);
        return new(DateTimeOffset.UtcNow, ProcedenciaDescriptiva, totals, new(rows, f.Pagina, f.Tamano, count), new(tires, paginaLlantas, f.Tamano, totals.Total));
    }

    public async Task<Pagina<IntervencionDesgaste>> IntervencionesAsync(Guid id, int pagina, AlcanceCentros a, CancellationToken ct)
    {
        if (pagina is < 1 or > 100000) throw new ValidacionException("Página fuera de rango.");
        if (!await Llantas(new(), a).AnyAsync(t => t.Id == id, ct)) throw new KeyNotFoundException("Llanta no encontrada en el alcance autorizado.");
        var positions = db.PosicionesVehiculo.AsNoTracking();
        var query = Detalles(new(), a).Where(d => d.LlantaId == id
            && (!d.PosicionOrigenId.HasValue || positions.Any(p => p.Id == d.PosicionOrigenId && (a.VerTodos || a.CentroIds.Contains(p.EjeVehiculo.Vehiculo.CentroId))))
            && (!d.PosicionDestinoId.HasValue || positions.Any(p => p.Id == d.PosicionDestinoId && (a.VerTodos || a.CentroIds.Contains(p.EjeVehiculo.Vehiculo.CentroId)))));
        var count = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(d => d.Movimiento.FechaGiro ?? d.Movimiento.FechaCreacion).ThenByDescending(d => d.Id)
            .Skip((pagina - 1) * 20).Take(20).Select(d => new IntervencionDesgaste(d.Id, d.MovimientoId, d.Movimiento.Numero,
                d.Movimiento.Tipo, d.Movimiento.FechaGiro ?? d.Movimiento.FechaCreacion, d.Movimiento.Motivo, d.Movimiento.Usuario, d.Movimiento.KilometrajeGiro,
                positions.Where(p => p.Id == d.PosicionOrigenId).Select(p => p.EjeVehiculo.Vehiculo.NumeroInterno + " · " + p.Codigo).FirstOrDefault(),
                positions.Where(p => p.Id == d.PosicionDestinoId).Select(p => p.EjeVehiculo.Vehiculo.NumeroInterno + " · " + p.Codigo).FirstOrDefault() ?? d.DestinoDescripcion ?? d.TipoDestino.ToString())).ToListAsync(ct);
        return new(rows, pagina, 20, count);
    }
}
