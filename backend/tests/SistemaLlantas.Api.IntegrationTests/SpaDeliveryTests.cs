using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class SpaDeliveryTests : IDisposable
{
    private readonly string webRoot = Path.Combine(Path.GetTempPath(), "llantas-spa-" + Guid.NewGuid().ToString("N"));
    private readonly WebApplicationFactory<Program> factory;

    public SpaDeliveryTests()
    {
        Directory.CreateDirectory(webRoot);
        File.WriteAllText(Path.Combine(webRoot, "index.html"), "<html><body>spa</body></html>");
        File.WriteAllText(Path.Combine(webRoot, "main.js"), "console.log('spa');");
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseWebRoot(webRoot));
    }

    [Fact]
    public async Task Root_without_session_returns_angular_application()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("spa", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Static_resources_are_served_without_session()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/main.js");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/javascript", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Private_api_without_session_remains_unauthorized()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        factory.Dispose();
        Directory.Delete(webRoot, recursive: true);
    }
}
