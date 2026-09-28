using SistemaLlantas.Domain.Common;
namespace SistemaLlantas.Domain.Entities;
public sealed class LoteDisposicionFinal:EntidadAuditable
{
 public string Codigo {get;set;}=string.Empty;
 public Guid CentroOrigenId {get;set;} public Centro CentroOrigen {get;set;}=null!;
 public Guid CentroDestinoId {get;set;} public Centro CentroDestino {get;set;}=null!;
 public string? RelevanciaDestino {get;set;}
 public string Estado {get;set;}="EN_TRANSITO";
 public DateTimeOffset FechaSalida {get;set;}
 public DateTimeOffset? FechaRecepcion {get;set;}
 public string? Remision {get;set;} public string? Transportador {get;set;} public string? Observaciones {get;set;}
 public string? Receptor {get;set;} public DateTimeOffset? FechaCierre {get;set;}
 public string IdempotencyKey {get;set;}=string.Empty;
 public ICollection<OrdenServicioLlanta> Ordenes {get;set;}=[];
}
