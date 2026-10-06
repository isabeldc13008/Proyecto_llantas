using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class DisposicionMigrationTests
{
    [Fact] public void DispatchSchemaProtectsOrderMembershipAndOriginActas()
    {
        using var db = new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SchemaOnly;Integrated Security=True").Options);
        var item = db.Model.GetEntityTypes().SingleOrDefault(x => x.ClrType.Name == "DespachoDisposicionItem");
        Assert.NotNull(item);
        Assert.Contains(item.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(new[]{"OrdenId"}));
        var acta = db.Model.GetEntityTypes().Single(x => x.ClrType.Name == "ActaDisposicion");
        Assert.Contains(acta.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(new[]{"DespachoId", "CentroOrigenId"}));
        Assert.True(db.Model.FindEntityType(typeof(LoteDisposicionFinal))!.FindProperty("Placa")!.IsNullable);
        var sql = db.Database.GenerateCreateScript();
        Assert.Contains("TBL_DespachoDisposicion", sql);
        Assert.Contains("TBL_SoporteActaDisposicion", sql);
    }
}
