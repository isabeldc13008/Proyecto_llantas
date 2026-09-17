using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Api.Controllers;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class MovementLedgerTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task LedgerProjectsInventoryPositionsCentersAndLinkedRequestWithoutDamagedText()
    {
        _ = factory.CreateClient();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var position = await db.PosicionesVehiculo.Include(x => x.EjeVehiculo.Vehiculo.Centro).FirstAsync();
            var vehicle = position.EjeVehiculo.Vehiculo;
            var tire = await db.Llantas.FirstAsync();
            var destination = await db.Centros.FirstAsync(x => x.Id != vehicle.CentroId);
            var marker = "LEDGER-" + Guid.NewGuid().ToString("N")[..10];
            var mount = new Movimiento { Numero = marker + "-M", Tipo = "Montaje", CentroId = vehicle.CentroId, Motivo = "Montaje de prueba", Usuario = "qa-ledger" };
            mount.Detalles.Add(new MovimientoDetalle { LlantaId = tire.Id, PosicionDestinoId = position.Id, TipoDestino = TipoDestinoLlanta.Posicion });
            var transfer = new Movimiento { Numero = marker + "-T", Tipo = "Traslado centro", CentroId = vehicle.CentroId, Motivo = "Traslado de prueba", Usuario = "qa-ledger" };
            transfer.Detalles.Add(new MovimientoDetalle { LlantaId = tire.Id, CentroDestinoId = destination.Id, TipoDestino = TipoDestinoLlanta.Traslado, DestinoDescripcion = "Traslado entre centros" });
            db.Movimientos.AddRange(mount, transfer);
            var request = new SolicitudOperacion { Tipo = "Montaje", Estado = EstadoSolicitudOperacion.EJECUTADO, CentroId = vehicle.CentroId, LlantaId = tire.Id, PosicionDestinoId = position.Id, TipoDestino = "Posicion", Motivo = mount.Motivo, Solicitante = "qa-ledger", MovimientoEjecutadoId = mount.Id, KilometrajeVehiculo = 1234 };
            db.SolicitudesOperacion.Add(request);
            await db.SaveChangesAsync();
            var controller = new OperacionesController(scope.ServiceProvider.GetRequiredService<IOperacionService>(), scope.ServiceProvider.GetRequiredService<ICicloVidaLlantaService>(), db)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("username", "qa-ledger"), new Claim("permiso", "centros.ver_todos")], "test")) } }
            };
            var result = await controller.Movimientos(new ConsultaMovimientos { Numero = marker }, CancellationToken.None);
            Assert.Equal(2, result.TotalItems);
            var mounted = Assert.Single(result.Items, x => x.Id == mount.Id);
            Assert.Equal("Inventario", mounted.Origen);
            Assert.Equal(vehicle.Placa + " / " + position.Codigo, mounted.Destino);
            Assert.Equal(vehicle.NumeroInterno, mounted.VehiculoInterno);
            Assert.Equal(vehicle.Placa, mounted.VehiculoPlaca);
            Assert.Null(mounted.PosicionOrigen);
            Assert.Equal(position.Codigo, mounted.PosicionDestino);
            Assert.Equal(request.Id, mounted.SolicitudId);
            Assert.Equal(1234m, mounted.KilometrajeVehiculo);
            var transferred = Assert.Single(result.Items, x => x.Id == transfer.Id);
            Assert.Equal(vehicle.Centro.Nombre, transferred.Origen);
            Assert.Equal(destination.Nombre, transferred.Destino);
            Assert.Equal(transferred.Origen, transferred.CentroOrigen);
            Assert.Equal(transferred.Destino, transferred.CentroDestino);
            Assert.Null(transferred.SolicitudId);
            foreach (var item in result.Items)
            {
                Assert.Equal("EJECUTADO", item.Estado);
                Assert.DoesNotContain("â€", item.Origen + item.Destino + item.VehiculoPosicion);
                Assert.DoesNotContain("Â", item.VehiculoPosicion);
            }
            await transaction.RollbackAsync();
        });
    }
}
