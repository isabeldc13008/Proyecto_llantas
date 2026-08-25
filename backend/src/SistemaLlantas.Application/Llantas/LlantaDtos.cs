using System.ComponentModel.DataAnnotations;

namespace SistemaLlantas.Application.Llantas;

public sealed record LlantaResumenDto(
    Guid Id, string Codigo, string Serial, string Marca, string Referencia,
    string Dimension, string Tipo, string Estado, string Centro, string UbicacionActual,
    decimal ProfundidadInicial, decimal KilometrajeAcumulado, int NumeroReencauches,
    string? VehiculoActual, string? PosicionActual, DateTimeOffset? UltimaInspeccion,decimal? UltimaProfundidadMinima,int NumeroReparaciones,int NumeroMontajes,string Atencion,
    bool Activo, string RowVersion);

public sealed record EventoVidaLlantaDto(DateTimeOffset Fecha,string Tipo,string Descripcion,string Usuario,string? Centro,string? Vehiculo,string? Posicion,decimal? Kilometraje,decimal? Recorrido);
public sealed record MontajeVidaDto(DateTimeOffset FechaInicio,DateTimeOffset? FechaFin,string Vehiculo,string Placa,string Posicion,decimal? KilometrajeMontaje,decimal? KilometrajeDesmontaje,decimal? Recorrido,bool Actual);
public sealed record InspeccionVidaDto(Guid Id,DateTimeOffset Fecha,string Vehiculo,string Placa,string Posicion,string Centro,decimal? Exterior,decimal? CentroProfundidad,decimal? Interior,decimal? Minima,string Estado);
public sealed record ServicioVidaDto(Guid Id,string Tipo,string Estado,DateTimeOffset Fecha,string? Proveedor,string Motivo,DateTimeOffset? FechaEnvio,DateTimeOffset? FechaRetorno);
public sealed record MovimientoVidaDto(Guid Id,DateTimeOffset Fecha,string Tipo,string Motivo,string Centro,string Usuario,string? PosicionOrigen,string? PosicionDestino);
public sealed record ResumenCicloDto(DateOnly FechaIngreso,int Montajes,int Reparaciones,int Reencauches,DateTimeOffset? UltimaReparacion,DateTimeOffset? UltimoReencauche);
public sealed record LlantaDetalleDto(LlantaResumenDto Llanta,ResumenCicloDto Resumen,IReadOnlyList<MontajeVidaDto> Montajes,IReadOnlyList<InspeccionVidaDto> Inspecciones,IReadOnlyList<ServicioVidaDto> Servicios,IReadOnlyList<MovimientoVidaDto> Movimientos,IReadOnlyList<EventoVidaLlantaDto> Historial,bool RequiereConciliacion);
public sealed record LlantaMetricasDto(int Total,int Montadas,int Disponibles,int Reparacion,int Reencauche,int RequierenAtencion);
public sealed record TrasladarLlantaDto(Guid CentroDestinoId,string Motivo,string? Observaciones);

public interface ICicloVidaLlantaService
{
    Task<LlantaDetalleDto?> ObtenerDetalleAsync(Guid id,Common.AlcanceCentros alcance,CancellationToken ct);
    Task TrasladarCentroAsync(Guid id,TrasladarLlantaDto dto,string usuario,Common.AlcanceCentros alcance,CancellationToken ct);
    Task ConciliarMontajeAsync(Guid id,string usuario,Common.AlcanceCentros alcance,CancellationToken ct);
}

public sealed class GuardarLlantaDto
{
    [Required, StringLength(50)] public string Codigo { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Serial { get; init; } = string.Empty;
    [Required] public Guid MarcaId { get; init; }
    [Required] public Guid ReferenciaId { get; init; }
    [Required] public Guid DimensionId { get; init; }
    [Required] public Guid TipoLlantaId { get; init; }
    [Required] public Guid EstadoLlantaId { get; init; }
    [Required] public Guid CentroId { get; init; }
    [Required, StringLength(150)] public string UbicacionActual { get; init; } = string.Empty;
    public DateOnly? FechaCompra { get; init; }
    [Range(0, 999999999999.99)] public decimal? Costo { get; init; }
    [Range(0, 100)] public decimal ProfundidadInicial { get; init; }
    public DateOnly? FechaIngreso { get; init; }
    [StringLength(1000)] public string? Observaciones { get; init; }
    public string? RowVersion { get; init; }
}
