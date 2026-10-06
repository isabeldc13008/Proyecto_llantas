using System.ComponentModel.DataAnnotations;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Analitica;

// Nullable type arguments are intentional: unavailable numeric/boolean values never serialize as zero/false.
public sealed record Evaluacion<T>(string Estado, T Valor, IReadOnlyList<string> Motivos);
public sealed record ReferenciaMantenimiento(Guid Id, string Nombre);
public sealed record FuenteMantenimiento(string Tipo, Guid Id);
public sealed record MotivoMantenimiento(string Codigo, string Descripcion, DateTimeOffset? Fecha, FuenteMantenimiento? Fuente);
public sealed record AccionMantenimiento(string Tipo, Guid LlantaId, Guid? VehiculoId, Guid? PosicionId, Guid? AlertaId);
public sealed record UbicacionMantenimiento(string Estado, Guid? AsignacionId, ReferenciaMantenimiento? Vehiculo, string? Placa, ReferenciaMantenimiento? Posicion);
public sealed record MedicionResumen(Guid LecturaId, Guid InspeccionId, DateTimeOffset FechaRegistro,
    decimal? Exterior, decimal? Centro, decimal? Interior, decimal? Minima, bool Completa, decimal? Odometro)
{
    public static MedicionResumen Crear(Guid id, Guid inspeccionId, DateTimeOffset fecha, decimal? exterior, decimal? centro, decimal? interior, decimal? odometro)
    {
        var completa = exterior >= 0 && centro >= 0 && interior >= 0;
        return new(id, inspeccionId, fecha, exterior, centro, interior,
            completa ? Math.Min(exterior!.Value, Math.Min(centro!.Value, interior!.Value)) : null, completa, odometro);
    }
}
public sealed record FilaMantenimiento(Guid Id, string Codigo, string Serial, ReferenciaMantenimiento Centro,
    ReferenciaMantenimiento EstadoOperativo, UbicacionMantenimiento Ubicacion, Evaluacion<string?> Clasificacion,
    bool EnCola, MotivoMantenimiento? MotivoPrincipal, IReadOnlyList<MotivoMantenimiento> MotivosSecundarios,
    MedicionResumen? UltimaLectura, MedicionResumen? UltimaCompleta, Evaluacion<bool?> Vigencia,
    int AlertasActivas, DateTimeOffset? FechaAlertaActivaMasAntigua, IReadOnlyList<AccionMantenimiento> Acciones);
public sealed record ConteoMantenimiento(string Clasificacion, Evaluacion<int?> Resultado);
public sealed record ConcentracionMotivo(string Codigo, string Nombre, int Llantas);
public sealed record ColaMantenimiento(DateTimeOffset GeneradoEn, FiltroMantenimiento Alcance, int TotalEnAlcance,
    int TotalEnCola, Evaluacion<int?> TotalEvaluacionCompleta, IReadOnlyList<ConteoMantenimiento> Conteos,
    IReadOnlyList<string> Limitaciones, Pagina<FilaMantenimiento> Pagina, IReadOnlyList<ConcentracionMotivo> Concentracion);
public sealed record ReferenciaVehiculo(Guid Id, string Placa, string NumeroInterno, Guid CentroId);
public sealed record AlertaObservada(Guid Id, Guid InspeccionId, string Tipo, string Descripcion, DateTimeOffset FechaRegistro,
    string EstadoAdministrativo, string EstadoInspeccion, Evaluacion<string?> Severidad);
public sealed record DetalleMantenimiento(DateTimeOffset GeneradoEn, FilaMantenimiento Llanta,
    Pagina<AlertaObservada> Alertas, IReadOnlyList<ReglaProfundidadAnalitica> ReglasProfundidad,
    Evaluacion<decimal?> Pronostico, Evaluacion<bool?> ResolucionRiesgo);

public sealed class FiltroMantenimiento
{
    [StringLength(100)] public string? Buscar { get; init; }
    public Guid? CentroId { get; init; }
    public Guid? VehiculoId { get; init; }
    public Guid? MarcaId { get; init; }
    public Guid? ReferenciaId { get; init; }
    public Guid? DimensionId { get; init; }
    [StringLength(100)] public string? TipoVehiculo { get; init; }
    public string Montaje { get; init; } = "MONTADAS";
    public string Vista { get; init; } = "COLA";
    public string? Clasificacion { get; init; }
    [Range(1, 100000)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int Tamano { get; init; } = 20;
    public void Validar(AlcanceCentros alcance)
    {
        if (CentroId.HasValue && !alcance.Autoriza(CentroId.Value)) throw new UnauthorizedAccessException("Centro fuera de su alcance.");
        if (Montaje is not ("MONTADAS" or "TODAS") || Vista is not ("COLA" or "TODAS" or "FUERA_COLA")
            || Clasificacion is not (null or "" or "CONDICION_POR_VERIFICAR")) throw new ValidacionException("Filtro de mantenimiento no disponible en V1.");
        if (Pagina is < 1 or > 100000 || Tamano is < 1 or > 100 || Buscar?.Length > 100 || TipoVehiculo?.Length > 100)
            throw new ValidacionException("Filtro o paginación fuera de rango.");
    }
    public FiltroAnalitica FiltroBase() => new() { Buscar = Buscar, CentroId = CentroId, MarcaId = MarcaId, ReferenciaId = ReferenciaId, DimensionId = DimensionId };
}
