using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SistemaLlantas.Api;

public sealed class DevelopmentAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Scheme = "Development";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "administrador.local"),
            new(ClaimTypes.Name, "Administrador local"),
            new("permiso", "llantas.consultar"), new("permiso", "llantas.administrar"), new("permiso", "catalogos.administrar"),
            new("permiso", "inspecciones.consultar"), new("permiso", "inspecciones.crear"), new("permiso", "inspecciones.reportar_inconsistencia"),
            new("permiso", "inspecciones.autorizar_inconsistencia_llanta"), new("permiso", "operaciones.ejecutar")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme)));
    }
}
