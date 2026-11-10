using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Analitica;

public sealed record TotalesCalidad(int Total, int Montadas, int Disponibles, int ConAlertas, int ConInspeccion, int UltimaCompleta)
{
    public int SinInspeccion => Total - ConInspeccion;
    public int UltimaIncompleta => ConInspeccion - UltimaCompleta;
    public decimal? CoberturaInspeccion => Total > 0 ? Math.Round(100m * ConInspeccion / Total, 2) : null;
}

public sealed record GrupoCalidad(Guid Id, string Nombre, int Total, int ConInspeccion);
public sealed record LlantaCalidad(Guid Id, string Codigo, string Centro, string Estado, Guid? InspeccionId,
    DateTimeOffset? FechaRegistro, bool UltimaCompleta, int AlertasActivas);
public sealed record CalidadDatosAnalitica(DateTimeOffset GeneradoEn, string Procedencia, TotalesCalidad Totales,
    IReadOnlyList<GrupoCalidad> Estados, IReadOnlyList<GrupoCalidad> Centros, Pagina<LlantaCalidad> Llantas);
