using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Persistence.Migrations;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class InspectionRetreadReadTests
{
    [Theory][InlineData(0,1,false)][InlineData(1,2,true)][InlineData(2,3,true)]
    public void ConsultaSerializaMarcaYVidaSinConfundirContador(int contador,long vida,bool reencauchada)
    {
        var marca=reencauchada?Guid.NewGuid():(Guid?)null;
        var dto=new LlantaPosicionDto(Guid.NewGuid(),"L","Montada","Original","R","D")
        {NumeroReencauches=contador,MarcaReencaucheId=marca,MarcaReencauche=reencauchada?"Reencauche":null};
        var json=JsonSerializer.SerializeToElement(dto,new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(vida,json.GetProperty("vida").GetInt64());
        Assert.Equal(reencauchada,json.GetProperty("esReencauchada").GetBoolean());
        Assert.Equal(contador,json.GetProperty("numeroReencauches").GetInt32());
        Assert.Equal(marca,json.GetProperty("marcaReencaucheId").ValueKind==JsonValueKind.Null?null:json.GetProperty("marcaReencaucheId").GetGuid());
        Assert.Equal(dto.MarcaReencauche,json.GetProperty("marcaReencauche").GetString());
        var busqueda=new LlantaBusquedaInspeccionDto(dto.Id,"L","S","Original","R","D","Montada")
        {NumeroReencauches=contador,MarcaReencaucheId=marca,MarcaReencauche=dto.MarcaReencauche};
        Assert.Equal(dto.Vida,busqueda.Vida);Assert.Equal(dto.EsReencauchada,busqueda.EsReencauchada);
    }

    [Fact]
    public void MigracionExistenteCoincideConPersistenciaYConsultaSqlSinAbrirConexion()
    {
        using var db=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Integrated Security=true").Options);
        var entity=db.Model.FindEntityType(typeof(Llanta))!;
        Assert.True(entity.FindProperty(nameof(Llanta.TipoLlantaId))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(Llanta.MarcaReencaucheId))!.IsNullable);
        Assert.Equal(typeof(int),entity.FindProperty(nameof(Llanta.NumeroReencauches))!.ClrType);
        var ops=new AddInspectionFoundTireRetread().UpOperations;
        var column=Assert.Single(ops.OfType<AddColumnOperation>());
        Assert.Equal("GMarcaReencaucheId",column.Name);Assert.Equal("uniqueidentifier",column.ColumnType);Assert.True(column.IsNullable);
        Assert.Equal("GTipoLlantaId",Assert.Single(ops.OfType<AlterColumnOperation>()).Name);
        var fk=Assert.Single(ops.OfType<AddForeignKeyOperation>());
        Assert.Equal("dbo",fk.PrincipalSchema);Assert.Equal("TBL_Marca",fk.PrincipalTable);Assert.Equal(new[]{"GId"},fk.PrincipalColumns);
        var service=new LlantaService(db);
        var projection=(Expression<Func<Llanta,LlantaResumenDto>>)typeof(LlantaService)
            .GetMethod("Map",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(service,null)!;
        var sql=db.Llantas.Select(projection).ToQueryString();
        Assert.Contains("GMarcaReencaucheId",sql);Assert.Contains("NNumeroReencauches",sql);
    }
}
