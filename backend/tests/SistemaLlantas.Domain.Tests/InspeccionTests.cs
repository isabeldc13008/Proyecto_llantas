using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Domain.Tests;

public sealed class InspeccionTests
{
    [Fact]
    public void NuevaInspeccion_PermiteKilometrajeVacio()
    {
        var inspeccion = new Inspeccion();
        Assert.Null(inspeccion.Kilometraje);
        Assert.Equal(EstadoInspeccion.Borrador, inspeccion.Estado);
    }

    [Fact]
    public void Inconsistencia_NacePendienteDeAutorizacion()
    {
        var inconsistencia = new InconsistenciaInspeccion();
        Assert.Equal(EstadoInconsistencia.PendienteAutorizacion, inconsistencia.Estado);
    }

    [Fact]
    public void RecomendacionReencauche_NoModificaLlantaNiPosicion()
    {
        var posicion = new PosicionVehiculo { LlantaActualId = Guid.NewGuid() };
        var original = posicion.LlantaActualId;
        var recomendacion = new RecomendacionInspeccion { EsCandidataReencauche = true };
        var detalle = new InspeccionDetalle { Recomendacion = recomendacion };
        Assert.True(detalle.Recomendacion.EsCandidataReencauche);
        Assert.Equal(original, posicion.LlantaActualId);
    }
}
