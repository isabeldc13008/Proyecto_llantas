using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Llantas;

public interface ILlantaService
{
    Task<Pagina<LlantaResumenDto>> ConsultarAsync(ConsultaPaginada consulta, Guid? centroId, CancellationToken cancellationToken);
    Task<LlantaResumenDto?> ObtenerAsync(Guid id, Guid? centroId, CancellationToken cancellationToken);
    Task<LlantaResumenDto> CrearAsync(GuardarLlantaDto dto, string usuario, CancellationToken cancellationToken);
    Task<LlantaResumenDto?> ActualizarAsync(Guid id, GuardarLlantaDto dto, string usuario, CancellationToken cancellationToken);
    Task<bool> CambiarEstadoAsync(Guid id, bool activo, string usuario, CancellationToken cancellationToken);
}
