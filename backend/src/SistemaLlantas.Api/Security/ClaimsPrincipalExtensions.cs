using System.Security.Claims;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static AlcanceCentros AlcanceCentros(this ClaimsPrincipal user) => new(
        user.HasClaim("permiso", "centros.ver_todos"),
        user.FindAll("centro_id").Select(x => Guid.TryParse(x.Value, out var id) ? id : Guid.Empty).Where(x => x != Guid.Empty).Distinct().ToArray());

    public static string Username(this ClaimsPrincipal user) => user.FindFirstValue("username") ?? user.Identity?.Name ?? "sistema";
}
