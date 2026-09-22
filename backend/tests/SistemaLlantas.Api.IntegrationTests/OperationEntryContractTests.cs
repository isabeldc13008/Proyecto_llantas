using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.IntegrationTests;

// Entry guards must work before touching SQL or evaluating the actual assignment.
public sealed class OperationEntryContractTests
{
    [Theory][InlineData(false,"SER-1")][InlineData(true,null)][InlineData(true,"")]
    public async Task InspectionRequiresPhysicalDiscrepancyAndObservedIdentity(bool observed,string? identifier)
    {
        var controller=new InspeccionesController(null!,null!,null!);
        var error=await Assert.ThrowsAsync<ValidacionException>(()=>controller.Asignar(Guid.NewGuid(),Guid.NewGuid(),new(Guid.NewGuid(),"Instalar",observed,identifier),null!,CancellationToken.None,null!));
        Assert.Contains("Programación o Montajes",error.Message);
    }

    [Fact]public async Task DirectMovementCannotBypassAuthorization()
    {
        var controller=new OperacionesController(null!,null!,null!);
        await Assert.ThrowsAsync<ValidacionException>(()=>controller.Mover(new(){LlantaId=Guid.NewGuid(),PosicionOrigenId=Guid.NewGuid(),PosicionDestinoId=Guid.NewGuid(),TipoDestino="Posicion",Motivo="Sin solicitud"},CancellationToken.None));
        await Assert.ThrowsAsync<ValidacionException>(()=>controller.Desmontar(new(){PosicionId=Guid.NewGuid(),Destino="Inventario",Motivo="Sin solicitud"},CancellationToken.None));
    }
}
