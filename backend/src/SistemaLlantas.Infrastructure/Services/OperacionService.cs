using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class OperacionService(LlantasDbContext db) : IOperacionService
{
    public async Task<IReadOnlyList<ActividadDto>> MisActividadesAsync(string usuario, Guid? centro, CancellationToken ct) =>
        await db.ActividadesProgramadas.AsNoTracking().Where(x=>x.TecnicoId==usuario && x.Estado!=EstadoActividad.Cancelada && (!centro.HasValue||x.CentroId==centro))
        .OrderBy(x=>x.FechaProgramada).Select(x=>new ActividadDto(x.Id,x.TipoActividad,x.FechaProgramada,x.Centro.Nombre,x.VehiculoId,
            x.Vehiculo==null?"Sin vehículo":$"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}",x.Prioridad,x.Estado.ToString(),
            x.TipoActividad=="Inspección"?$"/inspecciones?actividadId={x.Id}&vehiculoId={x.VehiculoId}":$"/montajes?actividadId={x.Id}&vehiculoId={x.VehiculoId}" )).ToListAsync(ct);

    public async Task<ActividadDto> IniciarActividadAsync(Guid id,string usuario,CancellationToken ct)
    {
        var x=await db.ActividadesProgramadas.Include(a=>a.Centro).Include(a=>a.Vehiculo).SingleOrDefaultAsync(a=>a.Id==id,ct)??throw new KeyNotFoundException("Actividad no encontrada.");
        if(x.TecnicoId!=usuario) throw new UnauthorizedAccessException("La actividad está asignada a otro técnico.");
        if(x.Estado is EstadoActividad.Cumplida or EstadoActividad.Cancelada) throw new InvalidOperationException("La actividad no se puede iniciar.");
        x.Estado=EstadoActividad.EnEjecucion; x.FechaInicioReal??=DateTimeOffset.UtcNow; x.UsuarioModificacion=usuario; await db.SaveChangesAsync(ct);
        return new(x.Id,x.TipoActividad,x.FechaProgramada,x.Centro.Nombre,x.VehiculoId,x.Vehiculo==null?"Sin vehículo":$"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}",x.Prioridad,x.Estado.ToString(),x.TipoActividad=="Inspección"?$"/inspecciones?actividadId={x.Id}&vehiculoId={x.VehiculoId}":$"/montajes?actividadId={x.Id}&vehiculoId={x.VehiculoId}");
    }

    public async Task<MovimientoDto> MoverAsync(EjecutarMovimientoDto dto,string usuario,Guid? centro,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var llanta=await db.Llantas.Include(x=>x.EstadoLlanta).SingleOrDefaultAsync(x=>x.Id==dto.LlantaId && (!centro.HasValue||x.CentroId==centro),ct)??throw new KeyNotFoundException("Llanta no encontrada.");
        if(!llanta.EstadoLlanta.PermiteMontaje && dto.PosicionDestinoId.HasValue) throw new InvalidOperationException($"La llanta está en estado {llanta.EstadoLlanta.Nombre} y no permite montaje.");
        var actual=await db.AsignacionesLlantaPosicion.SingleOrDefaultAsync(x=>x.LlantaId==dto.LlantaId&&x.EsActiva,ct);
        if(dto.PosicionOrigenId.HasValue && actual?.PosicionVehiculoId!=dto.PosicionOrigenId) throw new InvalidOperationException("La posición origen no coincide con la asignación activa.");
        AsignacionLlantaPosicion? ocupante=null;
        if(dto.PosicionDestinoId.HasValue) ocupante=await db.AsignacionesLlantaPosicion.SingleOrDefaultAsync(x=>x.PosicionVehiculoId==dto.PosicionDestinoId&&x.EsActiva,ct);
        if(ocupante is not null && ocupante.LlantaId!=dto.LlantaId && !dto.LlantaDesplazadaId.HasValue) throw new InvalidOperationException("POSICION_DESTINO_OCUPADA: debe indicar el destino de la llanta instalada.");
        var mov=new Movimiento{Numero=$"MOV-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",Tipo="Movimiento",Motivo=dto.Motivo,CentroId=llanta.CentroId,Usuario=usuario,UsuarioCreacion=usuario};
        if(actual is not null){actual.EsActiva=false;actual.FechaFin=DateTimeOffset.UtcNow;actual.UsuarioModificacion=usuario;}
        if(ocupante is not null && ocupante.LlantaId!=dto.LlantaId){ if(ocupante.LlantaId!=dto.LlantaDesplazadaId) throw new InvalidOperationException("La llanta desplazada no coincide."); ocupante.EsActiva=false;ocupante.FechaFin=DateTimeOffset.UtcNow;ocupante.UsuarioModificacion=usuario; mov.Detalles.Add(new(){LlantaId=ocupante.LlantaId,PosicionOrigenId=ocupante.PosicionVehiculoId,PosicionDestinoId=dto.PosicionDestinoDesplazadaId,TipoDestino=ParseDestino(dto.DestinoDesplazada),DestinoDescripcion=dto.DestinoDesplazada,UsuarioCreacion=usuario}); }
        var destino=ParseDestino(dto.TipoDestino); mov.Detalles.Add(new(){LlantaId=llanta.Id,PosicionOrigenId=actual?.PosicionVehiculoId,PosicionDestinoId=dto.PosicionDestinoId,TipoDestino=destino,DestinoDescripcion=dto.TipoDestino,UsuarioCreacion=usuario});
        db.Movimientos.Add(mov); await db.SaveChangesAsync(ct);
        if(dto.PosicionDestinoId.HasValue){db.AsignacionesLlantaPosicion.Add(new(){LlantaId=llanta.Id,PosicionVehiculoId=dto.PosicionDestinoId.Value,MovimientoOrigenId=mov.Id,UsuarioCreacion=usuario});}
        if(ocupante is not null&&dto.PosicionDestinoDesplazadaId.HasValue) db.AsignacionesLlantaPosicion.Add(new(){LlantaId=ocupante.LlantaId,PosicionVehiculoId=dto.PosicionDestinoDesplazadaId.Value,MovimientoOrigenId=mov.Id,UsuarioCreacion=usuario});
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Map(mov);
    }

    public async Task<MovimientoDto> DesmontarAsync(DesmontarLlantaDto dto,string usuario,Guid? centro,CancellationToken ct)
    { var a=await db.AsignacionesLlantaPosicion.AsNoTracking().Include(x=>x.Llanta).SingleOrDefaultAsync(x=>x.PosicionVehiculoId==dto.PosicionId&&x.EsActiva,ct)??throw new InvalidOperationException("La posición no tiene una llanta activa."); return await MoverAsync(new(){LlantaId=a.LlantaId,PosicionOrigenId=dto.PosicionId,TipoDestino=dto.Destino,Motivo=dto.Motivo},usuario,centro,ct); }
    private static TipoDestinoLlanta ParseDestino(string? value)=>Enum.TryParse<TipoDestinoLlanta>((value??"Otro").Replace("ó","o"),true,out var x)?x:TipoDestinoLlanta.Otro;
    private static MovimientoDto Map(Movimiento x)=>new(x.Id,x.Numero,x.Tipo,x.Motivo,x.FechaCreacion,x.Detalles.Select(d=>new MovimientoDetalleDto(d.LlantaId,d.Llanta?.Codigo??d.LlantaId.ToString(),d.PosicionOrigenId?.ToString(),d.PosicionDestinoId?.ToString()??d.DestinoDescripcion??d.TipoDestino.ToString())).ToList());
}
