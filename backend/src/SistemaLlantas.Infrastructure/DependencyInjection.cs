using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Application.Catalogos;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Vehiculos;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Application.Dashboard;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("Falta ConnectionStrings:SqlServer.");
        services.AddDbContext<LlantasDbContext>(o => o.UseSqlServer(connection, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<ILlantaService, LlantaService>(); services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<ICicloVidaLlantaService, CicloVidaLlantaService>();
        services.AddScoped<IInspeccionService, InspeccionService>();
        services.AddScoped<IOperacionService, OperacionService>();
        services.AddScoped<IVehiculoService, VehiculoService>();
        services.AddScoped<IProgramacionService, ProgramacionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
