using SistemaLlantas.Domain.Common;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Domain.Tests;
public class NomenclaturaTests
{
    [Theory] [InlineData("d 200","D200")] [InlineData("áé Ñ/7","AEN7")] [InlineData("X-Multi D","XMULTID")]
    public void NormalizaDiseno(string input,string expected)=>Assert.Equal(expected,Nomenclatura.Diseno(input));
    [Fact] public void ReferenciaUsaMarcaYDiseno()=>Assert.Equal("RF_MC_101_D200",Nomenclatura.Referencia("MC_101","d 200"));
    [Theory] [InlineData("")] [InlineData("---")] [InlineData("ESTE DISENO EXCEDE LA LONGITUD MAXIMA")]
    public void ReferenciaRechazaVacioOLargo(string s)=>Assert.Throws<ArgumentException>(()=>Nomenclatura.Referencia("MC_100",s));
    [Theory] [InlineData("G")] [InlineData("GOO")] [InlineData("G1")] [InlineData("ÑG")]
    public void AbreviaturaSoloDosLetras(string s)=>Assert.Throws<ArgumentException>(()=>Nomenclatura.Abreviatura(s));
    [Fact] public void AbreviaturaNormalizada()=>Assert.Equal("GO",Nomenclatura.Abreviatura(" go "));
    [Fact] public void ComienzaEnCien()=>Assert.Equal(100,Nomenclatura.Siguiente([],"MC_",100));
    [Fact] public void ConservaMayorReservadoAunqueBorreRegistro()=>Assert.Equal(153,Nomenclatura.Siguiente(["MC_100"],"MC_",100,152));
    [Fact] public void ContinuanHistoricosSinTomarOtrosFormatos()=>Assert.Equal(202,Nomenclatura.Siguiente(["ANTIGUA","MC_201","MC_199","MC_-1","MC_2e8","DM_999"],"MC_",100));
    [Fact] public void ReservaLlantasPorPrefijo()=>Assert.Equal(10,Nomenclatura.Siguiente(["LL_GO_000009","LL_MI_999999"],"LL_GO_",1));
    [Fact] public void SerialYProfundidadDesconocidosNoSeInventan(){var x=new Llanta("LL_GO_000001",null);Assert.Null(x.Serial);Assert.Null(x.ProfundidadInicial);}
    [Fact] public void LimiteConsecutivoNoDesborda()=>Assert.Throws<OverflowException>(()=>Nomenclatura.Siguiente([],"MC_",100,long.MaxValue));
}
