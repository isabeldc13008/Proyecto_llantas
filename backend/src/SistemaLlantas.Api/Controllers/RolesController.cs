using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/roles"), Authorize(Roles = "ADMINISTRADOR")]
public sealed class RolesController(LlantasDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<object> Listar(CancellationToken ct) => await db.RolesSistema.AsNoTracking()
        .OrderBy(x => x.Nombre).Select(x => new { x.Id, x.Codigo, x.Nombre, x.Activo,
            Usuarios = x.Usuarios.Count, UsuariosActivos = x.Usuarios.Count(u => u.Activo) }).ToListAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detalle(Guid id, CancellationToken ct)
    {
        var rol = await db.RolesSistema.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Codigo, x.Nombre, x.Activo,
                UsuariosActivos = x.Usuarios.Count(u => u.Activo),
                Permisos = x.Permisos.Select(p => p.Permiso.Codigo).ToArray() }).SingleOrDefaultAsync(ct);
        return rol is null ? NotFound() : Ok(rol);
    }

    [HttpGet("permisos")]
    public async Task<object> Permisos(CancellationToken ct) => await db.PermisosSistema.AsNoTracking()
        .OrderBy(x => x.Codigo).Select(x => new { x.Codigo, x.Nombre, x.Activo }).ToListAsync(ct);

    [HttpPost]
    public async Task<IActionResult> Crear(CrearRol dto, CancellationToken ct)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            if (await db.RolesSistema.AnyAsync(x => x.Codigo == codigo, ct))
                return Conflict(new { message = "El código de rol ya existe." });
            var permisos = await PermisosValidos(dto.Permisos, ct);
            if (permisos is null) return BadRequest(new { message = "Hay permisos que no existen." });
            var rol = new RolSistema { Codigo = codigo, Nombre = dto.Nombre.Trim(), Activo = dto.Activo,
                UsuarioCreacion = User.Username() };
            foreach (var id in permisos) rol.Permisos.Add(new RolPermiso { PermisoId = id });
            db.RolesSistema.Add(rol);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return CreatedAtAction(nameof(Detalle), new { id = rol.Id }, new { rol.Id });
        });
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Editar(Guid id, EditarRol dto, CancellationToken ct) => Cambiar(id, dto, null, ct);

    [HttpPatch("{id:guid}/estado")]
    public Task<IActionResult> Estado(Guid id, EstadoRol dto, CancellationToken ct) => Cambiar(id, null, dto, ct);

    private async Task<IActionResult> Cambiar(Guid id, EditarRol? editar, EstadoRol? estado, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var rol = await db.RolesSistema.Include(x => x.Permisos).SingleOrDefaultAsync(x => x.Id == id, ct);
            if (rol is null) return NotFound();
            var activo = editar?.Activo ?? estado!.Activo;
            var usuariosActivos = await db.UsuariosSistema.CountAsync(x => x.RolId == id && x.Activo, ct);
            if (rol.Activo && !activo)
            {
                if (rol.Codigo == "ADMINISTRADOR" && !await db.UsuariosSistema.AnyAsync(
                    x => x.Activo && x.RolId != id && x.Rol.Activo && x.Rol.Codigo == "ADMINISTRADOR", ct))
                    return Conflict(new { message = "No se puede desactivar el último rol ADMINISTRADOR activo.", usuariosActivos });
                if ((editar?.UsuariosActivosConfirmados ?? estado?.UsuariosActivosConfirmados) != usuariosActivos)
                    return Conflict(new { message = "Confirma la desactivación con la cantidad actual de usuarios activos.", usuariosActivos });
            }
            if (editar is not null)
            {
                var permisos = await PermisosValidos(editar.Permisos, ct);
                if (permisos is null) return BadRequest(new { message = "Hay permisos que no existen." });
                db.RolesPermisos.RemoveRange(rol.Permisos.Where(x => !permisos.Contains(x.PermisoId)).ToArray());
                foreach (var permisoId in permisos.Where(p => rol.Permisos.All(x => x.PermisoId != p)))
                    db.RolesPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
                rol.Nombre = editar.Nombre.Trim();
            }
            rol.Activo = activo;
            rol.UsuarioModificacion = User.Username();
            rol.FechaModificacion = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Ok(new { rol.Id, rol.Activo, usuariosActivos });
        });
    }

    private async Task<Guid[]?> PermisosValidos(string[] codigos, CancellationToken ct)
    {
        var distintos = codigos.Distinct().ToArray();
        var ids = await db.PermisosSistema.Where(x => distintos.Contains(x.Codigo)).Select(x => x.Id).ToArrayAsync(ct);
        return ids.Length == distintos.Length ? ids : null;
    }

    public sealed record CrearRol([Required, MaxLength(60)] string Codigo, [Required, MaxLength(100)] string Nombre,
        [Required] string[] Permisos, bool Activo = true);
    public sealed record EditarRol([Required, MaxLength(100)] string Nombre, [Required] string[] Permisos,
        bool Activo, int? UsuariosActivosConfirmados = null);
    public sealed record EstadoRol(bool Activo, int? UsuariosActivosConfirmados = null);
}
