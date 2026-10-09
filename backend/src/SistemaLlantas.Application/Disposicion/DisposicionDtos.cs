using SistemaLlantas.Application.Common;
namespace SistemaLlantas.Application.Disposicion;

public sealed record ConsultaDisposicion(string? Buscar=null, Guid? CentroId=null, string? Estado=null, int PageNumber=1, int PageSize=20, DateTimeOffset? Desde=null, DateTimeOffset? Hasta=null, Guid[]? CentroIds=null, Guid[]? LlantaIds=null, string[]? Estados=null, bool? ConEvidencia=null, string? OrdenarPor=null, bool Descendente=false, bool SoloPendientes=false, string? LlantaTexto=null, string? CentroTexto=null, string? EstadoTexto=null)
{
    public int Pagina=>Math.Max(1,PageNumber);
    public int Tamano=>Math.Clamp(PageSize,1,100);
}
public sealed record ReferenciaDisposicion(Guid Id,string Nombre);
public sealed record ContadorDisposicion(string Clave,string Etiqueta,string Unidad,int Cantidad,string Ruta);
public sealed record AtencionDisposicion(string Codigo,string Centro,string Motivo,DateTimeOffset? Fecha,string Ruta);
public sealed record ResumenDisposicionDto(DateTimeOffset GeneradoEn,IReadOnlyList<ContadorDisposicion> Contadores,IReadOnlyList<AtencionDisposicion> Atencion);
public sealed record OrdenDisposicionDto(Guid OrdenId,Guid LlantaId,string Codigo,string Serial,string Marca,string Referencia,string Dimension,ReferenciaDisposicion CentroOrigen,ReferenciaDisposicion UbicacionRegistrada,string Estado,string Etiqueta,DateTimeOffset? FechaPendiente,string? Responsable,Guid? LoteId,Guid? DespachoId,int Evidencias=0);
public sealed record EventoDisposicion(string Tipo,string Etiqueta,string EstadoVisual,DateTimeOffset? Fecha,string? Responsable,string? Observacion,string? Referencia);
public sealed record EvidenciaDisposicion(Guid Id,string NombreArchivo,string MimeType,long TamanoBytes,DateTimeOffset Fecha);
public sealed record DetalleDisposicionDto(OrdenDisposicionDto Orden,string Motivo,string? Concepto,string? Resultado,IReadOnlyList<EventoDisposicion> Eventos,IReadOnlyList<EvidenciaDisposicion> Evidencias,IReadOnlyList<string> AccionesPermitidas);
public sealed record NovedadDisposicionDto(Guid Id,Guid? OrdenId,string Observacion,DateTimeOffset Fecha,string Responsable,DateTimeOffset? FechaResolucion,string? ResueltaPor,string? Resolucion);
public sealed record LoteDisposicionDetalleDto(Guid Id,string Codigo,ReferenciaDisposicion Origen,ReferenciaDisposicion Destino,string Estado,DateTimeOffset FechaSalida,string? Transportador,string? Placa,string? Remision,string? Observaciones,IReadOnlyList<OrdenDisposicionDto> Items,IReadOnlyList<NovedadDisposicionDto> Novedades,int Enviadas,int Recibidas,int Pendientes,bool VistaParcial,bool PuedeRecibir);
public sealed record CrearDespachoDisposicionDto(Guid CentroR1Id,Guid ProveedorId,IReadOnlyList<Guid> OrdenIds,DateTimeOffset FechaSalida,string Transportador,string Placa,string? Remision,string? Observaciones,string IdempotencyKey);
public sealed record CerrarDespachoDisposicionDto(string RowVersion,string Observaciones);
public sealed record ActaLlanta(Guid OrdenId,string Codigo,string Serial,string Marca,string Dimension,string LoteEntrada);
public sealed record ActaSnapshot(int Version,string Codigo,DateTimeOffset FechaEmision,DateTimeOffset FechaSalida,string CentroOrigen,string CentroR1,string Proveedor,string Transportador,string Placa,string? Remision,IReadOnlyList<ActaLlanta> Llantas);
public sealed record SoporteActaDto(Guid Id,string NombreArchivo,string MimeType,long TamanoBytes,string Hash,DateTimeOffset Fecha,string Responsable);
public sealed record ActaDisposicionDto(Guid Id,string Codigo,Guid CentroOrigenId,ActaSnapshot Snapshot,IReadOnlyList<SoporteActaDto> Soportes);
public sealed record DespachoDisposicionDto(Guid Id,string Codigo,ReferenciaDisposicion CentroR1,ReferenciaDisposicion Proveedor,DateTimeOffset FechaSalida,string Transportador,string Placa,string? Remision,string Estado,string RowVersion,IReadOnlyList<OrdenDisposicionDto> Items,IReadOnlyList<ActaDisposicionDto> Actas,bool VistaParcial,bool PuedeCerrar,IReadOnlyList<string> MotivosBloqueo);
public sealed record ArchivoSoporte(string Ruta,string MimeType,string NombreArchivo);
public sealed record CrearNovedadDto(Guid? OrdenId,string Observacion);
public sealed record ResolverNovedadDto(string Observacion);
public sealed record ValorFiltroDisposicion(string Valor,string Etiqueta);
public sealed record LlantaElegibleDisposicion(Guid Id,string Codigo,string Serial,string Centro,string? Vehiculo,string? Posicion);
public sealed record ItemPropuestaDisposicion(Guid OrdenId,Guid LlantaId);
public sealed record CrearPropuestasDisposicion(IReadOnlyList<ItemPropuestaDisposicion> Items,string Motivo,string? Observacion,string Origen="PROPUESTA",Guid? PosicionOrigenId=null);


