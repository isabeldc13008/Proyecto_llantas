using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
namespace SistemaLlantas.Api.Controllers;
[ApiController,Route("api/alertas/parametros"),Authorize(Policy="Alertas.Parametrizar")]
public sealed class ParametrosAlertaController(LlantasDbContext db):ControllerBase
{
 public sealed class GuardarRegla
 {
  [Required,StringLength(80)]public string Codigo{get;init;}="";
  [Required,StringLength(150)]public string Nombre{get;init;}="";
  [Required]public string Tipo{get;init;}="DIFERENCIA_HOMBROS";
  [StringLength(1000)]public string Descripcion{get;init;}="";
  [Required]public string Operador{get;init;}=">=";
  [Range(0,100)]public decimal Valor{get;init;}
  [Required]public string Prioridad{get;init;}="Media";
  public Guid? CentroId{get;init;}
  public bool Activo{get;init;}=true;
  public string? RowVersion{get;init;}
 }
 public sealed record CambiarEstado(bool Activo,string RowVersion);
 private IQueryable<ParametroAlerta> Consulta(){var a=User.AlcanceCentros();return db.ParametrosAlerta.Where(x=>!x.CentroId.HasValue||a.VerTodos||a.CentroIds.Contains(x.CentroId.Value));}
 private static object Map(ParametroAlerta x)=>new{x.Id,x.Codigo,x.Nombre,x.Tipo,x.Descripcion,x.Operador,x.Valor,x.Unidad,x.Prioridad,x.CentroId,x.Activo,RowVersion=Convert.ToBase64String(x.RowVersion)};
 [HttpGet]public async Task<IActionResult> Listar(CancellationToken ct)=>Ok((await Consulta().AsNoTracking().OrderBy(x=>x.Nombre).ToListAsync(ct)).Select(Map));
 [HttpGet("{id:guid}")]public async Task<IActionResult> Detalle(Guid id,CancellationToken ct){var x=await Consulta().AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);return x is null?NotFound():Ok(Map(x));}
 [HttpPost]public async Task<IActionResult> Crear(GuardarRegla dto,CancellationToken ct)
 {
  await Validar(dto,ct);var code=dto.Codigo.Trim().ToUpperInvariant();
  if(await db.ParametrosAlerta.AnyAsync(x=>x.Codigo==code,ct))throw new ConflictoException("El código de regla ya existe.");
  var rule=new ParametroAlerta{Codigo=code,UsuarioCreacion=User.Username()};Aplicar(rule,dto);db.ParametrosAlerta.Add(rule);await db.SaveChangesAsync(ct);return CreatedAtAction(nameof(Detalle),new{id=rule.Id},Map(rule));
 }
 [HttpPut("{id:guid}")]public async Task<IActionResult> Editar(Guid id,GuardarRegla dto,CancellationToken ct)
 {
  var rule=await Consulta().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException("Regla no encontrada.");
  await Validar(dto,ct);if(rule.Codigo!=dto.Codigo.Trim().ToUpperInvariant())throw new ValidacionException("El código interno no se puede modificar.");
  Version(rule,dto.RowVersion);Aplicar(rule,dto);await db.SaveChangesAsync(ct);return Ok(Map(rule));
 }
 [HttpPatch("{id:guid}/estado")]public async Task<IActionResult> Estado(Guid id,CambiarEstado dto,CancellationToken ct)
 {
  var rule=await Consulta().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException("Regla no encontrada.");Version(rule,dto.RowVersion);rule.Activo=dto.Activo;rule.UsuarioModificacion=User.Username();rule.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return Ok(Map(rule));
 }
 private static void Version(ParametroAlerta rule,string? version){if(version!=Convert.ToBase64String(rule.RowVersion))throw new ConflictoException("La regla cambió. Actualiza antes de guardar.");}
 private async Task Validar(GuardarRegla dto,CancellationToken ct)
 {
  if(string.IsNullOrWhiteSpace(dto.Codigo)||string.IsNullOrWhiteSpace(dto.Nombre))throw new ValidacionException("Código y nombre obligatorios.");
  if(dto.Tipo is not "DIFERENCIA_HOMBROS" and not "PROFUNDIDAD_MINIMA" || dto.Operador is not ">=" and not ">" and not "<=" and not "<" and not "=" and not "!=" || dto.Prioridad is not "Alta" and not "Media" and not "Baja")throw new ValidacionException("Tipo, operador o prioridad inválidos.");
  if(dto.CentroId.HasValue&&(!User.AlcanceCentros().Autoriza(dto.CentroId.Value)||!await db.Centros.AnyAsync(x=>x.Id==dto.CentroId&&x.Activo,ct)))throw new ValidacionException("Centro no autorizado o inactivo.");
 }
 private void Aplicar(ParametroAlerta x,GuardarRegla dto){x.Nombre=dto.Nombre.Trim();x.Tipo=dto.Tipo;x.Descripcion=dto.Descripcion.Trim();x.Operador=dto.Operador;x.Valor=dto.Valor;x.Unidad="mm";x.Prioridad=dto.Prioridad;x.CentroId=dto.CentroId;x.Activo=dto.Activo;x.UsuarioModificacion=User.Username();x.FechaModificacion=DateTimeOffset.UtcNow;}
}
