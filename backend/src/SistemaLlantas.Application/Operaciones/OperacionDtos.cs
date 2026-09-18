using System.ComponentModel.DataAnnotations;

namespace SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Common;

public sealed record ActividadDto(Guid Id,string Tipo,DateTimeOffset Fecha,string Centro,Guid? VehiculoId,string Vehiculo,string Prioridad,string Estado,string RutaInicio,DateTimeOffset? FechaCumplimiento);
public sealed record MovimientoDto(Guid Id,string Numero,string Tipo,string Motivo,DateTimeOffset Fecha,IReadOnlyList<MovimientoDetalleDto> Detalles);
public sealed record MovimientoDetalleDto(Guid LlantaId,string Llanta,string? Origen,string Destino);
public sealed class ConsultaMovimientos
{
 public int Pagina {get;init;}=1; public int Tamano {get;init;}=20; public string? Buscar {get;init;} public string? Numero {get;init;} public string? Tipo {get;init;} public Guid? CentroId {get;init;} public string? Usuario {get;init;} public string? Origen {get;init;} public string? Destino {get;init;} public DateTimeOffset? Desde {get;init;} public DateTimeOffset? Hasta {get;init;}
}
public sealed record MovimientoTrazabilidadDto(Guid Id,string Numero,DateTimeOffset Fecha,string Tipo,Guid LlantaId,string Llanta,string Serial,string Origen,string Destino,string VehiculoPosicion,string Centro,decimal? KilometrajeVehiculo,decimal? KilometrosTramo,string Usuario,Guid? ActividadProgramadaId,string Motivo,string? Observaciones,string Estado,string? VehiculoInterno=null,string? VehiculoPlaca=null,string? PosicionOrigen=null,string? PosicionDestino=null,string? CentroOrigen=null,string? CentroDestino=null,Guid? SolicitudId=null);
public sealed record SolicitudOperacionDto(Guid Id,string Tipo,string Estado,Guid CentroId,string Centro,Guid LlantaId,string Llanta,Guid? PosicionOrigenId,Guid? PosicionDestinoId,string TipoDestino,Guid? CentroDestinoId,string Motivo,string? Observaciones,string Solicitante,string? Aprobador,string? MotivoRechazo,DateTimeOffset Fecha,DateTimeOffset? FechaRecepcionDestino,string RowVersion);
public sealed class CrearSolicitudOperacionDto
{
 public Guid? VehiculoId {get;init;}
 public IReadOnlyList<AsignacionMontajeDto>? Asignaciones {get;init;}
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
    Task<MovimientoDto> MontarEnInspeccionAsync(EjecutarMovimientoDto dto, Guid inspeccionId, string usuario, AlcanceCentros alcance, CancellationToken ct);
    Task ValidarAsignacionesAsync(Guid vehiculoId,IReadOnlyList<AsignacionMontajeDto> asignaciones,AlcanceCentros alcance,Guid? grupoExcluir,CancellationToken ct);
    Task EjecutarReemplazosAsync(IReadOnlyList<SistemaLlantas.Domain.Entities.SolicitudOperacion> solicitudes,decimal kilometraje,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task ValidarMontajeAsync(Guid llantaId, Guid posicionId, decimal? kilometraje, AlcanceCentros alcance, CancellationToken ct);
    Task<MovimientoDto> MoverAsync(EjecutarMovimientoDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<MovimientoDto> DesmontarAsync(DesmontarLlantaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
}

public sealed class SolicitudNoEncontradaException() : Exception("La solicitud no existe o fue desactivada. Actualiza Autorizaciones.");

public sealed record AsignacionMontajeDto(Guid PosicionId,Guid LlantaId,Guid? LlantaActualId,string? Codigo=null,string? Serial=null,string? MarcaReferencia=null,string? Dimension=null);
public sealed record TrabajoMontajeDto(Guid ActividadId,Guid? GrupoId,Guid VehiculoId,string Tipo,string Motivo,string? Observaciones,IReadOnlyList<AsignacionMontajeDto> Asignaciones);
public sealed record EjecutarTrabajoMontajeDto(decimal Kilometraje);
public static class AsignacionesMontaje
{
 public static void Validar(IReadOnlyList<AsignacionMontajeDto>? filas,bool individual=false)
 {
  if(filas is null||filas.Count==0)throw new ValidacionException("Asigna al menos una llanta a una posición.");
  if(filas.Count>100)throw new ValidacionException("Un trabajo admite hasta 100 posiciones.");
  if(individual&&filas.Count!=1)throw new ValidacionException("El montaje individual requiere exactamente una posición.");
  if(filas.Any(x=>x.PosicionId==Guid.Empty||x.LlantaId==Guid.Empty||x.LlantaId==x.LlantaActualId))throw new ValidacionException("Cada cambio requiere una posición y una llanta nueva diferente a la actual.");
  if(filas.Select(x=>x.LlantaId).Distinct().Count()!=filas.Count)throw new ValidacionException("La misma llanta no puede asignarse a dos posiciones.");
  if(filas.Select(x=>x.PosicionId).Distinct().Count()!=filas.Count)throw new ValidacionException("Una posición no puede repetirse.");
 }
}
