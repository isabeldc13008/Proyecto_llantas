using Microsoft.AspNetCore.Builder;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class AuthenticationConfigurationTests
{
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
