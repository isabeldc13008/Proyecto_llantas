using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

// Independent current-model database: deliberately does not claim to validate historical migrations.
public sealed class MantenimientoSqlTests
{
    [Fact] public async Task RealSqlPreservesEvidenceScopePaginationAndOperationalHistory()
    {
        var database = "SistemaLlantas_V1Test_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION")
            ?? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True") { InitialCatalog = database };
        await using var db = new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await db.Database.EnsureCreatedAsync();
            // Only this generated test database: permit legacy anomalies to exercise defensive reads.
            await db.Database.ExecuteSqlRawAsync("DROP INDEX [IX_AsignacionLlantaPosicion_GLlantaId] ON [dbo].[TBL_AsignacionLlantaPosicion]; DROP INDEX [IX_AsignacionLlantaPosicion_GPosicionVehiculoId] ON [dbo].[TBL_AsignacionLlantaPosicion];");
            var center = new Centro { Codigo = "A", Nombre = "Visible" }; var hidden = new Centro { Codigo = "B", Nombre = "Oculto" };
            var brand = new Marca { Codigo = "M", Nombre = "Marca" };var reference = new Referencia { Codigo = "R", Nombre = "Referencia", Marca = brand };
            var dimension = new Dimension { Codigo = "D", Nombre = "Dimension" };var type = new TipoLlanta { Codigo = "T", Nombre = "Tipo" };
            var state = new EstadoLlanta { Codigo = "MON", Nombre = "Montada" };
            var vehicle = new Vehiculo { Centro = center, NumeroInterno = "V1", Placa = "AAA111", Tipo = "Camion", Kilometraje = 5000 };
            var hiddenVehicle = new Vehiculo { Centro = hidden, NumeroInterno = "V2", Placa = "BBB222", Tipo = "Camion" };
            var axle = new EjeVehiculo { Vehiculo = vehicle, Numero = 1, Nombre = "Eje", TipoEje = "Direccional" };
            var p = new PosicionVehiculo { EjeVehiculo = axle, Codigo = "DD", Lado = "Derecha" };
            var p2 = new PosicionVehiculo { EjeVehiculo = axle, Codigo = "DI", Lado = "Izquierda" };
            Llanta Tire(string code) => new(code, "SER-" + code) { Centro = center, Marca = brand, Referencia = reference, Dimension = dimension, TipoLlanta = type, EstadoLlanta = state };
            var alertTire = Tire("A");var missing = Tire("B");var measured = Tire("C");var unmounted = Tire("D");
            db.AddRange(center,hidden,vehicle,hiddenVehicle,p,p2,alertTire,missing,measured,unmounted);await db.SaveChangesAsync();
            var move = new Movimiento { Numero = "M1", CentroId = center.Id, Tipo = "Montaje", Motivo = "Prueba" };
            db.Add(move);await db.SaveChangesAsync();
            foreach(var tire in new[]{alertTire,missing,measured})db.AsignacionesLlantaPosicion.Add(new(){LlantaId=tire.Id,PosicionVehiculoId=p.Id,MovimientoOrigenId=move.Id,EsActiva=true,FechaInicio=DateTimeOffset.UtcNow.AddDays(-10),KilometrajeMontaje=0});
            // Legacy inconsistent duplicate: one queue row, no arbitrary position.
            db.AsignacionesLlantaPosicion.Add(new(){LlantaId=alertTire.Id,PosicionVehiculoId=p2.Id,MovimientoOrigenId=move.Id,EsActiva=true,FechaInicio=DateTimeOffset.UtcNow.AddDays(-10),KilometrajeMontaje=0});
            Inspeccion Reading(Llanta tire, DateTimeOffset date, decimal? outside, Centro c, Vehiculo v) => new(){Centro=c,Vehiculo=v,FechaCreacion=date,Estado=EstadoInspeccion.Finalizada,Kilometraje=100,
                Detalles=[new(){Llanta=tire,PosicionVehiculo=p,ProfundidadExterior=outside,ProfundidadCentro=4,ProfundidadInterior=5}]};
            var old=Reading(alertTire,DateTimeOffset.UtcNow.AddDays(-3),3,center,vehicle);
            var last=Reading(alertTire,DateTimeOffset.UtcNow.AddDays(-1),null,center,vehicle);
            var zero=Reading(measured,DateTimeOffset.UtcNow.AddDays(-1),0,center,vehicle);
            var hiddenReading=Reading(alertTire,DateTimeOffset.UtcNow,1,hidden,hiddenVehicle);
            db.AddRange(old,last,zero,hiddenReading);await db.SaveChangesAsync();
            var alert = new AlertaInspeccion { LlantaId=alertTire.Id,CentroId=center.Id,VehiculoId=vehicle.Id,PosicionVehiculoId=p.Id,Inspeccion=last,InspeccionDetalle=last.Detalles.Single(),Tipo="CRIT PROFUNDIDAD",Descripcion="No debe inferirse severidad" };
            db.Add(alert);db.OrdenesServicioLlanta.Add(new(){LlantaId=alertTire.Id,CentroOrigenId=hidden.Id,Tipo=TipoServicioLlanta.Reparacion,Estado="CERRADA",Motivo="Servicio oculto"});await db.SaveChangesAsync();db.ChangeTracker.Clear();
            var service=new AnaliticaService(db);var scope=new AlcanceCentros(false,[center.Id]);
            var queue=await service.MantenimientoAsync(new(){Tamano=1},scope,default);
            Assert.Equal(3,queue.TotalEnAlcance);Assert.Equal(2,queue.TotalEnCola);Assert.Equal(2,queue.Pagina.TotalItems);
            var first=Assert.Single(queue.Pagina.Items);Assert.Equal(alertTire.Id,first.Id);Assert.Null(first.Clasificacion.Valor);
            Assert.Equal("AMBIGUA",first.Ubicacion.Estado);Assert.Null(first.Ubicacion.Posicion);
            Assert.False(first.UltimaLectura!.Completa);Assert.Equal(3m,first.UltimaCompleta!.Minima);
            Assert.Contains(first.MotivosSecundarios,x=>x.Codigo=="MEDICION_INCOMPLETA");
            var page2=await service.MantenimientoAsync(new(){Tamano=1,Pagina=2},scope,default);
            Assert.Equal(missing.Id,Assert.Single(page2.Pagina.Items).Id);Assert.Equal(queue.TotalEnCola,page2.TotalEnCola);
            var outside=await service.MantenimientoAsync(new(){Vista="FUERA_COLA"},scope,default);
            var zeroRow=Assert.Single(outside.Pagina.Items);Assert.Equal(0m,zeroRow.UltimaLectura!.Minima);Assert.Null(zeroRow.Clasificacion.Valor);
            Assert.Equal(4,(await service.MantenimientoAsync(new(){Montaje="TODAS",Vista="TODAS"},scope,default)).TotalEnAlcance);
            Assert.Equal(0,(await service.MantenimientoAsync(new(),new(false,[]),default)).TotalEnAlcance);
            Assert.Single((await service.VehiculosMantenimientoAsync(null,null,1,20,scope,default)).Items);
            var detail=await service.DetalleMantenimientoAsync(alertTire.Id,scope,default);
            Assert.Null(detail.Pronostico.Valor);Assert.Null(detail.ResolucionRiesgo.Valor);Assert.Null(Assert.Single(detail.Alertas.Items).Severidad.Valor);
            var changed = await Assert.ThrowsAsync<ValidacionException>(() => new ProgramacionService(db).CrearAsync(new() {
                Tipo="Inspección",Inicio=DateTimeOffset.UtcNow.AddDays(1),Fin=DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                CentroId=center.Id,VehiculoId=vehicle.Id,TecnicoUsuarioId=Guid.NewGuid(),LlantaId=measured.Id,PosicionVehiculoId=p2.Id
            },"qa",scope,default));
            Assert.Contains("montaje",changed.Message);
            // The legacy operational summary assumes its unique index. Restore that invariant before testing history scope.
            var duplicate=await db.AsignacionesLlantaPosicion.SingleAsync(x=>x.LlantaId==alertTire.Id && x.PosicionVehiculoId==p2.Id);
            db.Remove(duplicate);await db.SaveChangesAsync();
            var history=await new CicloVidaLlantaService(db,new LlantaService(db)).ObtenerDetalleAsync(alertTire.Id,scope,default);
            Assert.DoesNotContain(history!.Inspecciones,x=>x.Centro=="Oculto");Assert.Empty(history.Servicios);
            Assert.True(history.RequiereConciliacion);
        }
        finally { if(db.Database.GetDbConnection().Database==database)await db.Database.EnsureDeletedAsync(); }
    }
}
