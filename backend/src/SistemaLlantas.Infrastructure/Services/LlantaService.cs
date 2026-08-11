using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class LlantaService(LlantasDbContext db) : ILlantaService
{
    public async Task<Pagina<LlantaResumenDto>> ConsultarAsync(ConsultaPaginada c, Guid? centroId, CancellationToken ct)
    {
        var q = db.Llantas.AsNoTracking().AsQueryable();
        if (centroId.HasValue) q = q.Where(x => x.CentroId == centroId);
        if (!string.IsNullOrWhiteSpace(c.Search)) { var s = c.Search.Trim(); q = q.Where(x => x.Codigo.Contains(s) || x.Serial.Contains(s)); }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.Codigo).Skip((c.Pagina - 1) * c.Tamano).Take(c.Tamano).Select(Map()).ToListAsync(ct);
        return new(items, c.Pagina, c.Tamano, total);
    }

    public Task<LlantaResumenDto?> ObtenerAsync(Guid id, Guid? centroId, CancellationToken ct) =>
        db.Llantas.AsNoTracking().Where(x => x.Id == id && (!centroId.HasValue || x.CentroId == centroId)).Select(Map()).SingleOrDefaultAsync(ct);

    public async Task<LlantaResumenDto> CrearAsync(GuardarLlantaDto dto, string usuario, CancellationToken ct)
    {
        await ValidarAsync(dto, null, ct);
        var e = new Llanta(dto.Codigo, dto.Serial) { MarcaId = dto.MarcaId, ReferenciaId = dto.ReferenciaId, DimensionId = dto.DimensionId,
            TipoLlantaId = dto.TipoLlantaId, EstadoLlantaId = dto.EstadoLlantaId, CentroId = dto.CentroId, UbicacionActual = dto.UbicacionActual.Trim(),
            FechaCompra = dto.FechaCompra, Costo = dto.Costo, ProfundidadInicial = dto.ProfundidadInicial,
            FechaIngreso = dto.FechaIngreso ?? DateOnly.FromDateTime(DateTime.UtcNow), Observaciones = dto.Observaciones?.Trim(), UsuarioCreacion = usuario };
        db.Llantas.Add(e); await db.SaveChangesAsync(ct); return (await ObtenerAsync(e.Id, null, ct))!;
    }

    public async Task<LlantaResumenDto?> ActualizarAsync(Guid id, GuardarLlantaDto dto, string usuario, CancellationToken ct)
    {
        var e = await db.Llantas.SingleOrDefaultAsync(x => x.Id == id, ct); if (e is null) return null;
        if (!string.IsNullOrWhiteSpace(dto.RowVersion)) db.Entry(e).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(dto.RowVersion);
        await ValidarAsync(dto, id, ct); e.CambiarIdentificacion(dto.Codigo, dto.Serial); e.MarcaId = dto.MarcaId; e.ReferenciaId = dto.ReferenciaId;
        e.DimensionId = dto.DimensionId; e.TipoLlantaId = dto.TipoLlantaId; e.EstadoLlantaId = dto.EstadoLlantaId; e.CentroId = dto.CentroId;
        e.UbicacionActual = dto.UbicacionActual.Trim(); e.FechaCompra = dto.FechaCompra; e.Costo = dto.Costo; e.ProfundidadInicial = dto.ProfundidadInicial;
        e.FechaIngreso = dto.FechaIngreso ?? e.FechaIngreso; e.Observaciones = dto.Observaciones?.Trim(); e.FechaModificacion = DateTimeOffset.UtcNow; e.UsuarioModificacion = usuario;
        await db.SaveChangesAsync(ct); return await ObtenerAsync(id, null, ct);
    }

    public async Task<bool> CambiarEstadoAsync(Guid id, bool activo, string usuario, CancellationToken ct)
    {
        var e = await db.Llantas.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id, ct); if (e is null) return false;
        e.Activo = activo; e.FechaModificacion = DateTimeOffset.UtcNow; e.UsuarioModificacion = usuario; await db.SaveChangesAsync(ct); return true;
    }

    private async Task ValidarAsync(GuardarLlantaDto d, Guid? id, CancellationToken ct)
    {
        var codigo = d.Codigo.Trim().ToUpperInvariant(); var serial = d.Serial.Trim().ToUpperInvariant();
        if (await db.Llantas.IgnoreQueryFilters().AnyAsync(x => x.Id != id && (x.Codigo == codigo || x.Serial == serial), ct)) throw new ConflictoException("Ya existe una llanta con el código o serial indicado.");
        var referenciaValida = await db.Referencias.AnyAsync(x => x.Id == d.ReferenciaId && x.MarcaId == d.MarcaId, ct);
        if (!referenciaValida) throw new ValidacionException("La referencia no pertenece a la marca seleccionada.");
    }

    private static System.Linq.Expressions.Expression<Func<Llanta, LlantaResumenDto>> Map() => x => new(x.Id, x.Codigo, x.Serial, x.Marca.Nombre,
        x.Referencia.Nombre, x.Dimension.Nombre, x.TipoLlanta.Nombre, x.EstadoLlanta.Nombre, x.Centro.Nombre, x.UbicacionActual,
        x.ProfundidadInicial, x.Activo, Convert.ToBase64String(x.RowVersion));
}
