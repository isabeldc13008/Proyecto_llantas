using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;
public sealed class DisposicionFlujoSqlTests
{
    [Fact] public async Task EighteenProposedFifteenReceivedAndClosedLeaveThreePhysicallyPending()
    {
        var name="SistemaLlantas_DisposicionFlow_"+Guid.NewGuid().ToString("N");
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION")??@"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True"){InitialCatalog=name};
        await using var db=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer(cs.ConnectionString).Options);
        var storage=Path.Combine(Path.GetTempPath(),name);
        try
        {
            await db.Database.EnsureCreatedAsync();
            var origin=new Centro{Codigo="BEL",Nombre="Bello"};var r1=new Centro{Codigo="R1",Nombre="R1 Medellín",Relevancia="R1"};
            var brand=new Marca{Codigo="M",Nombre="Marca"};var reference=new Referencia{Codigo="R",Nombre="Referencia",Marca=brand};var dimension=new Dimension{Codigo="D",Nombre="Dimensión"};var type=new TipoLlanta{Codigo="T",Nombre="Tipo"};
            var available=new EstadoLlanta{Codigo="DISPONIBLE",Nombre="Disponible"};
            var provider=new ProveedorServicio{Codigo="SV",Nombre="Receptor",Tipo="DisposicionFinal"};
            var tires=Enumerable.Range(1,18).Select(i=>new Llanta($"LL-{i:000}",$"SER-{i:000}"){Centro=origin,Marca=brand,Referencia=reference,Dimension=dimension,TipoLlanta=type,EstadoLlanta=available}).ToArray();
            db.AddRange(tires);db.AddRange(r1,provider,new EstadoLlanta{Codigo="EN_TRASLADO",Nombre="En traslado"},new EstadoLlanta{Codigo="DISPOSICION_FINAL",Nombre="Disposición final",EsDisposicionFinal=true});await db.SaveChangesAsync();
            var op=new OperacionService(db);var cycle=new CicloVidaLlantaService(db,new LlantaService(db));
            // File endpoints are exercised by DisposicionService; this controller path does not use hosting.
            var controller=new ServiciosLlantaController(db,op,cycle,null!);
            void User(string username,Guid center)=>controller.ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("username",username),new Claim("centro_id",center.ToString())},"test"))}};
            User("tecnico",origin.Id);var ids=new List<Guid>();
            foreach(var tire in tires)
            {
                var created=await controller.Crear(new("DisposicionFinal",tire.Id,null,null,"Evaluación de prueba",null),default);
                var order=Assert.IsType<ServiciosLlantaController.OrdenDto>(Assert.IsType<CreatedResult>(created.Result).Value);ids.Add(order.Id);
                await controller.EvaluarDisposicion(order.Id,new(false,"Concepto no reutilizable"),default);
            }
            User("aprobador",origin.Id);foreach(var id in ids)await controller.Aprobar(id,default);
            var result=await controller.CrearLoteDisposicion(new(ids,origin.Id,r1.Id,DateTimeOffset.UtcNow,"REM-TEST","Transportes",null,Guid.NewGuid().ToString(),"ABC123"),default);
            var lot=Assert.IsType<ServiciosLlantaController.LoteDisposicionDto>(Assert.IsType<CreatedResult>(result.Result).Value);
            User("receptor",r1.Id);await controller.RecibirLoteDisposicion(lot.Id,new(ids.Take(15).ToArray()),default);
            await Assert.ThrowsAsync<ValidacionException>(()=>controller.CerrarLoteDisposicion(lot.Id,new(ids.Take(15).ToArray(),provider.Id,"No puede saltar soporte"),default));
            var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Disposicion:StorageRoot",storage}}).Build();var service=new DisposicionService(db,op,config);var scope=new AlcanceCentros(false,[r1.Id]);
            var dispatch=await service.CrearDespachoAsync(new(r1.Id,provider.Id,ids.Take(15).ToArray(),DateTimeOffset.UtcNow,"Transportes","ABC123",null,null,Guid.NewGuid().ToString()),"receptor",scope,default);
            Assert.Single(dispatch.Actas);Assert.Equal(15,dispatch.Actas[0].Snapshot.Llantas.Count);
            var bytes="%PDF-1.4 soporte de prueba"u8.ToArray();using(var stream=new MemoryStream(bytes))await service.AdjuntarSoporteAsync(dispatch.Actas[0].Id,stream,"acta.pdf","application/pdf",bytes.Length,"receptor",scope,default);
            dispatch=(await service.DespachoAsync(dispatch.Id,scope,default))!;await service.CerrarDespachoAsync(dispatch.Id,new(dispatch.RowVersion,"Confirmación final"),"receptor",scope,default);
            var finalLot=(await service.LoteAsync(lot.Id,scope,default))!;Assert.Equal(18,finalLot.Enviadas);Assert.Equal(15,finalLot.Recibidas);Assert.Equal(3,finalLot.Pendientes);Assert.True(finalLot.PuedeRecibir);
            Assert.Equal(15,await db.OrdenesServicioLlanta.CountAsync(o=>o.Estado=="DISPOSICION_FINAL"));Assert.Equal(3,await db.OrdenesServicioLlanta.CountAsync(o=>o.Estado=="EN_TRANSITO_DISPOSICION"));
            Assert.Equal(1,(await service.ResumenAsync(new(),scope,default)).Contadores.Single(c=>c.Clave=="PARCIAL").Cantidad);
        }
        finally{if(db.Database.GetDbConnection().Database==name)await db.Database.EnsureDeletedAsync();if(Directory.Exists(storage)&&Path.GetFileName(storage)==name)Directory.Delete(storage,true);}
    }
}
