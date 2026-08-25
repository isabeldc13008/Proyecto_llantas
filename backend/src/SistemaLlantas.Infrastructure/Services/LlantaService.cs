using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class LlantaService(LlantasDbContext db) : ILlantaService
{
    public async Task<Pagina<LlantaResumenDto>> ConsultarAsync(ConsultaPaginada c, AlcanceCentros alcance, CancellationToken ct)
    {
        var q = Filtrar(c, alcance);
        var total = await q.CountAsync(ct);
        var items = await Ordenar(q,c).Skip((c.Pagina - 1) * c.Tamano).Take(c.Tamano).Select(Map()).ToListAsync(ct);
        return new(items, c.Pagina, c.Tamano, total);
    }
    public async Task<LlantaMetricasDto> MetricasAsync(ConsultaPaginada c,AlcanceCentros alcance,CancellationToken ct)
    {
        var q=Filtrar(c,alcance);var total=await q.CountAsync(ct);
        var mounted=await q.CountAsync(x=>db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.Activo&&a.EsActiva),ct);
        var available=await q.CountAsync(x=>x.EstadoLlanta.PermiteMontaje&&!db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.Activo&&a.EsActiva),ct);
        var repair=await q.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("REPAR"),ct);var retread=await q.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("REENCAUCH"),ct);
        var attention=await q.CountAsync(x=>db.AlertasInspeccion.Any(a=>a.LlantaId==x.Id&&a.Activo&&(a.Estado==EstadoAlerta.ABIERTA||a.Estado==EstadoAlerta.EN_PROCESO))||db.OrdenesServicioLlanta.Any(o=>o.LlantaId==x.Id&&o.Activo&&o.Estado!="CERRADA"&&o.Estado!="DISPOSICION_FINAL"),ct);
        return new(total,mounted,available,repair,retread,attention);
    }

    public async Task<IReadOnlyList<LlantaResumenDto>> ExportarAsync(ConsultaPaginada c, AlcanceCentros alcance, CancellationToken ct) =>
        await Ordenar(Filtrar(c,alcance),c).Take(50_000).Select(Map()).ToListAsync(ct);

    public Task<LlantaResumenDto?> ObtenerAsync(Guid id, AlcanceCentros alcance, CancellationToken ct) =>
        db.Llantas.AsNoTracking().Where(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId))).Select(Map()).SingleOrDefaultAsync(ct);

    public async Task<LlantaResumenDto> CrearAsync(GuardarLlantaDto dto, string usuario, AlcanceCentros alcance, CancellationToken ct)
    {
        if (!alcance.Autoriza(dto.CentroId)) throw new UnauthorizedAccessException("El centro no está autorizado para el usuario.");
        await ValidarAsync(dto, null, ct);
        var e = new Llanta(dto.Codigo, dto.Serial) { MarcaId = dto.MarcaId, ReferenciaId = dto.ReferenciaId, DimensionId = dto.DimensionId,
            TipoLlantaId = dto.TipoLlantaId, EstadoLlantaId = dto.EstadoLlantaId, CentroId = dto.CentroId, UbicacionActual = dto.UbicacionActual.Trim(),
            FechaCompra = dto.FechaCompra, Costo = dto.Costo, ProfundidadInicial = dto.ProfundidadInicial,
            FechaIngreso = dto.FechaIngreso ?? DateOnly.FromDateTime(DateTime.UtcNow), Observaciones = dto.Observaciones?.Trim(), UsuarioCreacion = usuario };
        db.Llantas.Add(e); await db.SaveChangesAsync(ct); return (await ObtenerAsync(e.Id, alcance, ct))!;
    }

    public async Task<LlantaResumenDto?> ActualizarAsync(Guid id, GuardarLlantaDto dto, string usuario, AlcanceCentros alcance, CancellationToken ct)
    {
        if (!alcance.Autoriza(dto.CentroId)) throw new UnauthorizedAccessException("El centro no está autorizado para el usuario.");
        var e = await db.Llantas.SingleOrDefaultAsync(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct); if (e is null) return null;
        if(e.CentroId!=dto.CentroId) throw new ConflictoException("El centro de una llanta no se edita directamente. Use el comando transaccional de traslado.");
        if (!string.IsNullOrWhiteSpace(dto.RowVersion)) db.Entry(e).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(dto.RowVersion);
        await ValidarAsync(dto, id, ct); e.CambiarIdentificacion(dto.Codigo, dto.Serial); e.MarcaId = dto.MarcaId; e.ReferenciaId = dto.ReferenciaId;
        e.DimensionId = dto.DimensionId; e.TipoLlantaId = dto.TipoLlantaId; e.EstadoLlantaId = dto.EstadoLlantaId; e.CentroId = dto.CentroId;
        e.UbicacionActual = dto.UbicacionActual.Trim(); e.FechaCompra = dto.FechaCompra; e.Costo = dto.Costo; e.ProfundidadInicial = dto.ProfundidadInicial;
        e.FechaIngreso = dto.FechaIngreso ?? e.FechaIngreso; e.Observaciones = dto.Observaciones?.Trim(); e.FechaModificacion = DateTimeOffset.UtcNow; e.UsuarioModificacion = usuario;
        await db.SaveChangesAsync(ct); return await ObtenerAsync(id, alcance, ct);
    }

    public async Task<bool> CambiarEstadoAsync(Guid id, bool activo, string usuario, AlcanceCentros alcance, CancellationToken ct)
    {
        var e = await db.Llantas.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct); if (e is null) return false;
        e.Activo = activo; e.FechaModificacion = DateTimeOffset.UtcNow; e.UsuarioModificacion = usuario; await db.SaveChangesAsync(ct); return true;
    }

    private async Task ValidarAsync(GuardarLlantaDto d, Guid? id, CancellationToken ct)
    {
        var codigo = d.Codigo.Trim().ToUpperInvariant(); var serial = d.Serial.Trim().ToUpperInvariant();
        if (await db.Llantas.IgnoreQueryFilters().AnyAsync(x => x.Id != id && (x.Codigo == codigo || x.Serial == serial), ct)) throw new ConflictoException("Ya existe una llanta con el código o serial indicado.");
        var referenciaValida = await db.Referencias.AnyAsync(x => x.Id == d.ReferenciaId && x.MarcaId == d.MarcaId, ct);
        if (!referenciaValida) throw new ValidacionException("La referencia no pertenece a la marca seleccionada.");
    }

    private IQueryable<Llanta> Filtrar(ConsultaPaginada c, AlcanceCentros alcance)
    {
        var q=db.Llantas.AsNoTracking().AsQueryable();
        if(!alcance.VerTodos)q=q.Where(x=>alcance.CentroIds.Contains(x.CentroId));
        if(c.CentroId.HasValue){if(!alcance.Autoriza(c.CentroId.Value))return q.Where(_=>false);q=q.Where(x=>x.CentroId==c.CentroId);}
        if(!string.IsNullOrWhiteSpace(c.CentroIds)){var ids=c.CentroIds.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(x=>Guid.TryParse(x,out var id)?id:Guid.Empty).Where(x=>x!=Guid.Empty).Distinct().ToArray();if(ids.Any(x=>!alcance.Autoriza(x)))return q.Where(_=>false);if(ids.Length>0)q=q.Where(x=>ids.Contains(x.CentroId));}
        if(c.Activo.HasValue)q=q.Where(x=>x.Activo==c.Activo);
        if(!string.IsNullOrWhiteSpace(c.Estado)){var estado=c.Estado.Trim();q=q.Where(x=>x.EstadoLlanta.Nombre==estado);}
        if(!string.IsNullOrWhiteSpace(c.Estados)){var estados=c.Estados.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);if(estados.Length>0)q=q.Where(x=>estados.Contains(x.EstadoLlanta.Nombre));}
        if(c.ProfundidadMin.HasValue)q=q.Where(x=>x.ProfundidadInicial>=c.ProfundidadMin);
        if(c.ProfundidadMax.HasValue)q=q.Where(x=>x.ProfundidadInicial<=c.ProfundidadMax);
        if(c.MarcaId.HasValue)q=q.Where(x=>x.MarcaId==c.MarcaId);if(c.ReferenciaId.HasValue)q=q.Where(x=>x.ReferenciaId==c.ReferenciaId);if(c.DimensionId.HasValue)q=q.Where(x=>x.DimensionId==c.DimensionId);if(c.TipoLlantaId.HasValue)q=q.Where(x=>x.TipoLlantaId==c.TipoLlantaId);
        if(c.TieneReencauches.HasValue)q=q.Where(x=>(x.NumeroReencauches>0)==c.TieneReencauches);if(c.ReencauchesMin.HasValue)q=q.Where(x=>x.NumeroReencauches>=c.ReencauchesMin);
        if(c.TieneReparaciones.HasValue)q=q.Where(x=>db.OrdenesServicioLlanta.Any(o=>o.LlantaId==x.Id&&o.Activo&&o.Tipo==TipoServicioLlanta.Reparacion)==c.TieneReparaciones);
        if(!string.IsNullOrWhiteSpace(c.Vehiculo)){var vehicle=c.Vehiculo.Trim();q=q.Where(x=>db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.EsActiva&&(a.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa.Contains(vehicle)||a.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno.Contains(vehicle))));}
        if(c.KilometrajeMin.HasValue)q=q.Where(x=>x.KilometrajeAcumulado>=c.KilometrajeMin);if(c.KilometrajeMax.HasValue)q=q.Where(x=>x.KilometrajeAcumulado<=c.KilometrajeMax);
        if(c.InspeccionDesde.HasValue)q=q.Where(x=>db.InspeccionesDetalle.Any(d=>d.LlantaId==x.Id&&d.Inspeccion.FechaCreacion>=c.InspeccionDesde));if(c.InspeccionHasta.HasValue)q=q.Where(x=>db.InspeccionesDetalle.Any(d=>d.LlantaId==x.Id&&d.Inspeccion.FechaCreacion<=c.InspeccionHasta));
        if(c.RequiereAtencion.HasValue)q=q.Where(x=>(db.AlertasInspeccion.Any(a=>a.LlantaId==x.Id&&a.Activo&&(a.Estado==EstadoAlerta.ABIERTA||a.Estado==EstadoAlerta.EN_PROCESO))||db.OrdenesServicioLlanta.Any(o=>o.LlantaId==x.Id&&o.Activo&&o.Estado!="CERRADA"&&o.Estado!="DISPOSICION_FINAL"))==c.RequiereAtencion);
        if(!string.IsNullOrWhiteSpace(c.Search)){var s=c.Search.Trim();q=q.Where(x=>x.Codigo.Contains(s)||x.Serial.Contains(s)||x.Marca.Nombre.Contains(s)||x.Referencia.Nombre.Contains(s));}
        return q;
    }

    private static IQueryable<Llanta> Ordenar(IQueryable<Llanta> q,ConsultaPaginada c)
    {
        var desc=string.Equals(c.SortDirection,"desc",StringComparison.OrdinalIgnoreCase);
        return (c.SortBy?.ToLowerInvariant(),desc) switch{("serial",false)=>q.OrderBy(x=>x.Serial),("serial",true)=>q.OrderByDescending(x=>x.Serial),("centro",false)=>q.OrderBy(x=>x.Centro.Nombre),("centro",true)=>q.OrderByDescending(x=>x.Centro.Nombre),("estado",false)=>q.OrderBy(x=>x.EstadoLlanta.Nombre),("estado",true)=>q.OrderByDescending(x=>x.EstadoLlanta.Nombre),("codigo",true)=>q.OrderByDescending(x=>x.Codigo),_=>q.OrderBy(x=>x.Codigo)};
    }

    private System.Linq.Expressions.Expression<Func<Llanta, LlantaResumenDto>> Map() => x => new(x.Id, x.Codigo, x.Serial, x.Marca.Nombre,
        x.Referencia.Nombre, x.Dimension.Nombre, x.TipoLlanta.Nombre, x.EstadoLlanta.Nombre, x.Centro.Nombre, x.UbicacionActual,
        x.ProfundidadInicial,x.KilometrajeAcumulado,x.NumeroReencauches,
        db.AsignacionesLlantaPosicion.Where(a=>a.LlantaId==x.Id&&a.EsActiva).Select(a=>a.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno+" · "+a.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa).FirstOrDefault(),
        db.AsignacionesLlantaPosicion.Where(a=>a.LlantaId==x.Id&&a.EsActiva).Select(a=>a.PosicionVehiculo.Codigo).FirstOrDefault(),
        db.InspeccionesDetalle.Where(d=>d.LlantaId==x.Id).OrderByDescending(d=>d.Inspeccion.FechaCreacion).Select(d=>(DateTimeOffset?)d.Inspeccion.FechaCreacion).FirstOrDefault(),
        db.InspeccionesDetalle.Where(d=>d.LlantaId==x.Id).OrderByDescending(d=>d.Inspeccion.FechaCreacion).Select(d=>new[]{d.ProfundidadExterior,d.ProfundidadCentro,d.ProfundidadInterior}.Min()).FirstOrDefault(),
        db.OrdenesServicioLlanta.Count(o=>o.LlantaId==x.Id&&o.Activo&&o.Tipo==TipoServicioLlanta.Reparacion),db.AsignacionesLlantaPosicion.Count(a=>a.LlantaId==x.Id&&a.Activo),
        db.AlertasInspeccion.Where(a=>a.LlantaId==x.Id&&a.Activo&&(a.Estado==EstadoAlerta.ABIERTA||a.Estado==EstadoAlerta.EN_PROCESO)).OrderByDescending(a=>a.Tipo.Contains("PROFUNDIDAD")).Select(a=>a.Tipo).FirstOrDefault()??db.OrdenesServicioLlanta.Where(o=>o.LlantaId==x.Id&&o.Activo&&o.Estado!="CERRADA"&&o.Estado!="DISPOSICION_FINAL").Select(o=>o.Tipo==TipoServicioLlanta.Reparacion?"Pendiente reparación":o.Tipo==TipoServicioLlanta.Reencauche?"Pendiente reencauche":"Atención requerida").FirstOrDefault()??"Normal",
        x.Activo, Convert.ToBase64String(x.RowVersion));
}
