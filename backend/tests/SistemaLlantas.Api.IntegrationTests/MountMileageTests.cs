using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaLlantas.Api.Middleware;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class MountMileageTests
{
 [Theory][InlineData(99999,400)][InlineData(100000,200)][InlineData(105000,200)]
 public async Task ReglaComunDevuelve400OExito(decimal kilometraje,int status)
 {
  var context=new DefaultHttpContext();context.Response.Body=new MemoryStream();
  await new ApiExceptionMiddleware(_=>{KilometrajeOperacion.Validar(kilometraje,100000);return Task.CompletedTask;},NullLogger<ApiExceptionMiddleware>.Instance).InvokeAsync(context);
  Assert.Equal(status,context.Response.StatusCode);
  if(status==400){context.Response.Body.Position=0;var json=await new StreamReader(context.Response.Body).ReadToEndAsync();Assert.Contains("100.000",json);Assert.Contains("VALIDATION_ERROR",json);}
 }
 [Fact]public void SegundaLlantaInvalidaSeDetectaEnLaValidacionCompleta()
 {
  var ex=Assert.Throws<ValidacionException>(()=>KilometrajeOperacion.Validar(105000,100000,[new("LL-1","P1",90000),new("LL-2","P2",110000)]));
  Assert.Contains("LL-2",ex.Message);Assert.Contains("P2",ex.Message);Assert.Contains("110.000",ex.Message);Assert.Contains("105.000",ex.Message);
 }
 [Theory][InlineData(-1)][InlineData(null)]public void ValorAusenteONegativo(int? valor)=>Assert.Throws<ValidacionException>(()=>KilometrajeOperacion.Validar(valor,100000));
 [Theory][InlineData(0)][InlineData(2)]public async Task AsignacionesInconsistentesSon409No500(int count)
 {
  var tire=Guid.NewGuid();var position=new PosicionVehiculo{Codigo="P1",LlantaActualId=tire};var rows=Enumerable.Range(0,count).Select(_=>new AsignacionLlantaPosicion{LlantaId=tire,PosicionVehiculoId=position.Id}).ToArray();
  var context=new DefaultHttpContext();context.Response.Body=new MemoryStream();await new ApiExceptionMiddleware(_=>{OperacionService.AsignacionConsistente(position,tire,rows);return Task.CompletedTask;},NullLogger<ApiExceptionMiddleware>.Instance).InvokeAsync(context);
  Assert.Equal(409,context.Response.StatusCode);context.Response.Body.Position=0;var json=await new StreamReader(context.Response.Body).ReadToEndAsync();Assert.Contains("P1",json);Assert.Contains("CONFLICT",json);
 }
 [Fact]public void PosicionLibreConsistenteNoRequiereSalida()=>Assert.Null(OperacionService.AsignacionConsistente(new(){Codigo="P1"},null,[]));
 [Fact]public void PosicionCambiadaEsConflicto()=>Assert.Throws<ConflictoException>(()=>OperacionService.AsignacionConsistente(new(){Codigo="P1",LlantaActualId=Guid.NewGuid()},Guid.NewGuid(),[]));
}
