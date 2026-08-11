using SistemaLlantas.Domain.Common;

namespace SistemaLlantas.Domain.Entities;

public sealed class Llanta : EntidadAuditable
{
    public string Codigo { get; private set; } = string.Empty;
    public string Serial { get; private set; } = string.Empty;
    public Guid MarcaId { get; set; }
    public Marca Marca { get; set; } = null!;
    public Guid ReferenciaId { get; set; }
    public Referencia Referencia { get; set; } = null!;
    public Guid DimensionId { get; set; }
    public Dimension Dimension { get; set; } = null!;
    public Guid TipoLlantaId { get; set; }
    public TipoLlanta TipoLlanta { get; set; } = null!;
    public Guid EstadoLlantaId { get; set; }
    public EstadoLlanta EstadoLlanta { get; set; } = null!;
    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;
    public string UbicacionActual { get; set; } = string.Empty;
    public DateOnly? FechaCompra { get; set; }
    public decimal? Costo { get; set; }
    public decimal KilometrajeAcumulado { get; set; }
    public int NumeroReencauches { get; set; }
    public decimal ProfundidadInicial { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public string? Observaciones { get; set; }

    private Llanta() { }

    public Llanta(string codigo, string serial)
    {
        CambiarIdentificacion(codigo, serial);
        FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public void CambiarIdentificacion(string codigo, string serial)
    {
        Codigo = NormalizarRequerido(codigo, nameof(codigo), 50);
        Serial = NormalizarRequerido(serial, nameof(serial), 100);
    }

    private static string NormalizarRequerido(string valor, string campo, int maximo)
    {
        var resultado = valor?.Trim().ToUpperInvariant() ?? string.Empty;
        if (resultado.Length == 0) throw new ArgumentException($"{campo} es obligatorio.", campo);
        if (resultado.Length > maximo) throw new ArgumentException($"{campo} supera {maximo} caracteres.", campo);
        return resultado;
    }
}
