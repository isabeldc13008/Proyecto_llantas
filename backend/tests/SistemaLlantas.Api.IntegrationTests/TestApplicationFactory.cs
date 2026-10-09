using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SistemaLlantas.Api;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string database =
        "SistemaLlantas_Test_" + Guid.NewGuid().ToString("N");

    private bool schemaPrepared;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuredConnection =
            Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION");

        if (
            string.Equals(
                Environment.GetEnvironmentVariable("CI"),
                "true",
                StringComparison.OrdinalIgnoreCase
            )
            && string.IsNullOrWhiteSpace(configuredConnection)
        )
        {
            throw new InvalidOperationException(
                "TEST_SQL_CONNECTION is required in CI."
            );
        }

        var connection = new SqlConnectionStringBuilder(
            configuredConnection
                ?? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True"
        )
        {
            InitialCatalog = database
        };

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LlantasDbContext>>();

            services.AddDbContext<LlantasDbContext>(options =>
                options.UseSqlServer(
                    connection.ConnectionString,
                    sql => sql.EnableRetryOnFailure()
                )
            );
        });

        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SqlServer"] = connection.ConnectionString,
                    ["Authentication:Mode"] = "Local",
                    ["Authentication:SeedDevelopmentUsers"] = "false"
                }
            )
        );
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedAsync(host.Services).GetAwaiter().GetResult();
        return host;
    }

    private async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<LlantasDbContext>();

        if (!db.Database.GetDbConnection().Database.StartsWith(
            "SistemaLlantas_Test_",
            StringComparison.Ordinal
        ))
        {
            throw new InvalidOperationException(
                "La preparación de esquema solo puede ejecutarse " +
                "en una base temporal de pruebas."
            );
        }

        if (!schemaPrepared)
        {
            // 1. Aplicar las migraciones históricas.
            var migrator = db.GetService<IMigrator>();
    
            await migrator.MigrateAsync(
                "20260911160835_AddAlertRuleConfiguration"
            );
    
            // 2. Retirar los índices filtrados que bloquean los renombres.
            await db.Database.ExecuteSqlRawAsync("""
                DROP INDEX IF EXISTS
                    [IX_TBL_ActividadProgramada_TecnicoUsuarioId_VehiculoId_TipoActividad_FechaProgramada]
                    ON dbo.TBL_ActividadProgramada;
    
                DROP INDEX IF EXISTS
                    [IX_TBL_ActividadProgramada_IdempotencyKey]
                    ON dbo.TBL_ActividadProgramada;
    
                DROP INDEX IF EXISTS
                    [UX_Asignacion_LlantaActiva]
                    ON dbo.TBL_AsignacionLlantaPosicion;
    
                DROP INDEX IF EXISTS
                    [UX_Asignacion_PosicionActiva]
                    ON dbo.TBL_AsignacionLlantaPosicion;
                """);
    
            // 3. Adaptar las columnas a la nomenclatura actual.
            await AdaptarColumnasAsync(db);
    
            // 4. Restaurar los índices con columnas y filtros actualizados.
            // El primer índice usa el nombre esperado por AddScheduledMountSets.
            await db.Database.ExecuteSqlRawAsync("""
                CREATE UNIQUE INDEX
                    [IX_ActividadProgramada_GTecnicoUsuarioId_GVehiculoId_STipoActividad_DFechaProgramada]
                ON dbo.TBL_ActividadProgramada
                    (
                        GTecnicoUsuarioId,
                        GVehiculoId,
                        STipoActividad,
                        DFechaProgramada
                    )
                WHERE BActivo = 1
                  AND NEstado <> 4
                  AND GTecnicoUsuarioId IS NOT NULL;
    
                CREATE UNIQUE INDEX
                    [IX_TBL_ActividadProgramada_IdempotencyKey]
                ON dbo.TBL_ActividadProgramada (SIdempotencyKey)
                WHERE SIdempotencyKey IS NOT NULL;
    
                CREATE UNIQUE INDEX [UX_Asignacion_LlantaActiva]
                ON dbo.TBL_AsignacionLlantaPosicion (GLlantaId)
                WHERE BEsActiva = 1;
    
                CREATE UNIQUE INDEX [UX_Asignacion_PosicionActiva]
                ON dbo.TBL_AsignacionLlantaPosicion (GPosicionVehiculoId)
                WHERE BEsActiva = 1;
                """);
    
            // 5. Completar las migraciones.
            await db.Database.MigrateAsync();
    
            // Adaptar cualquier columna histórica restante.
            await AdaptarColumnasAsync(db);
    
            // 6. Insertar los centros con la nomenclatura actual.
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO dbo.TBL_Centro
                    (
                        GId,
                        SCodigo,
                        SNombre,
                        DFechaCreacion,
                        SUsuarioCreacion,
                        BActivo
                    )
                VALUES
                    (
                        NEWID(), N'8092', N'Centro prueba 1',
                        SYSDATETIMEOFFSET(), N'sistema', 1
                    ),
                    (
                        NEWID(), N'8279', N'Centro prueba 2',
                        SYSDATETIMEOFFSET(), N'sistema', 1
                    );
                """);
    
            // 7. Cargar el archivo SQL actualizado con prefijos.
            var seedPath = Path.Combine(
                AppContext.BaseDirectory,
                "seed-operational-data.sql"
            );
    
            await db.Database.ExecuteSqlRawAsync(
                await File.ReadAllTextAsync(seedPath)
            );
            schemaPrepared = true;
        }

        // 8. Crear los usuarios de prueba.
        await DevelopmentSecuritySeeder.SeedAsync(services);

        await CompletarDatosDePruebaAsync(db);
    }

    private static async Task CompletarDatosDePruebaAsync(LlantasDbContext db)
    {
        var centros = await db.Centros.Where(x => x.Activo).ToListAsync();
        var rolTecnico = await db.RolesSistema.SingleAsync(x => x.Codigo == "TECNICO");

        // Conservar el usuario "tecnico" sin centros para verificar la seguridad.
        // La programación utiliza un usuario exclusivo de pruebas.
        var tecnico = await db.UsuariosSistema
            .Include(x => x.Centros)
            .SingleOrDefaultAsync(x => x.Username == "tecnico-pruebas");

        if (tecnico is null)
        {
            tecnico = new UsuarioSistema
            {
                Username = "tecnico-pruebas",
                Nombre = "Técnico de programación de pruebas",
                RolId = rolTecnico.Id,
                Activo = true,
                UsuarioCreacion = "seed-integration-tests"
            };
            tecnico.PasswordHash = new PasswordHasher<UsuarioSistema>()
                .HashPassword(tecnico, "Pruebas-Locales-2026!");
            db.UsuariosSistema.Add(tecnico);
            await db.SaveChangesAsync();
        }

        foreach (var centro in centros)
        {
            var asignacion = tecnico.Centros.SingleOrDefault(x => x.CentroId == centro.Id);
            if (asignacion is null)
            {
                db.UsuariosCentros.Add(new UsuarioCentro
                {
                    UsuarioId = tecnico.Id,
                    CentroId = centro.Id,
                    Activo = true,
                    UsuarioCreacion = "seed-integration-tests"
                });
            }
            else
            {
                asignacion.Activo = true;
            }
        }

        // Las llantas del SQL están montadas. Crear también inventario disponible.
        var muestra = await db.Llantas.AsNoTracking().FirstAsync();
        var disponible = await db.EstadosLlanta.FirstAsync(x =>
            x.Activo && x.PermiteMontaje && !x.EsDisposicionFinal &&
            (x.Codigo == "DIS" || x.Codigo == "DISPONIBLE"));

        foreach (var centro in centros)
        {
            for (var numero = 1; numero <= 3; numero++)
            {
                var codigo = $"TEST-DISP-{centro.Codigo}-{numero}";
                if (await db.Llantas.IgnoreQueryFilters().AnyAsync(x => x.Codigo == codigo))
                    continue;

                db.Llantas.Add(new Llanta(codigo, "SER-" + codigo)
                {
                    CentroId = centro.Id,
                    MarcaId = muestra.MarcaId,
                    ReferenciaId = muestra.ReferenciaId,
                    DimensionId = muestra.DimensionId,
                    TipoLlantaId = muestra.TipoLlantaId,
                    EstadoLlantaId = disponible.Id,
                    ProfundidadInicial = 16,
                    UbicacionActual = "Inventario",
                    UsuarioCreacion = "seed-integration-tests"
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task AdaptarColumnasAsync(LlantasDbContext db)
    {
        var renames = new List<string>();

        static string Identifier(string value) =>
            "[" + value.Replace("]", "]]") + "]";

        static string Literal(string value) =>
            "N'" + value.Replace("'", "''") + "'";

        foreach (var entity in db.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();

            if (tableName is null)
                continue;

            var schema = entity.GetSchema() ?? "dbo";

            var table = StoreObjectIdentifier.Table(
                tableName,
                entity.GetSchema()
            );

            var qualifiedTable =
                Identifier(schema) + "." + Identifier(tableName);

            var tableLookup = schema + "." + tableName;

            foreach (var property in entity.GetProperties())
            {
                var column = property.GetColumnName(table);

                if (column is null || column == property.Name)
                    continue;

                renames.Add(
                    $"IF COL_LENGTH({Literal(tableLookup)}, " +
                    $"{Literal(column)}) IS NULL " +
                    $"AND COL_LENGTH({Literal(tableLookup)}, " +
                    $"{Literal(property.Name)}) IS NOT NULL " +
                    $"EXEC sys.sp_rename " +
                    $"{Literal(qualifiedTable + "." + Identifier(property.Name))}, " +
                    $"{Literal(column)}, N'COLUMN';"
                );
            }
        }

        if (renames.Count > 0)
        {
            var timeoutAnterior = db.Database.GetCommandTimeout();

            try
            {
                db.Database.SetCommandTimeout(180);

                foreach (var bloque in renames.Distinct().Chunk(25))
                {
                    await db.Database.ExecuteSqlRawAsync(
                        string.Join(Environment.NewLine, bloque)
                    );
                }
            }
            finally
            {
                db.Database.SetCommandTimeout(timeoutAnterior);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<LlantasDbContext>();

        if (db.Database.GetDbConnection().Database == database)
        {
            await db.Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }
}
