using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Infrastructure;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

// These tests parse configuration and resolve options only. They never open SQL connections.
public sealed class QaStartupConfigurationTests
{
    private static WebApplicationBuilder Builder()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "QA", ContentRootPath = AppContext.BaseDirectory });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
            ["Jwt:Issuer"] = "SistemaLlantas", ["Jwt:Audience"] = "SistemaLlantas.Web"
        });
        return builder;
    }

    [Theory]
    [InlineData("")]
    [InlineData(";Authentication=Sql Password")]
    public void SqlAuthenticationPreservesInlineCredentialsWhenNoOverrideExists(string authentication)
    {
        var password = Guid.NewGuid().ToString("N");
        var builder = Builder();
        builder.Configuration["ConnectionStrings:SqlServer"] = $"Server=127.0.0.1,1;Database=QaNeverConnect;User ID=qa_test;Password={password}{authentication}";
        builder.Services.AddInfrastructure(builder.Configuration);
        using var services = builder.Services.BuildServiceProvider();
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        var actual = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        Assert.Equal("qa_test", actual.UserID);
        Assert.True(actual.Password == password);
        Assert.Equal(System.Data.ConnectionState.Closed, db.Database.GetDbConnection().State);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CredentialOverridesCanBeDisabledForQa(bool allowOverrides)
    {
        var builder = Builder();
        var inline = Guid.NewGuid().ToString("N");
        var legacy = Guid.NewGuid().ToString("N");
        builder.Configuration["ConnectionStrings:SqlServer"] = $"Server=127.0.0.1,1;Database=QaNeverConnect;User ID=inline_user;Password={inline}";
        builder.Configuration["SqlCredentials:AllowOverrides"] = allowOverrides.ToString();
        builder.Configuration["SqlCredentials:Username"] = "legacy_user";
        builder.Configuration["SqlCredentials:Password"] = legacy;
        builder.Services.AddInfrastructure(builder.Configuration);
        using var services = builder.Services.BuildServiceProvider();
        using var scope = services.CreateScope();
        var actual = new SqlConnectionStringBuilder(scope.ServiceProvider.GetRequiredService<LlantasDbContext>().Database.GetConnectionString());
        Assert.Equal(allowOverrides ? "legacy_user" : "inline_user", actual.UserID);
        Assert.True(actual.Password == (allowOverrides ? legacy : inline));
    }

    [Fact]
    public void MissingConnectionFailsBeforeAnySqlConnection()
    {
        var builder = Builder();
        var error = Assert.Throws<InvalidOperationException>(() => builder.Services.AddInfrastructure(builder.Configuration));
        Assert.Equal("Falta ConnectionStrings:SqlServer.", error.Message);
    }

    [Fact]
    public void UnsupportedConnectionKeywordFailsDuringRegistration()
    {
        var builder = Builder();
        builder.Configuration["ConnectionStrings:SqlServer"] = "NotASqlKeyword=value";
        Assert.Throws<ArgumentException>(() => builder.Services.AddInfrastructure(builder.Configuration));
    }

    [Fact]
    public void MissingJwtKeyFailsInQa()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Builder().AddApplicationAuthentication());
        Assert.Contains("Jwt__Key", error.Message);
    }

    [Fact]
    public void ShortJwtKeyFailsInQa()
    {
        var builder = Builder();
        builder.Configuration["Jwt:Key"] = Guid.NewGuid().ToString("N")[..8];
        var error = Assert.Throws<InvalidOperationException>(() => builder.AddApplicationAuthentication());
        Assert.Contains("32 bytes", error.Message);
    }

    [Theory]
    [InlineData("Jwt:Issuer")]
    [InlineData("Jwt:Audience")]
    public void MissingIssuerOrAudienceFails(string key)
    {
        var builder = Builder();
        builder.Configuration["Jwt:Key"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        builder.Configuration[key] = null;
        Assert.Throws<InvalidOperationException>(() => builder.AddApplicationAuthentication());
    }
}

