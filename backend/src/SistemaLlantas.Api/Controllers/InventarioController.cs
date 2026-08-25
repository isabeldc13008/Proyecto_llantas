using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/inventario"),Authorize]
public sealed class InventarioController(LlantasDbContext db):ControllerBase
{
    [HttpGet("metricas")]
    public async Task<InventarioMetricasDto> Metricas(CancellationToken ct)
    {
        var q=Alcanzables();
        var disponibles=await q.CountAsync(x=>x.EstadoLlanta.PermiteMontaje&&!db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.EsActiva),ct);
        var reparacion=await q.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("REPAR"),ct);
        var reencauche=await q.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("REENCAUCH"),ct);
        var traslado=await q.CountAsync(x=>x.EstadoLlanta.Codigo=="EN_TRASLADO",ct);
        var bloqueadas=await q.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("BLOQUE")||!x.Activo,ct);
        var atencion=await q.CountAsync(x=>string.IsNullOrWhiteSpace(x.UbicacionActual)||db.AlertasInspeccion.Any(a=>a.LlantaId==x.Id&&a.Activo&&(a.Estado==EstadoAlerta.ABIERTA||a.Estado==EstadoAlerta.EN_PROCESO))||db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.EsActiva&&a.KilometrajeMontaje.HasValue&&a.PosicionVehiculo.EjeVehiculo.Vehiculo.Kilometraje<a.KilometrajeMontaje.Value),ct);
        return new(disponibles,reparacion,reencauche,traslado,bloqueadas,atencion);
    }

    [HttpGet("reservas")]
    public async Task<IReadOnlyList<ReservaInventarioDto>> Reservas(CancellationToken ct)
    {
        var a=User.AlcanceCentros();
        return await db.SolicitudesOperacion.AsNoTracking().Where(x=>x.Activo&&x.Tipo=="RESERVA"&&x.Estado==EstadoSolicitudOperacion.EJECUTADO&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)))
            .OrderByDescending(x=>x.FechaCreacion).Select(x=>new ReservaInventarioDto(x.Id,x.LlantaId,x.Llanta.Codigo,x.PosicionDestinoId,x.TipoDestino,x.Motivo,x.Solicitante,x.FechaCreacion)).ToListAsync(ct);
    }

    [HttpPost("{id:guid}/reservar"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<ActionResult> Reservar(Guid id,ReservarLlantaDto dto,CancellationToken ct)
    {
        var tire=await Alcanzables().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException("Llanta no encontrada.");
        if(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==id&&x.EsActiva,ct))throw new ConflictoException("Una llanta montada no se puede reservar.");
        if(await db.SolicitudesOperacion.AnyAsync(x=>x.LlantaId==id&&x.Activo&&x.Tipo=="RESERVA"&&x.Estado==EstadoSolicitudOperacion.EJECUTADO,ct))throw new ConflictoException("La llanta ya tiene una reserva activa.");
        if(dto.VehiculoId.HasValue&&!await db.Vehiculos.AnyAsync(x=>x.Id==dto.VehiculoId&&x.CentroId==tire.CentroId,ct))throw new ValidacionException("El vehículo de la reserva debe pertenecer al mismo centro.");
        var item=new SolicitudOperacion{Tipo="RESERVA",Estado=EstadoSolicitudOperacion.EJECUTADO,CentroId=tire.CentroId,LlantaId=id,PosicionDestinoId=dto.PosicionId,TipoDestino=dto.VehiculoId?.ToString()??"Sin vehículo",Motivo=string.IsNullOrWhiteSpace(dto.Motivo)?"Reserva operativa":dto.Motivo.Trim(),Solicitante=Usuario(),ActividadProgramadaId=dto.ActividadProgramadaId,UsuarioCreacion=Usuario()};
        db.SolicitudesOperacion.Add(item);await db.SaveChangesAsync(ct);return NoContent();
    }

    [HttpPost("{id:guid}/liberar-reserva"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<ActionResult> Liberar(Guid id,CancellationToken ct)
    {
        if(!await Alcanzables().AnyAsync(x=>x.Id==id,ct))throw new KeyNotFoundException("Llanta no encontrada.");
        var reservation=await db.SolicitudesOperacion.SingleOrDefaultAsync(x=>x.LlantaId==id&&x.Activo&&x.Tipo=="RESERVA"&&x.Estado==EstadoSolicitudOperacion.EJECUTADO,ct)??throw new KeyNotFoundException("Reserva activa no encontrada.");
        reservation.Activo=false;reservation.FechaModificacion=DateTimeOffset.UtcNow;reservation.UsuarioModificacion=Usuario();await db.SaveChangesAsync(ct);return NoContent();
    }

    [HttpPatch("{id:guid}/ubicacion"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<ActionResult> Ubicacion(Guid id,ActualizarUbicacionDto dto,CancellationToken ct)
    {
        var tire=await Alcanzables().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException("Llanta no encontrada.");
        if(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==id&&x.EsActiva,ct))throw new ConflictoException("La ubicación física solo se administra para llantas sin montaje activo.");
        tire.UbicacionActual=string.Join(" · ",new[]{dto.ZonaBodega?.Trim(),dto.Rack?.Trim()}.Where(x=>!string.IsNullOrWhiteSpace(x)));if(string.IsNullOrWhiteSpace(tire.UbicacionActual))tire.UbicacionActual="Ubicación no definida";
        tire.UsuarioModificacion=Usuario();tire.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return NoContent();
    }

    [HttpGet("compatibles")]
    public async Task<IReadOnlyList<CompatibleInventarioDto>> Compatibles(Guid centroId,Guid dimensionId,Guid? tipoLlantaId,CancellationToken ct)
    {
        var a=User.AlcanceCentros();if(!a.Autoriza(centroId))throw new UnauthorizedAccessException("Centro no autorizado.");
        return await db.Llantas.AsNoTracking().Where(x=>x.CentroId==centroId&&x.DimensionId==dimensionId&&(!tipoLlantaId.HasValue||x.TipoLlantaId==tipoLlantaId)&&x.EstadoLlanta.PermiteMontaje&&!db.AsignacionesLlantaPosicion.Any(m=>m.LlantaId==x.Id&&m.EsActiva)&&!db.SolicitudesOperacion.Any(r=>r.LlantaId==x.Id&&r.Activo&&r.Tipo=="RESERVA"&&r.Estado==EstadoSolicitudOperacion.EJECUTADO)).OrderBy(x=>x.Codigo).Take(50).Select(x=>new CompatibleInventarioDto(x.Id,x.Codigo,x.Serial,x.Dimension.Nombre,x.TipoLlanta.Nombre,x.UbicacionActual)).ToListAsync(ct);
    }

    private IQueryable<Llanta> Alcanzables(){var a=User.AlcanceCentros();var q=db.Llantas.AsQueryable();return a.VerTodos?q:q.Where(x=>a.CentroIds.Contains(x.CentroId));}
    private string Usuario()=>User.Identity?.Name??User.FindFirst("sub")?.Value??"sistema";
}

public sealed record InventarioMetricasDto(int Disponibles,int EnReparacion,int EnReencauche,int EnTraslado,int Bloqueadas,int ConAtencion);
public sealed record ReservaInventarioDto(Guid Id,Guid LlantaId,string Llanta,Guid? PosicionId,string VehiculoId,string Motivo,string Solicitante,DateTimeOffset Fecha);
public sealed record CompatibleInventarioDto(Guid Id,string Codigo,string Serial,string Dimension,string Tipo,string Ubicacion);
public sealed record ReservarLlantaDto(Guid? VehiculoId,Guid? PosicionId,Guid? ActividadProgramadaId,string? Motivo);
public sealed record ActualizarUbicacionDto(string? ZonaBodega,string? Rack);
