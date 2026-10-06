using SistemaLlantas.Domain.Common;
namespace SistemaLlantas.Domain.Entities;

public sealed class ActaDisposicion : EntidadAuditable
{
    public Guid DespachoId { get; set; }
    public DespachoDisposicion Despacho { get; set; } = null!;
    public Guid CentroOrigenId { get; set; }
    public Centro CentroOrigen { get; set; } = null!;
    public string Codigo { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = string.Empty;
    public ICollection<SoporteActaDisposicion> Soportes { get; set; } = [];
}

public sealed class SoporteActaDisposicion : EntidadAuditable
{
    public Guid ActaId { get; set; }
    public ActaDisposicion Acta { get; set; } = null!;
    public string NombreArchivo { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
}
