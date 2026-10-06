using SistemaLlantas.Domain.Common;
namespace SistemaLlantas.Domain.Entities;

public sealed class DespachoDisposicion : EntidadAuditable
{
    public string Codigo { get; set; } = string.Empty;
    public Guid CentroR1Id { get; set; }
    public Centro CentroR1 { get; set; } = null!;
    public Guid ProveedorId { get; set; }
    public ProveedorServicio Proveedor { get; set; } = null!;
    public DateTimeOffset FechaSalida { get; set; }
    public string Transportador { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string? Remision { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "ENVIADO";
    public string IdempotencyKey { get; set; } = string.Empty;
    public string SolicitudHash { get; set; } = string.Empty;
    public DateTimeOffset? FechaCierre { get; set; }
    public string? CerradoPor { get; set; }
    public ICollection<DespachoDisposicionItem> Items { get; set; } = [];
    public ICollection<ActaDisposicion> Actas { get; set; } = [];
}

public sealed class DespachoDisposicionItem : EntidadAuditable
{
    public Guid DespachoId { get; set; }
    public DespachoDisposicion Despacho { get; set; } = null!;
    public Guid OrdenId { get; set; }
    public OrdenServicioLlanta Orden { get; set; } = null!;
    public Guid LoteEntradaId { get; set; }
    public LoteDisposicionFinal LoteEntrada { get; set; } = null!;
}
