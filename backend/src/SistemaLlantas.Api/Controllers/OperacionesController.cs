using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Operaciones;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Authorize]
public sealed class OperacionesController(IOperacionService service):ControllerBase
{
    [HttpGet("api/mis-actividades"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<IReadOnlyList<ActividadDto>> Actividades(CancellationToken ct)=>service.MisActividadesAsync(Usuario(),CentroId(),ct);
    [HttpPost("api/actividades/{id:guid}/iniciar"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<ActividadDto> Iniciar(Guid id,CancellationToken ct)=>service.IniciarActividadAsync(id,Usuario(),ct);
    [HttpPost("api/movimientos"),Authorize(Policy="Operaciones.Ejecutar")]
    public Task<MovimientoDto> Mover(EjecutarMovimientoDto dto,CancellationToken ct)=>service.MoverAsync(dto,Usuario(),CentroId(),ct);
    [HttpPost("api/desmontajes"),Authorize(Policy="Operaciones.Ejecutar")]
    public Task<MovimientoDto> Desmontar(DesmontarLlantaDto dto,CancellationToken ct)=>service.DesmontarAsync(dto,Usuario(),CentroId(),ct);
    private string Usuario()=>User.FindFirstValue(ClaimTypes.NameIdentifier)??"sistema";
    private Guid? CentroId()=>Guid.TryParse(User.FindFirstValue("centro_id"),out var id)?id:null;
}
