using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class AuthenticationConfigurationTests
{
    private static WebApplicationBuilder Builder(string environment, string? key)
    {
        var b = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        b.Configuration["Jwt:Issuer"] = "SistemaLlantas";
        b.Configuration["Jwt:Audience"] = "SistemaLlantas.Web";
        b.Configuration["Jwt:Key"] = key;
        return b;
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public void ProductionRequiresStrongKey(string? key) =>
        Assert.Throws<InvalidOperationException>(() => Builder("Production", key).AddApplicationAuthentication());

    [Fact]
    public void DevelopmentGeneratesTemporaryKey()
    {
        var b = Builder("Development", null); b.AddApplicationAuthentication();
        Assert.True(Encoding.UTF8.GetByteCount(b.Configuration["Jwt:Key"]!) >= 32);
    }
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task PasswordJwtWorksInBothEnvironmentsAndRejectsInvalidTokens(string environment)
    {
        var key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        var b = Builder(environment, key); b.AddApplicationAuthentication();
        await using var app = b.Build();
        var options = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
        Assert.Null(options.Authority);
        string Token(string audience, string signingKey, DateTime expires) => new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken("SistemaLlantas", audience, expires: expires,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256)));
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(Token("SistemaLlantas.Web", key, DateTime.UtcNow.AddMinutes(5)), options.TokenValidationParameters, out _);
        Assert.ThrowsAny<SecurityTokenException>(() => handler.ValidateToken(Token("wrong", key, DateTime.UtcNow.AddMinutes(5)), options.TokenValidationParameters, out _));
        Assert.ThrowsAny<SecurityTokenException>(() => handler.ValidateToken(Token("SistemaLlantas.Web", new string('x',48), DateTime.UtcNow.AddMinutes(5)), options.TokenValidationParameters, out _));
        Assert.ThrowsAny<SecurityTokenException>(() => handler.ValidateToken(Token("SistemaLlantas.Web", key, DateTime.UtcNow.AddMinutes(-5)), options.TokenValidationParameters, out _));
    }
    [Fact]
    public void InternalClaimsExcludeInactiveCentersAssignmentsAndPermissions()
    {
        var allowed = new Centro { Codigo = "A", Nombre = "A" };
        var user = new UsuarioSistema
        {
            Username = "user@example.test", Nombre = "Test User", Rol = new RolSistema { Codigo = "TECNICO", Nombre = "Técnico" },
            Centros = [new UsuarioCentro { Centro = allowed, CentroId = allowed.Id },
                new UsuarioCentro { Centro = new Centro { Activo = false }, CentroId = Guid.NewGuid() },
                new UsuarioCentro { Centro = allowed, CentroId = Guid.NewGuid(), Activo = false }]
        };
        user.Rol.Permisos.Add(new RolPermiso { Permiso = new PermisoSistema { Codigo = "llantas.consultar" } });
        user.Rol.Permisos.Add(new RolPermiso { Permiso = new PermisoSistema { Codigo = "centros.ver_todos", Activo = false } });
        var principal = AuthenticationConfiguration.CreatePrincipal(user);
        Assert.Equal(allowed.Id.ToString(), Assert.Single(principal.FindAll("centro_id")).Value);
        Assert.Equal("llantas.consultar", Assert.Single(principal.FindAll("permiso")).Value);
        Assert.False(principal.AlcanceCentros().VerTodos);
        Assert.True(principal.IsInRole("TECNICO"));
    }
}
