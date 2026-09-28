using SistemaLlantas.Application.Analitica;

namespace SistemaLlantas.Application.Tests;

public sealed class DesgasteAnaliticaTests
{
    private static readonly Guid Posicion = Guid.NewGuid();
    private static readonly DateTimeOffset Fecha = new(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
    private static LecturaAnalitica Lectura(decimal? km = 120, decimal? exterior = 10) => new(Guid.NewGuid(), Guid.NewGuid(), Fecha, Posicion, "P1", km, exterior, 8, 9);
    private static TramoAnalitica Tramo() => new(Guid.NewGuid(), Posicion, Fecha.AddDays(-2), Fecha.AddDays(2), false, 100, 200);

    [Fact] public void ConservaLecturasYCalculaMinimoYRecorridoSinSumarOdometro()
    {
        var lectura = Lectura();var tramo = Tramo();
        var r = Assert.Single(DesgasteAnaliticaCalculo.Vincular([lectura], [tramo]));
        Assert.Equal(lectura, r.Lectura);Assert.Equal(8m, r.Minima);Assert.Equal(20m, r.KmDesdeMontaje);Assert.Equal(tramo.Id, r.TramoId);
    }
    [Theory] [InlineData(null)] [InlineData(-1)]
    public void LecturasIncompletasONegativasNoSeImputan(int? exterior)
        => Assert.Null(Assert.Single(DesgasteAnaliticaCalculo.Vincular([Lectura(exterior: exterior)], [Tramo()])).Minima);
    [Fact] public void CeroMedidoEsValido()
        => Assert.Equal(0m, Assert.Single(DesgasteAnaliticaCalculo.Vincular([Lectura(100, 0)], [Tramo()])).KmDesdeMontaje);
    [Theory] [InlineData(null)] [InlineData(-1)] [InlineData(99)] [InlineData(201)]
    public void OdometroFueraDeTramoNoProduceRecorrido(int? km)
        => Assert.Null(Assert.Single(DesgasteAnaliticaCalculo.Vincular([Lectura(km)], [Tramo()])).KmDesdeMontaje);
    [Fact] public void TramosSolapadosNoSeEligenArbitrariamente()
        => Assert.Null(Assert.Single(DesgasteAnaliticaCalculo.Vincular([Lectura()], [Tramo(), Tramo()])).TramoId);
    [Fact] public void LecturaFueraDeFechasOPosicionNoSeVincula()
    {
        foreach (var t in new[] { Tramo() with { PosicionId = Guid.NewGuid() }, Tramo() with { Inicio = Fecha.AddDays(1) } })
            Assert.Null(Assert.Single(DesgasteAnaliticaCalculo.Vincular([Lectura()], [t])).TramoId);
    }
    [Fact] public void RetrocesoInvalidaTodoElTramoNoSoloLaUltimaLectura()
    {
        var r = DesgasteAnaliticaCalculo.Vincular([Lectura(150), Lectura(120) with { Fecha = Fecha.AddDays(1) }], [Tramo()]);
        Assert.All(r, x => { Assert.Null(x.KmDesdeMontaje);Assert.Contains("discontinuos", x.Calidad); });
    }
    [Fact] public void TramosDiferentesNoCompartenOdometros()
    {
        var t = Tramo();var siguiente = Tramo() with { Inicio = Fecha.AddDays(3), Fin = Fecha.AddDays(5), Montaje = 0, Desmontaje = 100 };
        var r = DesgasteAnaliticaCalculo.Vincular([Lectura(150), Lectura(20) with { Fecha = Fecha.AddDays(4) }], [t, siguiente]);
        Assert.Equal(new decimal?[] { 50, 20 }, r.Select(x => x.KmDesdeMontaje));
    }
    [Theory] [InlineData(null, false)] [InlineData(110, false)] [InlineData(120, true)] [InlineData(150, true)]
    public void TramoAbiertoExigeOdometroActualCompatible(int? actual, bool valido)
    {
        var t = Tramo() with { Activo = true, Fin = null, Desmontaje = null, OdometroActual = actual };
        var r = Assert.Single(DesgasteAnaliticaCalculo.Vincular([Lectura()], [t]));
        Assert.Equal(valido, r.TramoId.HasValue);
    }
    [Fact] public void SinLecturasNoInventaMuestra() => Assert.Empty(DesgasteAnaliticaCalculo.Vincular([], [Tramo()]));
}
