using Microsoft.AspNetCore.Mvc.Controllers;
using SistemaLlantas.Api.Controllers;

namespace SistemaLlantas.Api.Security;

public sealed class LocalPasswordChangeMiddleware(RequestDelegate next)
{
    public static bool EsLocal(IConfiguration config) =>
        string.Equals(config["Authentication:Mode"] ?? "Local", "Local", StringComparison.OrdinalIgnoreCase);

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        if (EsLocal(config) && context.User.Identity?.IsAuthenticated == true && context.User.HasClaim("requiere_cambio_clave", "true"))
        {
            var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var allowed = action?.ControllerTypeInfo.AsType() == typeof(AuthController) &&
                action.ActionName is nameof(AuthController.Me) or nameof(AuthController.CambiarClave) or nameof(AuthController.Login) or nameof(AuthController.LocalConfig);
            if (!allowed)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { code = "CAMBIO_CLAVE_REQUERIDO", message = "Debes cambiar tu contraseña antes de continuar." });
                return;
            }
        }
        await next(context);
    }
}
