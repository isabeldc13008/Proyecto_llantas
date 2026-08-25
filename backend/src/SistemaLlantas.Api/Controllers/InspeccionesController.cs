using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Api.Security;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/inspecciones"), Authorize(Policy = "Inspecciones.Consultar")]
public sealed class InspeccionesController(IInspeccionService service, LlantasDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("vehiculos")]
    public Task<IReadOnlyList<VehiculoInspeccionDto>> Vehiculos([FromQuery]string? buscar,CancellationToken ct) => service.ObtenerVehiculosAsync(Usuario(), User.IsInRole("TECNICO"),buscar, User.AlcanceCentros(), ct, User.IsInRole("TECNICO"));
    [HttpGet("opciones")]
    public Task<OpcionesInspeccionDto> Opciones(CancellationToken ct) => service.ObtenerOpcionesAsync(ct);
    [HttpGet("contexto/{vehiculoId:guid}")]
    public async Task<ActionResult<ContextoInspeccionDto>> Contexto(Guid vehiculoId, CancellationToken ct) =>
        await service.ObtenerContextoAsync(vehiculoId, User.AlcanceCentros(), ct, User.IsInRole("TECNICO")) is { } x ? Ok(x) : NotFound();
    [HttpGet("resumen")]
    public Task<ResumenInspeccionesDto> Resumen(CancellationToken ct)=>service.ResumenAsync(Usuario(),User.IsInRole("TECNICO"),User.AlcanceCentros(),ct);
    [HttpGet("historial")]
    public Task<IReadOnlyList<HistorialInspeccionDto>> Historial(CancellationToken ct)=>service.HistorialAsync(Usuario(),User.IsInRole("TECNICO"),User.AlcanceCentros(),ct);
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InspeccionDto>> Obtener(Guid id, CancellationToken ct) => await service.ObtenerAsync(id, User.AlcanceCentros(), ct) is { } x ? Ok(x) : NotFound();
    [HttpPost, Authorize(Policy = "Inspecciones.Crear")]
    public async Task<ActionResult<InspeccionDto>> Crear(CrearInspeccionDto dto, CancellationToken ct) { var x=await service.CrearAsync(dto,Usuario(),User.AlcanceCentros(),ct,User.IsInRole("TECNICO")); return CreatedAtAction(nameof(Obtener),new{id=x.Id},x); }
    [HttpPost("{id:guid}/finalizar"),Authorize(Policy="Inspecciones.Crear")]
    public Task<InspeccionDto> Finalizar(Guid id,CancellationToken ct)=>service.FinalizarAsync(id,Usuario(),ct);
    [HttpPut("{id:guid}/posiciones/{posicionId:guid}"), Authorize(Policy = "Inspecciones.Crear")]
    public async Task<ActionResult<InspeccionDto>> Detalle(Guid id,Guid posicionId,GuardarDetalleInspeccionDto dto,CancellationToken ct) => await service.GuardarDetalleAsync(id,posicionId,dto,Usuario(),ct) is { } x ? Ok(x) : NotFound();
    [HttpPost("{id:guid}/inconsistencias"), Authorize(Policy = "Inspecciones.ReportarInconsistencia")]
    public async Task<ActionResult<InconsistenciaDto>> Reportar(Guid id,ReportarInconsistenciaDto dto,CancellationToken ct) { var x=await service.ReportarAsync(id,dto,Usuario(),ct); return Created(string.Empty,x); }
    [HttpGet("inconsistencias/pendientes"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<IReadOnlyList<InconsistenciaDto>> Pendientes(CancellationToken ct) => service.PendientesAsync(User.AlcanceCentros(),ct);
    [HttpGet("alertas"),Authorize(Policy="Alertas.Consultar")]
    public Task<IReadOnlyList<AlertaDto>> Alertas(CancellationToken ct)=>service.AlertasAsync(User.AlcanceCentros(),ct);
    [HttpPut("alertas/{id:guid}/estado"),Authorize(Policy="Alertas.Gestionar")]
    public Task<AlertaDto> EstadoAlerta(Guid id,CambiarAlertaDto dto,CancellationToken ct){if(string.Equals(dto.Estado,"DESCARTADA",StringComparison.OrdinalIgnoreCase)&&!User.HasClaim("permiso","alertas.descartar"))throw new UnauthorizedAccessException("No tiene permiso para descartar alertas.");return service.CambiarAlertaAsync(id,dto,Usuario(),User.AlcanceCentros(),ct);}
    [HttpPost("{id:guid}/evidencias"), Authorize(Policy = "Inspecciones.Crear"), RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<object>> AdjuntarEvidencia(Guid id, [FromForm] IFormFile archivo, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest(new { message = "Selecciona un archivo JPG, PNG o PDF." });
        if (archivo.Length > 10_000_000) return BadRequest(new { message = "La evidencia supera el límite de 10 MB." });
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".pdf" || archivo.ContentType is not "image/jpeg" and not "image/png" and not "application/pdf")
            return BadRequest(new { message = "Formato no permitido. Adjunta JPG, PNG o PDF." });
        var alcance = User.AlcanceCentros();
        if (!await db.Inspecciones.AsNoTracking().AnyAsync(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId) || (User.IsInRole("TECNICO") && x.TecnicoId == Usuario())), ct)) return NotFound(new { message = "La inspección no existe o no está autorizada." });
        await using var input = archivo.OpenReadStream();
        var signature = new byte[8]; var read = await input.ReadAsync(signature, ct); input.Position = 0;
        var jpeg = read >= 3 && signature[0] == 0xff && signature[1] == 0xd8 && signature[2] == 0xff;
        var png = read >= 8 && signature.SequenceEqual(new byte[] { 137,80,78,71,13,10,26,10 });
        var pdf=read>=5&&signature.Take(5).SequenceEqual("%PDF-"u8.ToArray());if (!jpeg && !png&&!pdf) return BadRequest(new { message = "El contenido no corresponde a JPG, PNG o PDF válido." });
        var evidence = new EvidenciaInspeccion { InspeccionId = id, NombreArchivo = Path.GetFileName(archivo.FileName), UsuarioCreacion = Usuario() };
        var storedName = evidence.Id + (png ? ".png" : pdf?".pdf":".jpg");
        var root = Path.Combine(environment.ContentRootPath, "App_Data", "evidencias"); Directory.CreateDirectory(root);
        var path = Path.Combine(root, storedName);
        await using (var output = System.IO.File.Create(path)) await input.CopyToAsync(output, ct);
        await using var hashStream = System.IO.File.OpenRead(path); var hash = Convert.ToHexString(await SHA256.HashDataAsync(hashStream, ct));
        evidence.Ubicacion = Path.Combine("App_Data", "evidencias", storedName); evidence.Hash = hash;evidence.MimeType=pdf?"application/pdf":png?"image/png":"image/jpeg";evidence.TamanoBytes=archivo.Length;
        db.EvidenciasInspeccion.Add(evidence); await db.SaveChangesAsync(ct);
        return Created(string.Empty, new { evidence.Id, evidence.NombreArchivo, archivo.Length, evidence.Hash });
    }
    [HttpGet("{id:guid}/evidencias")]
    public async Task<ActionResult<IReadOnlyList<EvidenciaDto>>> Evidencias(Guid id,CancellationToken ct){var alcance=User.AlcanceCentros();if(!await db.Inspecciones.AnyAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)||(User.IsInRole("TECNICO")&&x.TecnicoId==Usuario())),ct))return NotFound();return await db.EvidenciasInspeccion.AsNoTracking().IgnoreQueryFilters().Where(x=>x.InspeccionId==id).OrderByDescending(x=>x.FechaCreacion).Select(x=>new EvidenciaDto(x.Id,x.NombreArchivo,x.MimeType,x.TamanoBytes,x.Hash,x.FechaCreacion,x.Activo)).ToListAsync(ct);}
    [HttpGet("evidencias/{evidenciaId:guid}/archivo")]
    public async Task<IActionResult> Descargar(Guid evidenciaId,CancellationToken ct){var alcance=User.AlcanceCentros();var usuario=Usuario();var tecnico=User.IsInRole("TECNICO");var e=await db.EvidenciasInspeccion.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==evidenciaId&&x.Activo&&x.InspeccionId!=null&&db.Inspecciones.Any(i=>i.Id==x.InspeccionId&&(alcance.VerTodos||alcance.CentroIds.Contains(i.CentroId)||(tecnico&&i.TecnicoId==usuario))),ct);if(e is null)return NotFound();var root=Path.GetFullPath(Path.Combine(environment.ContentRootPath,"App_Data","evidencias"));var path=Path.GetFullPath(Path.Combine(environment.ContentRootPath,e.Ubicacion));if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||!System.IO.File.Exists(path))return NotFound();return PhysicalFile(path,e.MimeType,e.NombreArchivo);}
    [HttpDelete("evidencias/{evidenciaId:guid}"),Authorize(Policy="Evidencias.Eliminar")]
    public async Task<IActionResult> Eliminar(Guid evidenciaId,CancellationToken ct){var alcance=User.AlcanceCentros();var e=await db.EvidenciasInspeccion.SingleOrDefaultAsync(x=>x.Id==evidenciaId&&x.InspeccionId!=null&&db.Inspecciones.Any(i=>i.Id==x.InspeccionId&&(alcance.VerTodos||alcance.CentroIds.Contains(i.CentroId))),ct);if(e is null)return NotFound();e.Activo=false;e.UsuarioModificacion=Usuario();e.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return NoContent();}
    [HttpPost("inconsistencias/{id:guid}/autorizar"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<InconsistenciaDto> Autorizar(Guid id,ResolverInconsistenciaDto dto,CancellationToken ct) => service.ResolverAsync(id,dto,true,Usuario(),User.HasClaim("permiso","inspecciones.autorizar_propia_inconsistencia"),ct);
    [HttpPost("inconsistencias/{id:guid}/rechazar"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<InconsistenciaDto> Rechazar(Guid id,ResolverInconsistenciaDto dto,CancellationToken ct) => service.ResolverAsync(id,dto,false,Usuario(),User.HasClaim("permiso","inspecciones.autorizar_propia_inconsistencia"),ct);
    private string Usuario()=>User.Username();
}
