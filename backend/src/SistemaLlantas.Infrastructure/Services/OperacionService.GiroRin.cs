using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Services;

public sealed partial class OperacionService
{
    public Task<MovimientoDto> GirarEnRinAsync(GiroRinDto dto, string usuario, AlcanceCentros alcance, CancellationToken ct)
    {
        dto.Validar(DateTimeOffset.UtcNow);
        return db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            await ReglaLlantaTerminal.ValidarAsync(db, [dto.LlantaId], ct);
            var assignment = await AsignacionActiva(db.AsignacionesLlantaPosicion.Where(x => x.LlantaId == dto.LlantaId && x.EsActiva), "llanta seleccionada", ct)
                ?? throw new ConflictoException("La llanta ya no tiene una asignación activa.");
            var position = assignment.PosicionVehiculo;
            var vehicle = position.EjeVehiculo.Vehiculo;
            if (!alcance.Autoriza(vehicle.CentroId) || !alcance.Autoriza(assignment.Llanta.CentroId)) throw new UnauthorizedAccessException("Vehículo o llanta fuera de los centros autorizados.");
            if (!await db.Centros.AnyAsync(c => c.Id == vehicle.CentroId && c.Activo, ct) || !await db.Llantas.AnyAsync(t => t.Id == dto.LlantaId && t.EstadoLlanta.Activo && !t.EstadoLlanta.EsDisposicionFinal, ct)) throw new ConflictoException("El centro o estado de la llanta no permite la intervención.");
            var positionAssignments = await db.AsignacionesLlantaPosicion.Where(x => x.PosicionVehiculoId == position.Id && x.EsActiva).Take(2).ToListAsync(ct);
            AsignacionConsistente(position, dto.LlantaId, positionAssignments);
            var technician = await db.UsuariosSistema.SingleOrDefaultAsync(x => x.Id == dto.TecnicoId && x.Activo && x.Rol.Activo && x.Rol.Codigo == "TECNICO" && x.Centros.Any(c => c.Activo && c.CentroId == vehicle.CentroId), ct)
                ?? throw new ValidacionException("El técnico debe estar activo y asignado al centro del vehículo.");
            var movement = CrearRegistroGiro(dto, assignment, technician, usuario);
            db.Movimientos.Add(movement);
            ActualizarOdometro(vehicle, dto.Kilometraje);
            vehicle.UsuarioModificacion = usuario;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Map(movement);
        });
    }

    // An intervention records history only; it never closes or recreates an assignment.
    public static Movimiento CrearRegistroGiro(GiroRinDto dto, AsignacionLlantaPosicion assignment, UsuarioSistema technician, string usuario)
    {
        dto.Validar(DateTimeOffset.UtcNow);
        var position = assignment.PosicionVehiculo;
        var vehicle = position.EjeVehiculo.Vehiculo;
        if (!assignment.Activo || !assignment.EsActiva || assignment.LlantaId != dto.LlantaId || position.Id != dto.PosicionId || vehicle.Id != dto.VehiculoId || position.LlantaActualId != dto.LlantaId)
            throw new ConflictoException("La llanta, posición o vehículo cambió. Actualiza la selección.");
        if (!position.Activo || !position.EjeVehiculo.Activo || !vehicle.Activo || !assignment.Llanta.Activo)
            throw new ConflictoException("La llanta o el vehículo está inactivo.");
        if (assignment.FechaInicio.HasValue && dto.Fecha < assignment.FechaInicio.Value)
            throw new ValidacionException("La fecha no puede ser anterior al montaje vigente.");
        KilometrajeOperacion.Validar(dto.Kilometraje, vehicle.Kilometraje, [new(assignment.Llanta.Codigo, position.Codigo, assignment.KilometrajeMontaje)]);
        var movement = new Movimiento
        {
            Numero = $"GIR-{Guid.NewGuid():N}"[..28], Tipo = "Giro en el mismo rin", Motivo = dto.Motivo.Trim(),
            CentroId = vehicle.CentroId, Usuario = usuario, UsuarioCreacion = usuario,
            FechaGiro = dto.Fecha, KilometrajeGiro = dto.Kilometraje, TecnicoGiroId = technician.Id,
            Observaciones = FormattableString.Invariant($"Fecha intervención: {dto.Fecha:O} · Técnico: {technician.Nombre} ({technician.Username}) · Kilometraje: {dto.Kilometraje:0.##} km · Vehículo: {vehicle.Placa} · Posición: {position.Codigo} · Sin cambio de rin ni asignación.")
        };
        movement.Detalles.Add(new MovimientoDetalle { LlantaId = dto.LlantaId, PosicionOrigenId = position.Id, PosicionDestinoId = position.Id, TipoDestino = TipoDestinoLlanta.Posicion, DestinoDescripcion = "Mismo rin y posición", UsuarioCreacion = usuario });
        return movement;
    }
}
