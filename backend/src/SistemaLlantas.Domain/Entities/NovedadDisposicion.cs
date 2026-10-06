using SistemaLlantas.Domain.Common;
namespace SistemaLlantas.Domain.Entities;

public sealed class NovedadDisposicion : EntidadAuditable
{
    public Guid LoteId { get; set; }
    public LoteDisposicionFinal Lote { get; set; } = null!;
    public Guid? OrdenId { get; set; }
    public OrdenServicioLlanta? Orden { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public DateTimeOffset? FechaResolucion { get; set; }
    public string? ResueltaPor { get; set; }
    public string? Resolucion { get; set; }
}
