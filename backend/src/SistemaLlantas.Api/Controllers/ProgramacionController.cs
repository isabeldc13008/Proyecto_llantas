using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Programacion;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/programacion"),Authorize(Policy="Programacion.Consultar")]
public sealed class ProgramacionController(IProgramacionService service):ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<ProgramacionDto>> Listar([FromQuery]Guid? centroId,[FromQuery]Guid? vehiculoId,[FromQuery]Guid? tecnicoUsuarioId,[FromQuery]string? tipo,[FromQuery]string? estado,[FromQuery]DateTimeOffset? desde,[FromQuery]DateTimeOffset? hasta,[FromQuery]string? prioridad,CancellationToken ct)=>service.ListarAsync(new(centroId,vehiculoId,tecnicoUsuarioId,tipo,estado,desde,hasta,prioridad),User.AlcanceCentros(),ct);
    [HttpGet("tecnicos")] public Task<IReadOnlyList<TecnicoProgramacionDto>> Tecnicos(CancellationToken ct)=>service.TecnicosAsync(User.AlcanceCentros(),ct);
    [HttpPost,Authorize(Policy="Programacion.Administrar")] public async Task<ActionResult<ProgramacionDto>> Crear(GuardarProgramacionDto dto,CancellationToken ct){var result=await service.CrearAsync(dto,User.Username(),User.AlcanceCentros(),ct);return CreatedAtAction(nameof(Listar),new{id=result.Id},result);}
    [HttpPost("masiva"),Authorize(Policy="Programacion.Administrar")] public async Task<ActionResult<IReadOnlyList<ProgramacionDto>>> Masiva(ProgramacionMasivaDto dto,CancellationToken ct){var result=await service.CrearMasivaAsync(dto,User.Username(),User.AlcanceCentros(),ct);return Created(string.Empty,result);}
    [HttpPut("{id:guid}"),Authorize(Policy="Programacion.Administrar")] public Task<ProgramacionDto> Actualizar(Guid id,GuardarProgramacionDto dto,CancellationToken ct)=>service.ActualizarAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);
    [HttpPost("{id:guid}/cancelar"),Authorize(Policy="Programacion.Administrar")] public Task<ProgramacionDto> Cancelar(Guid id,CancelarProgramacionDto dto,CancellationToken ct)=>service.CancelarAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);
    [HttpDelete("{id:guid}"),Authorize(Policy="Programacion.Administrar")] public async Task<IActionResult> Eliminar(Guid id,CancellationToken ct){await service.EliminarAsync(id,User.Username(),User.AlcanceCentros(),ct);return NoContent();}
}
