namespace SistemaLlantas.Domain.Common;

public abstract class EntidadAuditable
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    public string UsuarioCreacion { get; set; } = "sistema";
    public DateTimeOffset? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public bool Activo { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
