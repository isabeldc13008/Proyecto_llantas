using System.ComponentModel.DataAnnotations;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Analitica;

public sealed class FiltroAnalitica
{
    public Guid? CentroId { get; init; }
    public Guid? MarcaId { get; init; }
    public Guid? ReferenciaId { get; init; }
    public Guid? DimensionId { get; init; }
    public Guid? EstadoId { get; init; }
    [StringLength(100)] public string? TipoVehiculo { get; init; }
    public DateOnly? IngresoDesde { get; init; }
    public DateOnly? IngresoHasta { get; init; }
    [Range(1, 1000)] public int MinimoMuestra { get; init; } = 5;
    [Range(1, 100000)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int Tamano { get; init; } = 20;
    public void Validar(AlcanceCentros alcance)
    {
        if (CentroId.HasValue && !alcance.Autoriza(CentroId.Value)) throw new UnauthorizedAccessException("Centro fuera de su alcance.");
        if (IngresoDesde > IngresoHasta) throw new ValidacionException("La fecha inicial de ingreso no puede ser posterior a la final.");
        if (MinimoMuestra is < 1 or > 1000 || Pagina is < 1 or > 100000 || Tamano is < 1 or > 100) throw new ValidacionException("Muestra o paginación fuera de rango.");
    }
}

public sealed record OpcionAnalitica(string Id, string Nombre);
public sealed record OpcionesAnalitica(IReadOnlyList<OpcionAnalitica> Centros, IReadOnlyList<OpcionAnalitica> Marcas, IReadOnlyList<OpcionAnalitica> Referencias, IReadOnlyList<OpcionAnalitica> Dimensiones, IReadOnlyList<OpcionAnalitica> Estados, IReadOnlyList<string> TiposVehiculo);
public sealed record EstadisticaKm(int Muestra, decimal? Promedio, decimal? Mediana, decimal? Desviacion, decimal? Minimo, decimal? Maximo);
public sealed record ConteoAnalitica(string Nombre, int Cantidad);
public sealed record ResumenAnalitica(int Total, int Montadas, int Disponibles, int EnReparacion, int EnReencauche, int DisposicionFinal, int ConAlertas, int SinKm, int ConTramosIncompletos, decimal? ProfundidadPromedio, int MuestraProfundidad, decimal? MovimientosPromedio, EstadisticaKm EnOperacion, EstadisticaKm Finalizadas, IReadOnlyList<ConteoAnalitica> Estados);
public sealed record GrupoAnalitica(string Id, string Nombre, int Total, int EnOperacionTotal, int FinalizadasTotal, EstadisticaKm EnOperacion, EstadisticaKm Finalizadas, decimal ReparacionesPromedio, decimal ReencauchesPromedio, int Alertas, int Movimientos, bool MuestraSuficiente);
public sealed record PosicionAnalitica(string Configuracion, string TipoVehiculo, int Eje, string TipoEje, string Posicion, int Llantas, int MuestraKm, int Tramos, int TramosValidos, decimal? KmPromedio, decimal? DiasPromedio, bool MuestraSuficiente);
public sealed record MovimientoAnalitica(Guid Id, string Codigo, string Serial, string Marca, string Referencia, string Estado, int Total, int Vehiculos, int Centros, int Posiciones, IReadOnlyList<ConteoAnalitica> Tipos);
public sealed record RankingMovimientos(Pagina<MovimientoAnalitica> Ranking, IReadOnlyList<ConteoAnalitica> Tipos);

public interface IAnaliticaService
{
    Task<OpcionesAnalitica> OpcionesAsync(AlcanceCentros alcance, CancellationToken ct);
    Task<ResumenAnalitica> ResumenAsync(FiltroAnalitica filtro, AlcanceCentros alcance, CancellationToken ct);
    Task<Pagina<GrupoAnalitica>> CompararAsync(FiltroAnalitica filtro, string agrupar, AlcanceCentros alcance, CancellationToken ct);
    Task<Pagina<PosicionAnalitica>> PosicionesAsync(FiltroAnalitica filtro, AlcanceCentros alcance, CancellationToken ct);
    Task<RankingMovimientos> MovimientosAsync(FiltroAnalitica filtro, AlcanceCentros alcance, CancellationToken ct);
}
