using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Dashboard;

public sealed record DashboardMetricasDto(int TotalLlantas,int Montadas,int Disponibles,int AtencionRequerida,int InspeccionesVencidas,int VehiculosIncompletos,int EnReparacion,int EnReencauche,int DisposicionFinal,int ProgramacionesPendientes);
public sealed record DashboardAtencionDto(string Prioridad,string Tipo,string? LlantaCodigo,string? Placa,string Centro,string Descripcion,DateTimeOffset Fecha,string Ruta);
public sealed record DashboardHoyDto(Guid Id,string Tipo,DateTimeOffset Fecha,string Centro,string? Vehiculo,string Estado,string Ruta);
public sealed record DashboardFlotaDto(int VehiculosControlados,int VehiculosCompletos,int VehiculosIncompletos,int VehiculosConAlerta,decimal? PorcentajeCompletos);
public sealed record DashboardDistribucionDto(int Montadas,int Disponibles,int Reparacion,int Reencauche,int Otros);
public sealed record DashboardCentroDto(Guid Id,string Nombre,int Llantas,int Vehiculos,int AlertasCriticas,int InspeccionesVencidas,int Pendientes,string Estado);
public sealed record DashboardResumenDto(DashboardMetricasDto Metrics,IReadOnlyList<DashboardAtencionDto> Attention,IReadOnlyList<DashboardHoyDto> Today,DashboardFlotaDto Fleet,DashboardDistribucionDto TireDistribution,IReadOnlyList<DashboardCentroDto> Centers);

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerAsync(Guid? centroId,AlcanceCentros alcance,CancellationToken ct);
}
