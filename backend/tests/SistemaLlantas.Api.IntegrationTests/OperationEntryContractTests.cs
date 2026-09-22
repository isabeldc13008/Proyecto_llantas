using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.IntegrationTests;

// Entry guards must work before touching SQL or evaluating the actual assignment.
public sealed class OperationEntryContractTests
{
    [Theory]
    [InlineData("tipo")][InlineData("motivo")][InlineData("destino")]
    [InlineData("llanta")][InlineData("posicion")][InlineData("kilometraje")]
    [InlineData("asignacion")][InlineData("cuerpo")]
    [InlineData("tipoLargo")][InlineData("kilometrajeGrande")]
    public async Task SolicitudInvalidaDevuelve400AntesDeConsultarSql(string campo)
    {
        var controller=new OperacionesController(null!,null!,null!);
        var dto=new SistemaLlantas.Application.Operaciones.CrearSolicitudOperacionDto
        {
            Tipo=campo=="tipo"?null!:campo=="tipoLargo"?new string('x',51):campo=="destino"?"Movimiento":"Montaje",
            Motivo=campo=="motivo"?null!:"Prueba",
            TipoDestino=campo=="destino"?null!:"Posicion",
            LlantaId=campo=="llanta"?Guid.Empty:Guid.NewGuid(),
            PosicionDestinoId=campo is "posicion" or "destino"?null:Guid.NewGuid(),
            KilometrajeVehiculo=campo=="kilometraje"?-1:campo=="kilometrajeGrande"?decimal.MaxValue:100000,
            Asignaciones=campo=="asignacion"?[null!]:null
        };
        var context=new Microsoft.AspNetCore.Http.DefaultHttpContext();
        context.Response.Body=new MemoryStream();
        await new SistemaLlantas.Api.Middleware.ApiExceptionMiddleware(
            async _=>{await controller.Solicitar(campo=="cuerpo"?null!:dto,CancellationToken.None);},
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SistemaLlantas.Api.Middleware.ApiExceptionMiddleware>.Instance).InvokeAsync(context);
        Assert.Equal(400,context.Response.StatusCode);
        context.Response.Body.Position=0;
        Assert.Contains("VALIDATION_ERROR",await new StreamReader(context.Response.Body).ReadToEndAsync());
    }

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
