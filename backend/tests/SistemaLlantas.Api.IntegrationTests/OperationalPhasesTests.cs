using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class OperationalPhasesTests:IClassFixture<WebApplicationFactory<Program>>
{
 private readonly WebApplicationFactory<Program> factory;
 public OperationalPhasesTests(WebApplicationFactory<Program> factory)=>this.factory=factory;

 [Fact]
 public async Task OperacionNoProgramada_RequiereDecisionYRegistraRechazo()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var tire=await db.Llantas.AsNoTracking().FirstAsync(x=>!db.SolicitudesOperacion.Any(s=>s.LlantaId==x.Id&&s.Activo));var controller=new OperacionesController(scope.ServiceProvider.GetRequiredService<IOperacionService>(),scope.ServiceProvider.GetRequiredService<ICicloVidaLlantaService>(),db){ControllerContext=Context("qa-operaciones","centros.ver_todos","operaciones.solicitar","operaciones.aprobar","operaciones.aprobar_propia","operaciones.montar")};Guid createdId=Guid.Empty;
  try{var created=await controller.Solicitar(new(){Tipo="Movimiento",LlantaId=tire.Id,TipoDestino="Inventario",Motivo="Prueba de aprobación"},CancellationToken.None);var pending=Assert.IsType<SolicitudOperacionDto>(Assert.IsType<CreatedResult>(created.Result).Value);createdId=pending.Id;Assert.Equal("PENDIENTE_APROBACION",pending.Estado);var rejected=await controller.Resolver(pending.Id,new(false,"No autorizado en QA"),CancellationToken.None);Assert.Equal("RECHAZADO",rejected.Estado);Assert.Equal("qa-operaciones",rejected.Aprobador);Assert.Equal("No autorizado en QA",rejected.MotivoRechazo);}finally{if(createdId!=Guid.Empty){db.ChangeTracker.Clear();var cleanup=await db.SolicitudesOperacion.SingleOrDefaultAsync(x=>x.Id==createdId);if(cleanup is not null){db.SolicitudesOperacion.Remove(cleanup);await db.SaveChangesAsync();}}}
 }

 [Fact]
 public async Task ServicioLlanta_ExigeAprobacionAntesDeEnvio()
 {
  _=factory.CreateClient();await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<LlantasDbContext>();var strategy=db.Database.CreateExecutionStrategy();await strategy.ExecuteAsync(async()=>
  {
   await using var tx=await db.Database.BeginTransactionAsync();var tire=await db.Llantas.AsNoTracking().FirstAsync(x=>!db.OrdenesServicioLlanta.Any(o=>o.LlantaId==x.Id&&o.Activo));var controller=new ServiciosLlantaController(db,scope.ServiceProvider.GetRequiredService<IOperacionService>(),scope.ServiceProvider.GetRequiredService<ICicloVidaLlantaService>(),scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>()){ControllerContext=Context("qa-servicios","centros.ver_todos","servicios_llanta.consultar","servicios_llanta.gestionar")};
   var created=await controller.Crear(new("Reparacion",tire.Id,null,null,"Diagnóstico QA",null),CancellationToken.None);var pending=Assert.IsType<ServiciosLlantaController.OrdenDto>(Assert.IsType<CreatedResult>(created.Result).Value);Assert.Equal("PENDIENTE_APROBACION",pending.Estado);await Assert.ThrowsAsync<SistemaLlantas.Application.Common.ConflictoException>(()=>controller.Enviar(pending.Id,CancellationToken.None));var approved=await controller.Aprobar(pending.Id,CancellationToken.None);Assert.Equal("APROBADA",approved.Estado);Assert.Equal("qa-servicios",(await db.OrdenesServicioLlanta.SingleAsync(x=>x.Id==pending.Id)).Aprobador);await tx.RollbackAsync();
  });
 }

 [Fact]
 public async Task CargaReportesYRolIntermedio_RespetanContratoReal()
 {
  var admin=factory.CreateClient();var adminLogin=await Login(admin,"administrador","admin123");Assert.Contains("operaciones.montar",adminLogin.Permissions);var template=await admin.GetAsync("/api/carga-masiva/plantillas/llantas?formato=csv");template.EnsureSuccessStatusCode();Assert.Contains("TipoLlanta",await template.Content.ReadAsStringAsync());var csv=await admin.GetAsync("/api/reportes/vehiculos?formato=csv");csv.EnsureSuccessStatusCode();Assert.Equal("text/csv",csv.Content.Headers.ContentType?.MediaType);var xlsx=await admin.GetAsync("/api/reportes/movimientos?formato=xlsx");xlsx.EnsureSuccessStatusCode();Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",xlsx.Content.Headers.ContentType?.MediaType);
  var intermediate=factory.CreateClient();var intermediateLogin=await Login(intermediate,"supervisoradmin","supadmin123");Assert.Contains("operaciones.solicitar",intermediateLogin.Permissions);Assert.DoesNotContain("operaciones.montar",intermediateLogin.Permissions);Assert.DoesNotContain("modulo.administracion",intermediateLogin.Permissions);Assert.DoesNotContain("modulo.programacion",intermediateLogin.Permissions);
 }

 private static ControllerContext Context(string username,params string[] permissions)=>new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("username",username)}.Concat(permissions.Select(x=>new Claim("permiso",x))),"test"))}};
 private static async Task<LoginResponse> Login(HttpClient client,string username,string password){var response=await client.PostAsJsonAsync("/api/auth/login",new{username,password});response.EnsureSuccessStatusCode();var login=(await response.Content.ReadFromJsonAsync<LoginResponse>())!;client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",login.AccessToken);return login;}
 private sealed record LoginResponse(string AccessToken,string[] Permissions);
}
