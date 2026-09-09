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
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connection =
            configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
                "Falta ConnectionStrings:SqlServer.");

        var connectionBuilder =
            new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection);

        // SQL credentials belong only to SQL authentication, never to the interactive MFA flow.
        if (connectionBuilder.Authentication is Microsoft.Data.SqlClient.SqlAuthenticationMethod.NotSpecified
            or Microsoft.Data.SqlClient.SqlAuthenticationMethod.SqlPassword)
        {
            if (!string.IsNullOrWhiteSpace(configuration["SqlCredentials:Username"]))
                connectionBuilder.UserID = configuration["SqlCredentials:Username"];
            if (!string.IsNullOrEmpty(configuration["SqlCredentials:Password"]))
                connectionBuilder.Password = configuration["SqlCredentials:Password"];
        }
        connection = connectionBuilder.ConnectionString;

        services.AddDbContext<LlantasDbContext>(options =>
            options.UseSqlServer(
                connection,
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<ILlantaService, LlantaService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<ICicloVidaLlantaService, CicloVidaLlantaService>();
        services.AddScoped<IInspeccionService, InspeccionService>();
        services.AddScoped<IOperacionService, OperacionService>();
        services.AddScoped<IVehiculoService, VehiculoService>();
        services.AddScoped<IProgramacionService, ProgramacionService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}