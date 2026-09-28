using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed partial class MountAuthorizationInspectionTests
{
    private static ServiciosLlantaController Servicios(IServiceProvider sp,LlantasDbContext db,Guid centro)=>new(db,sp.GetRequiredService<IOperacionService>(),sp.GetRequiredService<ICicloVidaLlantaService>(),sp.GetRequiredService<IWebHostEnvironment>()){ControllerContext=Context("qa-tecnico","TECNICO",centro)};
    private static ServiciosLlantaController.OrdenDto OrdenCreada(ActionResult<ServiciosLlantaController.OrdenDto> result)=>Assert.IsType<ServiciosLlantaController.OrdenDto>(Assert.IsType<CreatedResult>(result.Result).Value);

    [Theory][InlineData(true)][InlineData(false)]
    public async Task Disposicion_EvaluacionDefineRutaTerminalYNoSeRepite(bool reutilizable)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,_)=await Setup(db);var controller=Servicios(sp,db,v.CentroId);
        var orden=OrdenCreada(await controller.Crear(new("DisposicionFinal",t.Id,null,null,"Candidata",null),Ct));
        Assert.Equal("PENDIENTE_EVALUACION_TECNICA",orden.Estado);
        await Assert.ThrowsAsync<ConflictoException>(()=>controller.Aprobar(orden.Id,Ct));
        var result=await controller.EvaluarDisposicion(orden.Id,new(reutilizable,"Concepto técnico QA"),Ct);
        Assert.Equal(reutilizable?"REUTILIZABLE":"DISPOSICION",result.Resultado);
        Assert.Equal(reutilizable?"RETORNADA_INVENTARIO":"PENDIENTE_APROBACION",result.Estado);
        await Assert.ThrowsAsync<ConflictoException>(()=>controller.EvaluarDisposicion(orden.Id,new(reutilizable,"Repetida"),Ct));
        db.ChangeTracker.Clear();var persisted=await db.OrdenesServicioLlanta.SingleAsync(o=>o.Id==orden.Id);
        Assert.Equal("qa-tecnico",persisted.UsuarioModificacion);Assert.NotNull(persisted.FechaModificacion);Assert.Equal("Concepto técnico QA",persisted.Observaciones);
        if(reutilizable)
        {
            var tire=await db.Llantas.Include(x=>x.EstadoLlanta).SingleAsync(x=>x.Id==t.Id);Assert.Equal("Inventario",tire.UbicacionActual);Assert.Contains(tire.EstadoLlanta.Codigo,new[]{"DISPONIBLE","DIS"});
            var movement=Assert.Single(await db.Movimientos.Where(m=>m.Detalles.Any(d=>d.LlantaId==t.Id)).ToListAsync());Assert.Contains(orden.Id.ToString(),movement.Motivo);Assert.Equal("qa-tecnico",movement.Usuario);
            Assert.True(await LlantasDisponibles.Consulta(db).AnyAsync(x=>x.Id==t.Id));
            await Assert.ThrowsAsync<ConflictoException>(()=>controller.Aprobar(orden.Id,Ct));
            await Assert.ThrowsAsync<ValidacionException>(()=>controller.Enviar(orden.Id,Ct));
            await Assert.ThrowsAsync<ValidacionException>(()=>controller.Recibir(orden.Id,Ct));
            await Assert.ThrowsAsync<ValidacionException>(()=>controller.Cerrar(orden.Id,new(true,null),Ct));
            Assert.Equal("OPCIONADA",OrdenCreada(await controller.Crear(new("Reparacion",t.Id,null,null,"Nueva operación legítima",null),Ct)).Estado);
        }
        else Assert.False(await LlantasDisponibles.Consulta(db).AnyAsync(x=>x.Id==t.Id));
    }

    [Fact]public async Task Disposicion_MontadaRechazaRetornoSinCambios()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
        await sp.GetRequiredService<IOperacionService>().MoverAsync(new(){LlantaId=t.Id,PosicionDestinoId=p.Id,TipoDestino="Posicion",Motivo="Montar QA",KilometrajeVehiculo=1000},"qa",new(true,[]),Ct);
        var controller=Servicios(sp,db,v.CentroId);var orden=OrdenCreada(await controller.Crear(new("DisposicionFinal",t.Id,null,null,"Evaluar",null),Ct));
        var error=await Assert.ThrowsAsync<ConflictoException>(()=>controller.EvaluarDisposicion(orden.Id,new(true,"Reutilizable"),Ct));Assert.Contains("desmontarse",error.Message);
        db.ChangeTracker.Clear();Assert.Equal("PENDIENTE_EVALUACION_TECNICA",(await db.OrdenesServicioLlanta.SingleAsync(x=>x.Id==orden.Id)).Estado);Assert.True(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==t.Id&&x.EsActiva));Assert.Equal(t.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
    }

    [Fact]public async Task Disposicion_NoEvaluaOrdenDeReparacion()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,_)=await Setup(db);var controller=Servicios(sp,db,v.CentroId);
        var orden=OrdenCreada(await controller.Crear(new("Reparacion",t.Id,null,null,"Reparar",null),Ct));await Assert.ThrowsAsync<ValidacionException>(()=>controller.EvaluarDisposicion(orden.Id,new(false,"No aplica"),Ct));
    }


}

public sealed class DisposicionEntradaTests
{
    [Theory][InlineData("Disposición final",null,null)][InlineData("DisposicionFinal",null,null)][InlineData("Desmontaje","DisposicionFinal",null)][InlineData("Rotación","Posicion","DisposicionFinal")][InlineData("Desmontaje","4",null)]
    public async Task MovimientosNoPermiteDisposicion(string tipo,string? destino,string? desplazada)
    {
        var controller=new OperacionesController(null!,null!,null!);
        var ex=await Assert.ThrowsAsync<ValidacionException>(()=>controller.Solicitar(new(){Tipo=tipo,TipoDestino=destino!,DestinoDesplazada=desplazada},CancellationToken.None));Assert.Contains("evaluación técnica",ex.Message);
    }
    [Theory][InlineData(null)][InlineData("")][InlineData("   ")]
    public async Task EvaluacionExigeConcepto(string? concepto)
    {
        var controller=new ServiciosLlantaController(null!,null!,null!,null!);
        await Assert.ThrowsAsync<ValidacionException>(()=>controller.EvaluarDisposicion(Guid.NewGuid(),new(true,concepto!),CancellationToken.None));
    }
}
