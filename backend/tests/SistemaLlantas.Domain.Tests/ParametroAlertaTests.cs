using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Domain.Tests;
public sealed class ParametroAlertaTests
{
 [Theory]
 [InlineData(">=",3,true)] [InlineData(">",3,false)] [InlineData("<=",3,true)] [InlineData("<",3,false)] [InlineData("=",3,true)] [InlineData("!=",3,false)]
 public void Diferencia_EvaluaOperadores(string op,int threshold,bool expected)=>Assert.Equal(expected,new ParametroAlerta{Operador=op,Valor=threshold}.Cumple([10,9,7]));
 [Fact]public void ProfundidadMinima_EvaluaUmbral()=>Assert.True(new ParametroAlerta{Tipo="PROFUNDIDAD_MINIMA",Operador="<=",Valor=3}.Cumple([6,3,4]));
 [Fact]public void ReglaInactiva_NoGeneraAlerta()=>Assert.False(new ParametroAlerta{Activo=false,Valor=3}.Cumple([10,9,7]));
 [Fact]public void LecturasIncompletas_NoGeneranAlerta()=>Assert.False(new ParametroAlerta{Valor=3}.Cumple([10]));
}
