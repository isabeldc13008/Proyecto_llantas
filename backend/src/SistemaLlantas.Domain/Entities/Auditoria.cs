namespace SistemaLlantas.Domain.Entities;

public sealed class Auditoria
{
    public long Id { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public string Usuario { get; set; } = "sistema";
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string Identificador { get; set; } = string.Empty;
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? DireccionIp { get; set; }
    public string Origen { get; set; } = "API";
}
