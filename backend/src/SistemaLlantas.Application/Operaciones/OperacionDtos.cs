using System.ComponentModel.DataAnnotations;

namespace SistemaLlantas.Application.Operaciones;

public sealed record ActividadDto(Guid Id,string Tipo,DateTimeOffset Fecha,string Centro,Guid? VehiculoId,string Vehiculo,string Prioridad,string Estado,string RutaInicio);
public sealed record MovimientoDto(Guid Id,string Numero,string Tipo,string Motivo,DateTimeOffset Fecha,IReadOnlyList<MovimientoDetalleDto> Detalles);
public sealed record MovimientoDetalleDto(Guid LlantaId,string Llanta,string? Origen,string Destino);
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
}
public sealed class DesmontarLlantaDto { [Required] public Guid PosicionId { get; init; } [Required] public string Destino { get; init; }=string.Empty; [Required] public string Motivo { get; init; }=string.Empty; }

public interface IOperacionService
{
    Task<IReadOnlyList<ActividadDto>> MisActividadesAsync(string usuario,Guid? centro,CancellationToken ct);
    Task<ActividadDto> IniciarActividadAsync(Guid id,string usuario,CancellationToken ct);
    Task<MovimientoDto> MoverAsync(EjecutarMovimientoDto dto,string usuario,Guid? centro,CancellationToken ct);
    Task<MovimientoDto> DesmontarAsync(DesmontarLlantaDto dto,string usuario,Guid? centro,CancellationToken ct);
}
