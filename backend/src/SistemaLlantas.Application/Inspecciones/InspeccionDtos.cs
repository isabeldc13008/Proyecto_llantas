using System.ComponentModel.DataAnnotations;

namespace SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Common;

public sealed record LlantaPosicionDto(Guid Id, string Codigo, string Estado, string Marca, string Referencia, string Dimension);
public sealed record PosicionInspeccionDto(Guid Id, string Codigo, string Lado, int Orden, LlantaPosicionDto? Llanta);
public sealed record EjeInspeccionDto(Guid Id, int Numero, string Nombre, IReadOnlyList<PosicionInspeccionDto> Posiciones);
public sealed record ContextoInspeccionDto(Guid VehiculoId, string NumeroInterno, string Placa, string Tipo, Guid CentroId, string CentroNombre, string? Relevancia, string? Regional, decimal? Kilometraje, DateTimeOffset? UltimaInspeccion, IReadOnlyList<EjeInspeccionDto> Ejes);
public sealed record VehiculoInspeccionDto(Guid Id, string NumeroInterno, string Placa, string Tipo, Guid CentroId, string CentroCodigo, string CentroNombre, string? Regional);
public sealed record OpcionInspeccionDto(Guid Id, string Codigo, string Nombre);
public sealed record OpcionesInspeccionDto(IReadOnlyList<OpcionInspeccionDto> Condiciones, IReadOnlyList<OpcionInspeccionDto> Causas, IReadOnlyList<OpcionInspeccionDto> Recomendaciones);
public sealed record InspeccionDto(Guid Id, Guid VehiculoId, string Vehiculo, Guid CentroId, string Centro, decimal? Kilometraje, string Estado, string TecnicoId, IReadOnlyList<DetalleInspeccionDto> Detalles,IReadOnlyList<InconsistenciaPosicionDto> Inconsistencias);
public sealed record DetalleInspeccionDto(Guid Id, Guid PosicionId, string Posicion, Guid? LlantaId, string? Llanta, decimal? Exterior, decimal? Centro, decimal? Interior, Guid? CondicionId, Guid? CausaId, Guid? RecomendacionId, string? Observaciones);
public sealed record InconsistenciaPosicionDto(Guid Id,Guid PosicionId,Guid? LlantaEncontradaId,string IdentificadorEncontrado,string Estado,string? Observacion);

public sealed class CrearInspeccionDto
{
    [Required] public Guid VehiculoId { get; init; }
    [Required, Range(0, double.MaxValue)] public decimal? Kilometraje { get; init; }
    [StringLength(1000)] public string? Observaciones { get; init; }
}

public sealed class GuardarDetalleInspeccionDto
{
    [Range(0, 15)] public decimal? ProfundidadExterior { get; init; }
    [Range(0, 15)] public decimal? ProfundidadCentro { get; init; }
    [Range(0, 15)] public decimal? ProfundidadInterior { get; init; }
    public Guid? CondicionId { get; init; }
    public Guid? CausaId { get; init; }
    public Guid? RecomendacionId { get; init; }
    [StringLength(1000)] public string? Observaciones { get; init; }
}

public sealed class ReportarInconsistenciaDto
{
    [Required] public Guid PosicionId { get; init; }
    public Guid? LlantaEncontradaId { get; init; }
    public bool LlantaNoEncontrada { get; init; }
    [Required, StringLength(100)] public string IdentificadorEncontrado { get; init; } = string.Empty;
    [StringLength(1000)] public string? Observacion { get; init; }
}

