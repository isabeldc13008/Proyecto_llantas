using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Vehiculos;

public sealed record VehiculoResumenDto(Guid Id,string NumeroInterno,string Placa,string Tipo,Guid CentroId,string Centro,string? Configuracion,string Estado,decimal? Kilometraje,int Ejes,int Posiciones,string RowVersion);
public sealed record PosicionVehiculoDto(Guid Id,string Codigo,string Lado,string Ubicacion,int Orden,Guid? LlantaId,string? LlantaCodigo,string? LlantaSerial);
public sealed record EjeVehiculoDto(Guid Id,int Numero,int Orden,string Nombre,string TipoEje,IReadOnlyList<PosicionVehiculoDto> Posiciones);
public sealed record AsignacionVehiculoDto(Guid Id,string LlantaCodigo,string Posicion,DateTimeOffset FechaInicio,DateTimeOffset? FechaFin,bool EsActiva);
public sealed record VehiculoDetalleDto(Guid Id,string NumeroInterno,string Placa,string Tipo,Guid CentroId,string Centro,Guid? ConfiguracionVehiculoId,string? Configuracion,string Estado,decimal? Kilometraje,IReadOnlyList<EjeVehiculoDto> Ejes,IReadOnlyList<AsignacionVehiculoDto> Historial,string RowVersion);
public sealed record ConfiguracionPosicionDto(string Codigo,string Lado,string Ubicacion,int Orden);
public sealed record ConfiguracionEjeDto(int Orden,string Nombre,string TipoEje,IReadOnlyList<ConfiguracionPosicionDto> Posiciones);
public sealed record ConfiguracionVehiculoDto(Guid Id,string Codigo,string Nombre,string TipoVehiculo,IReadOnlyList<ConfiguracionEjeDto> Ejes,bool Activo);
public sealed record GuardarConfiguracionVehiculoDto(string Codigo,string Nombre,string TipoVehiculo,IReadOnlyList<ConfiguracionEjeDto> Ejes);
public sealed record GuardarVehiculoDto(string NumeroInterno,string Placa,string Tipo,Guid CentroId,Guid? ConfiguracionVehiculoId,decimal? Kilometraje,string Estado,string? RowVersion);

public interface IVehiculoService
{
    Task<Pagina<VehiculoResumenDto>> ConsultarAsync(ConsultaPaginada consulta,AlcanceCentros alcance,CancellationToken ct);
    Task<VehiculoDetalleDto?> ObtenerAsync(Guid id,AlcanceCentros alcance,CancellationToken ct);
    Task<VehiculoDetalleDto> CrearAsync(GuardarVehiculoDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<VehiculoDetalleDto?> ActualizarAsync(Guid id,GuardarVehiculoDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<IReadOnlyList<ConfiguracionVehiculoDto>> ConfiguracionesAsync(CancellationToken ct);
    Task<ConfiguracionVehiculoDto> CrearConfiguracionAsync(GuardarConfiguracionVehiculoDto dto,string usuario,CancellationToken ct);
}
