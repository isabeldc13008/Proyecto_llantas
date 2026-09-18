using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class AnaliticaService(LlantasDbContext db) : IAnaliticaService
{
    public const int MaximoCohorte = 5000;

    // Cohort dates refer to tire admission, never to inferred retirement or partial lifespan.
    public IQueryable<Llanta> Llantas(FiltroAnalitica f, AlcanceCentros a)
    {
        f.Validar(a);
        var q = db.Llantas.AsNoTracking().Where(x => x.Activo && (a.VerTodos || a.CentroIds.Contains(x.CentroId)));
        if (f.CentroId.HasValue) q = q.Where(x => x.CentroId == f.CentroId);
        if (f.MarcaId.HasValue) q = q.Where(x => x.MarcaId == f.MarcaId);
        if (f.ReferenciaId.HasValue) q = q.Where(x => x.ReferenciaId == f.ReferenciaId);
        if (f.DimensionId.HasValue) q = q.Where(x => x.DimensionId == f.DimensionId);
        if (f.EstadoId.HasValue) q = q.Where(x => x.EstadoLlantaId == f.EstadoId);
        if (f.IngresoDesde.HasValue) q = q.Where(x => x.FechaIngreso >= f.IngresoDesde);
        if (f.IngresoHasta.HasValue) q = q.Where(x => x.FechaIngreso <= f.IngresoHasta);
        if (!string.IsNullOrWhiteSpace(f.TipoVehiculo))
            q = q.Where(x => db.AsignacionesLlantaPosicion.Any(s => s.Activo && s.LlantaId == x.Id
                && s.PosicionVehiculo.EjeVehiculo.Vehiculo.Tipo == f.TipoVehiculo
                && (a.VerTodos || a.CentroIds.Contains(s.PosicionVehiculo.EjeVehiculo.Vehiculo.CentroId))
                && db.Movimientos.Any(m => m.Id == s.MovimientoOrigenId && m.Activo && (a.VerTodos || a.CentroIds.Contains(m.CentroId)))));
        return q;
    }

    public IQueryable<AsignacionLlantaPosicion> Asignaciones(FiltroAnalitica f, AlcanceCentros a)
    {
        var ids = Llantas(f, a).Select(x => x.Id);
        var q = db.AsignacionesLlantaPosicion.AsNoTracking().Where(x => x.Activo && ids.Contains(x.LlantaId)
            && (a.VerTodos || a.CentroIds.Contains(x.PosicionVehiculo.EjeVehiculo.Vehiculo.CentroId))
            && db.Movimientos.Any(m => m.Id == x.MovimientoOrigenId && m.Activo && (a.VerTodos || a.CentroIds.Contains(m.CentroId))));
        if (!string.IsNullOrWhiteSpace(f.TipoVehiculo)) q = q.Where(x => x.PosicionVehiculo.EjeVehiculo.Vehiculo.Tipo == f.TipoVehiculo);
        return q;
    }

    public IQueryable<MovimientoDetalle> Detalles(FiltroAnalitica f, AlcanceCentros a)
    {
        var ids = Llantas(f, a).Select(x => x.Id);
        // Both ends of a transfer must be visible before it contributes any analytic data.
        return db.MovimientosDetalle.AsNoTracking().Where(x => x.Activo && x.Movimiento.Activo && ids.Contains(x.LlantaId)
            && (a.VerTodos || a.CentroIds.Contains(x.Movimiento.CentroId))
            && (!x.CentroDestinoId.HasValue || a.VerTodos || a.CentroIds.Contains(x.CentroDestinoId.Value)));
    }

    public async Task<OpcionesAnalitica> OpcionesAsync(AlcanceCentros a, CancellationToken ct)
    {
        var tires = Llantas(new(), a);
        var centers = await db.Centros.AsNoTracking().Where(x => x.Activo && (a.VerTodos || a.CentroIds.Contains(x.Id)))
            .OrderBy(x => x.Nombre).Select(x => new OpcionAnalitica(x.Id.ToString(), x.Codigo + " · " + x.Nombre)).ToListAsync(ct);
        var brands = await tires.Select(x => new { Id = x.MarcaId, x.Marca.Nombre }).Distinct().OrderBy(x => x.Nombre).Select(x => new OpcionAnalitica(x.Id.ToString(), x.Nombre)).ToListAsync(ct);
        var references = await tires.Select(x => new { Id = x.ReferenciaId, Nombre = x.Marca.Nombre + " · " + x.Referencia.Nombre }).Distinct().OrderBy(x => x.Nombre).Select(x => new OpcionAnalitica(x.Id.ToString(), x.Nombre)).ToListAsync(ct);
        var dimensions = await tires.Select(x => new { Id = x.DimensionId, x.Dimension.Nombre }).Distinct().OrderBy(x => x.Nombre).Select(x => new OpcionAnalitica(x.Id.ToString(), x.Nombre)).ToListAsync(ct);
        var states = await tires.Select(x => new { Id = x.EstadoLlantaId, x.EstadoLlanta.Nombre }).Distinct().OrderBy(x => x.Nombre).Select(x => new OpcionAnalitica(x.Id.ToString(), x.Nombre)).ToListAsync(ct);
        var types = await Asignaciones(new(), a).Select(x => x.PosicionVehiculo.EjeVehiculo.Vehiculo.Tipo).Distinct().Order().ToListAsync(ct);
        return new(centers, brands, references, dimensions, states, types);
    }

    private sealed record TireRow(Guid Id, string Codigo, string Serial, Guid MarcaId, string Marca, Guid ReferenciaId,
        string Referencia, Guid DimensionId, string Dimension, Guid CentroId, string Centro, string Estado,
        string EstadoCodigo, bool Finalizada, bool Montada, bool Disponible, decimal? Profundidad);
    private sealed record Fact(TireRow Tire, decimal? Km, int Tramos, int Validos, int Reparaciones, int Reencauches, int Alertas, int Movimientos);

    private async Task<List<Fact>> DatosAsync(FiltroAnalitica f, AlcanceCentros a, CancellationToken ct)
    {
        var cohort = Llantas(f, a);
        var ids = cohort.Select(x => x.Id);
        var available = LlantasDisponibles.Consulta(db).Select(x => x.Id);
        var tires = await cohort.OrderBy(x => x.Id).Take(MaximoCohorte + 1).Select(x => new TireRow(
            x.Id, x.Codigo, x.Serial, x.MarcaId, x.Marca.Nombre, x.ReferenciaId, x.Referencia.Nombre,
            x.DimensionId, x.Dimension.Nombre, x.CentroId, x.Centro.Nombre, x.EstadoLlanta.Nombre, x.EstadoLlanta.Codigo,
            x.EstadoLlanta.EsDisposicionFinal,
            db.AsignacionesLlantaPosicion.Any(s => s.Activo && s.EsActiva && s.LlantaId == x.Id && (a.VerTodos || a.CentroIds.Contains(s.PosicionVehiculo.EjeVehiculo.Vehiculo.CentroId))),
            available.Contains(x.Id),
            db.InspeccionesDetalle.Where(d => d.Activo && d.LlantaId == x.Id && d.Inspeccion.Activo && d.Inspeccion.Estado == EstadoInspeccion.Finalizada
                && (a.VerTodos || a.CentroIds.Contains(d.Inspeccion.CentroId))
                && d.ProfundidadExterior >= 0 && d.ProfundidadCentro >= 0 && d.ProfundidadInterior >= 0)
                .OrderByDescending(d => d.Inspeccion.FechaCreacion).ThenByDescending(d => d.Id)
                .Select(d => d.ProfundidadExterior < d.ProfundidadCentro
                    ? (d.ProfundidadExterior < d.ProfundidadInterior ? d.ProfundidadExterior : d.ProfundidadInterior)
                    : (d.ProfundidadCentro < d.ProfundidadInterior ? d.ProfundidadCentro : d.ProfundidadInterior)).FirstOrDefault()
        )).ToListAsync(ct);
        if (tires.Count > MaximoCohorte) throw new ValidacionException($"La cohorte supera {MaximoCohorte:N0} llantas. Acote centro, marca o fechas de ingreso para obtener estadísticas exactas; no se ha tomado una muestra parcial.");
        if (tires.Count == 0) return [];

        var closed = Asignaciones(f, a).Where(x => !x.EsActiva);
        var distances = await closed.Select(x => new {
            x.LlantaId,
            Valido = x.FechaFin.HasValue && x.FechaFin >= x.FechaInicio && x.KilometrajeMontaje >= 0
                && x.KilometrajeDesmontaje >= x.KilometrajeMontaje && x.KilometrajeRecorrido >= 0
                && x.KilometrajeRecorrido == x.KilometrajeDesmontaje - x.KilometrajeMontaje,
            x.KilometrajeRecorrido
        }).GroupBy(x => x.LlantaId).Select(g => new {
            Id = g.Key, Total = g.Count(), Validos = g.Count(x => x.Valido),
            Km = g.Sum(x => x.Valido ? x.KilometrajeRecorrido ?? 0 : 0)
        }).ToDictionaryAsync(x => x.Id, ct);
        var services = await db.OrdenesServicioLlanta.AsNoTracking()
            .Where(x => x.Activo && x.Estado == "CERRADA" && ids.Contains(x.LlantaId) && (a.VerTodos || a.CentroIds.Contains(x.CentroOrigenId)))
            .GroupBy(x => x.LlantaId).Select(g => new { Id = g.Key, Reparaciones = g.Count(x => x.Tipo == TipoServicioLlanta.Reparacion), Reencauches = g.Count(x => x.Tipo == TipoServicioLlanta.Reencauche) }).ToDictionaryAsync(x => x.Id, ct);
        var alerts = await db.AlertasInspeccion.AsNoTracking().Where(x => x.Activo && x.LlantaId.HasValue && ids.Contains(x.LlantaId.Value)
            && (a.VerTodos || a.CentroIds.Contains(x.CentroId)) && (x.Estado == EstadoAlerta.ABIERTA || x.Estado == EstadoAlerta.EN_PROCESO))
            .GroupBy(x => x.LlantaId!.Value).Select(g => new { Id = g.Key, Total = g.Count() }).ToDictionaryAsync(x => x.Id, ct);
        var movements = await Detalles(f, a).GroupBy(x => x.LlantaId)
            .Select(g => new { Id = g.Key, Total = g.Select(x => x.MovimientoId).Distinct().Count() }).ToDictionaryAsync(x => x.Id, ct);
        return tires.Select(t => {
            distances.TryGetValue(t.Id, out var km); services.TryGetValue(t.Id, out var service);
            return new Fact(t, km is null ? null : EstadisticasAnalitica.RecorridoCompleto(km.Total, km.Validos, km.Km),
                km?.Total ?? 0, km?.Validos ?? 0, service?.Reparaciones ?? 0, service?.Reencauches ?? 0,
                alerts.GetValueOrDefault(t.Id)?.Total ?? 0, movements.GetValueOrDefault(t.Id)?.Total ?? 0);
        }).ToList();
    }

    public async Task<ResumenAnalitica> ResumenAsync(FiltroAnalitica f, AlcanceCentros a, CancellationToken ct)
    {
        var rows = await DatosAsync(f, a, ct);
        return new(rows.Count, rows.Count(x => x.Tire.Montada), rows.Count(x => x.Tire.Disponible),
            rows.Count(x => x.Tire.EstadoCodigo is "EN_REPARACION" or "REP"),
            rows.Count(x => x.Tire.EstadoCodigo is "EN_REENCAUCHE" or "REE"),
            rows.Count(x => x.Tire.Finalizada), rows.Count(x => x.Alertas > 0), rows.Count(x => x.Km is null),
            rows.Count(x => x.Tramos != x.Validos), rows.Select(x => x.Tire.Profundidad).Average(),
            rows.Count(x => x.Tire.Profundidad.HasValue), rows.Count == 0 ? null : Math.Round(rows.Average(x => (decimal)x.Movimientos), 2),
            EstadisticasAnalitica.Calcular(rows.Where(x => !x.Tire.Finalizada).Select(x => x.Km)),
            EstadisticasAnalitica.Calcular(rows.Where(x => x.Tire.Finalizada).Select(x => x.Km)),
            rows.GroupBy(x => x.Tire.Estado).OrderByDescending(g => g.Count()).Select(g => new ConteoAnalitica(g.Key, g.Count())).ToList());
    }

    public async Task<Pagina<GrupoAnalitica>> CompararAsync(FiltroAnalitica f, string agrupar, AlcanceCentros a, CancellationToken ct)
    {
        if (agrupar is not ("marca" or "referencia" or "marca-referencia" or "dimension" or "centro")) throw new ValidacionException("Agrupación no admitida.");
        var rows = await DatosAsync(f, a, ct);
        var groups = rows.GroupBy(x => agrupar switch {
            "marca" => (x.Tire.MarcaId, x.Tire.Marca),
            "referencia" or "marca-referencia" => (x.Tire.ReferenciaId, x.Tire.Marca + " · " + x.Tire.Referencia),
            "dimension" => (x.Tire.DimensionId, x.Tire.Dimension),
            _ => (x.Tire.CentroId, x.Tire.Centro)
        }).Select(g => {
            var operating = EstadisticasAnalitica.Calcular(g.Where(x => !x.Tire.Finalizada).Select(x => x.Km));
            var finalized = EstadisticasAnalitica.Calcular(g.Where(x => x.Tire.Finalizada).Select(x => x.Km));
            return new GrupoAnalitica(g.Key.Item1.ToString(), g.Key.Item2, g.Count(), g.Count(x => !x.Tire.Finalizada), g.Count(x => x.Tire.Finalizada), operating, finalized,
                Math.Round(g.Average(x => (decimal)x.Reparaciones), 2), Math.Round(g.Average(x => (decimal)x.Reencauches), 2),
                g.Sum(x => x.Alertas), g.Sum(x => x.Movimientos), operating.Muestra >= f.MinimoMuestra || finalized.Muestra >= f.MinimoMuestra);
        }).OrderByDescending(x => x.MuestraSuficiente).ThenByDescending(x => x.EnOperacion.Muestra + x.Finalizadas.Muestra).ThenBy(x => x.Nombre).ToList();
        return new(groups.Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano).ToList(), f.Pagina, f.Tamano, groups.Count);
    }

    public async Task<Pagina<PosicionAnalitica>> PosicionesAsync(FiltroAnalitica f, AlcanceCentros a, CancellationToken ct)
    {
        var groups = Asignaciones(f, a).Where(x => !x.EsActiva).Select(x => new {
            x.LlantaId,
            ConfigurationId = x.PosicionVehiculo.EjeVehiculo.Vehiculo.ConfiguracionVehiculoId ?? x.PosicionVehiculo.EjeVehiculo.VehiculoId,
            Configuracion = x.PosicionVehiculo.EjeVehiculo.Vehiculo.ConfiguracionVehiculoId.HasValue
                ? x.PosicionVehiculo.EjeVehiculo.Vehiculo.ConfiguracionVehiculo!.Codigo + " · " + x.PosicionVehiculo.EjeVehiculo.Vehiculo.ConfiguracionVehiculo.Nombre
                : "Sin configuración · " + x.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno,
            TipoVehiculo = x.PosicionVehiculo.EjeVehiculo.Vehiculo.Tipo,
            Eje = x.PosicionVehiculo.EjeVehiculo.Numero, x.PosicionVehiculo.EjeVehiculo.TipoEje,
            Posicion = x.PosicionVehiculo.Codigo, x.PosicionVehiculo.Lado, x.PosicionVehiculo.Ubicacion,
            Km = x.FechaFin.HasValue && x.FechaFin >= x.FechaInicio && x.KilometrajeMontaje >= 0 && x.KilometrajeDesmontaje >= x.KilometrajeMontaje
                && x.KilometrajeRecorrido == x.KilometrajeDesmontaje - x.KilometrajeMontaje ? x.KilometrajeRecorrido : null,
            Dias = x.FechaFin.HasValue && x.FechaFin >= x.FechaInicio ? (decimal?)EF.Functions.DateDiffDay(x.FechaInicio, x.FechaFin) : null
        }).GroupBy(x => new { x.ConfigurationId, x.Configuracion, x.TipoVehiculo, x.Eje, x.TipoEje, x.Posicion, x.Lado, x.Ubicacion })
            .Select(g => new {
                g.Key, Llantas = g.Select(x => x.LlantaId).Distinct().Count(), Tramos = g.Count(),
                Validos = g.Count(x => x.Km.HasValue), Muestra = g.Where(x => x.Km.HasValue).Select(x => x.LlantaId).Distinct().Count(),
                Km = g.Average(x => x.Km), Dias = g.Average(x => x.Dias)
            });
        var total = await groups.CountAsync(ct);
        var rows = await groups.OrderBy(x => x.Key.Configuracion).ThenBy(x => x.Key.Eje).ThenBy(x => x.Key.Posicion)
            .ThenBy(x => x.Key.ConfigurationId).ThenBy(x => x.Key.TipoVehiculo).ThenBy(x => x.Key.TipoEje).ThenBy(x => x.Key.Lado).ThenBy(x => x.Key.Ubicacion)
            .Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano).ToListAsync(ct);
        return new(rows.Select(x => new PosicionAnalitica(x.Key.Configuracion, x.Key.TipoVehiculo, x.Key.Eje, x.Key.TipoEje,
            x.Key.Posicion + " · " + x.Key.Lado + " " + x.Key.Ubicacion, x.Llantas, x.Muestra, x.Tramos, x.Validos, x.Km, x.Dias, x.Muestra >= f.MinimoMuestra)).ToList(), f.Pagina, f.Tamano, total);
    }

    public async Task<RankingMovimientos> MovimientosAsync(FiltroAnalitica f, AlcanceCentros a, CancellationToken ct)
    {
        var details = Detalles(f, a);
        var groups = details.GroupBy(x => new { x.LlantaId, x.Llanta.Codigo, x.Llanta.Serial, Marca = x.Llanta.Marca.Nombre, Referencia = x.Llanta.Referencia.Nombre, Estado = x.Llanta.EstadoLlanta.Nombre })
            .Select(g => new { g.Key, Total = g.Select(x => x.MovimientoId).Distinct().Count() });
        var total = await groups.CountAsync(ct);
        var rows = await groups.OrderByDescending(x => x.Total).ThenBy(x => x.Key.Codigo).ThenBy(x => x.Key.LlantaId)
            .Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano).ToListAsync(ct);
        var pageIds = rows.Select(x => x.Key.LlantaId).ToArray();
        var pageDetails = details.Where(x => pageIds.Contains(x.LlantaId));
        var typeCounts = await pageDetails.GroupBy(x => new { x.LlantaId, x.Movimiento.Tipo })
            .Select(g => new { g.Key.LlantaId, g.Key.Tipo, Total = g.Select(x => x.MovimientoId).Distinct().Count() }).ToListAsync(ct);
        var allTypes = await details.GroupBy(x => x.Movimiento.Tipo)
            .Select(g => new { Nombre = g.Key, Cantidad = g.Select(x => x.MovimientoId).Distinct().Count() }).OrderByDescending(x => x.Cantidad).Select(x => new ConteoAnalitica(x.Nombre, x.Cantidad)).ToListAsync(ct);
        // Union prevents counting source/destination twice; these queries only aggregate the displayed page.
        var centers = pageDetails.Select(x => new { x.LlantaId, Id = x.Movimiento.CentroId })
            .Union(pageDetails.Where(x => x.CentroDestinoId.HasValue).Select(x => new { x.LlantaId, Id = x.CentroDestinoId!.Value }));
        var centerCounts = await centers.GroupBy(x => x.LlantaId).Select(g => new { Id = g.Key, Total = g.Count() }).ToDictionaryAsync(x => x.Id, ct);
        var positionIds = pageDetails.Where(x => x.PosicionOrigenId.HasValue).Select(x => new { x.LlantaId, Id = x.PosicionOrigenId!.Value })
            .Union(pageDetails.Where(x => x.PosicionDestinoId.HasValue).Select(x => new { x.LlantaId, Id = x.PosicionDestinoId!.Value }));
        var positions = from p in positionIds join location in db.PosicionesVehiculo.AsNoTracking() on p.Id equals location.Id
            where a.VerTodos || a.CentroIds.Contains(location.EjeVehiculo.Vehiculo.CentroId)
            select new { p.LlantaId, p.Id, location.EjeVehiculo.VehiculoId };
        var locationCounts = await positions.GroupBy(x => x.LlantaId).Select(g => new { Id = g.Key, Posiciones = g.Select(x => x.Id).Distinct().Count(), Vehiculos = g.Select(x => x.VehiculoId).Distinct().Count() }).ToDictionaryAsync(x => x.Id, ct);
        var result = rows.Select(x => new MovimientoAnalitica(x.Key.LlantaId, x.Key.Codigo, x.Key.Serial, x.Key.Marca, x.Key.Referencia, x.Key.Estado, x.Total,
            locationCounts.GetValueOrDefault(x.Key.LlantaId)?.Vehiculos ?? 0, centerCounts.GetValueOrDefault(x.Key.LlantaId)?.Total ?? 0,
            locationCounts.GetValueOrDefault(x.Key.LlantaId)?.Posiciones ?? 0,
            typeCounts.Where(t => t.LlantaId == x.Key.LlantaId).OrderByDescending(t => t.Total).Select(t => new ConteoAnalitica(t.Tipo, t.Total)).ToList())).ToList();
        return new(new(result, f.Pagina, f.Tamano, total), allTypes);
    }
}
