using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Inspecciones;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/inspecciones"), Authorize(Policy = "Inspecciones.Consultar")]
public sealed class InspeccionesController(IInspeccionService service) : ControllerBase
{
    [HttpGet("contexto/{vehiculoId:guid}")]
    public async Task<ActionResult<ContextoInspeccionDto>> Contexto(Guid vehiculoId, CancellationToken ct) =>
        await service.ObtenerContextoAsync(vehiculoId, CentroId(), ct) is { } x ? Ok(x) : NotFound();
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InspeccionDto>> Obtener(Guid id, CancellationToken ct) => await service.ObtenerAsync(id, CentroId(), ct) is { } x ? Ok(x) : NotFound();
    [HttpPost, Authorize(Policy = "Inspecciones.Crear")]
    public async Task<ActionResult<InspeccionDto>> Crear(CrearInspeccionDto dto, CancellationToken ct) { var x=await service.CrearAsync(dto,Usuario(),CentroId(),ct); return CreatedAtAction(nameof(Obtener),new{id=x.Id},x); }
    [HttpPut("{id:guid}/posiciones/{posicionId:guid}"), Authorize(Policy = "Inspecciones.Crear")]
    public async Task<ActionResult<InspeccionDto>> Detalle(Guid id,Guid posicionId,GuardarDetalleInspeccionDto dto,CancellationToken ct) => await service.GuardarDetalleAsync(id,posicionId,dto,Usuario(),ct) is { } x ? Ok(x) : NotFound();
    [HttpPost("{id:guid}/inconsistencias"), Authorize(Policy = "Inspecciones.ReportarInconsistencia")]
    public async Task<ActionResult<InconsistenciaDto>> Reportar(Guid id,ReportarInconsistenciaDto dto,CancellationToken ct) { var x=await service.ReportarAsync(id,dto,Usuario(),ct); return Created(string.Empty,x); }
    [HttpGet("inconsistencias/pendientes"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<IReadOnlyList<InconsistenciaDto>> Pendientes(CancellationToken ct) => service.PendientesAsync(CentroId(),ct);
    [HttpPost("inconsistencias/{id:guid}/autorizar"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<InconsistenciaDto> Autorizar(Guid id,ResolverInconsistenciaDto dto,CancellationToken ct) => service.ResolverAsync(id,dto,true,Usuario(),User.HasClaim("permiso","inspecciones.autorizar_propia_inconsistencia"),ct);
    [HttpPost("inconsistencias/{id:guid}/rechazar"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<InconsistenciaDto> Rechazar(Guid id,ResolverInconsistenciaDto dto,CancellationToken ct) => service.ResolverAsync(id,dto,false,Usuario(),User.HasClaim("permiso","inspecciones.autorizar_propia_inconsistencia"),ct);
    private string Usuario()=>User.FindFirstValue(ClaimTypes.NameIdentifier)??"sistema";
    private Guid? CentroId()=>Guid.TryParse(User.FindFirstValue("centro_id"),out var id)?id:null;
}
