using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SistemaLlantas.Api.Security;
namespace SistemaLlantas.Api.IntegrationTests;

public sealed class EntraAuthenticationTests
{
    private const string Tenant="1be5ad33-84d3-42a0-a64a-1b673e4e9944", Client="44b4eaad-a201-404a-87f0-82277b0ebdde";
    private static IConfiguration Config(string version="2.0") => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
        ["Entra:TenantId"]=Tenant,["Entra:ClientId"]=Client,["Entra:Scope"]=$"api://{Client}/access_as_user",
        ["Entra:TokenVersion"]=version,["Entra:RedirectUri"]="https://localhost/acceso",["Entra:PostLogoutRedirectUri"]="https://localhost/acceso"
    }).Build();
    [Fact] public void DeshabilitadoPorDefectoYVersionRequerida() {
        Assert.False(EntraAuthentication.Enabled(Config()));
        Assert.Throws<InvalidOperationException>(()=>EntraAuthentication.ValidateConfiguration(Config("")));
    }
    [Theory]
    [InlineData("2.0","valid")][InlineData("1.0","valid")]
    [InlineData("2.0","signature")][InlineData("2.0","issuer")][InlineData("2.0","audience")]
    [InlineData("2.0","expired")][InlineData("2.0","scope")][InlineData("2.0","tenant")]
    [InlineData("2.0","actor")][InlineData("2.0","oid")]
    public void ValidaTokenRealFirmado(string version,string scenario) {
        using var rsa=RSA.Create(2048);using var other=RSA.Create(2048);
        var key=new RsaSecurityKey(rsa);var options=new JwtBearerOptions();var config=Config(version);
        EntraAuthentication.Configure(options,config);options.TokenValidationParameters.IssuerSigningKey=key;
        var claims=new[]{new Claim("ver",version),new Claim("tid",scenario=="tenant"?Guid.NewGuid().ToString():Tenant),
            new Claim("oid",scenario=="oid"?"":Guid.NewGuid().ToString()),new Claim(version=="1.0"?"appid":"azp",scenario=="actor"?Guid.NewGuid().ToString():Client),
            new Claim("scp",scenario=="scope"?"other":"access_as_user")};
        var token=new JwtSecurityToken(scenario=="issuer"?"https://wrong":options.TokenValidationParameters.ValidIssuer,
            scenario=="audience"?"wrong":options.TokenValidationParameters.ValidAudience,claims,
            notBefore:DateTime.UtcNow.AddHours(-1),expires:DateTime.UtcNow.AddMinutes(scenario=="expired"?-5:5),
            signingCredentials:new SigningCredentials(scenario=="signature"?new RsaSecurityKey(other):key,SecurityAlgorithms.RsaSha256));
        var handler=new JwtSecurityTokenHandler{MapInboundClaims=false};var encoded=handler.WriteToken(token);
        if(new[]{"signature","issuer","audience","expired"}.Contains(scenario))
            Assert.ThrowsAny<SecurityTokenException>(()=>handler.ValidateToken(encoded,options.TokenValidationParameters,out _));
        else {
            var principal=handler.ValidateToken(encoded,options.TokenValidationParameters,out _);
            Assert.Equal(scenario=="valid",EntraAuthentication.HasDelegatedIdentity(principal,config));
        }
    }
    [Fact] public async Task LoginSinMicrosoftRechazaAntesDeConsultarBaseDeDatos() {
        var config=Config();config["Entra:RequireDualAuthentication"]="true";
        using var services=new ServiceCollection().AddSingleton<IAuthenticationService,MissingAuthentication>().BuildServiceProvider();
        var controller=new AuthController(null!,config){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{RequestServices=services}}};
        var result=await controller.Login(new AuthController.LoginRequest("user","password"),CancellationToken.None);
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
    [Fact] public void V1UsaMetadataOpenIdConnectDelTenant() {
        var options=new JwtBearerOptions();EntraAuthentication.Configure(options,Config("1.0"));
        Assert.Equal($"https://login.microsoftonline.com/{Tenant}/.well-known/openid-configuration",options.MetadataAddress);
        Assert.Equal($"https://sts.windows.net/{Tenant}/",options.TokenValidationParameters.ValidIssuer);
        Assert.Equal($"api://{Client}",options.TokenValidationParameters.ValidAudience);
    }
    [Fact] public async Task LoginAceptaBearerEstandarComoTokenEntra() {
        var options=new JwtBearerOptions();EntraAuthentication.Configure(options,Config("1.0"));
        var http=new DefaultHttpContext();http.Request.Path="/api/auth/login";http.Request.Headers.Authorization="Bearer entra-token";
        var context=new MessageReceivedContext(http, new AuthenticationScheme(EntraAuthentication.Scheme,null,typeof(JwtBearerHandler)), options);
        await options.Events!.OnMessageReceived(context);
        Assert.Equal("entra-token",context.Token);
    }
    [Fact] public async Task RutaProtegidaNoUsaBearerEstandarComoTokenEntra() {
        var options=new JwtBearerOptions();EntraAuthentication.Configure(options,Config("1.0"));
        var http=new DefaultHttpContext();http.Request.Path="/api/auth/me";http.Request.Headers.Authorization="Bearer local-token";
        var context=new MessageReceivedContext(http, new AuthenticationScheme(EntraAuthentication.Scheme,null,typeof(JwtBearerHandler)), options);
        await options.Events!.OnMessageReceived(context);
        Assert.Null(context.Token);
    }
    private sealed class MissingAuthentication:IAuthenticationService {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context,string? scheme)=>Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext context,string? scheme,AuthenticationProperties? properties)=>Task.CompletedTask;
        public Task ForbidAsync(HttpContext context,string? scheme,AuthenticationProperties? properties)=>Task.CompletedTask;
        public Task SignInAsync(HttpContext context,string? scheme,ClaimsPrincipal principal,AuthenticationProperties? properties)=>Task.CompletedTask;
        public Task SignOutAsync(HttpContext context,string? scheme,AuthenticationProperties? properties)=>Task.CompletedTask;
    }
    [Fact] public void SesionInternaExigeMismaIdentidad() {
        var oid=Guid.NewGuid().ToString();
        var entra=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("tid",Tenant),new Claim("oid",oid)}));
        var local=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(EntraAuthentication.TenantClaim,Tenant),new Claim(EntraAuthentication.ObjectClaim,oid)}));
        Assert.True(EntraAuthentication.IsBound(local,entra));
        Assert.False(EntraAuthentication.IsBound(new ClaimsPrincipal(),entra));
        entra=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("tid",Tenant),new Claim("oid",Guid.NewGuid().ToString())}));
        Assert.False(EntraAuthentication.IsBound(local,entra));
    }
}
