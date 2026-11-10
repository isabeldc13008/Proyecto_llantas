using SistemaLlantas.Application.Analitica;

namespace SistemaLlantas.Application.Tests;

public sealed class TasasDesgasteTests
{
    private static readonly DateTimeOffset Fecha = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid Tramo = Guid.NewGuid();
    private static MedicionAnalitica Medicion(int day, decimal km, decimal? exterior = 10, decimal? interior = 6) => new(
        new(Guid.NewGuid(), Guid.NewGuid(), Fecha.AddDays(day), Guid.Empty, "P1", km, exterior, 8, interior),
        6, Tramo, km, "Vinculada");

    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void ComparaLaMismaZonaFisicaConCeroUnoYDosGiros(int turns)
    {
        var giros = Enumerable.Range(1, turns).Select(i => new GiroOrientacion(Guid.NewGuid(), Fecha.AddDays(i), 1000 + i * 100, Guid.NewGuid())).ToArray();
        var last = turns % 2 == 1 ? Medicion(3, 2000, 5, 9) : Medicion(3, 2000, 9, 5);
        var normalized = OrientacionLlanta.Normalizar([Medicion(0, 1000), last], giros);
        var rates = TasasDesgaste.Calcular(normalized);
        Assert.Equal(1m, rates.Single(x => x.Zona == "Exterior").MmPorMilKm);
        Assert.Equal(1m, rates.Single(x => x.Zona == "Interior").MmPorMilKm);
        Assert.Equal(0m, rates.Single(x => x.Zona == "Centro").MmPorMilKm);
    }

    [Theory][InlineData(1000)][InlineData(900)]
    public void SinIncrementoPositivoNoHayTasa(int km)
    {
        var rates = TasasDesgaste.Calcular(OrientacionLlanta.Normalizar([Medicion(0, 1000), Medicion(1, km)], []));
        Assert.All(rates, r => { Assert.Null(r.MmPorMilKm);Assert.NotNull(r.Motivo); });
    }
    [Fact] public void SinOrientacionOEntreTramosNoUneLecturas()
    {
        var a = Medicion(0, 1000);var b = Medicion(1, 2000);
        Assert.All(TasasDesgaste.Calcular([a, b]), r => Assert.Null(r.MmPorMilKm));
        Assert.All(TasasDesgaste.Calcular(OrientacionLlanta.Normalizar([a, b with { TramoId = Guid.NewGuid() }], [])), r => Assert.Null(r.MmPorMilKm));
    }
    [Fact] public void CeroRealNoSeConfundeConFaltanteNiSeSaltaLecturaIncompleta()
    {
        var rates = TasasDesgaste.Calcular(OrientacionLlanta.Normalizar([Medicion(0, 1000, 0), Medicion(1, 2000, 0), Medicion(2, 3000, null), Medicion(3, 4000, 0)], []));
        var exterior = rates.Where(r => r.Zona == "Exterior").ToArray();
        Assert.Equal(0m, exterior[0].MmPorMilKm);Assert.Null(exterior[1].MmPorMilKm);Assert.Null(exterior[2].MmPorMilKm);
    }
    [Fact] public void AumentoDeProfundidadYFechasSolapadasNoProducenDesgaste()
    {
        var a = Medicion(0, 1000);var b = Medicion(1, 2000, 11);
        Assert.Null(TasasDesgaste.Calcular(OrientacionLlanta.Normalizar([a, b], [])).Single(r => r.Zona == "Exterior").MmPorMilKm);
        a = a with { Lectura = a.Lectura with { FechaHasta = b.Lectura.Fecha } };
        Assert.All(TasasDesgaste.Calcular(OrientacionLlanta.Normalizar([a, b], [])), r => Assert.Null(r.MmPorMilKm));
        Assert.Empty(TasasDesgaste.Calcular([a]));
    }
}
