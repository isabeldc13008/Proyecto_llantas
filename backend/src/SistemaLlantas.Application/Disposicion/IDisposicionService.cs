using SistemaLlantas.Application.Common;
namespace SistemaLlantas.Application.Disposicion;

public interface IDisposicionService
{
    Task<ResumenDisposicionDto> ResumenAsync(ConsultaDisposicion filtro,AlcanceCentros alcance,CancellationToken ct);
    Task<Pagina<OrdenDisposicionDto>> OrdenesAsync(ConsultaDisposicion filtro,AlcanceCentros alcance,CancellationToken ct);
    Task<DetalleDisposicionDto?> DetalleAsync(Guid ordenId,AlcanceCentros alcance,CancellationToken ct);
    Task<Pagina<LoteDisposicionDetalleDto>> LotesAsync(ConsultaDisposicion filtro,AlcanceCentros alcance,CancellationToken ct);
    Task<LoteDisposicionDetalleDto?> LoteAsync(Guid id,AlcanceCentros alcance,CancellationToken ct);
    Task<Pagina<OrdenDisposicionDto>> DisponiblesAsync(ConsultaDisposicion filtro,Guid centroR1Id,AlcanceCentros alcance,CancellationToken ct);
    Task<DespachoDisposicionDto> CrearDespachoAsync(CrearDespachoDisposicionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<Pagina<DespachoDisposicionDto>> DespachosAsync(ConsultaDisposicion filtro,AlcanceCentros alcance,CancellationToken ct);
    Task<DespachoDisposicionDto?> DespachoAsync(Guid id,AlcanceCentros alcance,CancellationToken ct);
    Task<DespachoDisposicionDto> CerrarDespachoAsync(Guid id,CerrarDespachoDisposicionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<SoporteActaDto> AdjuntarSoporteAsync(Guid actaId,Stream contenido,string nombreArchivo,string mimeType,long tamanoBytes,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task EliminarSoporteAsync(Guid soporteId,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<ArchivoSoporte?> ArchivoAsync(Guid id,AlcanceCentros alcance,CancellationToken ct);
    Task<ActaDisposicionDto?> ActaAsync(Guid id,AlcanceCentros alcance,CancellationToken ct);
    Task RegistrarNovedadAsync(Guid loteId,CrearNovedadDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task ResolverNovedadAsync(Guid id,ResolverNovedadDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
}
