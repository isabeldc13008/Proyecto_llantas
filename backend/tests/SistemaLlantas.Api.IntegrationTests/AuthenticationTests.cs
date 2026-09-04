using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class AuthenticationTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AnonymousAndDevelopmentHeaderCannotReadProtectedApi()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Development-User", "administrador");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task RevocationTakesEffectWithoutWaitingForTokenExpiry()
    {
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "supervisor", password = "super123" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<Token>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var user = await db.UsuariosSistema.SingleAsync(x => x.Username == "supervisor");
        user.Activo = false; await db.SaveChangesAsync();
        try { Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode); }
        finally { user.Activo = true; await db.SaveChangesAsync(); }
    }

    [Fact]
    public async Task TechnicianCannotManageUsers()
    {
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "tecnico", password = "tec123" });
        login.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await login.Content.ReadFromJsonAsync<Token>())!.AccessToken);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/usuarios")).StatusCode);
    }
    private sealed record Token(string AccessToken);
}
