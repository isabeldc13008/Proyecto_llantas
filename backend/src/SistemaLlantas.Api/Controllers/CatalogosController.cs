using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Catalogos;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/catalogos"), Authorize]
public sealed class CatalogosController(ICatalogoService service) : ControllerBase
{
    [HttpGet("{tipo}")]
    public Task<Pagina<CatalogoDto>> Consultar(string tipo, [FromQuery] ConsultaPaginada consulta, CancellationToken ct) => service.ConsultarAsync(tipo, consulta, ct);

    [HttpPost("{tipo}"), Authorize(Policy = "Catalogos.Administrar")]
    public async Task<ActionResult<CatalogoDto>> Crear(string tipo, GuardarCatalogoDto dto, CancellationToken ct)
    {
        var item = await service.CrearAsync(tipo, dto, User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "sistema", ct); return Created(string.Empty, item);
    }

    [HttpPatch("{tipo}/{id:guid}/estado"), Authorize(Policy = "Catalogos.Administrar")]
    public async Task<IActionResult> Estado(string tipo, Guid id, [FromBody] EstadoRequest request, CancellationToken ct) =>
        await service.CambiarEstadoAsync(tipo, id, request.Activo, User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "sistema", ct) ? NoContent() : NotFound();
    public sealed record EstadoRequest(bool Activo);
}
