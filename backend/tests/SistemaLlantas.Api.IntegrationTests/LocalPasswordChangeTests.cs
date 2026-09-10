using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class LocalPasswordChangeTests
{
    [Theory]
    [InlineData("Local", true, false)]
    [InlineData(null, true, false)]
    [InlineData("Local", false, true)]
    [InlineData("Entra", true, true)]
    public async Task GateOnlyBlocksPendingLocalUsers(string? mode, bool pending, bool allowed)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Authentication:Mode"] = mode }).Build();
        var user = new UsuarioSistema { Nombre = "Test", DebeCambiarClave = pending, Rol = new RolSistema { Codigo = "CUSTOM" } };
        var context = new DefaultHttpContext { User = AuthenticationConfiguration.CreatePrincipal(user, true) };
        context.Response.Body = new MemoryStream();
        var called = false;
        await new LocalPasswordChangeMiddleware(_ => { called = true; return Task.CompletedTask; }).InvokeAsync(context, config);
        Assert.Equal(allowed, called);
        if (!allowed) Assert.Equal(403, context.Response.StatusCode);
    }

    [Theory]
    [InlineData(nameof(AuthController.Me))]
    [InlineData(nameof(AuthController.CambiarClave))]
    [InlineData(nameof(AuthController.Login))]
    public async Task RequiredAuthActionsRemainAccessible(string actionName)
    {
        var context = new DefaultHttpContext { User = AuthenticationConfiguration.CreatePrincipal(new UsuarioSistema
            { DebeCambiarClave = true, Rol = new RolSistema() }, true) };
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new ControllerActionDescriptor
            { ControllerTypeInfo = typeof(AuthController).GetTypeInfo(), ActionName = actionName }), "auth"));
        var called = false;
        await new LocalPasswordChangeMiddleware(_ => { called = true; return Task.CompletedTask; })
            .InvokeAsync(context, new ConfigurationBuilder().Build());
        Assert.True(called);
    }

    [Fact]
    public void FlagDefaultsFalseAndClaimsFollowDatabaseStateOnlyForLocal()
    {
        var user = new UsuarioSistema { Rol = new RolSistema() };
        Assert.False(user.DebeCambiarClave);
        user.DebeCambiarClave = true;
        Assert.False(AuthenticationConfiguration.CreatePrincipal(user, false).HasClaim("requiere_cambio_clave", "true"));
        Assert.True(AuthenticationConfiguration.CreatePrincipal(user, true).HasClaim("requiere_cambio_clave", "true"));
        user.DebeCambiarClave = false;
        Assert.False(AuthenticationConfiguration.CreatePrincipal(user, true).HasClaim("requiere_cambio_clave", "true"));
    }

    [Theory]
    [InlineData("1234567", false)]
    [InlineData("12345678", true)]
    [InlineData("        ", false)]
    public void NewPasswordHasMinimumAndRejectsWhitespace(string password, bool valid)
    {
        var attributes = typeof(AuthController.CambiarClaveRequest).GetConstructors().Single().GetParameters()
            .Single(p => p.Name == "Nueva").GetCustomAttributes<ValidationAttribute>();
        Assert.Equal(valid, attributes.All(a => a.IsValid(password)));
    }
}
