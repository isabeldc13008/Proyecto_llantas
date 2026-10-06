using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class DisposicionDespachoTests
{
    [Fact] public async Task TwoOriginsProduceTwoActasWithScopeIdempotencyAndMandatorySupports()
    {
        var name="SistemaLlantas_DisposicionTest_"+Guid.NewGuid().ToString("N");
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION")??@"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True"){InitialCatalog=name};
        await using var db=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer(cs.ConnectionString).Options);
        var storage=Path.Combine(Path.GetTempPath(),name);
        try
        {
            await db.Database.EnsureCreatedAsync();
            var a=new Centro{Codigo="A",Nombre="Bello"};var b=new Centro{Codigo="B",Nombre="Itagüí"};var r=new Centro{Codigo="R",Nombre="Medellín",Relevancia="R1"};
            var brand=new Marca{Codigo="M",Nombre="Marca"};var reference=new Referencia{Codigo="REF",Nombre="Referencia",Marca=brand};
            var dimension=new Dimension{Codigo="D",Nombre="Dimensión"};var type=new TipoLlanta{Codigo="T",Nombre="Tipo"};
            var state=new EstadoLlanta{Codigo="INV",Nombre="Inventario"};var final=new EstadoLlanta{Codigo="DISPOSICION_FINAL",Nombre="Disposición final",EsDisposicionFinal=true};
            var provider=new ProveedorServicio{Codigo="SV",Nombre="Receptor",Tipo="DisposicionFinal"};
            var orders=new List<OrdenServicioLlanta>();
            foreach(var origin in new[]{a,b})
            {
                var tire=new Llanta(origin.Codigo,"S-"+origin.Codigo){Centro=r,Marca=brand,Referencia=reference,Dimension=dimension,TipoLlanta=type,EstadoLlanta=state};
                var lot=new LoteDisposicionFinal{Codigo="DSP-"+origin.Codigo,CentroOrigen=origin,CentroDestino=r,RelevanciaDestino="R1",Estado="RECIBIDO",FechaSalida=DateTimeOffset.UtcNow.AddDays(-2),IdempotencyKey=Guid.NewGuid().ToString()};
                var order=new OrdenServicioLlanta{Tipo=TipoServicioLlanta.DisposicionFinal,Llanta=tire,CentroOrigen=origin,LoteDisposicionFinal=lot,Estado="PENDIENTE_DISPOSICION",Resultado="DISPOSICION",FechaRecepcion=DateTimeOffset.UtcNow.AddDays(-1)};
                orders.Add(order);db.Add(order);
            }
            db.AddRange(provider,final);await db.SaveChangesAsync();
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Disposicion:StorageRoot",storage}}).Build();
            var service=new DisposicionService(db,new OperacionService(db),cfg);
            var all=new AlcanceCentros(true,[]);var originScope=new AlcanceCentros(false,[a.Id]);
            Assert.Equal(2,(await service.OrdenesAsync(new(),all,default)).TotalItems);
            Assert.Single((await service.OrdenesAsync(new(),originScope,default)).Items);
            Assert.Equal(2,(await service.DisponiblesAsync(new(),r.Id,all,default)).TotalItems);
            Assert.Equal(2,(await service.ResumenAsync(new(),all,default)).Contadores.Single(c=>c.Clave=="DISPONIBLES").Cantidad);
            Assert.NotNull(await service.DetalleAsync(orders[0].Id,all,default));
            Assert.Equal(2,(await service.LotesAsync(new(),all,default)).TotalItems);
            var waiting=new OrdenServicioLlanta{Tipo=TipoServicioLlanta.DisposicionFinal,Estado="EN_TRANSITO_DISPOSICION",Resultado="DISPOSICION",CentroOrigenId=a.Id,LoteDisposicionFinalId=orders[0].LoteDisposicionFinalId,
                Llanta=new Llanta("C","S-C"){CentroId=r.Id,MarcaId=brand.Id,ReferenciaId=reference.Id,DimensionId=dimension.Id,TipoLlantaId=type.Id,EstadoLlantaId=state.Id}};
            db.Add(waiting);await db.SaveChangesAsync();
            var input=new CrearDespachoDisposicionDto(r.Id,provider.Id,orders.Select(x=>x.Id).ToArray(),DateTimeOffset.UtcNow,"Transportador","AAA123",null,null,Guid.NewGuid().ToString());
            var dispatch=await service.CrearDespachoAsync(input,"operador",all,default);
            Assert.Equal(2,dispatch.Actas.Count);
            Assert.Equal(dispatch.Id,(await service.CrearDespachoAsync(input,"operador",all,default)).Id);
            await Assert.ThrowsAsync<ConflictoException>(()=>service.CrearDespachoAsync(input with{IdempotencyKey=Guid.NewGuid().ToString()},"operador",all,default));
            await Assert.ThrowsAsync<ConflictoException>(()=>service.CrearDespachoAsync(input with{Placa="BBB123"},"operador",all,default));
            Assert.Single((await service.DespachoAsync(dispatch.Id,originScope,default))!.Items);
            Assert.Single((await service.DespachoAsync(dispatch.Id,originScope,default))!.Actas);
            Assert.False((await service.DespachoAsync(dispatch.Id,originScope,default))!.PuedeCerrar);
            await Assert.ThrowsAsync<ValidacionException>(()=>service.CerrarDespachoAsync(dispatch.Id,new(dispatch.RowVersion,"Cierre"),"operador",all,default));
            Assert.All(await db.OrdenesServicioLlanta.ToListAsync(),o=>Assert.NotEqual("DISPOSICION_FINAL",o.Estado));
            var bytes="%PDF-1.4 test document"u8.ToArray();
            foreach(var acta in dispatch.Actas)
            {
                using var stream=new MemoryStream(bytes);
                await service.AdjuntarSoporteAsync(acta.Id,stream,"acta.pdf","application/pdf",bytes.Length,"operador",all,default);
            }
            dispatch=(await service.DespachoAsync(dispatch.Id,all,default))!;
            Assert.True(dispatch.PuedeCerrar);
            var removed=dispatch.Actas[0].Soportes[0];
            await service.EliminarSoporteAsync(removed.Id,"operador",all,default);
            Assert.False((await service.DespachoAsync(dispatch.Id,all,default))!.PuedeCerrar);
            using(var stream=new MemoryStream(bytes))await service.AdjuntarSoporteAsync(dispatch.Actas[0].Id,stream,"acta.pdf","application/pdf",bytes.Length,"operador",all,default);
            dispatch=(await service.DespachoAsync(dispatch.Id,all,default))!;
            var closed=await service.CerrarDespachoAsync(dispatch.Id,new(dispatch.RowVersion,"Cierre comprobado"),"operador",all,default);
            Assert.Equal("CERRADO",closed.Estado);
            await Assert.ThrowsAsync<ConflictoException>(()=>service.EliminarSoporteAsync(closed.Actas[1].Soportes[0].Id,"operador",all,default));
            Assert.All(await db.OrdenesServicioLlanta.Where(o=>o.Id!=waiting.Id).ToListAsync(),o=>Assert.Equal("DISPOSICION_FINAL",o.Estado));
            Assert.Equal(1,(await service.ResumenAsync(new(),all,default)).Contadores.Single(x=>x.Clave=="PARCIAL").Cantidad);
            Assert.Single((await service.LotesAsync(new(Estado:"RECEPCION_PARCIAL"),all,default)).Items);
            Assert.DoesNotContain((await service.DetalleAsync(orders[0].Id,all,default))!.Eventos,x=>x.EstadoVisual=="ACTUAL");
            Assert.Equal("CERRADO",(await service.CerrarDespachoAsync(dispatch.Id,new(dispatch.RowVersion,"Reintento"),"operador",all,default)).Estado);
            // Exercise only the new migration delta against a current-model database.
            // This does not claim that the project's older migration chain is healthy.
            var migrations=db.Database.GetMigrations().ToArray();
            var latest=migrations.Single(x=>x.EndsWith("_AddDisposicionSistemaVerde"));
            var previous=migrations[Array.IndexOf(migrations,latest)-1];
            var migrator=db.GetService<IMigrator>();
            await db.Database.ExecuteSqlRawAsync(db.GetService<IHistoryRepository>().GetCreateScript());
            foreach(var sql in System.Text.RegularExpressions.Regex.Split(migrator.GenerateScript(latest,previous),@"(?im)^GO\s*$").Where(x=>!string.IsNullOrWhiteSpace(x)))
                await db.Database.ExecuteSqlRawAsync(sql);
            foreach(var sql in System.Text.RegularExpressions.Regex.Split(migrator.GenerateScript(previous,latest),@"(?im)^GO\s*$").Where(x=>!string.IsNullOrWhiteSpace(x)))
                await db.Database.ExecuteSqlRawAsync(sql);
            db.ChangeTracker.Clear();
            Assert.All(await db.OrdenesServicioLlanta.Where(o=>o.Id!=waiting.Id).ToListAsync(),o=>Assert.Equal("DISPOSICION_FINAL",o.Estado));
            Assert.Equal("EN_TRANSITO_DISPOSICION",(await db.OrdenesServicioLlanta.SingleAsync(o=>o.Id==waiting.Id)).Estado);
            Assert.All(await db.LotesDisposicionFinal.ToListAsync(),l=>Assert.Null(l.Placa));
        }
        finally
        {
            if(db.Database.GetDbConnection().Database==name)await db.Database.EnsureDeletedAsync();
            if(Directory.Exists(storage)&&Path.GetFileName(storage)==name)Directory.Delete(storage,true);
        }
    }
}
