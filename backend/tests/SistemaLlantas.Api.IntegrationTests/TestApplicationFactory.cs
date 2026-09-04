using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SistemaLlantas.Api;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string database = "SistemaLlantas_Test_" + Guid.NewGuid().ToString("N");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION")
            ?? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True") { InitialCatalog = database };
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LlantasDbContext>>();
            services.AddDbContext<LlantasDbContext>(options => options.UseSqlServer(connection.ConnectionString, sql => sql.EnableRetryOnFailure()));
        });
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:SqlServer"] = connection.ConnectionString,
            ["Authentication:Mode"] = "Local", ["Authentication:SeedDevelopmentUsers"] = "false"
        }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedAsync(host.Services).GetAwaiter().GetResult();
        return host;
    }

    private static async Task SeedAsync(IServiceProvider services)
    {
        await DevelopmentSecuritySeeder.SeedAsync(services);
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        db.Centros.AddRange(new Centro { Codigo = "8092", Nombre = "Centro prueba 1" }, new Centro { Codigo = "8279", Nombre = "Centro prueba 2" });
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "seed-operational-data.sql")));
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        if (db.Database.GetDbConnection().Database == database)
            await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}


