using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaLlantas.Api.Middleware;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class OperationErrorResponseTests
{
 [Theory][InlineData(false,404,"La solicitud no existe")][InlineData(true,409,"ya fue procesada")]
 public async Task RespuestasConservanMensajeFuncional(bool conflict,int status,string expected)
 {
  var context=new DefaultHttpContext();context.Response.Body=new MemoryStream();
  var middleware=new ApiExceptionMiddleware(_=>throw (conflict?new ConflictoException("La solicitud ya fue procesada: EJECUTADO."):new SolicitudNoEncontradaException()),NullLogger<ApiExceptionMiddleware>.Instance);
  await middleware.InvokeAsync(context);Assert.Equal(status,context.Response.StatusCode);context.Response.Body.Position=0;var text=await new StreamReader(context.Response.Body).ReadToEndAsync();Assert.Contains(expected,text);
 }
}
