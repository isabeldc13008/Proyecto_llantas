using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Tests;

public sealed class AlcanceCentrosTests
{
    [Fact]
    public void SinCentrosNiPermisoGlobal_NoAutorizaDatosOperativos()
    {
        var alcance = new AlcanceCentros(false, []);
        Assert.True(alcance.SinAcceso);
        Assert.False(alcance.Autoriza(Guid.NewGuid()));
    }

    [Fact]
    public void Multicentro_AutorizaSoloCentrosAsignados()
    {
        var primero = Guid.NewGuid(); var segundo = Guid.NewGuid();
        var alcance = new AlcanceCentros(false, [primero, segundo]);
        Assert.True(alcance.Autoriza(primero));
        Assert.True(alcance.Autoriza(segundo));
        Assert.False(alcance.Autoriza(Guid.NewGuid()));
    }

    [Fact]
    public void PermisoGlobal_AutorizaCualquierCentro()
    {
        var alcance = new AlcanceCentros(true, []);
        Assert.False(alcance.SinAcceso);
        Assert.True(alcance.Autoriza(Guid.NewGuid()));
    }
}
