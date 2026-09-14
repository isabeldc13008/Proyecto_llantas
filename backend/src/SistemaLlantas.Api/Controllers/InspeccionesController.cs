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
    public Task<IReadOnlyList<VehiculoInspeccionDto>> Vehiculos([FromQuery]string? buscar,CancellationToken ct) => service.ObtenerVehiculosAsync(Usuario(), false,buscar, User.AlcanceCentros(), ct);
    [HttpGet("opciones")]
    public Task<OpcionesInspeccionDto> Opciones(CancellationToken ct) => service.ObtenerOpcionesAsync(ct);
    [HttpGet("contexto/{vehiculoId:guid}")]
    public async Task<ActionResult<ContextoInspeccionDto>> Contexto(Guid vehiculoId, CancellationToken ct) =>
        await service.ObtenerContextoAsync(vehiculoId, User.AlcanceCentros(), ct) is { } x ? Ok(x) : NotFound();
    [HttpGet("resumen")]
    public Task<ResumenInspeccionesDto> Resumen(CancellationToken ct)=>service.ResumenAsync(Usuario(),User.IsInRole("TECNICO"),User.AlcanceCentros(),ct);
    [HttpGet("historial")]
    public Task<IReadOnlyList<HistorialInspeccionDto>> Historial(CancellationToken ct)=>service.HistorialAsync(Usuario(),User.IsInRole("TECNICO"),User.AlcanceCentros(),ct);
    [HttpGet("llantas/buscar")]
    public async Task<IActionResult> BuscarLlanta([FromQuery]string termino,CancellationToken ct,[FromQuery]Guid? inspeccionId=null,[FromQuery]string? modo=null)
    {
        if(!inspeccionId.HasValue||!await InspeccionEnAlcance(inspeccionId.Value).AnyAsync(ct))throw new UnauthorizedAccessException("Inspección fuera de los centros autorizados.");
        // Identification of inconsistencies must still find an already mounted tire.
        if(modo=="identificacion")return Ok(await service.BuscarLlantaExactaAsync(termino,ct));
        if(!User.HasClaim("permiso","inspecciones.crear"))return Forbid();
        if(!inspeccionId.HasValue || !await InspeccionAsignable(inspeccionId.Value).AnyAsync(ct))return NotFound();
        if(string.IsNullOrWhiteSpace(termino)||termino.Trim().Length<2)return Ok(Array.Empty<object>());
        var term=termino.Trim();
        return Ok(await SistemaLlantas.Infrastructure.Services.LlantasDisponibles.Consulta(db)
            .Where(x=>x.Codigo.Contains(term)||x.Serial.Contains(term)).OrderBy(x=>x.Codigo).Take(30)
            .Select(x=>new{x.Id,x.Codigo,x.Serial,Marca=x.Marca.Nombre,Referencia=x.Referencia.Nombre,Dimension=x.Dimension.Nombre,Estado=x.EstadoLlanta.Nombre,x.CentroId,Centro=x.Centro.Nombre}).ToListAsync(ct));
    }
    private IQueryable<Inspeccion> InspeccionEnAlcance(Guid id)
    {
        var a=User.AlcanceCentros();
        return db.Inspecciones.Where(x=>x.Id==id&&x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.Vehiculo.CentroId)));
    }
    private async Task ExigirAlcance(Guid id,CancellationToken ct)
    {
        if(!await InspeccionEnAlcance(id).AnyAsync(ct))throw new UnauthorizedAccessException("La inspección está fuera de los centros autorizados.");
    }
    private IQueryable<Inspeccion> InspeccionAsignable(Guid id)
    {
        var a=User.AlcanceCentros();var usuario=Usuario();
        return db.Inspecciones.Where(x=>x.Id==id&&x.Activo&&x.TecnicoId==usuario&&x.Estado==EstadoInspeccion.Borrador&&x.Vehiculo.Activo
            &&(a.VerTodos||a.CentroIds.Contains(x.Vehiculo.CentroId)));
    }
    public sealed record AsignarLlantaDto(Guid LlantaId,string Motivo);
    [HttpPost("{id:guid}/posiciones/{posicionId:guid}/asignar"),Authorize(Policy="Inspecciones.Crear")]
    public async Task<ActionResult<ContextoInspeccionDto>> Asignar(Guid id,Guid posicionId,AsignarLlantaDto dto,
        [FromServices]SistemaLlantas.Application.Operaciones.IOperacionService operaciones,CancellationToken ct,
        [FromServices]SistemaLlantas.Application.Llantas.ICicloVidaLlantaService ciclo)
    {
        if(string.IsNullOrWhiteSpace(dto.Motivo)||dto.Motivo.Length>400)throw new SistemaLlantas.Application.Common.ValidacionException("Indica el motivo de asignación (máximo 400 caracteres).");
        Guid vehicleId=Guid.Empty;
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            db.ChangeTracker.Clear();
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var inspection=await InspeccionAsignable(id).Include(x=>x.Vehiculo).SingleOrDefaultAsync(ct)??throw new UnauthorizedAccessException("La inspección no está autorizada, no te pertenece o ya está finalizada.");
            vehicleId=inspection.VehiculoId;
            var detail=await db.InspeccionesDetalle.Include(x=>x.PosicionVehiculo).ThenInclude(x=>x.EjeVehiculo).SingleOrDefaultAsync(x=>x.InspeccionId==id&&x.PosicionVehiculoId==posicionId,ct)
                ??throw new KeyNotFoundException("Posición no perteneciente a la inspección.");
            if(detail.LlantaId.HasValue||detail.PosicionVehiculo.EjeVehiculo.VehiculoId!=vehicleId)throw new SistemaLlantas.Application.Common.ConflictoException("La posición de inspección ya tiene llanta.");
            var tire=await SistemaLlantas.Infrastructure.Services.LlantasDisponibles.Consulta(db).SingleOrDefaultAsync(x=>x.Id==dto.LlantaId,ct)
                ??throw new SistemaLlantas.Application.Common.ConflictoException("La llanta ya no está disponible. Actualiza la búsqueda.");
            // This narrow scope is used only after validating ownership of the inspection.
            var scope=User.AlcanceCentros();
            var reason="Inspección: "+dto.Motivo.Trim();
            if(tire.CentroId!=inspection.Vehiculo.CentroId)await ciclo.TrasladarParaInspeccionAsync(tire.Id,id,reason,Usuario(),scope,ct);
            await operaciones.MontarEnInspeccionAsync(new(){LlantaId=tire.Id,PosicionDestinoId=posicionId,TipoDestino="Posicion",Motivo=reason,KilometrajeVehiculo=inspection.Kilometraje,Observaciones=$"Inspección {id}"},id,Usuario(),scope,ct);
            detail.LlantaId=tire.Id;detail.UsuarioModificacion=Usuario();detail.FechaModificacion=DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        });
        return Ok(await service.ObtenerContextoAsync(vehicleId,User.AlcanceCentros(),ct));
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InspeccionDto>> Obtener(Guid id, CancellationToken ct) => await service.ObtenerAsync(id, User.AlcanceCentros(), ct,Usuario(),User.IsInRole("TECNICO")) is { } x ? Ok(x) : NotFound();
    [HttpPost, Authorize(Policy = "Inspecciones.Crear")]
    public async Task<ActionResult<InspeccionDto>> Crear(CrearInspeccionDto dto, CancellationToken ct) { var x=await service.CrearAsync(dto,Usuario(),User.AlcanceCentros(),ct); return CreatedAtAction(nameof(Obtener),new{id=x.Id},x); }
    [HttpPost("{id:guid}/finalizar"),Authorize(Policy="Inspecciones.Crear")]
    public async Task<InspeccionDto> Finalizar(Guid id,CancellationToken ct){await ExigirAlcance(id,ct);return await service.FinalizarAsync(id,Usuario(),ct);}
    [HttpPut("{id:guid}/posiciones/{posicionId:guid}"), Authorize(Policy = "Inspecciones.Crear")]
    public async Task<ActionResult<InspeccionDto>> Detalle(Guid id,Guid posicionId,GuardarDetalleInspeccionDto dto,CancellationToken ct) {await ExigirAlcance(id,ct);return await service.GuardarDetalleAsync(id,posicionId,dto,Usuario(),ct) is { } x ? Ok(x) : NotFound();}
    [HttpPost("{id:guid}/inconsistencias"), Authorize(Policy = "Inspecciones.ReportarInconsistencia")]
    public async Task<ActionResult<InconsistenciaDto>> Reportar(Guid id,ReportarInconsistenciaDto dto,CancellationToken ct) { await ExigirAlcance(id,ct);var x=await service.ReportarAsync(id,dto,Usuario(),ct); return Created(string.Empty,x); }
    [HttpGet("inconsistencias/pendientes"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<IReadOnlyList<InconsistenciaDto>> Pendientes(CancellationToken ct) => service.PendientesAsync(User.AlcanceCentros(),ct);
    [HttpGet("alertas"),Authorize(Policy="Alertas.Consultar")]
    public Task<IReadOnlyList<AlertaDto>> Alertas(CancellationToken ct)=>service.AlertasAsync(User.AlcanceCentros(),ct);
    [HttpPut("alertas/{id:guid}/estado"),Authorize(Policy="Alertas.Gestionar")]
    public Task<AlertaDto> EstadoAlerta(Guid id,CambiarAlertaDto dto,CancellationToken ct){if(string.Equals(dto.Estado,"DESCARTADA",StringComparison.OrdinalIgnoreCase)&&!User.HasClaim("permiso","alertas.descartar"))throw new UnauthorizedAccessException("No tiene permiso para descartar alertas.");return service.CambiarAlertaAsync(id,dto,Usuario(),User.AlcanceCentros(),ct);}
    [HttpPost("{id:guid}/evidencias"), Authorize(Policy = "Inspecciones.Crear"), RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<object>> AdjuntarEvidencia(Guid id, [FromForm] IFormFile archivo, CancellationToken ct, [FromQuery]Guid? inconsistenciaId=null)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest(new { message = "Selecciona un archivo JPG, PNG o PDF." });
        if (archivo.Length > 10_000_000) return BadRequest(new { message = "La evidencia supera el límite de 10 MB." });
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".pdf" || archivo.ContentType is not "image/jpeg" and not "image/png" and not "application/pdf")
            return BadRequest(new { message = "Formato no permitido. Adjunta JPG, PNG o PDF." });
        var alcance = User.AlcanceCentros();
        if (!await db.Inspecciones.AsNoTracking().AnyAsync(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.Vehiculo.CentroId)), ct)) return NotFound(new { message = "La inspección no existe o no está autorizada." });
        await using var input = archivo.OpenReadStream();
        var signature = new byte[8]; var read = await input.ReadAsync(signature, ct); input.Position = 0;
        var jpeg = read >= 3 && signature[0] == 0xff && signature[1] == 0xd8 && signature[2] == 0xff;
        var png = read >= 8 && signature.SequenceEqual(new byte[] { 137,80,78,71,13,10,26,10 });
        var pdf=read>=5&&signature.Take(5).SequenceEqual("%PDF-"u8.ToArray());if (!jpeg && !png&&!pdf) return BadRequest(new { message = "El contenido no corresponde a JPG, PNG o PDF válido." });
        if(inconsistenciaId.HasValue&&!await db.InconsistenciasInspeccion.AnyAsync(x=>x.Id==inconsistenciaId&&x.InspeccionId==id,ct))return BadRequest(new{message="La inconsistencia no pertenece a la inspección."});
        var evidence = new EvidenciaInspeccion { InspeccionId = id, InconsistenciaInspeccionId=inconsistenciaId, NombreArchivo = Path.GetFileName(archivo.FileName), UsuarioCreacion = Usuario() };
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
    public async Task<ActionResult<IReadOnlyList<EvidenciaDto>>> Evidencias(Guid id,CancellationToken ct){var alcance=User.AlcanceCentros();if(!await db.Inspecciones.AnyAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.Vehiculo.CentroId)),ct))return NotFound();return await db.EvidenciasInspeccion.AsNoTracking().IgnoreQueryFilters().Where(x=>x.InspeccionId==id).OrderByDescending(x=>x.FechaCreacion).Select(x=>new EvidenciaDto(x.Id,x.NombreArchivo,x.MimeType,x.TamanoBytes,x.Hash,x.FechaCreacion,x.Activo)).ToListAsync(ct);}
    [HttpGet("evidencias/{evidenciaId:guid}/archivo")]
    public async Task<IActionResult> Descargar(Guid evidenciaId,CancellationToken ct){var alcance=User.AlcanceCentros();var usuario=Usuario();var tecnico=User.IsInRole("TECNICO");var e=await db.EvidenciasInspeccion.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==evidenciaId&&x.Activo&&x.InspeccionId!=null&&db.Inspecciones.Any(i=>i.Id==x.InspeccionId&&(alcance.VerTodos||alcance.CentroIds.Contains(i.Vehiculo.CentroId))),ct);if(e is null)return NotFound();var root=Path.GetFullPath(Path.Combine(environment.ContentRootPath,"App_Data","evidencias"));var path=Path.GetFullPath(Path.Combine(environment.ContentRootPath,e.Ubicacion));if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||!System.IO.File.Exists(path))return NotFound();return PhysicalFile(path,e.MimeType,e.NombreArchivo);}
    [HttpDelete("evidencias/{evidenciaId:guid}"),Authorize(Policy="Evidencias.Eliminar")]
    public async Task<IActionResult> Eliminar(Guid evidenciaId,CancellationToken ct){var alcance=User.AlcanceCentros();var e=await db.EvidenciasInspeccion.SingleOrDefaultAsync(x=>x.Id==evidenciaId&&x.InspeccionId!=null&&db.Inspecciones.Any(i=>i.Id==x.InspeccionId&&(alcance.VerTodos||alcance.CentroIds.Contains(i.Vehiculo.CentroId))),ct);if(e is null)return NotFound();e.Activo=false;e.UsuarioModificacion=Usuario();e.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return NoContent();}
    [HttpPost("inconsistencias/{id:guid}/autorizar"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<InconsistenciaDto> Autorizar(Guid id,ResolverInconsistenciaDto dto,CancellationToken ct) => service.ResolverAsync(id,dto,true,Usuario(),User.HasClaim("permiso","inspecciones.autorizar_propia_inconsistencia"),ct,User.AlcanceCentros());
    [HttpPost("inconsistencias/{id:guid}/rechazar"), Authorize(Policy = "Inspecciones.AutorizarInconsistencia")]
    public Task<InconsistenciaDto> Rechazar(Guid id,ResolverInconsistenciaDto dto,CancellationToken ct) => service.ResolverAsync(id,dto,false,Usuario(),User.HasClaim("permiso","inspecciones.autorizar_propia_inconsistencia"),ct,User.AlcanceCentros());
    private string Usuario()=>User.Username();
}
