using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class OperacionService(LlantasDbContext db) : IOperacionService
{
    public async Task<IReadOnlyList<ActividadDto>> MisActividadesAsync(string usuario, AlcanceCentros alcance, CancellationToken ct) =>
        await db.ActividadesProgramadas.AsNoTracking().Where(x=>x.Activo&&(x.TecnicoId==usuario||x.TecnicoId==usuario+".local"||x.TecnicoUsuario!.Username==usuario) && x.Estado!=EstadoActividad.Cancelada && (alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)))
        .OrderBy(x=>x.FechaProgramada).Select(x=>new ActividadDto(x.Id,x.TipoActividad,x.FechaProgramada,x.Centro.Nombre,x.VehiculoId,
            x.Vehiculo==null?"Sin vehículo":$"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}",x.Prioridad,x.Estado.ToString(),
            x.TipoActividad=="Inspección"?$"/inspecciones?actividadId={x.Id}&vehiculoId={x.VehiculoId}":$"/montajes?actividadId={x.Id}&vehiculoId={x.VehiculoId}",x.FechaFinReal)).ToListAsync(ct);

    public async Task<ActividadDto> IniciarActividadAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        var x=await db.ActividadesProgramadas.Include(a=>a.Centro).Include(a=>a.Vehiculo).Include(a=>a.TecnicoUsuario).SingleOrDefaultAsync(a=>a.Id==id&&a.Activo&&(alcance.VerTodos||alcance.CentroIds.Contains(a.CentroId)),ct)??throw new KeyNotFoundException("Actividad no encontrada.");
        if(x.TecnicoId!=usuario&&x.TecnicoId!=usuario+".local"&&x.TecnicoUsuario?.Username!=usuario) throw new UnauthorizedAccessException("La actividad está asignada a otro técnico.");
        if(x.Estado is EstadoActividad.Cumplida or EstadoActividad.Cancelada) throw new InvalidOperationException("La actividad no se puede iniciar.");
        x.Estado=EstadoActividad.EnEjecucion; x.FechaInicioReal??=DateTimeOffset.UtcNow; x.UsuarioModificacion=usuario; await db.SaveChangesAsync(ct);
        return new(x.Id,x.TipoActividad,x.FechaProgramada,x.Centro.Nombre,x.VehiculoId,x.Vehiculo==null?"Sin vehículo":$"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}",x.Prioridad,x.Estado.ToString(),x.TipoActividad=="Inspección"?$"/inspecciones?actividadId={x.Id}&vehiculoId={x.VehiculoId}":$"/montajes?actividadId={x.Id}&vehiculoId={x.VehiculoId}",x.FechaFinReal);
    }

    public async Task<ActividadDto> CompletarActividadAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        var x=await db.ActividadesProgramadas.Include(a=>a.Centro).Include(a=>a.Vehiculo).Include(a=>a.TecnicoUsuario).SingleOrDefaultAsync(a=>a.Id==id&&a.Activo&&(alcance.VerTodos||alcance.CentroIds.Contains(a.CentroId)),ct)??throw new KeyNotFoundException("Actividad no encontrada.");if(x.TecnicoId!=usuario&&x.TecnicoId!=usuario+".local"&&x.TecnicoUsuario?.Username!=usuario)throw new UnauthorizedAccessException("La actividad está asignada a otro técnico.");if(x.Estado!=EstadoActividad.EnEjecucion)throw new ConflictoException("Sólo una actividad en ejecución puede completarse.");x.Estado=EstadoActividad.Cumplida;x.FechaFinReal=DateTimeOffset.UtcNow;x.UsuarioModificacion=usuario;x.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return new(x.Id,x.TipoActividad,x.FechaProgramada,x.Centro.Nombre,x.VehiculoId,x.Vehiculo==null?"Sin vehículo":$"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}",x.Prioridad,x.Estado.ToString(),x.TipoActividad=="Inspección"?$"/inspecciones?actividadId={x.Id}&vehiculoId={x.VehiculoId}":$"/montajes?actividadId={x.Id}&vehiculoId={x.VehiculoId}",x.FechaFinReal);
    }

    public Task<MovimientoDto> MoverAsync(EjecutarMovimientoDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        if(db.Database.CurrentTransaction is not null)return MoverCoreAsync(dto,usuario,alcance,ct);
        var strategy=db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(()=>MoverCoreAsync(dto,usuario,alcance,ct));
    }

    private async Task<MovimientoDto> MoverCoreAsync(EjecutarMovimientoDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        if((dto.PosicionOrigenId.HasValue||dto.PosicionDestinoId.HasValue)&&!dto.KilometrajeVehiculo.HasValue)throw new ValidacionException("Ingresa el kilometraje actual para continuar.");
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var llanta=await db.Llantas.Include(x=>x.EstadoLlanta).SingleOrDefaultAsync(x=>x.Id==dto.LlantaId && (alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Llanta no encontrada.");
        if(!llanta.EstadoLlanta.PermiteMontaje && dto.PosicionDestinoId.HasValue) throw new InvalidOperationException($"La llanta está en estado {llanta.EstadoLlanta.Nombre} y no permite montaje.");
        var actual=await db.AsignacionesLlantaPosicion.Include(x=>x.PosicionVehiculo).ThenInclude(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).SingleOrDefaultAsync(x=>x.LlantaId==dto.LlantaId&&x.EsActiva,ct);
        if(dto.PosicionOrigenId.HasValue && actual?.PosicionVehiculoId!=dto.PosicionOrigenId) throw new InvalidOperationException("La posición origen no coincide con la asignación activa.");
        AsignacionLlantaPosicion? ocupante=null;
        PosicionVehiculo? posicionDestino=null;
        if(dto.PosicionDestinoId.HasValue){posicionDestino=await db.PosicionesVehiculo.Include(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).SingleOrDefaultAsync(x=>x.Id==dto.PosicionDestinoId&&(alcance.VerTodos||alcance.CentroIds.Contains(x.EjeVehiculo.Vehiculo.CentroId)),ct)??throw new UnauthorizedAccessException("La posición destino no pertenece a los centros autorizados.");ocupante=await db.AsignacionesLlantaPosicion.Include(x=>x.PosicionVehiculo).ThenInclude(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).SingleOrDefaultAsync(x=>x.PosicionVehiculoId==dto.PosicionDestinoId&&x.EsActiva,ct);}
        if(ocupante is not null && ocupante.LlantaId!=dto.LlantaId && !dto.LlantaDesplazadaId.HasValue) throw new InvalidOperationException("POSICION_DESTINO_OCUPADA: debe indicar el destino de la llanta instalada.");
        var operationType=actual is null&&dto.PosicionDestinoId.HasValue?"MONTAJE":actual is not null&&dto.PosicionDestinoId.HasValue?"ROTACION":dto.TipoDestino.ToUpperInvariant() switch{"REPARACION"=>"ENVIO_REPARACION","REENCAUCHE"=>"ENVIO_REENCAUCHE","DISPOSICIONFINAL"=>"DISPOSICION_FINAL",_=>"DESMONTAJE"};
        var mov=new Movimiento{Numero=$"MOV-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..28],Tipo=operationType,Motivo=dto.Motivo,Observaciones=dto.Observaciones,CentroId=llanta.CentroId,Usuario=usuario,UsuarioCreacion=usuario};
        if(actual is not null){CerrarAsignacion(actual,dto.KilometrajeVehiculo,llanta,usuario);actual.PosicionVehiculo.LlantaActualId=null;}
        if(ocupante is not null && ocupante.LlantaId!=dto.LlantaId){ if(ocupante.LlantaId!=dto.LlantaDesplazadaId) throw new InvalidOperationException("La llanta desplazada no coincide.");var displaced=await db.Llantas.SingleAsync(x=>x.Id==ocupante.LlantaId,ct);CerrarAsignacion(ocupante,dto.KilometrajeVehiculo,displaced,usuario);ocupante.PosicionVehiculo.LlantaActualId=null;mov.Detalles.Add(new(){LlantaId=ocupante.LlantaId,PosicionOrigenId=ocupante.PosicionVehiculoId,PosicionDestinoId=dto.PosicionDestinoDesplazadaId,TipoDestino=ParseDestino(dto.DestinoDesplazada),DestinoDescripcion=dto.DestinoDesplazada,UsuarioCreacion=usuario}); }
        var destino=ParseDestino(dto.TipoDestino); mov.Detalles.Add(new(){LlantaId=llanta.Id,PosicionOrigenId=actual?.PosicionVehiculoId,PosicionDestinoId=dto.PosicionDestinoId,TipoDestino=destino,DestinoDescripcion=dto.TipoDestino,UsuarioCreacion=usuario});
        await CambiarEstadoAsync(llanta,dto.PosicionDestinoId.HasValue?"MONTADA":EstadoDestino(destino),ct);
        db.Movimientos.Add(mov); await db.SaveChangesAsync(ct);
        if(dto.PosicionDestinoId.HasValue){var km=dto.KilometrajeVehiculo??posicionDestino!.EjeVehiculo.Vehiculo.Kilometraje;db.AsignacionesLlantaPosicion.Add(new(){LlantaId=llanta.Id,PosicionVehiculoId=dto.PosicionDestinoId.Value,MovimientoOrigenId=mov.Id,KilometrajeMontaje=km,UsuarioCreacion=usuario});posicionDestino!.LlantaActualId=llanta.Id;ActualizarOdometro(posicionDestino.EjeVehiculo.Vehiculo,dto.KilometrajeVehiculo);}
        if(ocupante is not null&&dto.PosicionDestinoDesplazadaId.HasValue){var displacedDestination=await db.PosicionesVehiculo.Include(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).SingleAsync(x=>x.Id==dto.PosicionDestinoDesplazadaId,ct);db.AsignacionesLlantaPosicion.Add(new(){LlantaId=ocupante.LlantaId,PosicionVehiculoId=dto.PosicionDestinoDesplazadaId.Value,MovimientoOrigenId=mov.Id,KilometrajeMontaje=dto.KilometrajeVehiculo??displacedDestination.EjeVehiculo.Vehiculo.Kilometraje,UsuarioCreacion=usuario});displacedDestination.LlantaActualId=ocupante.LlantaId;}
        await db.SaveChangesAsync(ct); if(tx is not null)await tx.CommitAsync(ct); return Map(mov);
    }

    public async Task<MovimientoDto> DesmontarAsync(DesmontarLlantaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    { var a=await db.AsignacionesLlantaPosicion.AsNoTracking().Include(x=>x.Llanta).SingleOrDefaultAsync(x=>x.PosicionVehiculoId==dto.PosicionId&&x.EsActiva&&(alcance.VerTodos||alcance.CentroIds.Contains(x.Llanta.CentroId)),ct)??throw new InvalidOperationException("La posición no tiene una llanta activa o no pertenece a tus centros autorizados."); return await MoverAsync(new(){LlantaId=a.LlantaId,PosicionOrigenId=dto.PosicionId,TipoDestino=dto.Destino,Motivo=dto.Motivo,KilometrajeVehiculo=dto.KilometrajeVehiculo,Observaciones=dto.Observaciones},usuario,alcance,ct); }
    private static void CerrarAsignacion(AsignacionLlantaPosicion assignment,decimal? odometer,Llanta tire,string usuario){var end=odometer??assignment.PosicionVehiculo.EjeVehiculo.Vehiculo.Kilometraje;if(end.HasValue&&assignment.KilometrajeMontaje.HasValue&&end<assignment.KilometrajeMontaje)throw new ValidacionException("El kilometraje de desmontaje no puede ser menor al de montaje.");assignment.EsActiva=false;assignment.FechaFin=DateTimeOffset.UtcNow;assignment.KilometrajeDesmontaje=end;assignment.KilometrajeRecorrido=end.HasValue&&assignment.KilometrajeMontaje.HasValue?end-assignment.KilometrajeMontaje:null;assignment.UsuarioModificacion=usuario;if(assignment.KilometrajeRecorrido.HasValue)tire.KilometrajeAcumulado+=assignment.KilometrajeRecorrido.Value;ActualizarOdometro(assignment.PosicionVehiculo.EjeVehiculo.Vehiculo,odometer);}
    private static void ActualizarOdometro(Vehiculo vehicle,decimal? odometer){if(!odometer.HasValue)return;if(vehicle.Kilometraje.HasValue&&odometer<vehicle.Kilometraje)throw new ValidacionException("El kilometraje no puede ser menor al odómetro registrado del vehículo.");vehicle.Kilometraje=odometer;}
    private static TipoDestinoLlanta ParseDestino(string? value)=>Enum.TryParse<TipoDestinoLlanta>((value??"Otro").Replace("ó","o"),true,out var x)?x:TipoDestinoLlanta.Otro;
    private static string EstadoDestino(TipoDestinoLlanta destino)=>destino switch{TipoDestinoLlanta.Reparacion=>"EN_REPARACION",TipoDestinoLlanta.Reencauche=>"EN_REENCAUCHE",TipoDestinoLlanta.DisposicionFinal=>"PEND_DISPOSICION",TipoDestinoLlanta.Traslado=>"EN_TRASLADO",_=>"DISPONIBLE"};
    private async Task CambiarEstadoAsync(Llanta tire,string code,CancellationToken ct){var stateId=await db.EstadosLlanta.Where(x=>x.Codigo==code).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct);if(stateId.HasValue)tire.EstadoLlantaId=stateId.Value;}
    private static MovimientoDto Map(Movimiento x)=>new(x.Id,x.Numero,x.Tipo,x.Motivo,x.FechaCreacion,x.Detalles.Select(d=>new MovimientoDetalleDto(d.LlantaId,d.Llanta?.Codigo??d.LlantaId.ToString(),d.PosicionOrigenId?.ToString(),d.PosicionDestinoId?.ToString()??d.DestinoDescripcion??d.TipoDestino.ToString())).ToList());
}
