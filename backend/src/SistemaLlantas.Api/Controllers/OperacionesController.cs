using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Api.Security;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Application.Llantas;
using Application = SistemaLlantas.Application;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Authorize]
public sealed class OperacionesController(IOperacionService service,ICicloVidaLlantaService ciclo,LlantasDbContext db):ControllerBase
{
    [HttpGet("api/mis-actividades"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<IReadOnlyList<ActividadDto>> Actividades(CancellationToken ct)=>service.MisActividadesAsync(Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/actividades/{id:guid}/iniciar"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<ActividadDto> Iniciar(Guid id,CancellationToken ct)=>service.IniciarActividadAsync(id,Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/actividades/{id:guid}/completar"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<ActividadDto> Completar(Guid id,CancellationToken ct)=>service.CompletarActividadAsync(id,Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/movimientos"),Authorize(Policy="Operaciones.Ejecutar")]
    public Task<MovimientoDto> Mover(EjecutarMovimientoDto dto,CancellationToken ct)=>service.MoverAsync(dto,Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/desmontajes"),Authorize(Policy="Operaciones.Ejecutar")]
    public Task<MovimientoDto> Desmontar(DesmontarLlantaDto dto,CancellationToken ct)=>service.DesmontarAsync(dto,Usuario(),User.AlcanceCentros(),ct);
    [HttpGet("api/operaciones/solicitudes"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<IReadOnlyList<SolicitudOperacionDto>> Solicitudes(CancellationToken ct){var a=User.AlcanceCentros();return await db.SolicitudesOperacion.AsNoTracking().Where(x=>x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)||(x.CentroDestinoId.HasValue&&a.CentroIds.Contains(x.CentroDestinoId.Value)))).OrderByDescending(x=>x.FechaCreacion).Select(x=>new SolicitudOperacionDto(x.Id,x.Tipo,x.Estado.ToString(),x.CentroId,x.Centro.Nombre,x.LlantaId,x.Llanta.Codigo,x.PosicionOrigenId,x.PosicionDestinoId,x.TipoDestino,x.CentroDestinoId,x.Motivo,x.Observaciones,x.Solicitante,x.Aprobador,x.MotivoRechazo,x.FechaCreacion,x.FechaRecepcionDestino,Convert.ToBase64String(x.RowVersion))).ToListAsync(ct);}
    [HttpPost("api/operaciones/solicitudes"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<ActionResult<SolicitudOperacionDto>> Solicitar(CrearSolicitudOperacionDto dto,CancellationToken ct){if(string.IsNullOrWhiteSpace(dto.Motivo))throw new Application.Common.ValidacionException("El motivo es obligatorio.");if(dto.Tipo.Equals("Montaje",StringComparison.OrdinalIgnoreCase)&&!User.HasClaim("permiso","operaciones.montar"))return Forbid();var a=User.AlcanceCentros();var tire=await db.Llantas.SingleOrDefaultAsync(x=>x.Id==dto.LlantaId&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Llanta no encontrada.");if(dto.CentroDestinoId.HasValue&&(!a.Autoriza(dto.CentroDestinoId.Value)||dto.CentroDestinoId==tire.CentroId))throw new UnauthorizedAccessException("Centro destino no autorizado o inválido.");var scheduled=dto.ActividadProgramadaId.HasValue&&await db.ActividadesProgramadas.AnyAsync(x=>x.Id==dto.ActividadProgramadaId&&x.Activo&&x.CentroId==tire.CentroId&&x.Estado!=EstadoActividad.Cancelada,ct);var item=new SolicitudOperacion{Tipo=dto.Tipo,Estado=scheduled?EstadoSolicitudOperacion.APROBADO:EstadoSolicitudOperacion.PENDIENTE_APROBACION,CentroId=tire.CentroId,LlantaId=tire.Id,PosicionOrigenId=dto.PosicionOrigenId,PosicionDestinoId=dto.PosicionDestinoId,TipoDestino=dto.CentroDestinoId.HasValue?"Traslado":dto.TipoDestino,CentroDestinoId=dto.CentroDestinoId,LlantaDesplazadaId=dto.LlantaDesplazadaId,PosicionDestinoDesplazadaId=dto.PosicionDestinoDesplazadaId,DestinoDesplazada=dto.DestinoDesplazada,Motivo=dto.Motivo.Trim(),Observaciones=dto.Observaciones,KilometrajeVehiculo=dto.KilometrajeVehiculo,ActividadProgramadaId=dto.ActividadProgramadaId,Solicitante=Usuario(),Aprobador=scheduled?"Programación autorizada":null,UsuarioCreacion=Usuario()};db.SolicitudesOperacion.Add(item);await db.SaveChangesAsync(ct);if(scheduled)await Ejecutar(item,a,ct);return Created(string.Empty,await ObtenerSolicitud(item.Id,a,ct));}
    [HttpPost("api/operaciones/solicitudes/{id:guid}/resolver"),Authorize(Policy="Operaciones.Aprobar")]
    public async Task<SolicitudOperacionDto> Resolver(Guid id,ResolverSolicitudDto dto,CancellationToken ct)
    {
        var a=User.AlcanceCentros();
        var strategy=db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async()=>
        {
            await using var tx=await db.Database.BeginTransactionAsync(ct);
            var item=await db.SolicitudesOperacion.SingleOrDefaultAsync(x=>x.Id==id&&x.Estado==EstadoSolicitudOperacion.PENDIENTE_APROBACION&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Solicitud pendiente no encontrada.");
            item.Aprobador=Usuario();item.FechaDecision=DateTimeOffset.UtcNow;
            if(!dto.Aprobar){if(string.IsNullOrWhiteSpace(dto.Motivo))throw new Application.Common.ValidacionException("El motivo de rechazo es obligatorio.");item.Estado=EstadoSolicitudOperacion.RECHAZADO;item.MotivoRechazo=dto.Motivo;await db.SaveChangesAsync(ct);}
            else{item.Estado=EstadoSolicitudOperacion.APROBADO;await db.SaveChangesAsync(ct);await Ejecutar(item,a,ct);}
            await tx.CommitAsync(ct);
        });
        db.ChangeTracker.Clear();
        return await ObtenerSolicitud(id,a,ct);
    }
    [HttpPost("api/operaciones/solicitudes/{id:guid}/recibir"),Authorize(Policy="Operaciones.Aprobar")]
    public async Task<SolicitudOperacionDto> Recibir(Guid id,CancellationToken ct){var a=User.AlcanceCentros();var item=await db.SolicitudesOperacion.Include(x=>x.Llanta).SingleOrDefaultAsync(x=>x.Id==id&&x.Estado==EstadoSolicitudOperacion.EJECUTADO&&x.CentroDestinoId.HasValue&&(a.VerTodos||a.CentroIds.Contains(x.CentroDestinoId.Value)),ct)??throw new KeyNotFoundException("Traslado pendiente de recepción no encontrado.");if(item.FechaRecepcionDestino.HasValue)throw new Application.Common.ConflictoException("El traslado ya fue recibido.");item.FechaRecepcionDestino=DateTimeOffset.UtcNow;item.Llanta.UbicacionActual="Inventario";var state=await db.EstadosLlanta.Where(x=>x.Codigo=="DISPONIBLE").Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct);if(state.HasValue)item.Llanta.EstadoLlantaId=state.Value;item.UsuarioModificacion=Usuario();await db.SaveChangesAsync(ct);return await ObtenerSolicitud(id,a,ct);}
    private string Usuario()=>User.Username();
    private async Task Ejecutar(SolicitudOperacion x,Application.Common.AlcanceCentros a,CancellationToken ct){if(x.CentroDestinoId.HasValue){await ciclo.TrasladarCentroAsync(x.LlantaId,new(x.CentroDestinoId.Value,x.Motivo,x.Observaciones),Usuario(),a,ct);}else{var move=await service.MoverAsync(new(){LlantaId=x.LlantaId,PosicionOrigenId=x.PosicionOrigenId,PosicionDestinoId=x.PosicionDestinoId,TipoDestino=x.TipoDestino,LlantaDesplazadaId=x.LlantaDesplazadaId,PosicionDestinoDesplazadaId=x.PosicionDestinoDesplazadaId,DestinoDesplazada=x.DestinoDesplazada,Motivo=x.Motivo,KilometrajeVehiculo=x.KilometrajeVehiculo,Observaciones=x.Observaciones},Usuario(),a,ct);x.MovimientoEjecutadoId=move.Id;}x.Estado=EstadoSolicitudOperacion.EJECUTADO;x.UsuarioModificacion=Usuario();x.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);}
    private async Task<SolicitudOperacionDto> ObtenerSolicitud(Guid id,Application.Common.AlcanceCentros a,CancellationToken ct)=>await db.SolicitudesOperacion.AsNoTracking().Where(x=>x.Id==id&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)||(x.CentroDestinoId.HasValue&&a.CentroIds.Contains(x.CentroDestinoId.Value)))).Select(x=>new SolicitudOperacionDto(x.Id,x.Tipo,x.Estado.ToString(),x.CentroId,x.Centro.Nombre,x.LlantaId,x.Llanta.Codigo,x.PosicionOrigenId,x.PosicionDestinoId,x.TipoDestino,x.CentroDestinoId,x.Motivo,x.Observaciones,x.Solicitante,x.Aprobador,x.MotivoRechazo,x.FechaCreacion,x.FechaRecepcionDestino,Convert.ToBase64String(x.RowVersion))).SingleAsync(ct);
}
