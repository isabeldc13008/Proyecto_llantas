using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Tests;

public sealed class AnaliticaTests
{
    [Fact] public void MissingAndInvalidKmAreNotZero()
    {
        var result = EstadisticasAnalitica.Calcular([null, -10]);
        Assert.Equal(0, result.Muestra); Assert.Null(result.Promedio); Assert.Null(result.Mediana);
        Assert.Null(result.Desviacion); Assert.Null(result.Minimo); Assert.Null(result.Maximo);
    }
    [Fact] public void RealZeroAndExactMedianRemainVisible()
    {
        var result = EstadisticasAnalitica.Calcular([0, 10, null, 20, 30, -5]);
        Assert.Equal(4, result.Muestra); Assert.Equal(15m, result.Promedio); Assert.Equal(15m, result.Mediana);
        Assert.Equal(11.18m, result.Desviacion); Assert.Equal(0m, result.Minimo); Assert.Equal(30m, result.Maximo);
    }
    [Fact] public void OddMedianDoesNotBecomeTheMean()
    {
        var result = EstadisticasAnalitica.Calcular([100, 300, 5000]);
        Assert.Equal(300m, result.Mediana); Assert.Equal(1800m, result.Promedio);
    }
    [Theory]
    [InlineData(0, 0, 0, null)] [InlineData(2, 1, 1000, null)] [InlineData(1, 1, -100, null)]
    [InlineData(2, 2, 1000, 1000)] [InlineData(1, 1, 0, 0)]
    public void IncompleteTripsCannotProduceAnApparentlyCompleteTotal(int trips, int valid, int sum, int? expected)
        => Assert.Equal(expected.HasValue ? (decimal?)expected.Value : null, EstadisticasAnalitica.RecorridoCompleto(trips, valid, sum));
    [Fact] public void CenterFilterCannotExpandScope()
    {
        var center = Guid.NewGuid(); var allowed = new AlcanceCentros(false, [center]);
        new FiltroAnalitica { CentroId = center }.Validar(allowed);
        Assert.Throws<UnauthorizedAccessException>(() => new FiltroAnalitica { CentroId = Guid.NewGuid() }.Validar(allowed));
    }
    [Fact] public void InvalidDatesAndSampleLimitsAreRejected()
    {
        var scope = new AlcanceCentros(true, []);
        Assert.Throws<ValidacionException>(() => new FiltroAnalitica { IngresoDesde = new(2026, 9, 2), IngresoHasta = new(2026, 9, 1) }.Validar(scope));
        Assert.Throws<ValidacionException>(() => new FiltroAnalitica { MinimoMuestra = 0 }.Validar(scope));
        Assert.Throws<ValidacionException>(() => new FiltroAnalitica { Tamano = 1000 }.Validar(scope));
    }
}
