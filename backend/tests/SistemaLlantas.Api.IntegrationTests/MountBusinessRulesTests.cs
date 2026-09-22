using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed partial class MountAuthorizationInspectionTests
{
    [Fact]public async Task AutorizarSinLlanta_DevuelveValidacionYConservaPendiente()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(_,v,p)=await Setup(db);
        var service=sp.GetRequiredService<IInspeccionService>();var inspection=await service.CrearAsync(new(){VehiculoId=v.Id,Kilometraje=1000},Technician,new(true,[]),Ct);
        var discrepancy=new InconsistenciaInspeccion{InspeccionId=inspection.Id,PosicionVehiculoId=p.Id,IdentificadorEncontrado="SIN-REGISTRO",TecnicoId=Technician,Observacion="Llanta física no identificada",Estado=EstadoInconsistencia.PendienteAutorizacion};db.InconsistenciasInspeccion.Add(discrepancy);await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ValidacionException>(()=>db.Database.CreateExecutionStrategy().ExecuteAsync(()=>service.ResolverAsync(discrepancy.Id,new(){Observacion="Autorizar sin seleccionar"},true,"qa-admin",false,Ct,new(true,[]))));
        db.ChangeTracker.Clear();Assert.Equal(EstadoInconsistencia.PendienteAutorizacion,(await db.InconsistenciasInspeccion.SingleAsync(x=>x.Id==discrepancy.Id)).Estado);Assert.False(await db.Movimientos.AnyAsync(x=>x.InspeccionId==inspection.Id));
    }

    [Theory][InlineData(EstadoActividad.Cumplida)][InlineData(EstadoActividad.Cancelada)]
    public async Task IniciarActividadCerrada_DevuelveConflicto(EstadoActividad estado)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(_,v,p)=await Setup(db);
        var activity=new ActividadProgramada{TipoActividad="Montaje",CentroId=v.CentroId,VehiculoId=v.Id,PosicionVehiculoId=p.Id,TecnicoId=Technician,Estado=estado,FechaProgramada=DateTimeOffset.UtcNow};db.ActividadesProgramadas.Add(activity);await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictoException>(()=>sp.GetRequiredService<IOperacionService>().IniciarActividadAsync(activity.Id,Technician,new(true,[]),Ct));
    }

    [Fact]public async Task LiberarReservasDuplicadas_DevuelveConflictoSinLiberarNinguna()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,_)=await Setup(db);
        for(var i=0;i<2;i++)db.SolicitudesOperacion.Add(new(){Tipo="RESERVA",Estado=EstadoSolicitudOperacion.EJECUTADO,CentroId=v.CentroId,LlantaId=t.Id,TipoDestino="Sin vehículo",Motivo="Inconsistencia heredada",Solicitante=Technician});await db.SaveChangesAsync();
        var controller=new InventarioController(db){ControllerContext=Context(Technician,"TECNICO",v.CentroId)};await Assert.ThrowsAsync<ConflictoException>(()=>controller.Liberar(t.Id,Ct));Assert.Equal(2,await db.SolicitudesOperacion.CountAsync(x=>x.LlantaId==t.Id&&x.Tipo=="RESERVA"&&x.Activo));
    }

    private static Task<MovimientoDto> Mount(IOperacionService service, Llanta tire, PosicionVehiculo p) =>
        service.MoverAsync(new() { LlantaId=tire.Id, PosicionDestinoId=p.Id, TipoDestino="Posicion", Motivo="Preparación", KilometrajeVehiculo=1000 }, Technician, new(true,[]), Ct);

    [Theory]
    [InlineData(false,false)] [InlineData(true,false)] [InlineData(false,true)]
    public async Task ReemplazoExplicito_ManualProgramadoEHistorico(bool programmed,bool historical)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();
        var(old,v,p)=await Setup(db);var(incoming,_,_)=await Setup(db);var service=sp.GetRequiredService<IOperacionService>();await Mount(service,old,p);
        Guid requestId;Guid? activityId=null;string user=Technician;
        if(programmed)
        {
            var tech=await db.UsuariosSistema.Include(x=>x.Centros).FirstAsync(x=>x.Activo&&x.Rol.Codigo=="TECNICO"&&x.Centros.Any(c=>c.Activo&&c.CentroId==v.CentroId));user=tech.Username;
            var start=DateTimeOffset.UtcNow.AddDays(7);
            var plan=await sp.GetRequiredService<IProgramacionService>().CrearAsync(new(){Tipo="Reemplazar llanta",CentroId=v.CentroId,VehiculoId=v.Id,TecnicoUsuarioId=tech.Id,Inicio=start,Fin=start.AddHours(1),Motivo="Reemplazo",Asignaciones=[new(p.Id,incoming.Id,old.Id)]},"planner",new(true,[]),Ct);
            activityId=plan.Id;requestId=await db.SolicitudesOperacion.Where(x=>x.ActividadProgramadaId==plan.Id).Select(x=>x.Id).SingleAsync();
            await Operations(sp,db,v.CentroId,user).EjecutarTrabajo(plan.Id,new(1250),Ct);
        }
        else if(historical)
        {
            var request=new SolicitudOperacion{Tipo="Cambio de juego",GrupoOperacionId=Guid.NewGuid(),CentroId=v.CentroId,LlantaId=incoming.Id,LlantaDesplazadaId=old.Id,PosicionDestinoId=p.Id,TipoDestino="Posicion",DestinoDesplazada="Inventario",KilometrajeVehiculo=1250,Motivo="Histórico de una fila",Solicitante=Technician,Estado=EstadoSolicitudOperacion.PENDIENTE_APROBACION};db.SolicitudesOperacion.Add(request);await db.SaveChangesAsync();requestId=request.Id;
            await Operations(sp,db,v.CentroId,"qa-admin").Resolver(requestId,new(true,null),Ct);
        }
        else
        {
            var dto=new CrearSolicitudOperacionDto{Tipo="Reemplazar llanta",VehiculoId=v.Id,Asignaciones=[new(p.Id,incoming.Id,old.Id)],Motivo="Cambio individual",KilometrajeVehiculo=1250};
            var request=Created(await Operations(sp,db,v.CentroId).Solicitar(dto,Ct));requestId=request.Id;Assert.Equal("PENDIENTE_APROBACION",request.Estado);Assert.Equal(old.Id,(await db.PosicionesVehiculo.AsNoTracking().SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
            await Operations(sp,db,v.CentroId,"qa-admin").Resolver(requestId,new(true,null),Ct);
        }
        db.ChangeTracker.Clear();Assert.Equal(incoming.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
        var closed=await db.AsignacionesLlantaPosicion.SingleAsync(x=>x.LlantaId==old.Id);Assert.False(closed.EsActiva);Assert.Equal(250,closed.KilometrajeRecorrido);
        Assert.Equal("Inventario",(await db.Llantas.SingleAsync(x=>x.Id==old.Id)).UbicacionActual);Assert.Single(await db.AsignacionesLlantaPosicion.Where(x=>x.PosicionVehiculoId==p.Id&&x.EsActiva).ToListAsync());
        Assert.Equal(EstadoSolicitudOperacion.EJECUTADO,(await db.SolicitudesOperacion.SingleAsync(x=>x.Id==requestId)).Estado);
        if(activityId.HasValue)Assert.Equal(EstadoActividad.Cumplida,(await db.ActividadesProgramadas.SingleAsync(x=>x.Id==activityId)).Estado);
    }

    [Theory][InlineData(false,false)][InlineData(true,false)][InlineData(false,true)]
    public async Task Rotacion_ValidaIntercambioLibreYMismoVehiculo(bool occupied,bool otherVehicle)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);var(other,_,foreign)=await Setup(db);var service=sp.GetRequiredService<IOperacionService>();await Mount(service,t,p);
        var dest=otherVehicle?foreign:new PosicionVehiculo{EjeVehiculoId=p.EjeVehiculoId,Codigo="P2",Lado="Derecha",Orden=2};if(!otherVehicle){db.PosicionesVehiculo.Add(dest);await db.SaveChangesAsync();}if(occupied)await Mount(service,other,dest);
        var request=Created(await Operations(sp,db,v.CentroId).Solicitar(new(){Tipo="Rotación",LlantaId=t.Id,PosicionOrigenId=p.Id,PosicionDestinoId=dest.Id,TipoDestino="Posicion",LlantaDesplazadaId=occupied?other.Id:null,DestinoDesplazada=occupied?"Posicion":null,Motivo="Rotar",KilometrajeVehiculo=1200},Ct));
        var approver=Operations(sp,db,v.CentroId,"qa-admin");
        if(otherVehicle){await Assert.ThrowsAsync<ValidacionException>(()=>approver.Resolver(request.Id,new(true,null),Ct));db.ChangeTracker.Clear();Assert.Equal(t.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);return;}
        await approver.Resolver(request.Id,new(true,null),Ct);db.ChangeTracker.Clear();Assert.Equal(t.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==dest.Id)).LlantaActualId);Assert.Equal(occupied?other.Id:(Guid?)null,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
        Assert.Equal(200,(await db.AsignacionesLlantaPosicion.SingleAsync(x=>x.LlantaId==t.Id&&!x.EsActiva)).KilometrajeRecorrido);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task Traslado_DesmontaRecibeORollback(bool fail)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);await Mount(sp.GetRequiredService<IOperacionService>(),t,p);
        var destination=await db.Centros.Where(x=>x.Activo&&x.Id!=v.CentroId).Select(x=>x.Id).FirstAsync();var controller=Operations(sp,db,v.CentroId);((ClaimsIdentity)controller.User.Identity!).AddClaim(new("permiso","centros.ver_todos"));
        var request=Created(await controller.Solicitar(new(){Tipo="Traslado",LlantaId=t.Id,PosicionOrigenId=p.Id,CentroDestinoId=destination,TipoDestino="Traslado",Motivo="Trasladar",KilometrajeVehiculo=1200},Ct));
        if(fail){var row=await db.SolicitudesOperacion.SingleAsync(x=>x.Id==request.Id);row.CentroDestinoId=v.CentroId;await db.SaveChangesAsync();}
        var approver=Operations(sp,db,v.CentroId,"qa-admin");((ClaimsIdentity)approver.User.Identity!).AddClaim(new("permiso","centros.ver_todos"));
        if(fail){await Assert.ThrowsAsync<ValidacionException>(()=>approver.Resolver(request.Id,new(true,null),Ct));db.ChangeTracker.Clear();Assert.Equal(t.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);Assert.True(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==t.Id&&x.EsActiva));Assert.Equal(1000,(await db.Vehiculos.SingleAsync(x=>x.Id==v.Id)).Kilometraje);Assert.Equal(EstadoSolicitudOperacion.PENDIENTE_APROBACION,(await db.SolicitudesOperacion.SingleAsync(x=>x.Id==request.Id)).Estado);return;}
        await approver.Resolver(request.Id,new(true,null),Ct);db.ChangeTracker.Clear();var moved=await db.Llantas.Include(x=>x.EstadoLlanta).SingleAsync(x=>x.Id==t.Id);Assert.Equal(destination,moved.CentroId);Assert.Equal("EN_TRASLADO",moved.EstadoLlanta.Codigo);Assert.Null((await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
        await approver.Recibir(request.Id,Ct);db.ChangeTracker.Clear();Assert.Equal("DISPONIBLE",(await db.Llantas.Include(x=>x.EstadoLlanta).SingleAsync(x=>x.Id==t.Id)).EstadoLlanta.Codigo);
    }

    [Fact]public async Task ReparacionDesdeMontajes_OpcionaSinEnviarNiDuplicar()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);await Mount(sp.GetRequiredService<IOperacionService>(),t,p);
        var request=Created(await Operations(sp,db,v.CentroId).Solicitar(new(){Tipo="Reparación",LlantaId=t.Id,PosicionOrigenId=p.Id,TipoDestino="Reparacion",Motivo="Revisar",KilometrajeVehiculo=1100},Ct));Assert.False(await db.OrdenesServicioLlanta.AnyAsync(x=>x.LlantaId==t.Id));
        await Operations(sp,db,v.CentroId,"qa-admin").Resolver(request.Id,new(true,null),Ct);db.ChangeTracker.Clear();Assert.Equal("OPCIONADA",(await db.OrdenesServicioLlanta.SingleAsync(x=>x.LlantaId==t.Id)).Estado);Assert.Contains((await db.Llantas.Include(x=>x.EstadoLlanta).SingleAsync(x=>x.Id==t.Id)).EstadoLlanta.Codigo,new[]{"DISPONIBLE","DIS"});
        await Assert.ThrowsAsync<ConflictoException>(()=>Operations(sp,db,v.CentroId,"qa-admin").Resolver(request.Id,new(true,null),Ct));Assert.Equal(1,await db.OrdenesServicioLlanta.CountAsync(x=>x.LlantaId==t.Id));
    }

    [Fact]public async Task ReservaManual_BloqueaSelectorIdYProgramacion_LiberarHabilita()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
        var inventory=new InventarioController(db){ControllerContext=Context(Technician,"TECNICO",v.CentroId)};await inventory.Reservar(t.Id,new(null,null,null,"Reserva manual"),Ct);
        Assert.False(await LlantasDisponibles.Consulta(db).AnyAsync(x=>x.Id==t.Id));await Assert.ThrowsAsync<ConflictoException>(()=>Operations(sp,db,v.CentroId).Solicitar(Request(t,p),Ct));await Assert.ThrowsAsync<ConflictoException>(()=>Mount(sp.GetRequiredService<IOperacionService>(),t,p));
        await Assert.ThrowsAsync<ConflictoException>(()=>sp.GetRequiredService<IOperacionService>().ValidarAsignacionesAsync(v.Id,[new(p.Id,t.Id,null)],new(true,[]),null,Ct));
        await inventory.Liberar(t.Id,Ct);Assert.True(await LlantasDisponibles.Consulta(db).AnyAsync(x=>x.Id==t.Id));
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task Inspeccion_CorrigeRealidadFisica_ConservaIdentidadesYSinDuplicados(bool occupied)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(old,v,p)=await Setup(db);var(found,_,_)=await Setup(db);var operations=sp.GetRequiredService<IOperacionService>();if(occupied)await Mount(operations,old,p);
        var controller=Inspections(sp,db,center:v.CentroId);var inspection=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await controller.Crear(new(){VehiculoId=v.Id,Kilometraje=1200},Ct)).Result).Value);
        var dto=new InspeccionesController.AsignarLlantaDto(found.Id,"Identificación física observada",true,found.Serial,occupied?old.Id:null);
        await controller.Asignar(inspection.Id,p.Id,dto,operations,Ct,sp.GetRequiredService<ICicloVidaLlantaService>());db.ChangeTracker.Clear();
        var record=await db.InconsistenciasInspeccion.SingleAsync(x=>x.InspeccionId==inspection.Id);Assert.Equal(occupied?old.Id:(Guid?)null,record.LlantaEsperadaId);Assert.Equal(found.Id,record.LlantaEncontradaId);Assert.Equal(Technician,record.TecnicoId);Assert.Equal(EstadoInconsistencia.Regularizada,record.Estado);
        Assert.Equal(found.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);Assert.Single(await db.AsignacionesLlantaPosicion.Where(x=>x.PosicionVehiculoId==p.Id&&x.EsActiva).ToListAsync());
        var count=await db.Movimientos.CountAsync(x=>x.InspeccionId==inspection.Id);await Assert.ThrowsAsync<ConflictoException>(()=>controller.Asignar(inspection.Id,p.Id,dto,operations,Ct,sp.GetRequiredService<ICicloVidaLlantaService>()));db.ChangeTracker.Clear();Assert.Equal(count,await db.Movimientos.CountAsync(x=>x.InspeccionId==inspection.Id));
    }

    [Fact]public async Task Inspeccion_NoPermiteInstalacionOperativaSinDiscrepancia()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);var controller=Inspections(sp,db,center:v.CentroId);
        var inspection=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await controller.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
        await Assert.ThrowsAsync<ValidacionException>(()=>controller.Asignar(inspection.Id,p.Id,new(t.Id,"Instalar llanta nueva"),sp.GetRequiredService<IOperacionService>(),Ct,sp.GetRequiredService<ICicloVidaLlantaService>()));Assert.False(await db.Movimientos.AnyAsync(x=>x.InspeccionId==inspection.Id));Assert.Null((await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
    }

    [Fact]public async Task Correccion_FalloAlInsertarEntranteRevierteSalidaYAuditoria()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(old,v,p)=await Setup(db);var(found,_,_)=await Setup(db);await Mount(sp.GetRequiredService<IOperacionService>(),old,p);
        var inspection=await sp.GetRequiredService<IInspeccionService>().CrearAsync(new(){VehiculoId=v.Id,Kilometraje=1200},Technician,new(true,[]),Ct);
        await using var failing=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer(db.Database.GetConnectionString()).AddInterceptors(new FailIncoming(found.Id)).Options);
        var controller=new InspeccionesController(sp.GetRequiredService<IInspeccionService>(),failing,sp.GetRequiredService<IWebHostEnvironment>()){ControllerContext=Context(Technician,"TECNICO",v.CentroId)};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>controller.Asignar(inspection.Id,p.Id,new(found.Id,"Corrección observada",true,found.Codigo,old.Id),new OperacionService(failing),Ct,sp.GetRequiredService<ICicloVidaLlantaService>()));
        db.ChangeTracker.Clear();Assert.Equal(old.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);Assert.True(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==old.Id&&x.EsActiva));Assert.False(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==found.Id));Assert.False(await db.InconsistenciasInspeccion.AnyAsync(x=>x.InspeccionId==inspection.Id));Assert.False(await db.Movimientos.AnyAsync(x=>x.InspeccionId==inspection.Id));
    }

    [Fact]public async Task Correccion_NoConsumeReservaManualAjena()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
        var inventory=new InventarioController(db){ControllerContext=Context("otro","TECNICO",v.CentroId)};await inventory.Reservar(t.Id,new(null,null,null,"Reservada"),Ct);
        var inspection=await sp.GetRequiredService<IInspeccionService>().CrearAsync(new(){VehiculoId=v.Id,Kilometraje=1000},Technician,new(true,[]),Ct);
        await Assert.ThrowsAsync<ConflictoException>(()=>Inspections(sp,db,center:v.CentroId).Asignar(inspection.Id,p.Id,new(t.Id,"Encontrada",true,t.Codigo),sp.GetRequiredService<IOperacionService>(),Ct,sp.GetRequiredService<ICicloVidaLlantaService>()));
        Assert.True(await db.SolicitudesOperacion.AnyAsync(x=>x.LlantaId==t.Id&&x.Tipo=="RESERVA"&&x.Activo));Assert.False(await db.Movimientos.AnyAsync(x=>x.InspeccionId==inspection.Id));
    }

    private sealed class FailIncoming(Guid tireId):SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,InterceptionResult<int> result,CancellationToken cancellationToken=default)
        {
            if(eventData.Context!.ChangeTracker.Entries<AsignacionLlantaPosicion>().Any(x=>x.State==EntityState.Added&&x.Entity.LlantaId==tireId))throw new InvalidOperationException("Fallo de persistencia simulado tras cerrar la saliente");
            return ValueTask.FromResult(result);
        }
    }
}
