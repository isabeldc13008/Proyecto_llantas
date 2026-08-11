using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/llantas"), Authorize(Policy = "Llantas.Consultar")]
public sealed class LlantasController(ILlantaService service) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<LlantaResumenDto>> Consultar([FromQuery] ConsultaPaginada consulta, CancellationToken ct) => service.ConsultarAsync(consulta, CentroId(), ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LlantaResumenDto>> Obtener(Guid id, CancellationToken ct) => await service.ObtenerAsync(id, CentroId(), ct) is { } item ? Ok(item) : NotFound();

    [HttpPost, Authorize(Policy = "Llantas.Administrar")]
    public async Task<ActionResult<LlantaResumenDto>> Crear(GuardarLlantaDto dto, CancellationToken ct)
    {
        var item = await service.CrearAsync(dto, Usuario(), ct); return CreatedAtAction(nameof(Obtener), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = "Llantas.Administrar")]
    public async Task<ActionResult<LlantaResumenDto>> Actualizar(Guid id, GuardarLlantaDto dto, CancellationToken ct) => await service.ActualizarAsync(id, dto, Usuario(), ct) is { } item ? Ok(item) : NotFound();

    [HttpPatch("{id:guid}/estado"), Authorize(Policy = "Llantas.Administrar")]
    public async Task<IActionResult> Estado(Guid id, [FromBody] CambiarEstadoRequest request, CancellationToken ct) => await service.CambiarEstadoAsync(id, request.Activo, Usuario(), ct) ? NoContent() : NotFound();

    private string Usuario() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "sistema";
    private Guid? CentroId() => Guid.TryParse(User.FindFirstValue("centro_id"), out var id) ? id : null;
    public sealed record CambiarEstadoRequest(bool Activo);
}
