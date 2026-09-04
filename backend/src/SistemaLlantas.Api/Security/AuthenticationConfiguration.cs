using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Security;

public static class AuthenticationConfiguration
{
    public static bool IsLocal(IConfiguration config, IHostEnvironment environment) =>
        environment.IsDevelopment() && config["Authentication:Mode"] == "Local";

    public static void AddApplicationAuthentication(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var local = IsLocal(config, builder.Environment);
        if (!local && config["Authentication:Mode"] != "Entra")
            throw new InvalidOperationException("Producción requiere Authentication:Mode=Entra.");
        var tenant = config["Entra:TenantId"];
        var audience = config["Entra:ClientId"];
        if (!local && (!Guid.TryParse(tenant, out _) || !Guid.TryParse(audience, out _)))
            throw new InvalidOperationException("Configure Entra:TenantId y Entra:ClientId (registro de la API).");
        if (local)
        {
            config["Jwt:Key"] ??= Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
            if (Encoding.UTF8.GetByteCount(config["Jwt:Key"]!) < 32)
                throw new InvalidOperationException("Jwt:Key requiere al menos 32 bytes.");
        }
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            if (local)
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"], ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromMinutes(1)
                };
            else
            {
                options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
                options.Audience = audience;
                options.TokenValidationParameters.ValidIssuer = options.Authority;
                options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.RsaSha256];
            }
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal!;
                    var db = context.HttpContext.RequestServices.GetRequiredService<LlantasDbContext>();
                    var query = db.UsuariosSistema.AsNoTracking().Include(x => x.Centros).ThenInclude(x => x.Centro)
                        .Include(x => x.Rol).ThenInclude(x => x.Permisos).ThenInclude(x => x.Permiso)
                        .Where(x => x.Activo && x.Rol.Activo);
                    UsuarioSistema? user;
                    if (local)
                    {
                        if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id)) { context.Fail("Identidad inválida."); return; }
                        user = await query.SingleOrDefaultAsync(x => x.Id == id, context.HttpContext.RequestAborted);
                    }
                    else
                    {
                        var scopes = principal.FindFirstValue("scp")?.Split(' ') ?? [];
                        if (principal.FindFirstValue("tid") != tenant || !scopes.Contains(config["Entra:Scope"] ?? "access_as_user"))
                        { context.Fail("Se requiere un token delegado de la API y del tenant configurado."); return; }
                        var username = (principal.FindFirstValue("preferred_username") ?? principal.FindFirstValue("upn"))?.Trim().ToLowerInvariant();
                        if (!Guid.TryParse(principal.FindFirstValue("oid"), out var oid) || string.IsNullOrWhiteSpace(username))
                        { context.Fail("Identidad corporativa incompleta."); return; }
                        user = await query.SingleOrDefaultAsync(x => x.Username == username && x.EntraObjectId == oid,
                            context.HttpContext.RequestAborted);
                    }
                    if (user is null) { context.Fail("Usuario interno no habilitado."); return; }
                    // Replace all external authorization claims; SQL is the only authority for permissions and centers.
                    context.Principal = CreatePrincipal(user);
                }
            };
        });
    }

    public static ClaimsPrincipal CreatePrincipal(UsuarioSistema user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), new("username", user.Username),
            new(ClaimTypes.Name, user.Nombre), new(ClaimTypes.Role, user.Rol.Codigo), new("rol_nombre", user.Rol.Nombre)
        };
        claims.AddRange(user.Centros.Where(x => x.Activo && x.Centro.Activo).Select(x => new Claim("centro_id", x.CentroId.ToString())));
        claims.AddRange(user.Rol.Permisos.Where(x => x.Permiso.Activo).Select(x => new Claim("permiso", x.Permiso.Codigo)));
        return new(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
    }
}
