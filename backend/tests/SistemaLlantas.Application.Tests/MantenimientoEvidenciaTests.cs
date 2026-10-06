using SistemaLlantas.Application.Analitica;

namespace SistemaLlantas.Application.Tests;

public sealed class MantenimientoEvidenciaTests
{
    private static MedicionResumen Reading(decimal? exterior, decimal? centro = 4, decimal? interior = 5) =>
        MedicionResumen.Crear(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, exterior, centro, interior, null);

    [Fact] public void MissingReadingRequiresVerification()
    {
        var result = EvidenciaMantenimiento.Evaluar(new(0, null, null, null, null));
        Assert.True(result.EnCola);
        Assert.Equal("CONDICION_POR_VERIFICAR", result.Clasificacion.Valor);
        Assert.Equal("SIN_MEDICION", result.MotivoPrincipal!.Codigo);
    }
    [Fact] public void ZeroIsMeasuredAndDoesNotMeanSafe()
    {
        var reading = Reading(0);
        var result = EvidenciaMantenimiento.Evaluar(new(0, null, null, reading, reading));
        Assert.Equal(0m, reading.Minima);
        Assert.True(reading.Completa);
        Assert.False(result.EnCola);
        Assert.Null(result.Clasificacion.Valor);
    }
    [Fact] public void LaterIncompleteReadingDoesNotHideEarlierCompleteEvidence()
    {
        var complete = Reading(3);
        var result = EvidenciaMantenimiento.Evaluar(new(0, null, null, Reading(null), complete));
        Assert.True(result.EnCola);
        Assert.Equal("MEDICION_INCOMPLETA", result.MotivoPrincipal!.Codigo);
        Assert.Contains(result.MotivosSecundarios, x => x.Codigo == "COMPLETA_ANTERIOR");
    }
    [Fact] public void AlertNeverInventsSeverityAndRetainsOtherReasons()
    {
        var result = EvidenciaMantenimiento.Evaluar(new(2, Guid.NewGuid(), DateTimeOffset.UtcNow, null, null));
        Assert.True(result.EnCola);
        Assert.Null(result.Clasificacion.Valor);
        Assert.Equal("NO_EVALUABLE", result.Clasificacion.Estado);
        Assert.Equal("ALERTA_ACTIVA", result.MotivoPrincipal!.Codigo);
        Assert.Contains(result.MotivosSecundarios, x => x.Codigo == "SIN_MEDICION");
    }
    [Fact] public void MissingMileageDoesNotInvalidateMeasuredDepth()
    {
        var reading = Reading(5);
        Assert.Null(reading.Odometro);
        Assert.False(EvidenciaMantenimiento.Evaluar(new(0, null, null, reading, reading)).EnCola);
    }
    [Fact] public void NegativeDepthIsIncomplete()
    {
        var reading = Reading(-1);
        Assert.Null(reading.Minima);
        Assert.False(reading.Completa);
    }
}
