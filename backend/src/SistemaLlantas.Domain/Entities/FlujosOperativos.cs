using SistemaLlantas.Domain.Common;

namespace SistemaLlantas.Domain.Entities;

public enum EstadoSolicitudOperacion { BORRADOR, PENDIENTE_APROBACION, APROBADO, RECHAZADO, EJECUTADO }
public sealed class SolicitudOperacion : EntidadAuditable
{
    public string Tipo {get;set;}=string.Empty;
    public EstadoSolicitudOperacion Estado {get;set;}=EstadoSolicitudOperacion.BORRADOR;
    public Guid CentroId {get;set;} public Centro Centro {get;set;}=null!;
    public Guid LlantaId {get;set;} public Llanta Llanta {get;set;}=null!;
    public Guid? PosicionOrigenId {get;set;}
    public Guid? PosicionDestinoId {get;set;}
    public string TipoDestino {get;set;}=string.Empty;
    public Guid? CentroDestinoId {get;set;}
    public Guid? LlantaDesplazadaId {get;set;}
    public Guid? PosicionDestinoDesplazadaId {get;set;}
    public string? DestinoDesplazada {get;set;}
    public string Motivo {get;set;}=string.Empty;
    public string? Observaciones {get;set;}
    public decimal? KilometrajeVehiculo {get;set;}
    public Guid? ActividadProgramadaId {get;set;}
    public string Solicitante {get;set;}=string.Empty;
    public string? Aprobador {get;set;}
    public string? MotivoRechazo {get;set;}
    public DateTimeOffset? FechaDecision {get;set;}
    public Guid? MovimientoEjecutadoId {get;set;}
    public DateTimeOffset? FechaRecepcionDestino {get;set;}
}

public enum TipoServicioLlanta { Reparacion, Reencauche, DisposicionFinal }
public sealed class ProveedorServicio : EntidadAuditable
{
    public string Codigo {get;set;}=string.Empty; public string Nombre {get;set;}=string.Empty; public string Tipo {get;set;}=string.Empty;
}
public sealed class OrdenServicioLlanta : EntidadAuditable
{
    public TipoServicioLlanta Tipo {get;set;}
    public string Estado {get;set;}=string.Empty;
    public Guid LlantaId {get;set;} public Llanta Llanta {get;set;}=null!;
    public Guid CentroOrigenId {get;set;} public Centro CentroOrigen {get;set;}=null!;
    public Guid? ProveedorId {get;set;} public ProveedorServicio? Proveedor {get;set;}
    public decimal? Costo {get;set;}
    public string Motivo {get;set;}=string.Empty;
    public string? Observaciones {get;set;}
    public bool Elegible {get;set;}
    public string? CriterioElegibilidad {get;set;}
    public string Solicitante {get;set;}=string.Empty;
    public string? Aprobador {get;set;}
    public DateTimeOffset? FechaEnvio {get;set;}
    public DateTimeOffset? FechaRecepcion {get;set;}
    public ICollection<EvidenciaFlujo> Evidencias {get;set;}=[];
}
public sealed class EvidenciaFlujo : EntidadAuditable
{
    public Guid OrdenServicioLlantaId {get;set;} public OrdenServicioLlanta Orden {get;set;}=null!;
    public string NombreArchivo {get;set;}=string.Empty; public string Ubicacion {get;set;}=string.Empty; public string MimeType {get;set;}=string.Empty; public long TamanoBytes {get;set;} public string Hash {get;set;}=string.Empty;
}

public sealed class CargaMasiva : EntidadAuditable
{
    public string Tipo {get;set;}=string.Empty; public string NombreArchivo {get;set;}=string.Empty; public string Estado {get;set;}="PREVISUALIZADA";
    public int TotalFilas {get;set;} public int FilasValidas {get;set;} public int FilasConError {get;set;}
    public string FilasJson {get;set;}="[]"; public string ErroresJson {get;set;}="[]"; public string Usuario {get;set;}=string.Empty; public DateTimeOffset? FechaProcesamiento {get;set;}
}
