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
        // Ampliaciones explícitas de las migraciones publicadas posteriores a la captura.
        // No se derivan del modelo bajo prueba: nombres, tipos y nulabilidad siguen verificándose.
        void AddColumn(string table, string name, string type, int length, bool nullable)
            => columns.Add(("dbo", table, name), new[] { "dbo", table, "0", name, type, length.ToString(), "0", "0", nullable ? "1" : "0" });
        // 20260918131215_AddScheduledMountSets
        AddColumn("TBL_SolicitudOperacion", "GGrupoOperacionId", "uniqueidentifier", 16, true);
        // 20260923182748_AddDisposalShipments
        AddColumn("TBL_OrdenServicioLlanta", "DFechaDisposicion", "datetimeoffset", 10, true);
        AddColumn("TBL_OrdenServicioLlanta", "DFechaEvaluacion", "datetimeoffset", 10, true);
        AddColumn("TBL_OrdenServicioLlanta", "GLoteDisposicionFinalId", "uniqueidentifier", 16, true);
        AddColumn("TBL_OrdenServicioLlanta", "SEvaluadoPor", "nvarchar", 300, true);
        foreach (var field in new (string Name, string Type, int Length, bool Nullable)[]
        {
            ("GId", "uniqueidentifier", 16, false), ("SCodigo", "nvarchar", 60, false),
            ("GCentroOrigenId", "uniqueidentifier", 16, false), ("GCentroDestinoId", "uniqueidentifier", 16, false),
            ("SRelevanciaDestino", "nvarchar", 4, true), ("SEstado", "nvarchar", 80, false),
            ("DFechaSalida", "datetimeoffset", 10, false), ("DFechaRecepcion", "datetimeoffset", 10, true),
            ("SRemision", "nvarchar", 200, true), ("STransportador", "nvarchar", 300, true),
            ("SObservaciones", "nvarchar", 2000, true), ("SReceptor", "nvarchar", 300, true),
            ("DFechaCierre", "datetimeoffset", 10, true), ("SIdempotencyKey", "nvarchar", 200, false),
            ("DFechaCreacion", "datetimeoffset", 10, false), ("SUsuarioCreacion", "nvarchar", -1, false),
            ("DFechaModificacion", "datetimeoffset", 10, true), ("SUsuarioModificacion", "nvarchar", -1, true),
            ("BActivo", "bit", 1, false), ("TRowVersion", "timestamp", 8, false)
        }) AddColumn("TBL_LoteDisposicionFinal", field.Name, field.Type, field.Length, field.Nullable);
        // 20261006162816_AddDisposicionSistemaVerde: contrato tomado de la migración.
        AddColumn("TBL_DespachoDisposicion", "Id", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicion", "Codigo", "nvarchar", 100, false);
        AddColumn("TBL_DespachoDisposicion", "CentroR1Id", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicion", "ProveedorId", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicion", "FechaSalida", "datetimeoffset", 10, false);
        AddColumn("TBL_DespachoDisposicion", "Transportador", "nvarchar", 300, false);
        AddColumn("TBL_DespachoDisposicion", "Placa", "nvarchar", 40, false);
        AddColumn("TBL_DespachoDisposicion", "Remision", "nvarchar", 200, true);
        AddColumn("TBL_DespachoDisposicion", "Observaciones", "nvarchar", 2000, true);
        AddColumn("TBL_DespachoDisposicion", "Estado", "nvarchar", 60, false);
        AddColumn("TBL_DespachoDisposicion", "IdempotencyKey", "nvarchar", 200, false);
        AddColumn("TBL_DespachoDisposicion", "SolicitudHash", "nvarchar", 128, false);
        AddColumn("TBL_DespachoDisposicion", "FechaCierre", "datetimeoffset", 10, true);
        AddColumn("TBL_DespachoDisposicion", "CerradoPor", "nvarchar", 300, true);
        AddColumn("TBL_DespachoDisposicion", "FechaCreacion", "datetimeoffset", 10, false);
        AddColumn("TBL_DespachoDisposicion", "UsuarioCreacion", "nvarchar", 300, false);
        AddColumn("TBL_DespachoDisposicion", "FechaModificacion", "datetimeoffset", 10, true);
        AddColumn("TBL_DespachoDisposicion", "UsuarioModificacion", "nvarchar", 300, true);
        AddColumn("TBL_DespachoDisposicion", "Activo", "bit", 1, false);
        AddColumn("TBL_DespachoDisposicion", "RowVersion", "timestamp", 8, false);
        AddColumn("TBL_NovedadDisposicion", "Id", "uniqueidentifier", 16, false);
        AddColumn("TBL_NovedadDisposicion", "LoteId", "uniqueidentifier", 16, false);
        AddColumn("TBL_NovedadDisposicion", "OrdenId", "uniqueidentifier", 16, true);
        AddColumn("TBL_NovedadDisposicion", "Observacion", "nvarchar", 2000, false);
        AddColumn("TBL_NovedadDisposicion", "FechaResolucion", "datetimeoffset", 10, true);
        AddColumn("TBL_NovedadDisposicion", "ResueltaPor", "nvarchar", 300, true);
        AddColumn("TBL_NovedadDisposicion", "Resolucion", "nvarchar", 2000, true);
        AddColumn("TBL_NovedadDisposicion", "FechaCreacion", "datetimeoffset", 10, false);
        AddColumn("TBL_NovedadDisposicion", "UsuarioCreacion", "nvarchar", 300, false);
        AddColumn("TBL_NovedadDisposicion", "FechaModificacion", "datetimeoffset", 10, true);
        AddColumn("TBL_NovedadDisposicion", "UsuarioModificacion", "nvarchar", 300, true);
        AddColumn("TBL_NovedadDisposicion", "Activo", "bit", 1, false);
        AddColumn("TBL_NovedadDisposicion", "RowVersion", "timestamp", 8, false);
        AddColumn("TBL_ActaDisposicion", "Id", "uniqueidentifier", 16, false);
        AddColumn("TBL_ActaDisposicion", "DespachoId", "uniqueidentifier", 16, false);
        AddColumn("TBL_ActaDisposicion", "CentroOrigenId", "uniqueidentifier", 16, false);
        AddColumn("TBL_ActaDisposicion", "Codigo", "nvarchar", 100, false);
        AddColumn("TBL_ActaDisposicion", "SnapshotJson", "nvarchar", -1, false);
        AddColumn("TBL_ActaDisposicion", "FechaCreacion", "datetimeoffset", 10, false);
        AddColumn("TBL_ActaDisposicion", "UsuarioCreacion", "nvarchar", 300, false);
        AddColumn("TBL_ActaDisposicion", "FechaModificacion", "datetimeoffset", 10, true);
        AddColumn("TBL_ActaDisposicion", "UsuarioModificacion", "nvarchar", 300, true);
        AddColumn("TBL_ActaDisposicion", "Activo", "bit", 1, false);
        AddColumn("TBL_ActaDisposicion", "RowVersion", "timestamp", 8, false);
        AddColumn("TBL_DespachoDisposicionItem", "Id", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicionItem", "DespachoId", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicionItem", "OrdenId", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicionItem", "LoteEntradaId", "uniqueidentifier", 16, false);
        AddColumn("TBL_DespachoDisposicionItem", "FechaCreacion", "datetimeoffset", 10, false);
        AddColumn("TBL_DespachoDisposicionItem", "UsuarioCreacion", "nvarchar", 300, false);
        AddColumn("TBL_DespachoDisposicionItem", "FechaModificacion", "datetimeoffset", 10, true);
        AddColumn("TBL_DespachoDisposicionItem", "UsuarioModificacion", "nvarchar", 300, true);
        AddColumn("TBL_DespachoDisposicionItem", "Activo", "bit", 1, false);
        AddColumn("TBL_DespachoDisposicionItem", "RowVersion", "timestamp", 8, false);
        AddColumn("TBL_SoporteActaDisposicion", "Id", "uniqueidentifier", 16, false);
        AddColumn("TBL_SoporteActaDisposicion", "ActaId", "uniqueidentifier", 16, false);
        AddColumn("TBL_SoporteActaDisposicion", "NombreArchivo", "nvarchar", 510, false);
        AddColumn("TBL_SoporteActaDisposicion", "MimeType", "nvarchar", 200, false);
        AddColumn("TBL_SoporteActaDisposicion", "TamanoBytes", "bigint", 8, false);
        AddColumn("TBL_SoporteActaDisposicion", "Hash", "nvarchar", 128, false);
        AddColumn("TBL_SoporteActaDisposicion", "Ubicacion", "nvarchar", 1000, false);
        AddColumn("TBL_SoporteActaDisposicion", "FechaCreacion", "datetimeoffset", 10, false);
        AddColumn("TBL_SoporteActaDisposicion", "UsuarioCreacion", "nvarchar", 300, false);
        AddColumn("TBL_SoporteActaDisposicion", "FechaModificacion", "datetimeoffset", 10, true);
        AddColumn("TBL_SoporteActaDisposicion", "UsuarioModificacion", "nvarchar", 300, true);
        AddColumn("TBL_SoporteActaDisposicion", "Activo", "bit", 1, false);
        AddColumn("TBL_SoporteActaDisposicion", "RowVersion", "timestamp", 8, false);
        AddColumn("TBL_LoteDisposicionFinal", "SPlaca", "nvarchar", 40, true);
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
        Assert.Equal(674,count);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
