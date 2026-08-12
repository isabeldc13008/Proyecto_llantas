using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Domain.Tests;

public sealed class OperacionesTests
{
    [Fact]
    public void AsignacionNueva_EsActivaYConservaOrigen()
    {
        var movimiento=Guid.NewGuid();
        var asignacion=new AsignacionLlantaPosicion{LlantaId=Guid.NewGuid(),PosicionVehiculoId=Guid.NewGuid(),MovimientoOrigenId=movimiento};
        Assert.True(asignacion.EsActiva);
        Assert.Equal(movimiento,asignacion.MovimientoOrigenId);
        Assert.Null(asignacion.FechaFin);
    }

    [Fact]
    public void Movimiento_PuedeRegistrarLlantaDesplazadaConDestino()
    {
        var movimiento=new Movimiento();
        movimiento.Detalles.Add(new MovimientoDetalle{LlantaId=Guid.NewGuid(),PosicionOrigenId=Guid.NewGuid(),TipoDestino=TipoDestinoLlanta.Inventario,DestinoDescripcion="Inventario"});
        Assert.Single(movimiento.Detalles);
        Assert.Equal(TipoDestinoLlanta.Inventario,movimiento.Detalles.Single().TipoDestino);
    }

    [Fact]
    public void ActividadNueva_QuedaPendiente()
    {
        Assert.Equal(EstadoActividad.Pendiente,new ActividadProgramada().Estado);
    }
}
