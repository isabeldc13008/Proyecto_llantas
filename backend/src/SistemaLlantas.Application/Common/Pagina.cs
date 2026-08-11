namespace SistemaLlantas.Application.Common;

public sealed record Pagina<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalItems)
{
    public int TotalPages => TotalItems == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public sealed record ConsultaPaginada(int PageNumber = 1, int PageSize = 20, string? Search = null, bool? Activo = null)
{
    public int Pagina => Math.Max(1, PageNumber);
    public int Tamano => Math.Clamp(PageSize, 1, 100);
}
