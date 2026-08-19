using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Catalogos;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class CatalogoService(LlantasDbContext db) : ICatalogoService
{
    public async Task<Pagina<CatalogoDto>> ConsultarAsync(string tipo, ConsultaPaginada c, CancellationToken ct)
    {
        if (tipo.Equals("centros", StringComparison.OrdinalIgnoreCase))
        {
            var centros = db.Centros.IgnoreQueryFilters().AsQueryable();
            if (c.Activo.HasValue) centros = centros.Where(x => x.Activo == c.Activo);
            if (!string.IsNullOrWhiteSpace(c.Search)) { var s = c.Search.Trim(); centros = centros.Where(x => x.Codigo.Contains(s) || x.Nombre.Contains(s)); }
            var totalCentros = await centros.CountAsync(ct);
            var centrosItems = await centros.OrderBy(x => x.Nombre).Skip((c.Pagina - 1) * c.Tamano).Take(c.Tamano)
                .Select(x => new CatalogoDto(x.Id, x.Codigo, x.Nombre, x.Activo, x.RegionalId, x.Regional == null ? null : x.Regional.Nombre)).ToListAsync(ct);
            return new(centrosItems, c.Pagina, c.Tamano, totalCentros);
        }
        var q = Set(tipo).IgnoreQueryFilters();
        if (c.Activo.HasValue) q = q.Where(x => x.Activo == c.Activo);
        if (!string.IsNullOrWhiteSpace(c.Search)) { var s = c.Search.Trim(); q = q.Where(x => x.Codigo.Contains(s) || x.Nombre.Contains(s)); }
        var total = await q.CountAsync(ct); var items = await q.OrderBy(x => x.Nombre).Skip((c.Pagina - 1) * c.Tamano).Take(c.Tamano)
            .Select(x => new CatalogoDto(x.Id, x.Codigo, x.Nombre, x.Activo)).ToListAsync(ct);
        return new(items, c.Pagina, c.Tamano, total);
    }

    public async Task<CatalogoDto> CrearAsync(string tipo, GuardarCatalogoDto dto, string usuario, CancellationToken ct)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant(); var set = Set(tipo);
        if (await set.IgnoreQueryFilters().AnyAsync(x => x.Codigo == codigo, ct)) throw new ConflictoException("Ya existe un registro con ese código.");
        CatalogoBase e = tipo.ToLowerInvariant() switch { "marcas" => new Marca(), "referencias" => new Referencia { MarcaId = dto.PadreId ?? throw new ValidacionException("La marca es obligatoria.") },
            "dimensiones" => new Dimension(), "tipos-llanta" => new TipoLlanta(), "estados-llanta" => new EstadoLlanta(), "regionales" => new Regional(), "centros" => new Centro { RegionalId = dto.PadreId ?? throw new ValidacionException("La regional es obligatoria.") }, _ => throw new ValidacionException("Tipo de catálogo no válido.") };
        e.Codigo = codigo; e.Nombre = dto.Nombre.Trim(); e.UsuarioCreacion = usuario; db.Add(e); await db.SaveChangesAsync(ct);
        return new(e.Id, e.Codigo, e.Nombre, e.Activo, dto.PadreId, null);
    }

    public async Task<CatalogoDto?> ActualizarAsync(string tipo, Guid id, GuardarCatalogoDto dto, string usuario, CancellationToken ct)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant(); var set = Set(tipo);
        var e = await set.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id, ct); if (e is null) return null;
        if (await set.IgnoreQueryFilters().AnyAsync(x => x.Id != id && x.Codigo == codigo, ct)) throw new ConflictoException("Ya existe un registro con ese código.");
        e.Codigo = codigo; e.Nombre = dto.Nombre.Trim(); e.UsuarioModificacion = usuario; e.FechaModificacion = DateTimeOffset.UtcNow;
        if (e is Referencia referencia) referencia.MarcaId = dto.PadreId ?? throw new ValidacionException("La marca es obligatoria.");
        if (e is Centro centro) centro.RegionalId = dto.PadreId ?? throw new ValidacionException("La regional es obligatoria.");
        await db.SaveChangesAsync(ct);
        var padreNombre = e is Centro c ? await db.Regionales.Where(x => x.Id == c.RegionalId).Select(x => x.Nombre).SingleAsync(ct) : null;
        return new(e.Id, e.Codigo, e.Nombre, e.Activo, dto.PadreId, padreNombre);
    }

    public async Task<bool> CambiarEstadoAsync(string tipo, Guid id, bool activo, string usuario, CancellationToken ct)
    {
        var e = await Set(tipo).IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id, ct); if (e is null) return false;
        e.Activo = activo; e.UsuarioModificacion = usuario; e.FechaModificacion = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    private IQueryable<CatalogoBase> Set(string tipo) => tipo.ToLowerInvariant() switch { "marcas" => db.Marcas, "referencias" => db.Referencias,
        "dimensiones" => db.Dimensiones, "tipos-llanta" => db.TiposLlanta, "estados-llanta" => db.EstadosLlanta, "regionales" => db.Regionales, "centros" => db.Centros,
        _ => throw new ValidacionException("Tipo de catálogo no válido.") };
}
