using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.IntegrationTests;

public sealed class InspectionVisualStateTests
{
    [Theory]
    [InlineData(EstadoAlerta.ABIERTA,true,true,true)]
    [InlineData(EstadoAlerta.EN_PROCESO,true,true,true)]
    [InlineData(EstadoAlerta.GESTIONADA,true,true,false)]
    [InlineData(EstadoAlerta.DESCARTADA,true,true,false)]
    [InlineData(EstadoAlerta.ABIERTA,false,true,false)]
    [InlineData(EstadoAlerta.ABIERTA,true,false,false)]
    public void SoloAlertasActivasDeLaInspeccion(EstadoAlerta estado,bool activa,bool misma,bool esperada)
    {
        using var db=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer("Server=unused;Database=unused;Integrated Security=true").Options);
        var id=Guid.NewGuid();var service=new InspeccionService(db);
        var query=(IQueryable<AlertaInspeccion>)typeof(InspeccionService).GetMethod("AlertasActivasDeInspeccion",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(service,[id])!;
        Assert.Contains("WHERE",query.ToQueryString()); // Translation only; no database connection.
        var where=(MethodCallExpression)query.Expression;
        var predicate=(Expression<Func<AlertaInspeccion,bool>>)((UnaryExpression)where.Arguments[1]).Operand;
        Assert.Equal(esperada,predicate.Compile()(new AlertaInspeccion{InspeccionId=misma?id:Guid.NewGuid(),Activo=activa,Estado=estado}));
    }

    [Fact]
    public void ReglasExistentesExigenTresLecturasYConservanElCeroReal()
    {
        var regla=new ParametroAlerta{Activo=true,Tipo="PROFUNDIDAD_MINIMA",Operador="<=",Valor=0};
        Assert.False(regla.Cumple([0,0]));Assert.True(regla.Cumple([0,4,4]));Assert.False(regla.Cumple([1,4,4]));
        regla.Activo=false;Assert.False(regla.Cumple([0,4,4]));
        regla.Activo=true;regla.Tipo="DIFERENCIA_HOMBROS";regla.Operador=">=";regla.Valor=3;
        Assert.True(regla.Cumple([5,2,5]));Assert.False(regla.Cumple([5,3,5]));
    }
}
