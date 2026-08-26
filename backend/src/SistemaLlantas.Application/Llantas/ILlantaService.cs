using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Llantas;

public interface ILlantaService
{
    Task<Pagina<LlantaResumenDto>> ConsultarAsync(ConsultaPaginada consulta, AlcanceCentros alcance, CancellationToken cancellationToken);
    Task<LlantaMetricasDto> MetricasAsync(ConsultaPaginada consulta,AlcanceCentros alcance,CancellationToken cancellationToken);
    Task<IReadOnlyList<LlantaResumenDto>> ExportarAsync(ConsultaPaginada consulta, AlcanceCentros alcance, CancellationToken cancellationToken);
    Task<LlantaResumenDto?> ObtenerAsync(Guid id, AlcanceCentros alcance, CancellationToken cancellationToken);
    Task<LlantaResumenDto> CrearAsync(GuardarLlantaDto dto, string usuario, AlcanceCentros alcance, CancellationToken cancellationToken);
    Task<LlantaResumenDto?> ActualizarAsync(Guid id, GuardarLlantaDto dto, string usuario, AlcanceCentros alcance, CancellationToken cancellationToken);
    Task<bool> CambiarEstadoAsync(Guid id, bool activo, string usuario, AlcanceCentros alcance, CancellationToken cancellationToken);
}
