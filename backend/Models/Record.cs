namespace TireControl.Api.Models;

public class Record
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string TireId { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Dimension { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Center { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Observation { get; set; } = string.Empty;
    public string Alert { get; set; } = string.Empty;
    public decimal? DepthExt { get; set; }
    public decimal? DepthCenter { get; set; }
    public decimal? DepthInt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Extra { get; set; } = new();
}
