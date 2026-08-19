using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class InspeccionService(LlantasDbContext db) : IInspeccionService
{
    public async Task<IReadOnlyList<VehiculoInspeccionDto>> ObtenerVehiculosAsync(string usuario, bool soloAsignados,string? buscar, AlcanceCentros alcance, CancellationToken ct)
    {
        var q = db.Vehiculos.AsNoTracking().Where(x => alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId));
        if (soloAsignados)
            q = q.Where(x => db.ActividadesProgramadas.Any(a => (a.TecnicoId == usuario || a.TecnicoId == usuario + ".local") && a.VehiculoId == x.Id && a.Estado != EstadoActividad.Cancelada && a.Estado != EstadoActividad.Cumplida));
        if(!string.IsNullOrWhiteSpace(buscar)){var s=buscar.Trim();q=q.Where(x=>x.NumeroInterno.Contains(s)||x.Placa.Contains(s)||x.Tipo.Contains(s)||x.Centro.Nombre.Contains(s));}
        return await q.OrderBy(x => x.NumeroInterno).Select(x => new VehiculoInspeccionDto(x.Id, x.NumeroInterno, x.Placa, x.Tipo, x.CentroId, x.Centro.Codigo, x.Centro.Nombre)).ToListAsync(ct);
    }

    public async Task<OpcionesInspeccionDto> ObtenerOpcionesAsync(CancellationToken ct) => new(
        await db.CondicionesLlanta.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new OpcionInspeccionDto(x.Id, x.Codigo, x.Nombre)).ToListAsync(ct),
        await db.CausasLlanta.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new OpcionInspeccionDto(x.Id, x.Codigo, x.Nombre)).ToListAsync(ct),
        await db.RecomendacionesInspeccion.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new OpcionInspeccionDto(x.Id, x.Codigo, x.Nombre)).ToListAsync(ct));

    public async Task<ContextoInspeccionDto?> ObtenerContextoAsync(Guid vehiculoId, AlcanceCentros alcance, CancellationToken ct)
    {
        var v = await db.Vehiculos.AsNoTracking().Include(x => x.Centro)
            .Include(x => x.Ejes).ThenInclude(x => x.Posiciones).ThenInclude(x => x.LlantaActual).ThenInclude(x => x!.EstadoLlanta)
            .SingleOrDefaultAsync(x => x.Id == vehiculoId && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct);
        if(v is null)return null;var positionIds=v.Ejes.SelectMany(x=>x.Posiciones).Select(x=>x.Id).ToArray();var active=await db.AsignacionesLlantaPosicion.AsNoTracking().Where(x=>positionIds.Contains(x.PosicionVehiculoId)&&x.EsActiva).Include(x=>x.Llanta).ThenInclude(x=>x.EstadoLlanta).ToDictionaryAsync(x=>x.PosicionVehiculoId,ct);return new(v.Id, v.NumeroInterno, v.Placa, v.Tipo, v.CentroId, v.Centro.Nombre, v.Centro.Relevancia,
            v.Ejes.OrderBy(e => e.Numero).Select(e => new EjeInspeccionDto(e.Id, e.Numero, e.Nombre,
                e.Posiciones.OrderBy(p => p.Orden).Select(p => new PosicionInspeccionDto(p.Id, p.Codigo, p.Lado, p.Orden,active.TryGetValue(p.Id,out var a)?new(a.Llanta.Id,a.Llanta.Codigo,a.Llanta.EstadoLlanta.Nombre):null)).ToList())).ToList());
    }

    public async Task<InspeccionDto> CrearAsync(CrearInspeccionDto dto, string usuario, AlcanceCentros alcance, CancellationToken ct)
    {
        var vehiculo = await db.Vehiculos.Include(x => x.Ejes).ThenInclude(x => x.Posiciones)
            .SingleOrDefaultAsync(x => x.Id == dto.VehiculoId && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct)
            ?? throw new KeyNotFoundException("Vehículo no encontrado o fuera del centro autorizado.");
        var inspeccion = new Inspeccion { VehiculoId = vehiculo.Id, CentroId = vehiculo.CentroId, Kilometraje = dto.Kilometraje,
            TecnicoId = usuario, Observaciones = dto.Observaciones, UsuarioCreacion = usuario };
        var positions=vehiculo.Ejes.SelectMany(x=>x.Posiciones).ToList();var ids=positions.Select(x=>x.Id).ToArray();var mounted=await db.AsignacionesLlantaPosicion.Where(x=>ids.Contains(x.PosicionVehiculoId)&&x.EsActiva).ToDictionaryAsync(x=>x.PosicionVehiculoId,x=>x.LlantaId,ct);foreach (var posicion in positions){var hasTire=mounted.TryGetValue(posicion.Id,out var tireId);inspeccion.Detalles.Add(new InspeccionDetalle { PosicionVehiculoId = posicion.Id, LlantaId = hasTire?tireId:null, UsuarioCreacion = usuario });}
        db.Inspecciones.Add(inspeccion); await db.SaveChangesAsync(ct);
        return (await ObtenerAsync(inspeccion.Id, alcance, ct))!;
    }

    public async Task<InspeccionDto?> ObtenerAsync(Guid id, AlcanceCentros alcance, CancellationToken ct)
    {
        var item = await ConsultaCompleta().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct);
        return item is null ? null : Map(item);
    }

    public async Task<InspeccionDto?> GuardarDetalleAsync(Guid id, Guid posicionId, GuardarDetalleInspeccionDto dto, string usuario, CancellationToken ct)
    {
        var detalle = await db.InspeccionesDetalle.Include(x => x.Inspeccion).SingleOrDefaultAsync(x => x.InspeccionId == id && x.PosicionVehiculoId == posicionId, ct);
        if (detalle is null) return null;
        if (detalle.Inspeccion.Estado != EstadoInspeccion.Borrador) throw new InvalidOperationException("Solo se puede modificar una inspección en borrador.");
        detalle.ProfundidadExterior = dto.ProfundidadExterior; detalle.ProfundidadCentro = dto.ProfundidadCentro; detalle.ProfundidadInterior = dto.ProfundidadInterior;
        detalle.CondicionLlantaId = dto.CondicionId; detalle.CausaLlantaId = dto.CausaId; detalle.RecomendacionId = dto.RecomendacionId;
        detalle.Observaciones = dto.Observaciones; detalle.UsuarioModificacion = usuario; detalle.FechaModificacion = DateTimeOffset.UtcNow;
        var values=new[]{dto.ProfundidadExterior,dto.ProfundidadCentro,dto.ProfundidadInterior}.Where(x=>x.HasValue).Select(x=>x!.Value).ToArray();var threshold=await db.ParametrosAlerta.Where(x=>x.Codigo=="DIFERENCIA_HOMBROS_MM").Select(x=>(decimal?)x.Valor).SingleOrDefaultAsync(ct);if(threshold.HasValue&&values.Length>=2&&values.Max()-values.Min()>=threshold.Value&&!await db.AlertasInspeccion.AnyAsync(x=>x.InspeccionDetalleId==detalle.Id&&x.Tipo=="DIFERENCIA_HOMBROS",ct)){var alert=new AlertaInspeccion{Tipo="DIFERENCIA_HOMBROS",Descripcion=$"Diferencia de {values.Max()-values.Min():0.##} mm entre mediciones de la llanta.",InspeccionId=id,InspeccionDetalleId=detalle.Id,VehiculoId=detalle.Inspeccion.VehiculoId,CentroId=detalle.Inspeccion.CentroId,PosicionVehiculoId=posicionId,LlantaId=detalle.LlantaId,UsuarioCreacion=usuario};alert.Historial.Add(new(){EstadoAnterior=EstadoAlerta.ABIERTA,EstadoNuevo=EstadoAlerta.ABIERTA,Observacion="Generada automáticamente por regla parametrizada.",UsuarioCreacion=usuario});db.AlertasInspeccion.Add(alert);}
        await db.SaveChangesAsync(ct); return await ObtenerAsync(id, new(true, []), ct);
    }

    public async Task<InconsistenciaDto> ReportarAsync(Guid inspeccionId, ReportarInconsistenciaDto dto, string usuario, CancellationToken ct)
    {
        var inspeccion = await db.Inspecciones.Include(x => x.Vehiculo).ThenInclude(x => x.Centro).SingleOrDefaultAsync(x => x.Id == inspeccionId, ct)
            ?? throw new KeyNotFoundException("Inspección no encontrada.");
        var posicion = await db.PosicionesVehiculo.Include(x => x.LlantaActual).SingleOrDefaultAsync(x => x.Id == dto.PosicionId, ct)
            ?? throw new KeyNotFoundException("Posición no encontrada.");
        if (!await db.InspeccionesDetalle.AnyAsync(x => x.InspeccionId == inspeccionId && x.PosicionVehiculoId == dto.PosicionId, ct))
            throw new InvalidOperationException("La posición no pertenece a la inspección.");
        var novedad = new InconsistenciaInspeccion { InspeccionId = inspeccionId, PosicionVehiculoId = posicion.Id, LlantaEsperadaId = posicion.LlantaActualId,
            IdentificadorEncontrado = dto.IdentificadorEncontrado.Trim().ToUpperInvariant(), TecnicoId = usuario, Observacion = dto.Observacion, UsuarioCreacion = usuario };
        novedad.LlantaTemporal = new LlantaTemporal { IdentificadorTemporal = $"TMP-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..29],
            IdentificadorFisico = novedad.IdentificadorEncontrado, UsuarioCreacion = usuario };
        db.InconsistenciasInspeccion.Add(novedad); await db.SaveChangesAsync(ct);
        return await MapInconsistencia(novedad.Id, ct);
    }

    public async Task<IReadOnlyList<InconsistenciaDto>> PendientesAsync(AlcanceCentros alcance, CancellationToken ct)
    {
        var ids = await db.InconsistenciasInspeccion.AsNoTracking()
            .Where(x => x.Estado == EstadoInconsistencia.PendienteAutorizacion && (alcance.VerTodos || alcance.CentroIds.Contains(x.Inspeccion.CentroId)))
            .OrderBy(x => x.FechaCreacion).Select(x => x.Id).ToListAsync(ct);
        var result = new List<InconsistenciaDto>(); foreach (var id in ids) result.Add(await MapInconsistencia(id, ct)); return result;
    }

    public async Task<InconsistenciaDto> ResolverAsync(Guid id, ResolverInconsistenciaDto dto, bool autorizar, string usuario, bool puedeAutorizarPropia, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var item = await db.InconsistenciasInspeccion.Include(x => x.Inspeccion).Include(x => x.PosicionVehiculo).Include(x => x.LlantaTemporal)
            .SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Solicitud no encontrada.");
        if (item.Estado != EstadoInconsistencia.PendienteAutorizacion) throw new InvalidOperationException("La solicitud ya fue resuelta.");
        if (item.TecnicoId == usuario && !puedeAutorizarPropia) throw new UnauthorizedAccessException("El técnico no puede autorizar su propia solicitud.");
        item.Estado = autorizar ? EstadoInconsistencia.Autorizada : EstadoInconsistencia.Rechazada; item.UsuarioAutorizador = usuario;
        item.FechaAutorizacion = DateTimeOffset.UtcNow; item.ObservacionAutorizacion = dto.Observacion; item.UsuarioModificacion = usuario;
        item.LlantaTemporal!.Estado = item.Estado; item.LlantaTemporal.UsuarioModificacion = usuario;
        if (autorizar)
        {
            if (!dto.LlantaInventarioId.HasValue) throw new InvalidOperationException("Para autorizar debe seleccionar la llanta validada del inventario.");
            var nueva = await db.Llantas.SingleOrDefaultAsync(x => x.Id == dto.LlantaInventarioId, ct) ?? throw new KeyNotFoundException("Llanta de inventario no encontrada.");
            db.MovimientosLlanta.Add(new MovimientoLlanta { InspeccionId = item.InspeccionId, InconsistenciaInspeccionId = item.Id,
                PosicionVehiculoId = item.PosicionVehiculoId, LlantaAnteriorId = item.PosicionVehiculo.LlantaActualId, LlantaNuevaId = nueva.Id,
                CentroId = item.Inspeccion.CentroId, Motivo = "Regularización de inconsistencia detectada en inspección", TecnicoReporta = item.TecnicoId,
                UsuarioAutoriza = usuario, FechaReporte = item.FechaCreacion, FechaAutorizacion = item.FechaAutorizacion.Value, Observaciones = dto.Observacion, UsuarioCreacion = usuario });
            item.PosicionVehiculo.LlantaActualId = nueva.Id; item.PosicionVehiculo.UsuarioModificacion = usuario; item.PosicionVehiculo.FechaModificacion = DateTimeOffset.UtcNow;
            item.LlantaTemporal.Estado = EstadoInconsistencia.Regularizada;
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return await MapInconsistencia(id, ct);
    }

    private IQueryable<Inspeccion> ConsultaCompleta() => db.Inspecciones.Include(x => x.Vehiculo).Include(x => x.Centro)
        .Include(x => x.Detalles).ThenInclude(x => x.PosicionVehiculo).Include(x => x.Detalles).ThenInclude(x => x.Llanta);
    private static InspeccionDto Map(Inspeccion x) => new(x.Id, x.VehiculoId, $"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}", x.CentroId, x.Centro.Nombre,
        x.Kilometraje, x.Estado.ToString(), x.TecnicoId, x.Detalles.OrderBy(d => d.PosicionVehiculo.Orden).Select(d => new DetalleInspeccionDto(d.Id,d.PosicionVehiculoId,d.PosicionVehiculo.Codigo,d.LlantaId,d.Llanta?.Codigo,d.ProfundidadExterior,d.ProfundidadCentro,d.ProfundidadInterior,d.CondicionLlantaId,d.CausaLlantaId,d.RecomendacionId,d.Observaciones)).ToList());
    private async Task<InconsistenciaDto> MapInconsistencia(Guid id, CancellationToken ct) => await db.InconsistenciasInspeccion.AsNoTracking()
        .Where(x => x.Id == id).Select(x => new InconsistenciaDto(x.Id,x.InspeccionId,x.Inspeccion.Centro.Nombre,$"Interno {x.Inspeccion.Vehiculo.NumeroInterno} - {x.Inspeccion.Vehiculo.Placa}",x.PosicionVehiculo.Codigo,x.LlantaEsperada != null ? x.LlantaEsperada.Codigo : null,x.IdentificadorEncontrado,x.TecnicoId,x.FechaCreacion,x.Estado.ToString(),x.UsuarioAutorizador,x.ObservacionAutorizacion)).SingleAsync(ct);
    public async Task<IReadOnlyList<AlertaDto>> AlertasAsync(AlcanceCentros alcance,CancellationToken ct){var ids=await db.AlertasInspeccion.AsNoTracking().Where(x=>alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)).OrderByDescending(x=>x.FechaCreacion).Select(x=>x.Id).ToListAsync(ct);var result=new List<AlertaDto>();foreach(var id in ids)result.Add(await MapAlerta(id,ct));return result;}
    public async Task<AlertaDto> CambiarAlertaAsync(Guid id,CambiarAlertaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct){if(!Enum.TryParse<EstadoAlerta>(dto.Estado,true,out var state))throw new ValidacionException("Estado de alerta no válido.");var item=await db.AlertasInspeccion.SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Alerta no encontrada.");var previous=item.Estado;item.Estado=state;item.UsuarioModificacion=usuario;item.FechaModificacion=DateTimeOffset.UtcNow;db.AlertasHistorial.Add(new(){AlertaInspeccionId=id,EstadoAnterior=previous,EstadoNuevo=state,Observacion=dto.Observacion,UsuarioCreacion=usuario});await db.SaveChangesAsync(ct);return await MapAlerta(id,ct);}
    private Task<AlertaDto> MapAlerta(Guid id,CancellationToken ct)=>db.AlertasInspeccion.AsNoTracking().Where(x=>x.Id==id).Select(x=>new AlertaDto(x.Id,x.Tipo,x.Descripcion,x.Estado.ToString(),x.FechaCreacion,x.InspeccionId,x.Inspeccion.Vehiculo.NumeroInterno+" · "+x.Inspeccion.Vehiculo.Placa,x.Inspeccion.Centro.Nombre,x.InspeccionDetalle.PosicionVehiculo.Codigo,x.InspeccionDetalle.Llanta!=null?x.InspeccionDetalle.Llanta.Codigo:null,x.Historial.OrderBy(h=>h.FechaCreacion).Select(h=>new AlertaEventoDto(h.FechaCreacion,h.EstadoAnterior.ToString(),h.EstadoNuevo.ToString(),h.UsuarioCreacion,h.Observacion)).ToList())).SingleAsync(ct);
}
