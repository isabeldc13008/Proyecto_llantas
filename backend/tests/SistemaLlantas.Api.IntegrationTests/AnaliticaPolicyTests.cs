using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemaLlantas.Api.Controllers;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class AnaliticaPolicyTests
{
    [Fact] public async Task ExistingModulePermissionIsRequiredForEveryAnalyticsEndpoint()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => {
            b.UseEnvironment("Development");b.ConfigureLogging(l => l.ClearProviders());
            b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?> { ["Authentication:Mode"] = "Local", ["Authentication:SeedDevelopmentUsers"] = "false" }));
        });
        using var scope = factory.Services.CreateScope();var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var policy = Assert.Single(typeof(AnaliticaController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Policy!;
        Assert.True((await auth.AuthorizeAsync(User("modulos.analitica.consultar"), null, policy)).Succeeded);
        Assert.False((await auth.AuthorizeAsync(User("modulos.resumen.consultar"), null, policy)).Succeeded);
        Assert.False((await auth.AuthorizeAsync(User("reportes.exportar"), null, policy)).Succeeded);
        Assert.False((await auth.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), null, policy)).Succeeded);
    }
    private static ClaimsPrincipal User(string permission) => new(new ClaimsIdentity([new Claim("permiso", permission)], "test"));
}
