using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class LlantasQuerySecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
 private readonly WebApplicationFactory<Program> factory;
 public LlantasQuerySecurityTests(WebApplicationFactory<Program> factory)=>this.factory=factory;

 [Fact]
 public async Task FiltrosCombinados_DevuelvenSoloFilasQueCumplenTodo()
 {
  var client=factory.CreateClient();await Login(client,"administrador","admin123");
  var all=await client.GetFromJsonAsync<Page>("/api/llantas?pageSize=100");Assert.NotNull(all);var sample=Assert.Single(all!.Items.Take(1));var centers=await client.GetFromJsonAsync<CatalogPage>($"/api/catalogos/centros?pageSize=100&search={Uri.EscapeDataString(sample.Centro)}");var center=Assert.Single(centers!.Items,x=>x.Nombre==sample.Centro);
  var url=$"/api/llantas?pageSize=100&centroIds={center.Id}&estados={Uri.EscapeDataString(sample.Estado)}&profundidadMin={sample.ProfundidadInicial}&profundidadMax={sample.ProfundidadInicial}";
  var filtered=await client.GetFromJsonAsync<Page>(url);Assert.NotNull(filtered);Assert.All(filtered!.Items,x=>{Assert.Equal(sample.Centro,x.Centro);Assert.Equal(sample.Estado,x.Estado);Assert.Equal(sample.ProfundidadInicial,x.ProfundidadInicial);});
 }

 [Fact]
 public async Task UsuarioSinCentros_NoPuedeForzarCentroPorQuery()
 {
  var admin=factory.CreateClient();await Login(admin,"administrador","admin123");var centers=await admin.GetFromJsonAsync<CatalogPage>("/api/catalogos/centros?pageSize=1");var center=Assert.Single(centers!.Items).Id;
  var tech=factory.CreateClient();await Login(tech,"tecnico","tec123");var denied=await tech.GetFromJsonAsync<Page>($"/api/llantas?centroIds={center}");Assert.NotNull(denied);Assert.Empty(denied!.Items);Assert.Equal(0,denied.TotalItems);
 }

 private static async Task Login(HttpClient client,string username,string password){var response=await client.PostAsJsonAsync("/api/auth/login",new{username,password});response.EnsureSuccessStatusCode();var login=await response.Content.ReadFromJsonAsync<LoginResponse>();client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",login!.AccessToken);}
 private sealed record LoginResponse(string AccessToken);
 private sealed record Page(Item[] Items,int TotalItems);
 private sealed record Item(Guid Id,string Centro,string Estado,decimal ProfundidadInicial);
 private sealed record CatalogPage(CatalogItem[] Items);
 private sealed record CatalogItem(Guid Id,string Nombre);
}
