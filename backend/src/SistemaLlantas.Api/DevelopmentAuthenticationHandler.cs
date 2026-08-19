using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SistemaLlantas.Api;

public sealed class DevelopmentAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "Development";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestedUser = Request.Headers["X-Development-User"].FirstOrDefault()?.Trim().ToLowerInvariant();
        var userId = requestedUser switch
        {
            "tecnico" => "tecnico.local",
            "supervisor" => "supervisor.local",
            _ => "administrador.local"
        };
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "Administrador local"),
            new("permiso", "llantas.consultar"), new("permiso", "llantas.administrar"), new("permiso", "catalogos.administrar"),
            new("permiso", "inspecciones.consultar"), new("permiso", "inspecciones.crear"), new("permiso", "inspecciones.reportar_inconsistencia"),
            new("permiso", "inspecciones.autorizar_inconsistencia_llanta"), new("permiso", "operaciones.ejecutar")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationScheme));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationScheme)));
    }
}
