using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
        var configuredConnection = Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION");
        if (string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(configuredConnection))
            throw new InvalidOperationException("TEST_SQL_CONNECTION is required in CI.");
        var connection = new SqlConnectionStringBuilder(configuredConnection
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
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        if (!db.Database.GetDbConnection().Database.StartsWith("SistemaLlantas_Test_", StringComparison.Ordinal))
            throw new InvalidOperationException("La preparación de esquema solo puede ejecutarse en una base temporal de pruebas.");
        await db.Database.MigrateAsync();
        // Las migraciones y el seed SQL histórico usan nombres CLR sin prefijos.
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO dbo.TBL_Centro (Id, Codigo, Nombre, FechaCreacion, UsuarioCreacion, Activo)
            VALUES (NEWID(), N'8092', N'Centro prueba 1', SYSDATETIMEOFFSET(), N'sistema', 1),
                   (NEWID(), N'8279', N'Centro prueba 2', SYSDATETIMEOFFSET(), N'sistema', 1);
            """);
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "seed-operational-data.sql")));
        // Reproducir en LocalDB los nombres de la BD existente antes de consultar con EF.
        // No se cambian migraciones publicadas ni el mapeo funcional de la aplicación.
        var renames = new List<string>();
        static string Identifier(string value) => "[" + value.Replace("]", "]]") + "]";
        static string Literal(string value) => "N'" + value.Replace("'", "''") + "'";
        foreach (var entity in db.Model.GetEntityTypes())
        {
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            var tableName = Identifier(table.Schema ?? "dbo") + "." + Identifier(table.Name);
            foreach (var property in entity.GetProperties())
            {
                var column = property.GetColumnName(table)!;
                if (column == property.Name) continue;
                // COL_LENGTH receives an object name, not a bracketed SQL identifier.
                // Keeping brackets only in sp_rename prevents the prefixed schema
                // columns (SCodigo, GId, ...) from being missed in LocalDB.
                var tableLookup = $"{table.Schema}.{table.Name}";
                renames.Add($"IF COL_LENGTH({Literal(tableLookup)}, {Literal(column)}) IS NULL AND COL_LENGTH({Literal(tableLookup)}, {Literal(property.Name)}) IS NOT NULL " +
                    $"EXEC sys.sp_rename {Literal(tableName + "." + Identifier(property.Name))}, {Literal(column)}, N'COLUMN';");
            }
        }
        await db.Database.ExecuteSqlRawAsync(string.Join(Environment.NewLine, renames));
        await DevelopmentSecuritySeeder.SeedAsync(services);
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

