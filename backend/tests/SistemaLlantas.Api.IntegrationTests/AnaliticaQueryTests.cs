using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

/// <summary>Compiles real SQL Server queries, but substitutes readers. Not a SQL execution/integration test.</summary>
public sealed class AnaliticaQueryTests
{
    private static LlantasDbContext Context(SqlProbe probe) => new(new DbContextOptionsBuilder<LlantasDbContext>()
        .UseSqlServer("Server=localhost;Database=AnaliticaQueryOnly;Integrated Security=True;TrustServerCertificate=True")
        .AddInterceptors(probe, new NoConnection()).Options);

    [Theory]
    [InlineData("resumen")] [InlineData("marca")] [InlineData("centro")] [InlineData("posiciones")]
    [InlineData("movimientos")] [InlineData("opciones")]
    public async Task QueriesTranslateWithoutClientEvaluatingHistory(string endpoint)
    {
        var probe = new SqlProbe(endpoint is "resumen" or "marca" or "centro");
        await using var db = Context(probe);var service = new AnaliticaService(db);
        var scope = new AlcanceCentros(false, [Guid.NewGuid()]);var filter = new FiltroAnalitica { TipoVehiculo = "Camión" };
        switch (endpoint)
        {
            case "resumen": var summary = await service.ResumenAsync(filter, scope, default);Assert.Equal(1, summary.Total);Assert.Equal(1, summary.SinKm);Assert.Null(summary.EnOperacion.Promedio);break;
            case "marca": case "centro": var groups = await service.CompararAsync(filter, endpoint, scope, default);Assert.Single(groups.Items);Assert.False(groups.Items[0].MuestraSuficiente);break;
            case "posiciones": await service.PosicionesAsync(filter, scope, default);break;
            case "movimientos": await service.MovimientosAsync(filter, scope, default);break;
            case "opciones": await service.OpcionesAsync(scope, default);break;
        }
        Assert.NotEmpty(probe.Commands);
        Assert.All(probe.Commands, sql => Assert.Contains("SELECT", sql));
        Assert.True(probe.Commands.Count <= 8, "Query count must not grow with the tire count.");
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact] public void ScopeAndCohortFiltersAreInSqlBeforeAggregation()
    {
        using var db = Context(new(false));var service = new AnaliticaService(db);var center = Guid.NewGuid();
        var filter = new FiltroAnalitica { CentroId = center, MarcaId = Guid.NewGuid(), ReferenciaId = Guid.NewGuid(), DimensionId = Guid.NewGuid(), EstadoId = Guid.NewGuid(), IngresoDesde = new(2026, 1, 1), IngresoHasta = new(2026, 9, 17) };
        var scope = new AlcanceCentros(false, [center]);
        var sql = service.Llantas(filter, scope).ToQueryString();
        foreach (var name in new[] { "CentroId", "MarcaId", "ReferenciaId", "DimensionId", "Estado", "Ingreso" }) Assert.Contains(name, sql);
        Assert.Contains("WHERE", sql);Assert.Contains(center.ToString(), sql);
        Assert.Contains("Movimiento", service.Asignaciones(filter, scope).ToQueryString());
        Assert.Contains("CentroDestino", service.Detalles(filter, scope).ToQueryString());
        Assert.Contains("WHERE", service.Llantas(new(), new(false, [])).ToQueryString());
    }

    [Fact] public async Task OversizedCohortIsRejectedRatherThanSilentlySampled()
    {
        var probe = new SqlProbe(true, AnaliticaService.MaximoCohorte + 1);
        await using var db = Context(probe);
        var error = await Assert.ThrowsAsync<ValidacionException>(() => new AnaliticaService(db).ResumenAsync(new(), new(true, []), default));
        Assert.Contains("no se ha tomado una muestra parcial", error.Message);Assert.Single(probe.Commands);
    }
    [Fact] public async Task EmptyCohortPreservesNullMetricsAndDoesNotReadHistory()
    {
        var probe = new SqlProbe(false);await using var db = Context(probe);
        var summary = await new AnaliticaService(db).ResumenAsync(new(), new(false, []), default);
        Assert.Equal(0, summary.Total);Assert.Null(summary.MovimientosPromedio);Assert.Null(summary.ProfundidadPromedio);
        Assert.Null(summary.EnOperacion.Mediana);Assert.Single(probe.Commands);
    }
    private sealed class NoConnection : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(InterceptionResult.Suppress());
    }
    private sealed class SqlProbe(bool supplyTire, int tireCount = 1) : DbCommandInterceptor
    {
        public List<string> Commands { get; } = [];
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);var table = new DataTable();
            if (supplyTire && Commands.Count == 1)
            {
                object[] values = [Guid.NewGuid(), "TEST", "SERIAL", Guid.NewGuid(), "Marca", Guid.NewGuid(), "Referencia", Guid.NewGuid(), "Dimensión", Guid.NewGuid(), "Centro", "Disponible", "DISPONIBLE", false, false, true, 10m];
                for (var i = 0; i < values.Length; i++) table.Columns.Add("c" + i, values[i].GetType());
                for (var i = 0; i < tireCount; i++) { values[0] = Guid.NewGuid();table.Rows.Add(values); }
            }
            else if (command.CommandText.StartsWith("SELECT COUNT(*)")) { table.Columns.Add("count", typeof(int));table.Rows.Add(0); }
            return ValueTask.FromResult(InterceptionResult<DbDataReader>.SuppressWithResult(table.CreateDataReader()));
        }
    }
}
