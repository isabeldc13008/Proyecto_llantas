using SistemaLlantas.Domain.Common;

namespace SistemaLlantas.Domain.Entities;

public abstract class CatalogoBase : EntidadAuditable
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public sealed class Marca : CatalogoBase { public ICollection<Referencia> Referencias { get; set; } = []; }
public sealed class Referencia : CatalogoBase { public Guid MarcaId { get; set; } public Marca Marca { get; set; } = null!; }
public sealed class Dimension : CatalogoBase { }
public sealed class TipoLlanta : CatalogoBase { }
public sealed class EstadoLlanta : CatalogoBase { public bool EsDisposicionFinal { get; set; } public bool PermiteMontaje { get; set; } }
public sealed class Centro : CatalogoBase { public string? Relevancia { get; set; } }
