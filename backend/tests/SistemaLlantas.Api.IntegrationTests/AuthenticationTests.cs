using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;
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

    [Fact]
    public async Task EntraMappingRequiresRegisteredObjectAndUsesOnlyInternalPermissions()
    {
        _ = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var oid = Guid.NewGuid(); var tenant = Guid.NewGuid().ToString();
        var center = await db.Centros.FirstAsync();
        var user = new UsuarioSistema { Username = "corporate@example.test", Nombre = "Corporate User", EntraObjectId = oid,
            RolId = await db.RolesSistema.Where(x => x.Codigo == "TECNICO").Select(x => x.Id).SingleAsync(),
            Centros = [new UsuarioCentro { CentroId = center.Id }] };
        db.UsuariosSistema.Add(user); await db.SaveChangesAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["Authentication:Mode"] = "Entra";
        builder.Configuration["Entra:TenantId"] = tenant;
        builder.Configuration["Entra:ClientId"] = Guid.NewGuid().ToString();
        builder.AddApplicationAuthentication();
        await using var app = builder.Build();
        var options = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
        async Task<TokenValidatedContext> Map(string objectId, string tokenTenant, string tokenScope)
        {
            var context = new TokenValidatedContext(new DefaultHttpContext { RequestServices = scope.ServiceProvider },
                new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options)
            {
                Principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim("oid", objectId), new Claim("tid", tokenTenant), new Claim("scp", tokenScope),
                    new Claim("preferred_username", user.Username), new Claim("permiso", "centros.ver_todos"),
                    new Claim(ClaimTypes.Role, "ADMINISTRADOR") }, "Bearer"))
            };
            await options.Events.TokenValidated(context); return context;
        }
        var mapped = await Map(oid.ToString(), tenant, "access_as_user");
        Assert.Null(mapped.Result?.Failure);
        Assert.True(mapped.Principal!.IsInRole("TECNICO"));
        Assert.False(mapped.Principal.IsInRole("ADMINISTRADOR"));
        Assert.False(mapped.Principal.AlcanceCentros().VerTodos);
        Assert.Equal(center.Id, Assert.Single(mapped.Principal.AlcanceCentros().CentroIds));
        Assert.NotNull((await Map(Guid.NewGuid().ToString(), tenant, "access_as_user")).Result?.Failure);
        Assert.NotNull((await Map(oid.ToString(), Guid.NewGuid().ToString(), "access_as_user")).Result?.Failure);
        Assert.NotNull((await Map(oid.ToString(), tenant, "")).Result?.Failure);
    }
    private sealed record Token(string AccessToken);
}
