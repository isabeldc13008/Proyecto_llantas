using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace SistemaLlantas.Api.Security;

public static class EntraAuthentication
{
    public const string Scheme = "Entra";
    public const string Header = "X-Entra-Authorization";
    public const string TenantClaim = "entra_tid";
    public const string ObjectClaim = "entra_oid";

    public static bool Enabled(IConfiguration config) => config.GetValue<bool>("Entra:RequireDualAuthentication");

    public static void ValidateConfiguration(IConfiguration config)
    {
        if (!Guid.TryParse(config["Entra:TenantId"], out var tenant) || tenant == Guid.Empty
            || !Guid.TryParse(config["Entra:ClientId"], out var client) || client == Guid.Empty
            || config["Entra:TokenVersion"] is not ("1.0" or "2.0")
            || config["Entra:Scope"] != $"api://{client}/access_as_user")
            throw new InvalidOperationException("Confirme con TI TenantId, ClientId, Scope y TokenVersion antes de activar Entra.");
        foreach (var key in new[] { "RedirectUri", "PostLogoutRedirectUri" })
            if (!Uri.TryCreate(config[$"Entra:{key}"], UriKind.Absolute, out var uri)
                || !(uri.Scheme == "https" || (uri.Scheme == "http" && uri.IsLoopback)) || uri.Fragment.Length != 0)
                throw new InvalidOperationException($"Configure Entra:{key} con la URI autorizada por TI.");
    }

    public static bool HasDelegatedIdentity(ClaimsPrincipal principal, IConfiguration config)
        => DelegatedIdentityFailure(principal, config) is null;

    public static string? DelegatedIdentityFailure(ClaimsPrincipal principal, IConfiguration config)
    {
        var tenant=config["Entra:TenantId"];
        var client=config["Entra:ClientId"];
        var version=principal.FindFirstValue("ver");
        var actor=principal.FindFirstValue(version=="1.0"?"appid":"azp");
        if(!Guid.TryParse(principal.FindFirstValue("oid"),out var oid)||oid==Guid.Empty)return "oid_missing";
        if(principal.FindFirstValue("tid")!=tenant)return "tid_mismatch";
        if(version!=config["Entra:TokenVersion"]||(version is not ("1.0" or "2.0")))return "version_mismatch";
        if(actor!=client)return "appid_mismatch";
        if(principal.FindFirstValue("iss")!=(version=="1.0"?$"https://sts.windows.net/{tenant}/":$"https://login.microsoftonline.com/{tenant}/v2.0"))return "issuer_mismatch";
        if(principal.FindFirstValue("aud")!=(version=="1.0"?$"api://{client}":client))return "audience_mismatch";
        if(!(principal.FindFirstValue("scp")??"").Split(' ',StringSplitOptions.RemoveEmptyEntries).Contains("access_as_user",StringComparer.Ordinal))return "scope_missing";
        return null;
    }

    public static bool IsBound(ClaimsPrincipal local, ClaimsPrincipal entra) =>
        Guid.TryParse(local.FindFirstValue(TenantClaim),out var tenant)&&tenant!=Guid.Empty
        &&Guid.TryParse(local.FindFirstValue(ObjectClaim),out var oid)&&oid!=Guid.Empty
        &&local.FindFirstValue(TenantClaim)==entra.FindFirstValue("tid")
        &&local.FindFirstValue(ObjectClaim)==entra.FindFirstValue("oid");

    public static void Configure(JwtBearerOptions options,IConfiguration config)
    {
        ValidateConfiguration(config);
        var tenant=config["Entra:TenantId"];
        var client=config["Entra:ClientId"];
        if(!Guid.TryParse(tenant,out _)||!Guid.TryParse(client,out _))
            throw new InvalidOperationException("Configure Entra:TenantId y Entra:ClientId para el ambiente.");
        options.MapInboundClaims=false;
        options.Authority=$"https://login.microsoftonline.com/{tenant}"+(config["Entra:TokenVersion"]=="2.0"?"/v2.0":"");
        options.MetadataAddress=$"https://login.microsoftonline.com/{tenant}/.well-known/openid-configuration";
        options.RequireHttpsMetadata=true;
        options.TokenValidationParameters=new TokenValidationParameters {
            ValidateIssuer=true,ValidateAudience=true,ValidateLifetime=true,ValidateIssuerSigningKey=true,
            RequireSignedTokens=true,RequireExpirationTime=true,
            ValidIssuer=config["Entra:TokenVersion"]=="2.0"?$"https://login.microsoftonline.com/{tenant}/v2.0":$"https://sts.windows.net/{tenant}/",
            ValidAudience=config["Entra:TokenVersion"]=="2.0"?client:$"api://{client}",ValidAlgorithms=[SecurityAlgorithms.RsaSha256],ClockSkew=TimeSpan.Zero
        };
        options.Events=new JwtBearerEvents {
            OnMessageReceived=context=>{
                // Never fall back to the internal Authorization bearer for Entra validation.
                var values=context.Request.Headers[Header];
                if(values.Count==0 && context.Request.Path.Equals("/api/auth/login")
                    && context.Request.Headers.TryGetValue("Authorization",out var standard))
                    values=standard;
                if(values.Count!=1||!values[0]!.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase))context.NoResult();
                else context.Token=values[0]![7..].Trim();
                if(string.IsNullOrEmpty(context.Token))context.NoResult();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed=context=>{
                var logger=context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("EntraAuthentication");
                logger.LogWarning("Entra token rejected during cryptographic validation: {Code}", AuthenticationFailureCode(context.Exception));
                return Task.CompletedTask;
            },
            OnTokenValidated=context=>{
                var code=DelegatedIdentityFailure(context.Principal!,config);
                if(code is not null){context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("EntraAuthentication").LogWarning("Entra token rejected after claims validation: {Code}",code);context.Fail("Token Entra sin identidad delegada, emisor, audiencia, cliente o scope autorizado.");}
                return Task.CompletedTask;
            }
        };
    }

    private static string AuthenticationFailureCode(Exception exception) => exception switch
    {
        SecurityTokenInvalidIssuerException => "issuer_mismatch",
        SecurityTokenInvalidAudienceException => "audience_mismatch",
        SecurityTokenExpiredException => "expired",
        SecurityTokenInvalidSignatureException => "signature_invalid",
        SecurityTokenNoExpirationException => "expiration_missing",
        _ => "token_validation_failed"
    };
}
