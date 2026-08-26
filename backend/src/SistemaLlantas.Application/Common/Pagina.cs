namespace SistemaLlantas.Application.Common;

public sealed record Pagina<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalItems)
{
    public int TotalPages => TotalItems == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public sealed record ConsultaPaginada(int PageNumber = 1, int PageSize = 20, string? Search = null, bool? Activo = null, string? SortBy = null, string? SortDirection = null, Guid? CentroId = null, string? Estado = null, string? CentroIds = null, string? Estados = null, decimal? ProfundidadMin = null, decimal? ProfundidadMax = null, Guid? MarcaId=null,Guid? ReferenciaId=null,Guid? DimensionId=null,Guid? TipoLlantaId=null,bool? TieneReencauches=null,int? ReencauchesMin=null,bool? TieneReparaciones=null,string? Vehiculo=null,decimal? KilometrajeMin=null,decimal? KilometrajeMax=null,DateTimeOffset? InspeccionDesde=null,DateTimeOffset? InspeccionHasta=null,bool? RequiereAtencion=null,string? MarcaIds=null,string? ReferenciaIds=null,string? DimensionIds=null,string? TipoLlantaIds=null,string? Reencauches=null)
{
    public int Pagina => Math.Max(1, PageNumber);
    public int Tamano => Math.Clamp(PageSize, 1, 100);
}
