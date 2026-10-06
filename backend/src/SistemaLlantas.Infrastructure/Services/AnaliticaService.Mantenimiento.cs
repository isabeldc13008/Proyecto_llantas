using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Services;

public sealed partial class AnaliticaService
{
    public sealed class HechoMantenimiento
    {
        public Guid Id { get; init; }
        public string Codigo { get; init; } = "";
        public string Serial { get; init; } = "";
        public Guid CentroId { get; init; }
        public string Centro { get; init; } = "";
        public Guid EstadoId { get; init; }
        public string Estado { get; init; } = "";
        public int Alertas { get; init; }
        public Guid? AlertaId { get; init; }
        public DateTimeOffset? FechaAlerta { get; init; }
        public Guid? LecturaId { get; init; }
        public DateTimeOffset? FechaLectura { get; init; }
        public bool UltimaCompleta { get; init; }
    }

    private IQueryable<InspeccionDetalle> LecturasMantenimiento(AlcanceCentros a) => db.InspeccionesDetalle.AsNoTracking()
        .Where(d => d.Activo && d.Inspeccion.Activo && d.Inspeccion.Estado == EstadoInspeccion.Finalizada
            && (a.VerTodos || a.CentroIds.Contains(d.Inspeccion.CentroId))
            && (a.VerTodos || a.CentroIds.Contains(d.Inspeccion.Vehiculo.CentroId)));

    private IQueryable<AlertaInspeccion> AlertasObservadas(AlcanceCentros a) => db.AlertasInspeccion.AsNoTracking()
        .Where(x => x.Activo && x.LlantaId.HasValue && x.Inspeccion.Activo && x.InspeccionDetalle.Activo
            && (a.VerTodos || a.CentroIds.Contains(x.CentroId))
            && (a.VerTodos || a.CentroIds.Contains(x.Inspeccion.CentroId))
            && (a.VerTodos || a.CentroIds.Contains(x.Inspeccion.Vehiculo.CentroId)));

    public IQueryable<HechoMantenimiento> ConsultaMantenimiento(FiltroMantenimiento f, AlcanceCentros a)
    {
        f.Validar(a);
        var tires = Llantas(f.FiltroBase(), a);
        var mounted = Asignaciones(new(), a).Where(x => x.EsActiva);
        if (f.Montaje == "MONTADAS") tires = tires.Where(x => mounted.Any(s => s.LlantaId == x.Id));
        if (f.VehiculoId.HasValue) tires = tires.Where(x => mounted.Any(s => s.LlantaId == x.Id && s.PosicionVehiculo.EjeVehiculo.VehiculoId == f.VehiculoId));
        if (!string.IsNullOrWhiteSpace(f.TipoVehiculo)) tires = tires.Where(x => mounted.Any(s => s.LlantaId == x.Id && s.PosicionVehiculo.EjeVehiculo.Vehiculo.Tipo == f.TipoVehiculo));
        var readings = LecturasMantenimiento(a);
        var alerts = AlertasObservadas(a).Where(x => x.Estado == EstadoAlerta.ABIERTA || x.Estado == EstadoAlerta.EN_PROCESO);
        return tires.Select(t => new HechoMantenimiento {
            Id = t.Id, Codigo = t.Codigo, Serial = t.Serial, CentroId = t.CentroId, Centro = t.Centro.Nombre,
            EstadoId = t.EstadoLlantaId, Estado = t.EstadoLlanta.Nombre,
            Alertas = alerts.Count(x => x.LlantaId == t.Id),
            AlertaId = alerts.Where(x => x.LlantaId == t.Id).OrderBy(x => x.FechaCreacion).ThenBy(x => x.Id).Select(x => (Guid?)x.Id).FirstOrDefault(),
            FechaAlerta = alerts.Where(x => x.LlantaId == t.Id).Select(x => (DateTimeOffset?)x.FechaCreacion).Min(),
            LecturaId = readings.Where(x => x.LlantaId == t.Id).OrderByDescending(x => x.Inspeccion.FechaCreacion).ThenByDescending(x => x.Id).Select(x => (Guid?)x.Id).FirstOrDefault(),
            FechaLectura = readings.Where(x => x.LlantaId == t.Id).Select(x => (DateTimeOffset?)x.Inspeccion.FechaCreacion).Max(),
            UltimaCompleta = readings.Where(x => x.LlantaId == t.Id).OrderByDescending(x => x.Inspeccion.FechaCreacion).ThenByDescending(x => x.Id)
                .Select(x => x.ProfundidadExterior >= 0 && x.ProfundidadCentro >= 0 && x.ProfundidadInterior >= 0).FirstOrDefault()
        });
    }

