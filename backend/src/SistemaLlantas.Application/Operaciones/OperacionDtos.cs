using System.ComponentModel.DataAnnotations;

namespace SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Common;

public sealed record ActividadDto(Guid Id,string Tipo,DateTimeOffset Fecha,string Centro,Guid? VehiculoId,string Vehiculo,string Prioridad,string Estado,string RutaInicio);
public sealed record MovimientoDto(Guid Id,string Numero,string Tipo,string Motivo,DateTimeOffset Fecha,IReadOnlyList<MovimientoDetalleDto> Detalles);
public sealed record MovimientoDetalleDto(Guid LlantaId,string Llanta,string? Origen,string Destino);
public sealed record SolicitudOperacionDto(Guid Id,string Tipo,string Estado,Guid CentroId,string Centro,Guid LlantaId,string Llanta,Guid? PosicionOrigenId,Guid? PosicionDestinoId,string TipoDestino,Guid? CentroDestinoId,string Motivo,string? Observaciones,string Solicitante,string? Aprobador,string? MotivoRechazo,DateTimeOffset Fecha,DateTimeOffset? FechaRecepcionDestino,string RowVersion);
public sealed class CrearSolicitudOperacionDto
{
 public string Tipo {get;init;}="Movimiento";public Guid LlantaId {get;init;}public Guid? PosicionOrigenId {get;init;}public Guid? PosicionDestinoId {get;init;}public string TipoDestino {get;init;}="Inventario";public Guid? CentroDestinoId {get;init;}public Guid? LlantaDesplazadaId {get;init;}public Guid? PosicionDestinoDesplazadaId {get;init;}public string? DestinoDesplazada {get;init;}public string Motivo {get;init;}=string.Empty;public string? Observaciones {get;init;}public decimal? KilometrajeVehiculo {get;init;}public Guid? ActividadProgramadaId {get;init;}
}
public sealed record ResolverSolicitudDto(bool Aprobar,string? Motivo);
public sealed class EjecutarMovimientoDto
{
    [Required] public Guid LlantaId { get; init; }
    public Guid? PosicionOrigenId { get; init; }
    public Guid? PosicionDestinoId { get; init; }
    [Required] public string TipoDestino { get; init; } = string.Empty;
    public Guid? LlantaDesplazadaId { get; init; }
    public Guid? PosicionDestinoDesplazadaId { get; init; }
    public string? DestinoDesplazada { get; init; }
    [Required,StringLength(500)] public string Motivo { get; init; } = string.Empty;
    public decimal? KilometrajeVehiculo { get; init; }
    [StringLength(1000)] public string? Observaciones { get; init; }
}
public sealed class DesmontarLlantaDto { [Required] public Guid PosicionId { get; init; } [Required] public string Destino { get; init; }=string.Empty; [Required] public string Motivo { get; init; }=string.Empty; public decimal? KilometrajeVehiculo {get;init;} public string? Observaciones {get;init;} }

public interface IOperacionService
{
    Task<IReadOnlyList<ActividadDto>> MisActividadesAsync(string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<ActividadDto> IniciarActividadAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<ActividadDto> CompletarActividadAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<MovimientoDto> MoverAsync(EjecutarMovimientoDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<MovimientoDto> DesmontarAsync(DesmontarLlantaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
}
