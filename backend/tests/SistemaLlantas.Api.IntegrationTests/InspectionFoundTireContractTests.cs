using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class InspectionFoundTireContractTests
{
    [Fact]
    public void AltaNormalConservaTipoObligatorio()
    {
        var service=new SistemaLlantas.Infrastructure.Services.LlantaService(null!);
        Assert.Throws<ValidacionException>(()=>{_ = service.CrearAsync(new(),"inspector",new(true,[]),default);});
    }
    [Theory][InlineData(false, null, false, true)][InlineData(true, 2, true, true)]
    [InlineData(true, null, true, false)][InlineData(true, 1, true, false)][InlineData(true, 2, false, false)]
    public void CorreccionNoExigeTipoYValidaReencaucheCondicional(bool reencauchada, int? vida, bool marca, bool valida)
    {
        var dto=new InspeccionesController.RegistrarLlantaEncontradaDto {
            PosicionId=Guid.NewGuid(),CentroId=Guid.NewGuid(),MarcaId=Guid.NewGuid(),ReferenciaId=Guid.NewGuid(),DimensionId=Guid.NewGuid(),
            Serial="SERIAL",Motivo="Observación física",DiscrepanciaFisica=true,ProfundidadInicial=0,
            EsReencauchada=reencauchada,MarcaReencaucheId=marca?Guid.NewGuid():null,Vida=vida
        };
        if(valida)dto.Validar();else Assert.Throws<ValidacionException>(()=>dto.Validar());
        Assert.Null(dto.TipoLlantaId);
    }
    [Theory]
    [InlineData("fisica")][InlineData("serial")][InlineData("motivo")]
    [InlineData("posicion")][InlineData("centro")][InlineData("catalogo")][InlineData("profundidad")]
    public async Task EntradaInvalidaSeRechazaAntesDeSql(string campo)
    {
        var dto=new InspeccionesController.RegistrarLlantaEncontradaDto
        {
            DiscrepanciaFisica=campo!="fisica",Codigo=campo=="codigo"?null!:"NUEVA",Serial=campo=="serial"?null!:"SER",
            Motivo=campo=="motivo"?null!:"Observada físicamente",PosicionId=campo=="posicion"?Guid.Empty:Guid.NewGuid(),
            CentroId=campo=="centro"?Guid.Empty:Guid.NewGuid(),MarcaId=campo=="catalogo"?Guid.Empty:Guid.NewGuid(),
            ReferenciaId=Guid.NewGuid(),DimensionId=Guid.NewGuid(),TipoLlantaId=Guid.NewGuid(),ProfundidadInicial=campo=="profundidad"?-1:12
        };
        await Assert.ThrowsAsync<ValidacionException>(()=>new InspeccionesController(null!,null!,null!)
            .RegistrarLlantaEncontrada(Guid.NewGuid(),dto,null!,null!,null!,CancellationToken.None));
    }

    [Fact]
    public async Task TecnicoSoloPuedeAccederAlAltaLimitada_NoObtieneAdministracionGlobal()
    {
        await using var factory=new WebApplicationFactory<Program>().WithWebHostBuilder(b=>
        {
            b.UseEnvironment("Development");b.ConfigureLogging(l=>l.ClearProviders());
            b.ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>
                {["Authentication:Mode"]="Local",["Authentication:SeedDevelopmentUsers"]="false"}));
            b.ConfigureServices(s=>s.AddAuthentication(o=>
                {o.DefaultAuthenticateScheme="InspectionTest";o.DefaultChallengeScheme="InspectionTest";o.DefaultForbidScheme="InspectionTest";})
                .AddScheme<AuthenticationSchemeOptions,InspectorHandler>("InspectionTest",_=>{}));
        });
        var client=factory.CreateClient();
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/llantas",new{})).StatusCode);
        // Pasa autorización y llega a validación del contrato, sin consultar SQL.
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync($"/api/inspecciones/{Guid.NewGuid()}/llantas-encontradas",new{})).StatusCode);
        using var scope=factory.Services.CreateScope();
        var auth=scope.ServiceProvider.GetRequiredService<IAuthorizationService>();var user=InspectorHandler.Usuario();
        Assert.True((await auth.AuthorizeAsync(user,null,"Inspecciones.Crear")).Succeeded);
        Assert.False((await auth.AuthorizeAsync(user,null,"Llantas.Administrar")).Succeeded);
        Assert.DoesNotContain(user.Claims,c=>c.Value=="llantas.administrar");
    }

    public sealed class InspectorHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder)
        :AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
    {
        public static ClaimsPrincipal Usuario()=>new(new ClaimsIdentity([new Claim("username","inspector"),
            new Claim(ClaimTypes.Role,"TECNICO"),new Claim("permiso","inspecciones.crear")],"InspectionTest"));
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()=>Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(Usuario(),Scheme.Name)));
    }
}
