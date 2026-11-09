using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class VehiculoVisibilidadTests
{
    private static readonly Guid CentroA = Guid.NewGuid();
    private static readonly Guid CentroB = Guid.NewGuid();
    private static IQueryable<Vehiculo> Datos() => new[]
    {
        new Vehiculo { NumeroInterno = "T1", CentroId = CentroB, SGrupoOperativo = "T1" },
        new Vehiculo { NumeroInterno = "T2-A", CentroId = CentroA, SGrupoOperativo = "T2" },
        new Vehiculo { NumeroInterno = "T2-B", CentroId = CentroB, SGrupoOperativo = "T2" },
        new Vehiculo { NumeroInterno = "SIN-GRUPO", CentroId = CentroA, SGrupoOperativo = null },
        new Vehiculo { NumeroInterno = "DESCONOCIDO", CentroId = CentroB, SGrupoOperativo = "OTRO" }
    }.AsQueryable();

    [Fact]
    public void UnCentro_veT1_yT2Propio_peroNoT2Ajeno()
    {
        var visibles = Datos().VisiblePara(new(false, [CentroA])).Select(x => x.NumeroInterno).ToArray();
        Assert.Equal(["T1", "T2-A", "SIN-GRUPO"], visibles);
    }

    [Fact]
    public void VariosCentros_veT1_yT2DeTodosSusCentros()
    {
        var visibles = Datos().VisiblePara(new(false, [CentroA, CentroB])).Select(x => x.NumeroInterno).ToArray();
        Assert.Equal(["T1", "T2-A", "T2-B", "SIN-GRUPO", "DESCONOCIDO"], visibles);
    }

    [Fact]
    public void SinCentros_soloVeT1_yLosValoresDesconocidosQuedanRestringidos()
    {
        var visibles = Datos().VisiblePara(new(false, [])).Select(x => x.NumeroInterno).ToArray();
        Assert.Equal(["T1"], visibles);
    }
}
