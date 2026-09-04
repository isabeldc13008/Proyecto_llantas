using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            var (status, code, message, errors) = ex switch
            {
                ValidacionException v => (400, "VALIDATION_ERROR", v.Message, v.Errores),
                UnauthorizedAccessException => (403, "FORBIDDEN", "No tiene acceso al recurso solicitado.", null),
                KeyNotFoundException => (404, "NOT_FOUND", "El recurso solicitado no existe.", null),
                ConflictoException => (409, "CONFLICT", ex.Message, null),
                DbUpdateConcurrencyException => (409, "CONCURRENCY_CONFLICT", "El registro fue modificado por otro usuario. Recargue e intente de nuevo.", null),
                _ => (500, "UNEXPECTED_ERROR", "Ocurrió un error inesperado.", null)
            };
            if (status == 500) logger.LogError(ex, "Error no controlado. TraceId {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = status; context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new { status, code, message, errors, traceId = context.TraceIdentifier });
        }
    }
}
