using System.ComponentModel.DataAnnotations;

namespace SistemaLlantas.Application.Inspecciones;

public sealed record LlantaPosicionDto(Guid Id, string Codigo, string Estado);
public sealed record PosicionInspeccionDto(Guid Id, string Codigo, string Lado, int Orden, LlantaPosicionDto? Llanta);
public sealed record EjeInspeccionDto(Guid Id, int Numero, string Nombre, IReadOnlyList<PosicionInspeccionDto> Posiciones);
public sealed record ContextoInspeccionDto(Guid VehiculoId, string NumeroInterno, string Placa, string Tipo, Guid CentroId, string CentroNombre, string? Relevancia, IReadOnlyList<EjeInspeccionDto> Ejes);
public sealed record VehiculoInspeccionDto(Guid Id, string NumeroInterno, string Placa, string Tipo, Guid CentroId, string CentroCodigo, string CentroNombre);
public sealed record OpcionInspeccionDto(Guid Id, string Codigo, string Nombre);
public sealed record OpcionesInspeccionDto(IReadOnlyList<OpcionInspeccionDto> Condiciones, IReadOnlyList<OpcionInspeccionDto> Causas, IReadOnlyList<OpcionInspeccionDto> Recomendaciones);
public sealed record InspeccionDto(Guid Id, Guid VehiculoId, string Vehiculo, Guid CentroId, string Centro, decimal? Kilometraje, string Estado, string TecnicoId, IReadOnlyList<DetalleInspeccionDto> Detalles);
public sealed record DetalleInspeccionDto(Guid Id, Guid PosicionId, string Posicion, Guid? LlantaId, string? Llanta, decimal? Exterior, decimal? Centro, decimal? Interior, Guid? CondicionId, Guid? CausaId, Guid? RecomendacionId, string? Observaciones);

public sealed class CrearInspeccionDto
{
    [Required] public Guid VehiculoId { get; init; }
    [Range(0, double.MaxValue)] public decimal? Kilometraje { get; init; }
    [StringLength(1000)] public string? Observaciones { get; init; }
}

public sealed class GuardarDetalleInspeccionDto
{
    [Range(0, 100)] public decimal? ProfundidadExterior { get; init; }
    [Range(0, 100)] public decimal? ProfundidadCentro { get; init; }
    [Range(0, 100)] public decimal? ProfundidadInterior { get; init; }
    public Guid? CondicionId { get; init; }
    public Guid? CausaId { get; init; }
    public Guid? RecomendacionId { get; init; }
    [StringLength(1000)] public string? Observaciones { get; init; }
}

public sealed class ReportarInconsistenciaDto
{
    [Required] public Guid PosicionId { get; init; }
    [Required, StringLength(100)] public string IdentificadorEncontrado { get; init; } = string.Empty;
    [Required, StringLength(1000)] public string Observacion { get; init; } = string.Empty;
}

public sealed record InconsistenciaDto(Guid Id, Guid InspeccionId, string Centro, string Vehiculo, string Posicion, string? LlantaEsperada, string LlantaEncontrada, string Tecnico, DateTimeOffset Fecha, string Estado, string? Autorizador, string? ObservacionAutorizacion);
public sealed class ResolverInconsistenciaDto { [Required, StringLength(1000)] public string Observacion { get; init; } = string.Empty; public Guid? LlantaInventarioId { get; init; } }

public interface IInspeccionService
{
    Task<IReadOnlyList<VehiculoInspeccionDto>> ObtenerVehiculosAsync(Guid? centroUsuario, CancellationToken ct);
    Task<OpcionesInspeccionDto> ObtenerOpcionesAsync(CancellationToken ct);
    Task<ContextoInspeccionDto?> ObtenerContextoAsync(Guid vehiculoId, Guid? centroUsuario, CancellationToken ct);
    Task<InspeccionDto> CrearAsync(CrearInspeccionDto dto, string usuario, Guid? centroUsuario, CancellationToken ct);
    Task<InspeccionDto?> ObtenerAsync(Guid id, Guid? centroUsuario, CancellationToken ct);
    Task<InspeccionDto?> GuardarDetalleAsync(Guid id, Guid posicionId, GuardarDetalleInspeccionDto dto, string usuario, CancellationToken ct);
    Task<InconsistenciaDto> ReportarAsync(Guid inspeccionId, ReportarInconsistenciaDto dto, string usuario, CancellationToken ct);
    Task<IReadOnlyList<InconsistenciaDto>> PendientesAsync(Guid? centroUsuario, CancellationToken ct);
    Task<InconsistenciaDto> ResolverAsync(Guid id, ResolverInconsistenciaDto dto, bool autorizar, string usuario, bool puedeAutorizarPropia, CancellationToken ct);
}