public sealed record InconsistenciaDto(Guid Id, Guid InspeccionId, string Centro, string Vehiculo, string Posicion, string? LlantaEsperada, Guid? LlantaEncontradaId, string LlantaEncontrada, string Tecnico, DateTimeOffset Fecha, string Estado, string? Autorizador, string? ObservacionAutorizacion, string? Observacion,IReadOnlyList<EvidenciaDto> Evidencias);
public sealed class ResolverInconsistenciaDto { [Required, StringLength(1000)] public string Observacion { get; init; } = string.Empty; public Guid? LlantaInventarioId { get; init; } }
public sealed record AlertaDto(Guid Id,string Tipo,string Descripcion,string Estado,DateTimeOffset Fecha,Guid InspeccionId,string Vehiculo,string Centro,string Posicion,string? Llanta,IReadOnlyList<AlertaEventoDto> Historial);
public sealed record AlertaEventoDto(DateTimeOffset Fecha,string EstadoAnterior,string EstadoNuevo,string Usuario,string? Observacion);
public sealed record CambiarAlertaDto(string Estado,string? Observacion);
public sealed record EvidenciaDto(Guid Id,string NombreArchivo,string MimeType,long TamanoBytes,string Hash,DateTimeOffset Fecha,bool Activo);
public sealed record LlantaBusquedaInspeccionDto(Guid Id,string Codigo,string Serial,string Marca,string Referencia,string Dimension,string Estado);
public sealed record ResumenInspeccionesDto(int PendientesHoy,int RealizadasHoy,int ConNovedad,int ConAlerta);
public sealed record HistorialInspeccionDto(Guid Id,DateTimeOffset Fecha,string Placa,string NumeroInterno,string Centro,string Tecnico,decimal? Kilometraje,int Inspeccionadas,int Novedades,int Alertas,string Estado);

public interface IInspeccionService
{
    Task<IReadOnlyList<VehiculoInspeccionDto>> ObtenerVehiculosAsync(string usuario, bool soloAsignados, string? buscar, AlcanceCentros alcance, CancellationToken ct, bool permitirVehiculosGlobales = false);
    Task<OpcionesInspeccionDto> ObtenerOpcionesAsync(CancellationToken ct);
    Task<ContextoInspeccionDto?> ObtenerContextoAsync(Guid vehiculoId, AlcanceCentros alcance, CancellationToken ct, bool permitirVehiculoGlobal = false);
    Task<InspeccionDto> CrearAsync(CrearInspeccionDto dto, string usuario, AlcanceCentros alcance, CancellationToken ct, bool permitirVehiculoGlobal = false);
    Task<InspeccionDto?> ObtenerAsync(Guid id, AlcanceCentros alcance, CancellationToken ct, string? usuario = null, bool permitirPropia = false);
    Task<InspeccionDto?> GuardarDetalleAsync(Guid id, Guid posicionId, GuardarDetalleInspeccionDto dto, string usuario, CancellationToken ct);
    Task<InconsistenciaDto> ReportarAsync(Guid inspeccionId, ReportarInconsistenciaDto dto, string usuario, CancellationToken ct);
    Task<IReadOnlyList<InconsistenciaDto>> PendientesAsync(AlcanceCentros alcance, CancellationToken ct);
    Task<InconsistenciaDto> ResolverAsync(Guid id, ResolverInconsistenciaDto dto, bool autorizar, string usuario, bool puedeAutorizarPropia, CancellationToken ct, AlcanceCentros? alcance = null);
    Task<IReadOnlyList<AlertaDto>> AlertasAsync(AlcanceCentros alcance,CancellationToken ct);
    Task<AlertaDto> CambiarAlertaAsync(Guid id,CambiarAlertaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<ResumenInspeccionesDto> ResumenAsync(string usuario,bool soloPropias,AlcanceCentros alcance,CancellationToken ct);
    Task<IReadOnlyList<HistorialInspeccionDto>> HistorialAsync(string usuario,bool soloPropias,AlcanceCentros alcance,CancellationToken ct);
    Task<InspeccionDto> FinalizarAsync(Guid id,string usuario,CancellationToken ct);
    Task<IReadOnlyList<LlantaBusquedaInspeccionDto>> BuscarLlantaExactaAsync(string termino,CancellationToken ct);
}
