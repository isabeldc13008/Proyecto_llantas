using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed partial class MountAuthorizationInspectionTests
{
 private static void GlobalDisposicion(ServiciosLlantaController c,string user){c.ControllerContext=Context(user,"ADMINISTRADOR");((ClaimsIdentity)c.User.Identity!).AddClaim(new("permiso","centros.ver_todos"));}
 [Fact]public async Task Disposicion_LoteMultipleParcialIdempotenteConDestinoHistoricoYCierre()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,_)=await Setup(db);var(t2,_,_)=await Setup(db);
  var destino=new Centro{Codigo="MED-"+Guid.NewGuid().ToString("N")[..8],Nombre="Destino seleccionado",Relevancia="R1"};db.Centros.Add(destino);var empresa=new ProveedorServicio{Codigo="DSP-"+Guid.NewGuid().ToString("N")[..8],Nombre="Receptora",Tipo="DisposicionFinal"};db.ProveedoresServicio.Add(empresa);await db.SaveChangesAsync();
  var c=Servicios(sp,db,v.CentroId);GlobalDisposicion(c,"tecnico");var ids=new List<Guid>();
  foreach(var tire in new[]{t,t2}){var order=OrdenCreada(await c.Crear(new("DisposicionFinal",tire.Id,null,null,"Evaluar",null),Ct));await c.EvaluarDisposicion(order.Id,new(false,"Concepto técnico"),Ct);ids.Add(order.Id);}
  GlobalDisposicion(c,"aprobador");foreach(var id in ids)await c.Aprobar(id,Ct);
  var body=new ServiciosLlantaController.CrearLoteDisposicionDto(ids,v.CentroId,destino.Id,DateTimeOffset.UtcNow,"REM","Transporte","Consolidado",Guid.NewGuid().ToString());
  var lote=Assert.IsType<ServiciosLlantaController.LoteDisposicionDto>(Assert.IsType<CreatedResult>((await c.CrearLoteDisposicion(body,Ct)).Result).Value);Assert.Equal(2,lote.Items.Count);Assert.Equal("EN_TRANSITO",lote.Estado);
  var repeated=Assert.IsType<ServiciosLlantaController.LoteDisposicionDto>(Assert.IsType<CreatedResult>((await c.CrearLoteDisposicion(body,Ct)).Result).Value);Assert.Equal(lote.Id,repeated.Id);Assert.Equal(2,await db.Movimientos.CountAsync(m=>m.Motivo.Contains(lote.Codigo)));
  await Assert.ThrowsAsync<ConflictoException>(()=>c.CrearLoteDisposicion(body with{CentroDestinoId=v.CentroId},Ct));
  db.ChangeTracker.Clear();var centro=await db.Centros.SingleAsync(x=>x.Id==destino.Id);centro.Relevancia="R2";await db.SaveChangesAsync();
  c.ControllerContext=Context("origen","TECNICO",v.CentroId);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>c.RecibirLoteDisposicion(lote.Id,new([ids[0]]),Ct));
  c.ControllerContext=Context("receptor","TECNICO",destino.Id);var parcial=await c.RecibirLoteDisposicion(lote.Id,new([ids[0]]),Ct);Assert.Equal("RECEPCION_PARCIAL",parcial.Estado);Assert.Equal("R1",parcial.RelevanciaDestino);Assert.Equal(destino.Id,parcial.CentroDestinoId);
  await Assert.ThrowsAsync<ConflictoException>(()=>c.RecibirLoteDisposicion(lote.Id,new([ids[0]]),Ct));
  await Assert.ThrowsAsync<ValidacionException>(()=>c.CerrarLoteDisposicion(lote.Id,new([ids[0]],empresa.Id,"Cierre"),Ct));
  db.ChangeTracker.Clear();db.EvidenciasFlujo.Add(new(){OrdenServicioLlantaId=ids[0],NombreArchivo="foto.jpg",MimeType="image/jpeg",Ubicacion="qa",Hash="qa"});await db.SaveChangesAsync();
  await Assert.ThrowsAsync<ValidacionException>(()=>c.CerrarLoteDisposicion(lote.Id,new([ids[0]],empresa.Id,"Cierre"),Ct));
  await c.RecibirLoteDisposicion(lote.Id,new([ids[1]]),Ct);db.ChangeTracker.Clear();db.EvidenciasFlujo.Add(new(){OrdenServicioLlantaId=ids[1],NombreArchivo="foto.jpg",MimeType="image/jpeg",Ubicacion="qa",Hash="qa"});await db.SaveChangesAsync();
  await Assert.ThrowsAsync<ValidacionException>(()=>c.CerrarLoteDisposicion(lote.Id,new([ids[1]],empresa.Id,"Cierre"),Ct));var received=(await c.LotesDisposicion(Ct)).Single(x=>x.Id==lote.Id);Assert.Equal("RECIBIDO",received.Estado);Assert.All(received.Items,i=>Assert.Equal("PENDIENTE_DISPOSICION",i.Estado));
 }
 [Theory][InlineData("centro")][InlineData("montada")][InlineData("pendiente")][InlineData("inactiva")][InlineData("traslado")]
 public async Task Disposicion_LoteInvalidoNoEnviaNinguna(string caso)
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<LlantasDbContext>();var(t,v,_)=await Setup(db);var(t2,_,p2)=await Setup(db);var other=await db.Centros.FirstAsync(x=>x.Id!=v.CentroId);var c=Servicios(sp,db,v.CentroId);GlobalDisposicion(c,"tecnico");
  var ids=new List<Guid>();foreach(var tire in new[]{t,t2}){var o=OrdenCreada(await c.Crear(new("DisposicionFinal",tire.Id,null,null,"Evaluar",null),Ct));await c.EvaluarDisposicion(o.Id,new(false,"Concepto"),Ct);ids.Add(o.Id);}GlobalDisposicion(c,"aprobador");foreach(var id in ids)await c.Aprobar(id,Ct);
  db.ChangeTracker.Clear();var bad=await db.Llantas.SingleAsync(x=>x.Id==t2.Id);if(caso=="centro")bad.CentroId=other.Id;if(caso=="montada")(await db.PosicionesVehiculo.SingleAsync(x=>x.Id==p2.Id)).LlantaActualId=t2.Id;if(caso=="pendiente")(await db.OrdenesServicioLlanta.SingleAsync(x=>x.Id==ids[1])).Estado="PENDIENTE_APROBACION";if(caso=="inactiva")bad.Activo=false;if(caso=="traslado")bad.EstadoLlantaId=await db.EstadosLlanta.Where(x=>x.Codigo=="EN_TRASLADO").Select(x=>x.Id).FirstAsync();await db.SaveChangesAsync();
  var key=Guid.NewGuid().ToString();await Assert.ThrowsAsync<ConflictoException>(()=>c.CrearLoteDisposicion(new(ids,v.CentroId,other.Id,DateTimeOffset.UtcNow,null,null,null,key),Ct));db.ChangeTracker.Clear();Assert.False(await db.LotesDisposicionFinal.AnyAsync(x=>x.IdempotencyKey==key));Assert.Null((await db.OrdenesServicioLlanta.SingleAsync(x=>x.Id==ids[0])).LoteDisposicionFinalId);Assert.Equal(v.CentroId,(await db.Llantas.SingleAsync(x=>x.Id==t.Id)).CentroId);
 }
 [Theory][InlineData(null)][InlineData("R1")][InlineData("R2")][InlineData("R3")][InlineData("R4")]
 public async Task Centro_GuardaYRecuperaRelevancia(string? relevancia)
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var regional=new Regional{Codigo=Guid.NewGuid().ToString("N")[..10],Nombre="Regional QA"};db.Regionales.Add(regional);await db.SaveChangesAsync();var service=new CatalogoService(db);var dto=new SistemaLlantas.Application.Catalogos.GuardarCatalogoDto(Guid.NewGuid().ToString("N")[..10],"Centro QA",regional.Id,relevancia);var created=await service.CrearAsync("centros",dto,"qa",Ct);Assert.Equal(relevancia,created.Relevancia);Assert.Equal(relevancia,(await db.Centros.SingleAsync(c=>c.Id==created.Id)).Relevancia);Assert.Null((await service.ActualizarAsync("centros",created.Id,dto with{Relevancia=null},"qa",Ct))!.Relevancia);
 }
}
public sealed class LotesDisposicionValidacionTests
{
 [Theory][InlineData("")][InlineData("R5")][InlineData("r1")][InlineData("R1 ")]
 public void RechazaRelevanciaInvalida(string valor)=>Assert.Throws<ValidacionException>(()=>CatalogoService.ValidarRelevancia(valor));
 [Theory][InlineData(null)][InlineData("R1")][InlineData("R2")][InlineData("R3")][InlineData("R4")]
 public void AceptaRelevancias(string? valor)=>CatalogoService.ValidarRelevancia(valor);
 [Fact]public void RechazaSeleccionDuplicadaOVacia(){var id=Guid.NewGuid();Assert.Throws<ValidacionException>(()=>ServiciosLlantaController.ValidarSeleccionDisposicion([]));Assert.Throws<ValidacionException>(()=>ServiciosLlantaController.ValidarSeleccionDisposicion([id,id]));}
}
