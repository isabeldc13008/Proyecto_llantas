using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class OperationalPolicyTests
{
 [Theory]
 [InlineData("ADMINISTRADOR",true)] [InlineData("SUPERVISOR_ADMINISTRADOR",true)] [InlineData("SUPERVISOR",false)] [InlineData("TECNICO",false)]
 public async Task Policies_RequireApprovedRoleAndPermission(string role,bool allowed)
 {
  await using var factory=new WebApplicationFactory<Program>().WithWebHostBuilder(b=>{
   b.UseEnvironment("Development");b.ConfigureLogging(l=>l.ClearProviders());
   b.ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{["Authentication:Mode"]="Local",["Authentication:SeedDevelopmentUsers"]="false"}));
  });
  using var scope=factory.Services.CreateScope();var authorization=scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
  foreach(var pair in new[]{("Operaciones.Aprobar","operaciones.aprobar"),("Alertas.Parametrizar","alertas.parametrizar")})
  {
   var user=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role,role),new Claim("permiso",pair.Item2)],"test"));
   Assert.Equal(allowed,(await authorization.AuthorizeAsync(user,null,pair.Item1)).Succeeded);
   var noPermission=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role,role)],"test"));
   Assert.False((await authorization.AuthorizeAsync(noPermission,null,pair.Item1)).Succeeded);
  }
 }
}
