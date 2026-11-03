using SistemaLlantas.Application.Analitica;
namespace SistemaLlantas.Application.Tests;
public sealed class OrientacionLlantaTests
{
    private static readonly DateTimeOffset T = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);
    private static GiroOrientacion Giro(int day) => new(Guid.NewGuid(), T.AddDays(day), 1000 + day, Guid.NewGuid());
    private static MedicionAnalitica Medicion(int day, decimal e=8, decimal c=7, decimal i=6) => new(new(Guid.NewGuid(), Guid.NewGuid(), T.AddDays(day), Guid.NewGuid(), "P1", 1000 + day, e, c, i), Math.Min(e,Math.Min(c,i)), Guid.NewGuid(), day, "Completa");
    [Fact] public void CeroGirosInicializaReferenciaSinModificarLecturas()
    {
        var input=new[]{Medicion(0),Medicion(2)};var result=OrientacionLlanta.Normalizar(input,[]);
        Assert.All(result,m=>{Assert.False(m.Normalizada!.Invertida);Assert.Equal(m.Lectura.Exterior,m.Normalizada.Exterior);Assert.Empty(m.Normalizada.GirosAplicados);});
        Assert.Equal(input.Select(m=>m.Lectura),result.Select(m=>m.Lectura));Assert.All(input,m=>Assert.Null(m.Normalizada));
    }
    [Fact] public void UnGiroIntercambiaHombrosDespuesYConservaCentro()
    {
        var giro=Giro(1);var input=new[]{Medicion(0,8,7,6),Medicion(2,5,6,7)};
        var result=OrientacionLlanta.Normalizar(input,[giro]);
        Assert.Equal(8,result[0].Normalizada!.Exterior);Assert.Equal(7,result[1].Normalizada!.Exterior);Assert.Equal(5,result[1].Normalizada!.Interior);Assert.Equal(6,result[1].Normalizada!.Centro);Assert.Equal(giro.Id,Assert.Single(result[1].Normalizada!.GirosAplicados));Assert.Equal(5,result[1].Lectura.Exterior);
    }
    [Fact] public void DosGirosRestablecenReferenciaInclusoEntreMontajes()
    {
        var input=new[]{Medicion(0),Medicion(2),Medicion(4)};
        var result=OrientacionLlanta.Normalizar(input,[Giro(3),Giro(1)]);
        Assert.Equal(new[]{false,true,false},result.Select(m=>m.Normalizada!.Invertida));
        Assert.Equal(input[2].Lectura.Exterior,result[2].Normalizada!.Exterior);Assert.Equal(2,result[2].Normalizada!.GirosAplicados.Count);
        Assert.Equal(input.Select(m=>m.TramoId),result.Select(m=>m.TramoId));
    }
    [Theory][InlineData("fecha")][InlineData("km")][InlineData("tecnico")][InlineData("inactivo")][InlineData("alcance")][InlineData("duplicado")]
    public void SinEvidenciaNoProduceComparativo(string problem)
    {
        var giro=Giro(1);giro=problem switch{"fecha"=>giro with{Fecha=null},"km"=>giro with{Kilometraje=null},"tecnico"=>giro with{TecnicoId=null},"inactivo"=>giro with{EvidenciaCompleta=false},_=>giro};
        var result=OrientacionLlanta.Normalizar([Medicion(0),Medicion(2)],problem=="duplicado"?[giro,giro]:[giro],problem!="alcance");
        Assert.All(result,m=>{Assert.Null(m.Normalizada);Assert.Equal("No verificable",m.Orientacion);});
    }
    [Fact] public void GiroDuranteInspeccionOEnMismaFechaNoSuponeOrden()
    {
        var m=Medicion(0);m=m with{Lectura=m.Lectura with{FechaHasta=T.AddDays(2)}};
        var result=OrientacionLlanta.Normalizar([m,Medicion(1),Medicion(3)],[Giro(1)]);
        Assert.Null(result[0].Normalizada);Assert.Null(result[1].Normalizada);Assert.True(result[2].Normalizada!.Invertida);
    }
    [Fact] public void GiroRegistradoTardeSeOrdenaPorFechaIntervencion()
    {
        var result=OrientacionLlanta.Normalizar([Medicion(4),Medicion(0),Medicion(2)],[Giro(3),Giro(1)]);
        Assert.Equal(new[]{false,false,true},result.Select(m=>m.Normalizada!.Invertida));
    }
    [Fact] public void ValoresAusentesNoSeSustituyenPorCero()
    {
        var m=Medicion(2);m=m with{Lectura=m.Lectura with{Interior=null}};
        var result=Assert.Single(OrientacionLlanta.Normalizar([m],[Giro(1)]));Assert.Null(result.Normalizada!.Exterior);Assert.Equal(8,result.Normalizada.Interior);
    }
}
