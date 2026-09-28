using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class AnaliticaIntegrationTests(TestApplicationFactory factory) : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AggregatesRespectScopeCohortsTripsSamplesAndDistinctMovements()
    {
        _ = factory.CreateClient();await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LlantasDbContext>();
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            var position = await db.PosicionesVehiculo.Include(x => x.EjeVehiculo.Vehiculo).FirstAsync();
            var center = position.EjeVehiculo.Vehiculo.CentroId;var other = await db.Centros.FirstAsync(x => x.Id != center);
            var seed = await db.Llantas.Include(x => x.EstadoLlanta).FirstAsync(x => !x.EstadoLlanta.EsDisposicionFinal);
            var key = Guid.NewGuid().ToString("N")[..8];
            var brand = new Marca { Codigo = key, Nombre = "Analítica " + key };
            var reference = new Referencia { Codigo = key, Nombre = "Referencia", Marca = brand };
            var disposed = new EstadoLlanta { Codigo = key, Nombre = "Finalizada", EsDisposicionFinal = true };
            db.AddRange(brand, reference, disposed);
            Llanta Tire(string suffix, Guid c, bool final = false) => new("AN-" + key + suffix, "SER-" + key + suffix)
            {
                Marca = brand, Referencia = reference, DimensionId = seed.DimensionId, TipoLlantaId = seed.TipoLlantaId,
                CentroId = c, EstadoLlanta = final ? disposed : seed.EstadoLlanta, FechaIngreso = new(2026, 1, 1)
            };
            var operating = Tire("A", center);var finalized = Tire("B", center, true);var missing = Tire("C", center);var invalid = Tire("D", center);var hidden = Tire("E", other.Id);
            db.Llantas.AddRange(operating, finalized, missing, invalid, hidden);await db.SaveChangesAsync();
            Movimiento Trip(Llanta tire, decimal start, decimal end, string type, bool duplicate = false)
            {
                var move = new Movimiento { Numero = "AN-" + Guid.NewGuid().ToString("N")[..20], Tipo = type, CentroId = tire.CentroId, Motivo = "Analítica QA", Usuario = "qa" };
                move.Detalles.Add(new MovimientoDetalle { LlantaId = tire.Id, PosicionDestinoId = position.Id, TipoDestino = TipoDestinoLlanta.Posicion });
                if (duplicate) move.Detalles.Add(new MovimientoDetalle { LlantaId = tire.Id, PosicionDestinoId = position.Id, TipoDestino = TipoDestinoLlanta.Posicion });
                db.Movimientos.Add(move);
                db.AsignacionesLlantaPosicion.Add(new() { LlantaId = tire.Id, PosicionVehiculoId = position.Id, MovimientoOrigenId = move.Id, EsActiva = false, FechaInicio = DateTimeOffset.UtcNow.AddDays(-5), FechaFin = DateTimeOffset.UtcNow.AddDays(-1), KilometrajeMontaje = start, KilometrajeDesmontaje = end, KilometrajeRecorrido = end - start });
                return move;
            }
            Trip(operating, 100, 400, "MONTAJE", true);Trip(operating, 400, 900, "ROTACION");Trip(finalized, 0, 2000, "MONTAJE");Trip(invalid, -100, 100, "MONTAJE");Trip(hidden, 0, 9000, "MONTAJE");
            db.Movimientos.Add(new Movimiento { Numero = "HIDDEN-" + key, Tipo = "OCULTO", CentroId = other.Id, Motivo = "Otro alcance", Detalles = [new() { LlantaId = operating.Id, TipoDestino = TipoDestinoLlanta.Inventario }] });
            db.OrdenesServicioLlanta.AddRange(new() { LlantaId = operating.Id, CentroOrigenId = center, Tipo = TipoServicioLlanta.Reparacion, Estado = "CERRADA", Motivo = "Terminada" }, new() { LlantaId = operating.Id, CentroOrigenId = center, Tipo = TipoServicioLlanta.Reparacion, Estado = "OPCIONADA", Motivo = "Pendiente" });
            await db.SaveChangesAsync();
            var service = new AnaliticaService(db);var access = new AlcanceCentros(false, [center]);var filter = new FiltroAnalitica { MarcaId = brand.Id, MinimoMuestra = 2 };
            var summary = await service.ResumenAsync(filter, access, default);
            Assert.Equal(4, summary.Total);Assert.Equal(800m, summary.EnOperacion.Promedio);Assert.Equal(2000m, summary.Finalizadas.Promedio);
            Assert.Equal(2, summary.SinKm);Assert.Equal(1, summary.ConTramosIncompletos);Assert.Null(summary.ProfundidadPromedio);
            var comparison = Assert.Single((await service.CompararAsync(filter, "marca", access, default)).Items);
            Assert.Equal(.25m, comparison.ReparacionesPromedio);Assert.False(comparison.MuestraSuficiente);
            var positions = await service.PosicionesAsync(filter, access, default);var p = Assert.Single(positions.Items);
            Assert.Equal(4, p.Tramos);Assert.Equal(3, p.TramosValidos);Assert.Equal(3, p.Llantas);Assert.True(p.MuestraSuficiente);
            var movements = await service.MovimientosAsync(filter, access, default);
            var ranked = Assert.Single(movements.Ranking.Items, x => x.Id == operating.Id);Assert.Equal(2, ranked.Total);
            Assert.DoesNotContain(movements.Tipos, x => x.Nombre == "OCULTO");Assert.DoesNotContain(movements.Ranking.Items, x => x.Id == hidden.Id);
            Assert.Equal(0, (await service.ResumenAsync(new() { MarcaId = brand.Id, IngresoDesde = new(2026, 2, 1) }, access, default)).Total);
            Assert.Equal(0, (await service.ResumenAsync(filter, new(false, []), default)).Total);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ResumenAsync(new() { CentroId = other.Id }, access, default));
            var inspection = new Inspeccion { CentroId = center, VehiculoId = position.EjeVehiculo.VehiculoId, Estado = EstadoInspeccion.Finalizada, Kilometraje = 1000,
                Detalles = [new() { LlantaId = missing.Id, PosicionVehiculoId = position.Id, ProfundidadExterior = 9, ProfundidadCentro = 8, ProfundidadInterior = 10 }] };
            var hiddenInspection = new Inspeccion { CentroId = other.Id, VehiculoId = position.EjeVehiculo.VehiculoId, Estado = EstadoInspeccion.Finalizada, Kilometraje = 2000,
                Detalles = [new() { LlantaId = missing.Id, PosicionVehiculoId = position.Id, ProfundidadExterior = 3, ProfundidadCentro = 3, ProfundidadInterior = 3 }] };
            db.AddRange(inspection, hiddenInspection);
            db.ParametrosAlerta.Add(new() { Codigo = "PRIVADO-" + key, CentroId = other.Id, Tipo = "PROFUNDIDAD_MINIMA", Valor = 5 });
            await db.SaveChangesAsync();
            var wear = await service.DesgasteAsync(missing.Id, access, default);
            Assert.Equal(1, wear.TotalLecturas);Assert.Equal(1, wear.LecturasCompletas);Assert.Equal(0, wear.LecturasConTramo);
            Assert.Equal(8m, Assert.Single(wear.Mediciones).Minima);Assert.Equal("Datos insuficientes", wear.Pronostico);
            Assert.DoesNotContain(wear.Reglas, x => x.Codigo == "PRIVADO-" + key);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DesgasteAsync(hidden.Id, access, default));
            Assert.Single((await service.LlantasAsync(new() { MarcaId = brand.Id, Buscar = missing.Codigo }, "medidas", access, default)).Items);
            var finalList = await service.LlantasAsync(filter, "finalizadas", access, default);
            Assert.Equal(summary.DisposicionFinal, finalList.TotalItems);Assert.Equal(finalized.Id, Assert.Single(finalList.Items).Id);
            Assert.Empty((await service.LlantasAsync(filter, "todas", new(false, []), default)).Items);
            await tx.RollbackAsync();
        });
    }
}