    public IOrderedQueryable<HechoMantenimiento> OrdenarMantenimiento(IQueryable<HechoMantenimiento> q) => q
        .OrderBy(x => x.Alertas > 0 ? 0 : !x.UltimaCompleta ? 1 : 2)
        .ThenBy(x => x.Alertas > 0 ? x.FechaAlerta : x.FechaLectura).ThenBy(x => x.Codigo).ThenBy(x => x.Id);

    public async Task<ColaMantenimiento> MantenimientoAsync(FiltroMantenimiento f, AlcanceCentros a, CancellationToken ct)
    {
        var all = ConsultaMantenimiento(f, a);
        var totals = await all.GroupBy(x => 1).Select(g => new {
            Total = g.Count(), Queue = g.Count(x => x.Alertas > 0 || !x.UltimaCompleta),
            Alerts = g.Count(x => x.Alertas > 0), Missing = g.Count(x => x.Alertas == 0 && x.LecturaId == null),
            Incomplete = g.Count(x => x.Alertas == 0 && x.LecturaId != null && !x.UltimaCompleta)
        }).SingleOrDefaultAsync(ct);
        var query = all;
        if (f.Vista == "COLA") query = query.Where(x => x.Alertas > 0 || !x.UltimaCompleta);
        if (f.Vista == "FUERA_COLA") query = query.Where(x => x.Alertas == 0 && x.UltimaCompleta);
        if (f.Clasificacion == "CONDICION_POR_VERIFICAR") query = query.Where(x => x.Alertas == 0 && !x.UltimaCompleta);
        var count = await query.CountAsync(ct);
        var facts = await OrdenarMantenimiento(query).Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano).ToListAsync(ct);
        var rows = await FilasMantenimientoAsync(facts, a, ct);
        return new(DateTimeOffset.UtcNow, f, totals?.Total ?? 0, totals?.Queue ?? 0,
            new("NO_EVALUABLE", null, ["Clasificación integral pendiente de reglas"]),
            [new("ATENCION_INMEDIATA", new("NO_EVALUABLE", null, ["Severidad no estructurada"])),
             new("INTERVENCION_PROXIMA", new("PENDIENTE_CONFIGURACION", null, ["Pronóstico no disponible en V1"])),
             new("SEGUIMIENTO", new("PENDIENTE_CONFIGURACION", null, ["Tendencias relevantes no configuradas"])),
             new("CONDICION_POR_VERIFICAR", new("PARCIAL", (totals?.Missing ?? 0) + (totals?.Incomplete ?? 0), ["Falta de evidencia; vigencia no evaluada"])),
             new("SIN_SENALES", new("NO_EVALUABLE", null, ["No se deduce seguridad de ausencia de alertas"]))],
            ["Orden de revisión; severidad no evaluable", "Fechas de registro de inspecciones, no de toma independiente de lectura"],
            new(rows, f.Pagina, f.Tamano, count),
            [new("ALERTA_ACTIVA", "Con alertas activas", totals?.Alerts ?? 0),
             new("SIN_MEDICION", "Sin medición; sin alerta activa", totals?.Missing ?? 0),
             new("MEDICION_INCOMPLETA", "Última incompleta; sin alerta activa", totals?.Incomplete ?? 0)]);
    }

    private async Task<IReadOnlyList<FilaMantenimiento>> FilasMantenimientoAsync(IReadOnlyList<HechoMantenimiento> facts, AlcanceCentros a, CancellationToken ct)
    {
        if (facts.Count == 0) return [];
        var ids = facts.Select(x => x.Id).ToArray();
        var readingIds = facts.Where(x => x.LecturaId.HasValue).Select(x => x.LecturaId!.Value).ToArray();
        var complete = LecturasMantenimiento(a).Where(x => x.ProfundidadExterior >= 0 && x.ProfundidadCentro >= 0 && x.ProfundidadInterior >= 0);
        var completeIds = Llantas(new(), a).Where(x => ids.Contains(x.Id)).Select(t => complete.Where(d => d.LlantaId == t.Id)
            .OrderByDescending(d => d.Inspeccion.FechaCreacion).ThenByDescending(d => d.Id).Select(d => (Guid?)d.Id).FirstOrDefault());
        var readings = await LecturasMantenimiento(a).Where(d => ids.Contains(d.LlantaId!.Value) && (readingIds.Contains(d.Id) || completeIds.Contains(d.Id)))
            .Select(d => new { d.LlantaId, d.Id, d.InspeccionId, Fecha = d.Inspeccion.FechaCreacion, d.ProfundidadExterior, d.ProfundidadCentro, d.ProfundidadInterior, d.Inspeccion.Kilometraje }).ToListAsync(ct);
        var mounts = await Asignaciones(new(), a).Where(s => s.EsActiva && ids.Contains(s.LlantaId))
            .Select(s => new { s.LlantaId, s.Id, s.PosicionVehiculoId, Posicion = s.PosicionVehiculo.Codigo,
                s.PosicionVehiculo.EjeVehiculo.VehiculoId, s.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa, s.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno }).ToListAsync(ct);
        return facts.Select(t => {
            var measurements = readings.Where(d => d.LlantaId == t.Id).OrderByDescending(d => d.Fecha).ThenByDescending(d => d.Id)
                .Select(d => MedicionResumen.Crear(d.Id, d.InspeccionId, d.Fecha, d.ProfundidadExterior, d.ProfundidadCentro, d.ProfundidadInterior, d.Kilometraje)).ToArray();
            var last = measurements.FirstOrDefault(d => d.LecturaId == t.LecturaId);
            var full = measurements.FirstOrDefault(d => d.Completa);
            var locations = mounts.Where(s => s.LlantaId == t.Id).ToArray();
            var location = locations.Length == 1 ? locations[0] : null;
            var ubicacion = new UbicacionMantenimiento(locations.Length == 0 ? "SIN_ASIGNACION" : location is null ? "AMBIGUA" : "UNICA",
                location?.Id, location is null ? null : new(location.VehiculoId, location.NumeroInterno), location?.Placa,
                location is null ? null : new(location.PosicionVehiculoId, location.Posicion));
            var evaluation = EvidenciaMantenimiento.Evaluar(new(t.Alertas, t.AlertaId, t.FechaAlerta, last, full));
            var secondary = evaluation.MotivosSecundarios.ToList();
            if (locations.Length > 1) secondary.Add(new("ASIGNACION_AMBIGUA", "Varias asignaciones activas; ubicación no seleccionada arbitrariamente", null, null));
            var actions = evaluation.TiposAccion.Where(type => type is not ("INSPECCIONAR" or "PROGRAMAR_EVALUACION") || location is not null)
                .Select(type => new AccionMantenimiento(type, t.Id, location?.VehiculoId, location?.PosicionVehiculoId, t.AlertaId)).ToArray();
            return new FilaMantenimiento(t.Id, t.Codigo, t.Serial, new(t.CentroId, t.Centro), new(t.EstadoId, t.Estado), ubicacion,
                evaluation.Clasificacion, evaluation.EnCola, evaluation.MotivoPrincipal, secondary, last, full,
                new("PENDIENTE_CONFIGURACION", null, ["Vigencia de inspección no configurada"]), t.Alertas, t.FechaAlerta, actions);
        }).ToArray();
    }

    public async Task<DetalleMantenimiento> DetalleMantenimientoAsync(Guid id, AlcanceCentros a, CancellationToken ct)
    {
        var fact = await ConsultaMantenimiento(new() { Montaje = "TODAS", Vista = "TODAS" }, a).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Llanta no encontrada en el alcance autorizado.");
        var row = (await FilasMantenimientoAsync([fact], a, ct)).Single();
        var alerts = await AlertasMantenimientoAsync(id, 1, 20, a, ct);
        var rules = await db.ParametrosAlerta.AsNoTracking().Where(x => x.Activo && x.Tipo == "PROFUNDIDAD_MINIMA" && (!x.CentroId.HasValue || x.CentroId == fact.CentroId))
            .OrderBy(x => x.Codigo).Select(x => new ReglaProfundidadAnalitica(x.Codigo, x.Operador, x.Valor, x.Unidad, x.CentroId.HasValue ? "Centro actual" : "Global")).ToListAsync(ct);
        return new(DateTimeOffset.UtcNow, row, alerts, rules,
            new("NO_EVALUABLE", null, ["Pronóstico no disponible en V1; continuidad y reglas pendientes de validación"]),
            new("NO_EVALUABLE", null, ["El estado administrativo de una alerta o actividad no prueba resolución del riesgo"]));
    }

    public async Task<Pagina<AlertaObservada>> AlertasMantenimientoAsync(Guid id, int pagina, int tamano, AlcanceCentros a, CancellationToken ct)
    {
        new FiltroMantenimiento { Pagina = pagina, Tamano = tamano }.Validar(a);
        if (!await Llantas(new(), a).AnyAsync(x => x.Id == id, ct)) throw new KeyNotFoundException("Llanta no encontrada en el alcance autorizado.");
        var q = AlertasObservadas(a).Where(x => x.LlantaId == id);
        var total = await q.CountAsync(ct);
        var data = await q.OrderByDescending(x => x.FechaCreacion).ThenBy(x => x.Id).Skip((pagina - 1) * tamano).Take(tamano)
            .Select(x => new { x.Id, x.InspeccionId, x.Tipo, x.Descripcion, x.FechaCreacion, x.Estado, InspeccionEstado = x.Inspeccion.Estado }).ToListAsync(ct);
        return new(data.Select(x => new AlertaObservada(x.Id, x.InspeccionId, x.Tipo, x.Descripcion, x.FechaCreacion,
            x.Estado.ToString(), x.InspeccionEstado.ToString(), new("NO_EVALUABLE", null, ["Severidad no estructurada"]))).ToArray(), pagina, tamano, total);
    }

    public async Task<Pagina<ReferenciaVehiculo>> VehiculosMantenimientoAsync(string? buscar, Guid? centroId, int pagina, int tamano, AlcanceCentros a, CancellationToken ct)
    {
        new FiltroMantenimiento { Buscar = buscar, CentroId = centroId, Pagina = pagina, Tamano = tamano }.Validar(a);
        var q = Asignaciones(new() { CentroId = centroId }, a).Where(s => s.EsActiva)
            .Select(s => new { s.PosicionVehiculo.EjeVehiculo.Vehiculo.Id, s.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa,
                s.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno, s.PosicionVehiculo.EjeVehiculo.Vehiculo.CentroId }).Distinct();
        if (!string.IsNullOrWhiteSpace(buscar)) q = q.Where(x => x.Placa.Contains(buscar.Trim()) || x.NumeroInterno.Contains(buscar.Trim()));
        var count = await q.CountAsync(ct);
        var data = await q.OrderBy(x => x.NumeroInterno).ThenBy(x => x.Id).Skip((pagina - 1) * tamano).Take(tamano)
            .Select(x => new ReferenciaVehiculo(x.Id, x.Placa, x.NumeroInterno, x.CentroId)).ToListAsync(ct);
        return new(data, pagina, tamano, count);
    }
}
