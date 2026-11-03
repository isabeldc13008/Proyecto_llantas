using SistemaLlantas.Application.Analitica;

namespace SistemaLlantas.Application.Tests;

public sealed class CalidadDatosTests
{
    [Fact] public void EmptyPopulationDoesNotInventCoverage()
    {
        var totals = new TotalesCalidad(0, 0, 0, 0, 0, 0);
        Assert.Null(totals.CoberturaInspeccion);
        Assert.Equal(0, totals.SinInspeccion);
    }
    [Fact] public void IncompleteInspectionCountsAsInspectedButNotComplete()
    {
        var totals = new TotalesCalidad(3, 1, 1, 1, 2, 1);
        Assert.Equal(66.67m, totals.CoberturaInspeccion);
        Assert.Equal(1, totals.SinInspeccion);
        Assert.Equal(1, totals.UltimaIncompleta);
    }
    [Fact] public void NoInspectionInNonEmptyPopulationIsRealZeroCoverage()
        => Assert.Equal(0m, new TotalesCalidad(2, 0, 0, 0, 0, 0).CoberturaInspeccion);
    [Theory] [InlineData(0, true)] [InlineData(-1, false)] [InlineData(null, false)]
    public void ZeroIsACompleteMeasurementButMissingOrNegativeIsNot(int? depth, bool complete)
    {
        var reading = MedicionResumen.Crear(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, depth, 0, 0, null);
        Assert.Equal(complete, reading.Completa);
        Assert.Equal(complete ? 0m : null, reading.Minima);
    }
}
