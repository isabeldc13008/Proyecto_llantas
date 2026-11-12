using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Persistence.Migrations;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class OrientacionMigrationTests
{
    [Fact] public void SoloAgregaTresColumnasOpcionalesSinCambiarInspecciones()
    {
        var migration=new AddTireTurnEvidence();Assert.Equal(3,migration.UpOperations.Count);
        Assert.All(migration.UpOperations,operation=>{var column=Assert.IsType<AddColumnOperation>(operation);Assert.Equal("TBL_Movimiento",column.Table);Assert.True(column.IsNullable);Assert.Null(column.DefaultValue);Assert.Contains(column.Name,new[]{"DFechaGiro","NKilometrajeGiro","GTecnicoGiroId"});});
        using var db=new OfflineDesignFactory().CreateDbContext([]);Assert.False(db.Database.HasPendingModelChanges());Assert.Equal(System.Data.ConnectionState.Closed,db.Database.GetDbConnection().State);
    }
}
