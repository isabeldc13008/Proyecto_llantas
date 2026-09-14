using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class ExistingDatabaseSchemaTests
{
    [Fact]
    public void EveryMappedColumnMatchesProvidedDatabaseAndSnapshot()
    {
        using var db = new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>()
            .UseSqlServer("Server=localhost;Database=MetadataOnly;Integrated Security=true").Options);
        var columns = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory,"Schema","GDLLSQLDLLO.columns.tsv"))
            .Skip(1).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Split('\t'))
            .ToDictionary(x => (Schema:x[0],Table:x[1],Column:x[3]));
        // La captura base precede las migraciones de contraseña y parametrización de alertas.
        columns.Add(("dbo", "TBL_Usuario", "BDebeCambiarClave"), new[] { "dbo", "TBL_Usuario", "13", "BDebeCambiarClave", "bit", "1", "1", "0", "0" });
        foreach(var field in new[]{("SNombre","nvarchar","300","0"),("STipo","nvarchar","100","0"),("SDescripcion","nvarchar","2000","0"),("SOperador","nvarchar","4","0"),("SPrioridad","nvarchar","20","0"),("GCentroId","uniqueidentifier","16","1")})
            columns.Add(("dbo","TBL_ParametroAlerta",field.Item1),new[]{"dbo","TBL_ParametroAlerta","0",field.Item1,field.Item2,field.Item3,"0","0",field.Item4});
        var count = 0;
        foreach (var entity in db.Model.GetEntityTypes())
        {
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            foreach (var property in entity.GetProperties())
            {
                var key = (entity.GetSchema()!, entity.GetTableName()!, property.GetColumnName(table)!);
                Assert.True(columns.TryGetValue(key,out var column), $"Unknown column {key}");
                Assert.Equal(column![8] == "1", property.IsNullable);
                var sqlType = column[4];
                if (new[]{"nvarchar","varchar","varbinary","char","nchar"}.Contains(sqlType))
                {
                    var size = int.Parse(column[5]);
                    sqlType += "(" + (size == -1 ? "max" : (size / (sqlType is "nvarchar" or "nchar" ? 2 : 1)).ToString()) + ")";
                }
                if (sqlType is "decimal" or "numeric") sqlType += $"({column[6]},{column[7]})";
                Assert.Equal(sqlType, property.GetColumnType()!.Replace(" ", "").Replace("rowversion","timestamp"));
                count++;
            }
        }
        Assert.Equal(581,count);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
