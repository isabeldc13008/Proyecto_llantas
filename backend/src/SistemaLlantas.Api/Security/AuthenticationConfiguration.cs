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
    public static void AddApplicationAuthentication(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        if (string.IsNullOrWhiteSpace(config["Jwt:Key"]))
        {
            if (!builder.Environment.IsDevelopment())
                throw new InvalidOperationException("Configure Jwt__Key con una clave aleatoria de al menos 32 bytes para producción.");
            config["Jwt:Key"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        }
        if (Encoding.UTF8.GetByteCount(config["Jwt:Key"]!) < 32)
            throw new InvalidOperationException("Jwt:Key requiere al menos 32 bytes.");
        if (string.IsNullOrWhiteSpace(config["Jwt:Issuer"]) || string.IsNullOrWhiteSpace(config["Jwt:Audience"]))
            throw new InvalidOperationException("Configure Jwt:Issuer y Jwt:Audience.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"], ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromMinutes(1)
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    if (!Guid.TryParse(context.Principal!.FindFirstValue("sub"), out var id))
                    { context.Fail("Identidad inválida."); return; }
                    var db = context.HttpContext.RequestServices.GetRequiredService<LlantasDbContext>();
                    var user = await db.UsuariosSistema.AsNoTracking().AsSplitQuery()
                        .Include(x => x.Centros).ThenInclude(x => x.Centro)
                        .Include(x => x.Rol).ThenInclude(x => x.Permisos).ThenInclude(x => x.Permiso)
                        .SingleOrDefaultAsync(x => x.Id == id && x.Activo && x.Rol.Activo, context.HttpContext.RequestAborted);
                    if (user is null) { context.Fail("Usuario interno no habilitado."); return; }
                    context.Principal = CreatePrincipal(user, LocalPasswordChangeMiddleware.EsLocal(config));
                }
            };
        });
    }

    public static ClaimsPrincipal CreatePrincipal(UsuarioSistema user, bool local = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), new("username", user.Username),
            new(ClaimTypes.Name, user.Nombre), new(ClaimTypes.Role, user.Rol.Codigo), new("rol_nombre", user.Rol.Nombre)
        };
        claims.AddRange(user.Centros.Where(x => x.Activo && x.Centro.Activo).Select(x => new Claim("centro_id", x.CentroId.ToString())));
        claims.AddRange(user.Rol.Permisos.Where(x => x.Permiso.Activo).Select(x => new Claim("permiso", x.Permiso.Codigo)));
        if (local && user.DebeCambiarClave) claims.Add(new("requiere_cambio_clave", "true"));
        return new(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
    }
}
