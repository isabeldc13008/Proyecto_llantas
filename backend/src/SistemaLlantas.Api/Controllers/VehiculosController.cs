using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Vehiculos;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/vehiculos"),Authorize(Policy="Vehiculos.Consultar")]
public sealed class VehiculosController(IVehiculoService service):ControllerBase
{
    [HttpGet] public Task<Pagina<VehiculoResumenDto>> Consultar([FromQuery] ConsultaPaginada consulta,CancellationToken ct)=>service.ConsultarAsync(consulta,User.AlcanceCentros(),ct);
    [HttpGet("{id:guid}")] public async Task<ActionResult<VehiculoDetalleDto>> Obtener(Guid id,CancellationToken ct)=>await service.ObtenerAsync(id,User.AlcanceCentros(),ct) is { } x?Ok(x):NotFound();
    [HttpPost,Authorize(Policy="Vehiculos.Administrar")] public async Task<ActionResult<VehiculoDetalleDto>> Crear(GuardarVehiculoDto dto,CancellationToken ct){var x=await service.CrearAsync(dto,User.Username(),User.AlcanceCentros(),ct);return CreatedAtAction(nameof(Obtener),new{id=x.Id},x);}
    [HttpPut("{id:guid}"),Authorize(Policy="Vehiculos.Administrar")] public async Task<ActionResult<VehiculoDetalleDto>> Actualizar(Guid id,GuardarVehiculoDto dto,CancellationToken ct)=>await service.ActualizarAsync(id,dto,User.Username(),User.AlcanceCentros(),ct) is { } x?Ok(x):NotFound();
    [HttpGet("configuraciones")] public Task<IReadOnlyList<ConfiguracionVehiculoDto>> Configuraciones(CancellationToken ct)=>service.ConfiguracionesAsync(ct);
    [HttpPost("configuraciones"),Authorize(Policy="Vehiculos.Administrar")] public async Task<ActionResult<ConfiguracionVehiculoDto>> CrearConfiguracion(GuardarConfiguracionVehiculoDto dto,CancellationToken ct)=>Ok(await service.CrearConfiguracionAsync(dto,User.Username(),ct));
}
