namespace SistemaLlantas.Application.Common;

public sealed class ConflictoException(string mensaje) : Exception(mensaje);
public sealed class ValidacionException(string mensaje, IReadOnlyDictionary<string, string[]>? errores = null) : Exception(mensaje)
{
    public IReadOnlyDictionary<string, string[]> Errores { get; } = errores ?? new Dictionary<string, string[]>();
}
