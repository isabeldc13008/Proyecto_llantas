using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/programacion"),Authorize(Policy="Programacion.Consultar")]
public sealed class ProgramacionController(IProgramacionService service,LlantasDbContext db):ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<ProgramacionDto>> Listar([FromQuery]Guid? centroId,[FromQuery]Guid? vehiculoId,[FromQuery]Guid? tecnicoUsuarioId,[FromQuery]string? tipo,[FromQuery]string? estado,[FromQuery]DateTimeOffset? desde,[FromQuery]DateTimeOffset? hasta,[FromQuery]string? prioridad,CancellationToken ct)=>service.ListarAsync(new(centroId,vehiculoId,tecnicoUsuarioId,tipo,estado,desde,hasta,prioridad),User.AlcanceCentros(),ct);
    [HttpGet("tecnicos")] public Task<IReadOnlyList<TecnicoProgramacionDto>> Tecnicos(CancellationToken ct)=>service.TecnicosAsync(User.AlcanceCentros(),ct);
    [HttpGet("necesidades"),Authorize(Policy="Programacion.Administrar")] public async Task<IReadOnlyList<NecesidadProgramacionDto>> Necesidades(CancellationToken ct)
    {
        var a=User.AlcanceCentros();var result=new List<NecesidadProgramacionDto>();
        var alerts=await db.AlertasInspeccion.AsNoTracking().Where(x=>x.Activo&&(x.Estado==EstadoAlerta.ABIERTA||x.Estado==EstadoAlerta.EN_PROCESO)&&(a.VerTodos||a.CentroIds.Contains(x.CentroId))&&!db.ActividadesProgramadas.Any(p=>p.Activo&&p.Origen=="ALERTA"&&p.OrigenEntidadId==x.Id&&p.Estado!=EstadoActividad.Cancelada)).Include(x=>x.Inspeccion).ThenInclude(x=>x.Vehiculo).Include(x=>x.Inspeccion).ThenInclude(x=>x.Centro).Include(x=>x.InspeccionDetalle).ThenInclude(x=>x.PosicionVehiculo).Include(x=>x.InspeccionDetalle).ThenInclude(x=>x.Llanta).ToListAsync(ct);
        result.AddRange(alerts.Select(x=>new NecesidadProgramacionDto(x.Id,"ALERTA",x.Tipo,x.Descripcion,x.CentroId,x.Inspeccion.Centro.Nombre,x.VehiculoId,$"{x.Inspeccion.Vehiculo.NumeroInterno} · {x.Inspeccion.Vehiculo.Placa}",x.PosicionVehiculoId,x.InspeccionDetalle.PosicionVehiculo.Codigo,x.LlantaId,x.InspeccionDetalle.Llanta?.Codigo,"Alta",x.FechaCreacion)));
        var inconsistencies=await db.InconsistenciasInspeccion.AsNoTracking().Where(x=>x.Activo&&x.Estado==EstadoInconsistencia.PendienteAutorizacion&&(a.VerTodos||a.CentroIds.Contains(x.Inspeccion.CentroId))&&!db.ActividadesProgramadas.Any(p=>p.Activo&&p.Origen=="INCONSISTENCIA"&&p.OrigenEntidadId==x.Id&&p.Estado!=EstadoActividad.Cancelada)).Include(x=>x.Inspeccion).ThenInclude(x=>x.Vehiculo).Include(x=>x.Inspeccion).ThenInclude(x=>x.Centro).Include(x=>x.PosicionVehiculo).Include(x=>x.LlantaEsperada).ToListAsync(ct);
        result.AddRange(inconsistencies.Select(x=>new NecesidadProgramacionDto(x.Id,"INCONSISTENCIA","Validar inconsistencia",x.Observacion,x.Inspeccion.CentroId,x.Inspeccion.Centro.Nombre,x.Inspeccion.VehiculoId,$"{x.Inspeccion.Vehiculo.NumeroInterno} · {x.Inspeccion.Vehiculo.Placa}",x.PosicionVehiculoId,x.PosicionVehiculo.Codigo,x.LlantaEsperadaId,x.LlantaEsperada?.Codigo,"Alta",x.FechaCreacion)));
        return result.OrderByDescending(x=>x.FechaOrigen).ToList();
    }
    [HttpPost,Authorize(Policy="Programacion.Administrar")] public async Task<ActionResult<ProgramacionDto>> Crear(GuardarProgramacionDto dto,CancellationToken ct){var result=await service.CrearAsync(dto,User.Username(),User.AlcanceCentros(),ct);return CreatedAtAction(nameof(Listar),new{id=result.Id},result);}
    [HttpPost("masiva"),Authorize(Policy="Programacion.Administrar")] public async Task<ActionResult<IReadOnlyList<ProgramacionDto>>> Masiva(ProgramacionMasivaDto dto,CancellationToken ct){var result=await service.CrearMasivaAsync(dto,User.Username(),User.AlcanceCentros(),ct);return Created(string.Empty,result);}
    [HttpPut("{id:guid}"),Authorize(Policy="Programacion.Administrar")] public Task<ProgramacionDto> Actualizar(Guid id,GuardarProgramacionDto dto,CancellationToken ct)=>service.ActualizarAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);
    [HttpPost("{id:guid}/cancelar"),Authorize(Policy="Programacion.Administrar")] public Task<ProgramacionDto> Cancelar(Guid id,CancelarProgramacionDto dto,CancellationToken ct)=>service.CancelarAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);
    [HttpDelete("{id:guid}"),Authorize(Policy="Programacion.Administrar")] public async Task<IActionResult> Eliminar(Guid id,CancellationToken ct){await service.EliminarAsync(id,User.Username(),User.AlcanceCentros(),ct);return NoContent();}
}
public sealed record NecesidadProgramacionDto(Guid Id,string Origen,string Tipo,string Motivo,Guid CentroId,string Centro,Guid? VehiculoId,string Vehiculo,Guid? PosicionId,string? Posicion,Guid? LlantaId,string? Llanta,string Prioridad,DateTimeOffset FechaOrigen);
