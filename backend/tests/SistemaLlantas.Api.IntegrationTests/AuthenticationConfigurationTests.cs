using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class AuthenticationConfigurationTests
{
    [Fact]
    public async Task EntraValidatesApiAudienceAndNormalizesTenant()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        var tenant = Guid.NewGuid(); var api = Guid.NewGuid();
        builder.Configuration["Authentication:Mode"] = "Entra";
        builder.Configuration["Entra:TenantId"] = tenant.ToString().ToUpperInvariant();
        builder.Configuration["Entra:ClientId"] = api.ToString();
        builder.AddApplicationAuthentication();
        await using var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
        Assert.Equal($"https://login.microsoftonline.com/{tenant}/v2.0", options.Authority);
        Assert.Equal(api.ToString(), options.Audience);
        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
    }

    [Fact]
    public void ApiScopeConfigurationRejectsTheFullSpaScopeUri()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["Authentication:Mode"] = "Entra";
        builder.Configuration["Entra:TenantId"] = Guid.NewGuid().ToString();
        builder.Configuration["Entra:ClientId"] = Guid.NewGuid().ToString();
        builder.Configuration["Entra:Scope"] = "api://api/access_as_user";
        Assert.Throws<InvalidOperationException>(() => builder.AddApplicationAuthentication());
    }

    [Fact]
    public void ProductionRejectsLocalPasswords()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["Authentication:Mode"] = "Local";
        Assert.Throws<InvalidOperationException>(() => builder.AddApplicationAuthentication());
    }

    [Fact]
    public void EntraRequiresExplicitTenantAndApiRegistration()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["Authentication:Mode"] = "Entra";
        Assert.Throws<InvalidOperationException>(() => builder.AddApplicationAuthentication());
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
