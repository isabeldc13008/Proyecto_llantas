using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed partial class MountAuthorizationInspectionTests
{
    private static InspeccionesController.RegistrarLlantaEncontradaDto Encontrada(Llanta sample,Guid position,Guid center,
        Guid? anterior=null,string? codigo=null,string? serial=null)=>new()
    {
        PosicionId=position,CentroId=center,LlantaAnteriorId=anterior,Codigo=codigo??"FIS-"+Guid.NewGuid().ToString("N"),
        Serial=serial??"FIS-"+Guid.NewGuid().ToString("N"),MarcaId=sample.MarcaId,ReferenciaId=sample.ReferenciaId,
        DimensionId=sample.DimensionId,TipoLlantaId=sample.TipoLlantaId,ProfundidadInicial=12,
        DiscrepanciaFisica=true,Motivo="Identidad leída durante inspección física"
    };
    private static Task<ActionResult<ContextoInspeccionDto>> RegistrarEncontrada(InspeccionesController c,Guid id,
        InspeccionesController.RegistrarLlantaEncontradaDto dto,IServiceProvider sp)=>c.RegistrarLlantaEncontrada(id,dto,
        sp.GetRequiredService<ILlantaService>(),sp.GetRequiredService<IOperacionService>(),sp.GetRequiredService<ICicloVidaLlantaService>(),Ct);

    [Theory][InlineData(false)][InlineData(true)]
    public async Task LlantaEncontrada_CreaMontaAuditaYPermiteContinuar(bool ocupada)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;
        var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
        if(ocupada)await sp.GetRequiredService<IOperacionService>().MoverAsync(new(){LlantaId=t.Id,PosicionDestinoId=p.Id,TipoDestino="Posicion",Motivo="Previa",KilometrajeVehiculo=1000},Technician,new(true,[]),Ct);
        var c=Inspections(sp,db,center:v.CentroId);
        var inspection=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await c.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
        if(ocupada)await c.Detalle(inspection.Id,p.Id,new(){ProfundidadExterior=5,ProfundidadCentro=5,ProfundidadInterior=5},Ct);
        var dto=Encontrada(t,p.Id,v.CentroId,ocupada?t.Id:null);
        Assert.IsType<CreatedResult>((await RegistrarEncontrada(c,inspection.Id,dto,sp)).Result);
        db.ChangeTracker.Clear();var nueva=await db.Llantas.Include(x=>x.EstadoLlanta).SingleAsync(x=>x.Codigo==dto.Codigo);
        Assert.True(nueva.Activo);Assert.Equal(Technician,nueva.UsuarioCreacion);Assert.Equal(v.CentroId,nueva.CentroId);
        Assert.Contains("Llanta registrada durante inspección física",nueva.Observaciones);Assert.Contains(nueva.EstadoLlanta.Codigo,new[]{"MON","MONTADA"});
        Assert.Equal(nueva.Id,(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
        Assert.Equal(nueva.Id,Assert.Single(await db.AsignacionesLlantaPosicion.Where(x=>x.PosicionVehiculoId==p.Id&&x.EsActiva).ToListAsync()).LlantaId);
        var detalle=await db.InspeccionesDetalle.SingleAsync(x=>x.InspeccionId==inspection.Id&&x.PosicionVehiculoId==p.Id);Assert.Null(detalle.ProfundidadExterior);
        var correccion=Assert.Single(await db.MovimientosLlanta.Where(x=>x.InspeccionId==inspection.Id).ToListAsync());
        Assert.Equal(nueva.Id,correccion.LlantaNuevaId);Assert.Equal(ocupada?t.Id:(Guid?)null,correccion.LlantaAnteriorId);Assert.Equal(Technician,correccion.UsuarioCreacion);
        await c.Detalle(inspection.Id,p.Id,new(){ProfundidadExterior=11,ProfundidadCentro=11,ProfundidadInterior=11},Ct);
        db.ChangeTracker.Clear();Assert.Equal(11,(await db.InspeccionesDetalle.SingleAsync(x=>x.Id==detalle.Id)).ProfundidadCentro);
        await Assert.ThrowsAsync<ConflictoException>(()=>RegistrarEncontrada(c,inspection.Id,dto,sp));
        Assert.Equal(1,await db.Llantas.CountAsync(x=>x.Codigo==dto.Codigo));
    }

    [Theory][InlineData(true)][InlineData(false)]
    public async Task LlantaEncontrada_NoDuplicaCodigoNiSerial(bool codigo)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
        var c=Inspections(sp,db,center:v.CentroId);var i=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await c.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
        await Assert.ThrowsAsync<ConflictoException>(()=>RegistrarEncontrada(c,i.Id,Encontrada(t,p.Id,v.CentroId,codigo:codigo?t.Codigo.ToLowerInvariant():null,serial:codigo?null:t.Serial.ToLowerInvariant()),sp));
        db.ChangeTracker.Clear();Assert.Null((await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
    }

    [Theory][InlineData("ajena")][InlineData("centro")][InlineData("finalizada")]
    public async Task LlantaEncontrada_ExigeInspeccionPropiaBorradorYAlcance(string caso)
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db,true);
        var c=Inspections(sp,db,center:v.CentroId);var i=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await c.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
        if(caso=="finalizada"){(await db.Inspecciones.SingleAsync(x=>x.Id==i.Id)).Estado=EstadoInspeccion.Finalizada;await db.SaveChangesAsync();}
        var denied=Inspections(sp,db,user:caso=="ajena"?"otro":Technician,center:caso=="centro"?t.CentroId:v.CentroId);
        var dto=Encontrada(t,p.Id,v.CentroId);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>RegistrarEncontrada(denied,i.Id,dto,sp));
        Assert.False(await db.Llantas.AnyAsync(x=>x.Codigo==dto.Codigo));
    }

    [Fact]
    public async Task LlantaEncontrada_RollbackIncluyeCreacionSiFallaMontaje()
    {
        _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,p)=await Setup(db);
        var c=Inspections(sp,db,center:v.CentroId);var i=Assert.IsType<InspeccionDto>(Assert.IsType<CreatedAtActionResult>((await c.Crear(new(){VehiculoId=v.Id,Kilometraje=1000},Ct)).Result).Value);
        v.Kilometraje=2000;await db.SaveChangesAsync();var dto=Encontrada(t,p.Id,v.CentroId);
        await Assert.ThrowsAsync<ValidacionException>(()=>RegistrarEncontrada(c,i.Id,dto,sp));db.ChangeTracker.Clear();
        Assert.False(await db.Llantas.AnyAsync(x=>x.Codigo==dto.Codigo));Assert.Null((await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p.Id)).LlantaActualId);
        Assert.False(await db.MovimientosLlanta.AnyAsync(x=>x.InspeccionId==i.Id));Assert.False(await db.InconsistenciasInspeccion.AnyAsync(x=>x.InspeccionId==i.Id));
    }
}
