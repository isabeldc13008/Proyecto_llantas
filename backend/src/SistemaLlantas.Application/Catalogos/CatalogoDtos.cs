using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Catalogos;

public sealed record CatalogoDto(Guid Id, string Codigo, string Nombre, bool Activo, Guid? PadreId=null, string? PadreNombre=null);
public sealed record GuardarCatalogoDto(string Codigo, string Nombre, Guid? PadreId = null);

public interface ICatalogoService
{
    Task<Pagina<CatalogoDto>> ConsultarAsync(string tipo, ConsultaPaginada consulta, CancellationToken cancellationToken);
    Task<CatalogoDto> CrearAsync(string tipo, GuardarCatalogoDto dto, string usuario, CancellationToken cancellationToken);
    Task<CatalogoDto?> ActualizarAsync(string tipo, Guid id, GuardarCatalogoDto dto, string usuario, CancellationToken cancellationToken);
    Task<bool> CambiarEstadoAsync(string tipo, Guid id, bool activo, string usuario, CancellationToken cancellationToken);
}
