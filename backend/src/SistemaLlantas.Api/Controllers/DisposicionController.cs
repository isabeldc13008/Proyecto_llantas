using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/disposicion"),Authorize(Policy="ServiciosLlanta.Consultar")]
public sealed class DisposicionController(IDisposicionService service,LlantasDbContext db):ControllerBase
{
    [HttpGet("llantas")]
    public async Task<IActionResult> Llantas([FromQuery]string? buscar,[FromQuery]int pageNumber=1,CancellationToken ct=default)
    {
        var a=User.AlcanceCentros();var q=db.Llantas.AsNoTracking().Where(t=>t.Activo&&!t.EstadoLlanta.EsDisposicionFinal&&(a.VerTodos||a.CentroIds.Contains(t.CentroId)));
        if(!string.IsNullOrWhiteSpace(buscar)){var term=buscar.Trim();q=q.Where(t=>t.Codigo.Contains(term)||t.Serial.Contains(term));}
        var page=Math.Max(1,pageNumber);var total=await q.CountAsync(ct);var items=await q.OrderBy(t=>t.Codigo).ThenBy(t=>t.Id).Skip((page-1)*20).Take(20).Select(t=>new ReferenciaDisposicion(t.Id,t.Codigo+" · "+t.Serial+" · "+t.Centro.Nombre)).ToListAsync(ct);
        return Ok(new Pagina<ReferenciaDisposicion>(items,page,20,total));
    }
    [HttpGet("resumen")] public Task<ResumenDisposicionDto> Resumen([FromQuery]ConsultaDisposicion filtro,CancellationToken ct)=>service.ResumenAsync(filtro,User.AlcanceCentros(),ct);
    [HttpGet("ordenes")] public Task<Pagina<OrdenDisposicionDto>> Ordenes([FromQuery]ConsultaDisposicion filtro,CancellationToken ct)=>service.OrdenesAsync(filtro,User.AlcanceCentros(),ct);
    [HttpGet("ordenes/{id:guid}")] public async Task<IActionResult> Detalle(Guid id,CancellationToken ct)
    {
        var result=await service.DetalleAsync(id,User.AlcanceCentros(),ct);if(result is null)return NotFound();
        var manage=User.HasClaim("permiso","servicios_llanta.gestionar");var evaluate=manage||User.HasClaim("permiso","servicios_llanta.opcionar");
        return Ok(result with{AccionesPermitidas=result.AccionesPermitidas.Where(x=>x.StartsWith("VER_")||x=="EVALUAR"&&evaluate||x!="EVALUAR"&&manage).ToArray()});
    }
    [HttpGet("lotes")] public Task<Pagina<LoteDisposicionDetalleDto>> Lotes([FromQuery]ConsultaDisposicion filtro,CancellationToken ct)=>service.LotesAsync(filtro,User.AlcanceCentros(),ct);
    [HttpGet("lotes/{id:guid}")] public async Task<IActionResult> Lote(Guid id,CancellationToken ct)=>await service.LoteAsync(id,User.AlcanceCentros(),ct) is {} result?Ok(result with{PuedeRecibir=result.PuedeRecibir&&User.HasClaim("permiso","servicios_llanta.gestionar")}):NotFound();
    [HttpGet("disponibles")] public Task<Pagina<OrdenDisposicionDto>> Disponibles([FromQuery]ConsultaDisposicion filtro,[FromQuery]Guid centroR1Id,CancellationToken ct)=>service.DisponiblesAsync(filtro,centroR1Id,User.AlcanceCentros(),ct);
    [HttpGet("despachos")] public Task<Pagina<DespachoDisposicionDto>> Despachos([FromQuery]ConsultaDisposicion filtro,CancellationToken ct)=>service.DespachosAsync(filtro,User.AlcanceCentros(),ct);
    [HttpGet("despachos/{id:guid}")] public async Task<IActionResult> Despacho(Guid id,CancellationToken ct)=>await service.DespachoAsync(id,User.AlcanceCentros(),ct) is {} result?Ok(result with{PuedeCerrar=result.PuedeCerrar&&User.HasClaim("permiso","servicios_llanta.gestionar")}):NotFound();
    [HttpPost("despachos"),Authorize(Policy="ServiciosLlanta.Gestionar")]
    public async Task<IActionResult> Crear(CrearDespachoDisposicionDto dto,CancellationToken ct){var result=await service.CrearDespachoAsync(dto,User.Username(),User.AlcanceCentros(),ct);return Created($"/api/disposicion/despachos/{result.Id}",result);}
    [HttpPost("despachos/{id:guid}/cerrar"),Authorize(Policy="ServiciosLlanta.Gestionar")]
    public Task<DespachoDisposicionDto> Cerrar(Guid id,CerrarDespachoDisposicionDto dto,CancellationToken ct)=>service.CerrarDespachoAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);
    [HttpPost("actas/{id:guid}/soportes"),Authorize(Policy="ServiciosLlanta.Gestionar"),RequestSizeLimit(11_000_000)]
    public async Task<IActionResult> Adjuntar(Guid id,IFormFile archivo,CancellationToken ct){await using var stream=archivo.OpenReadStream();return Ok(await service.AdjuntarSoporteAsync(id,stream,archivo.FileName,archivo.ContentType,archivo.Length,User.Username(),User.AlcanceCentros(),ct));}
    [HttpDelete("soportes/{id:guid}"),Authorize(Policy="ServiciosLlanta.Gestionar")]
    public async Task<IActionResult> Eliminar(Guid id,CancellationToken ct){await service.EliminarSoporteAsync(id,User.Username(),User.AlcanceCentros(),ct);return NoContent();}
    [HttpGet("soportes/{id:guid}/archivo")]
    public async Task<IActionResult> Archivo(Guid id,CancellationToken ct)=>await service.ArchivoAsync(id,User.AlcanceCentros(),ct) is {} file?PhysicalFile(file.Ruta,file.MimeType,file.NombreArchivo):NotFound();
    [HttpGet("actas/{id:guid}")]
    public async Task<IActionResult> Acta(Guid id,CancellationToken ct)=>await service.ActaAsync(id,User.AlcanceCentros(),ct) is {} result?Ok(result):NotFound();
    [HttpGet("actas/{id:guid}/soportes")]
    public async Task<IActionResult> Soportes(Guid id,CancellationToken ct)=>await service.ActaAsync(id,User.AlcanceCentros(),ct) is {} result?Ok(result.Soportes):NotFound();
    [HttpPost("lotes/{id:guid}/novedades"),Authorize(Policy="ServiciosLlanta.Gestionar")]
    public async Task<IActionResult> Novedad(Guid id,CrearNovedadDto dto,CancellationToken ct){await service.RegistrarNovedadAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);return NoContent();}
    [HttpPost("novedades/{id:guid}/resolver"),Authorize(Policy="ServiciosLlanta.Gestionar")]
    public async Task<IActionResult> Resolver(Guid id,ResolverNovedadDto dto,CancellationToken ct){await service.ResolverNovedadAsync(id,dto,User.Username(),User.AlcanceCentros(),ct);return NoContent();}
}
