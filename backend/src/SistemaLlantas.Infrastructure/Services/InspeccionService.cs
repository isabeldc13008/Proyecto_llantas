using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class InspeccionService(LlantasDbContext db) : IInspeccionService
{
    public async Task<IReadOnlyList<VehiculoInspeccionDto>> ObtenerVehiculosAsync(string usuario, bool soloAsignados,string? buscar, AlcanceCentros alcance, CancellationToken ct, bool permitirVehiculosGlobales = false)
    {
        var q = db.Vehiculos.AsNoTracking().Where(x => x.Activo && (permitirVehiculosGlobales || alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)));
        if (soloAsignados && !permitirVehiculosGlobales)
            q = q.Where(x => db.ActividadesProgramadas.Any(a => (a.TecnicoId == usuario || a.TecnicoId == usuario + ".local") && a.VehiculoId == x.Id && a.Estado != EstadoActividad.Cancelada && a.Estado != EstadoActividad.Cumplida));
        if(!string.IsNullOrWhiteSpace(buscar)){var s=buscar.Trim();q=q.Where(x=>x.NumeroInterno.Contains(s)||x.Placa.Contains(s)||x.Tipo.Contains(s)||x.Centro.Nombre.Contains(s));}
        return await q.OrderBy(x => x.NumeroInterno).Select(x => new VehiculoInspeccionDto(x.Id, x.NumeroInterno, x.Placa, x.Tipo, x.CentroId, x.Centro.Codigo, x.Centro.Nombre,x.Centro.Regional!=null?x.Centro.Regional.Nombre:null)).ToListAsync(ct);
    }

    public async Task<OpcionesInspeccionDto> ObtenerOpcionesAsync(CancellationToken ct) => new(
        await db.CondicionesLlanta.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new OpcionInspeccionDto(x.Id, x.Codigo, x.Nombre)).ToListAsync(ct),
        await db.CausasLlanta.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new OpcionInspeccionDto(x.Id, x.Codigo, x.Nombre)).ToListAsync(ct),
        await db.RecomendacionesInspeccion.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).Select(x => new OpcionInspeccionDto(x.Id, x.Codigo, x.Nombre)).ToListAsync(ct));

    public async Task<ContextoInspeccionDto?> ObtenerContextoAsync(Guid vehiculoId, AlcanceCentros alcance, CancellationToken ct, bool permitirVehiculoGlobal = false)
    {
        var v = await db.Vehiculos.AsNoTracking().Include(x => x.Centro).ThenInclude(x=>x.Regional)
            .Include(x => x.Ejes).ThenInclude(x => x.Posiciones)
            .SingleOrDefaultAsync(x => x.Id == vehiculoId && (permitirVehiculoGlobal || alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct);
        if(v is null)return null;var positionIds=v.Ejes.SelectMany(x=>x.Posiciones).Select(x=>x.Id).ToArray();var active=await db.AsignacionesLlantaPosicion.AsNoTracking().Where(x=>positionIds.Contains(x.PosicionVehiculoId)&&x.EsActiva).Include(x=>x.Llanta).ThenInclude(x=>x.EstadoLlanta).Include(x=>x.Llanta).ThenInclude(x=>x.Marca).Include(x=>x.Llanta).ThenInclude(x=>x.Referencia).Include(x=>x.Llanta).ThenInclude(x=>x.Dimension).ToDictionaryAsync(x=>x.PosicionVehiculoId,ct);var last=await db.Inspecciones.AsNoTracking().Where(x=>x.VehiculoId==v.Id&&x.Estado==EstadoInspeccion.Finalizada).MaxAsync(x=>(DateTimeOffset?)x.FechaCreacion,ct);return new(v.Id, v.NumeroInterno, v.Placa, v.Tipo, v.CentroId, v.Centro.Nombre, v.Centro.Relevancia,v.Centro.Regional!=null?v.Centro.Regional.Nombre:null,v.Kilometraje,last,
            v.Ejes.OrderBy(e => e.Numero).Select(e => new EjeInspeccionDto(e.Id, e.Numero, e.Nombre,
                e.Posiciones.OrderBy(p => p.Orden).Select(p => new PosicionInspeccionDto(p.Id, p.Codigo, p.Lado, p.Orden,active.TryGetValue(p.Id,out var a)?new(a.Llanta.Id,a.Llanta.Codigo,a.Llanta.EstadoLlanta.Nombre,a.Llanta.Marca.Nombre,a.Llanta.Referencia.Nombre,a.Llanta.Dimension.Nombre):null)).ToList())).ToList());
    }

    public async Task<InspeccionDto> CrearAsync(CrearInspeccionDto dto, string usuario, AlcanceCentros alcance, CancellationToken ct, bool permitirVehiculoGlobal = false)
    {
        if(!dto.Kilometraje.HasValue)throw new ValidacionException("Ingresa el kilometraje actual para continuar.");
        var vehiculo = await db.Vehiculos.Include(x => x.Ejes).ThenInclude(x => x.Posiciones)
            .SingleOrDefaultAsync(x => x.Id == dto.VehiculoId && x.Activo && (permitirVehiculoGlobal || alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId)), ct)
            ?? throw new KeyNotFoundException("Vehículo no encontrado o fuera del centro autorizado.");
        if(vehiculo.Kilometraje.HasValue&&dto.Kilometraje.Value<vehiculo.Kilometraje.Value)throw new ValidacionException($"El kilometraje actual no puede ser inferior al último kilometraje registrado ({vehiculo.Kilometraje:0} km).");
        vehiculo.Kilometraje=dto.Kilometraje;
        var inspeccion = new Inspeccion { VehiculoId = vehiculo.Id, CentroId = vehiculo.CentroId, Kilometraje = dto.Kilometraje,
            TecnicoId = usuario, Observaciones = dto.Observaciones, UsuarioCreacion = usuario };
        var positions=vehiculo.Ejes.SelectMany(x=>x.Posiciones).ToList();var ids=positions.Select(x=>x.Id).ToArray();var mounted=await db.AsignacionesLlantaPosicion.Where(x=>ids.Contains(x.PosicionVehiculoId)&&x.EsActiva).ToDictionaryAsync(x=>x.PosicionVehiculoId,x=>x.LlantaId,ct);foreach (var posicion in positions){var hasTire=mounted.TryGetValue(posicion.Id,out var tireId);inspeccion.Detalles.Add(new InspeccionDetalle { PosicionVehiculoId = posicion.Id, LlantaId = hasTire?tireId:null, UsuarioCreacion = usuario });}
        db.Inspecciones.Add(inspeccion); await db.SaveChangesAsync(ct);
        return (await ObtenerAsync(inspeccion.Id, alcance, ct))!;
    }

    public async Task<InspeccionDto?> ObtenerAsync(Guid id, AlcanceCentros alcance, CancellationToken ct, string? usuario = null, bool permitirPropia = false)
    {
        var item = await ConsultaCompleta().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && (alcance.VerTodos || alcance.CentroIds.Contains(x.CentroId) || (permitirPropia&&x.TecnicoId==usuario)), ct);
        if(item is null)return null;var inconsistencias=await db.InconsistenciasInspeccion.AsNoTracking().Where(x=>x.InspeccionId==id).Select(x=>new InconsistenciaPosicionDto(x.Id,x.PosicionVehiculoId,x.LlantaEncontradaId,x.IdentificadorEncontrado,x.Estado.ToString(),x.Observacion)).ToListAsync(ct);return Map(item,inconsistencias);
    }

    public async Task<InspeccionDto?> GuardarDetalleAsync(Guid id, Guid posicionId, GuardarDetalleInspeccionDto dto, string usuario, CancellationToken ct)
    {
        var detalle = await db.InspeccionesDetalle.Include(x => x.Inspeccion).SingleOrDefaultAsync(x => x.InspeccionId == id && x.PosicionVehiculoId == posicionId && x.Inspeccion.TecnicoId == usuario, ct);
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
        var inspeccion = await db.Inspecciones.Include(x => x.Vehiculo).ThenInclude(x => x.Centro).SingleOrDefaultAsync(x => x.Id == inspeccionId && x.TecnicoId == usuario, ct)
            ?? throw new KeyNotFoundException("Inspección no encontrada.");
        var posicion = await db.PosicionesVehiculo.Include(x => x.LlantaActual).SingleOrDefaultAsync(x => x.Id == dto.PosicionId, ct)
            ?? throw new KeyNotFoundException("Posición no encontrada.");
        if (!await db.InspeccionesDetalle.AnyAsync(x => x.InspeccionId == inspeccionId && x.PosicionVehiculoId == dto.PosicionId, ct))
            throw new InvalidOperationException("La posición no pertenece a la inspección.");
        if(dto.LlantaEncontradaId.HasValue==dto.LlantaNoEncontrada)throw new ValidacionException("Selecciona una llanta existente o marca que no fue encontrada en el sistema.");
        Llanta? encontrada=null;if(dto.LlantaEncontradaId.HasValue)encontrada=await db.Llantas.SingleOrDefaultAsync(x=>x.Id==dto.LlantaEncontradaId.Value&&x.Activo,ct)??throw new KeyNotFoundException("La llanta encontrada no existe.");
        if(dto.LlantaNoEncontrada&&string.IsNullOrWhiteSpace(dto.IdentificadorEncontrado))throw new ValidacionException("Ingresa la identificación encontrada en la llanta.");
        var identificador=encontrada?.Codigo??dto.IdentificadorEncontrado.Trim().ToUpperInvariant();
        var novedad = new InconsistenciaInspeccion { InspeccionId = inspeccionId, PosicionVehiculoId = posicion.Id, LlantaEsperadaId = posicion.LlantaActualId,LlantaEncontradaId=encontrada?.Id,
            IdentificadorEncontrado = identificador, TecnicoId = usuario, Observacion = dto.Observacion?.Trim()??string.Empty, UsuarioCreacion = usuario };
        if(dto.LlantaNoEncontrada)novedad.LlantaTemporal = new LlantaTemporal { IdentificadorTemporal = $"TMP-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..29],IdentificadorFisico = novedad.IdentificadorEncontrado, UsuarioCreacion = usuario };
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

    public async Task<InconsistenciaDto> ResolverAsync(Guid id, ResolverInconsistenciaDto dto, bool autorizar, string usuario, bool puedeAutorizarPropia, CancellationToken ct, AlcanceCentros? alcance = null)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var item = await db.InconsistenciasInspeccion.Include(x => x.Inspeccion).ThenInclude(x=>x.Vehiculo).Include(x => x.PosicionVehiculo).ThenInclude(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).Include(x => x.LlantaTemporal)
            .SingleOrDefaultAsync(x => x.Id == id && (alcance==null||alcance.VerTodos||alcance.CentroIds.Contains(x.Inspeccion.CentroId)), ct) ?? throw new KeyNotFoundException("Solicitud no encontrada o fuera de los centros autorizados.");
        if (item.Estado != EstadoInconsistencia.PendienteAutorizacion) throw new InvalidOperationException("La solicitud ya fue resuelta.");
        if (item.TecnicoId == usuario && !puedeAutorizarPropia) throw new UnauthorizedAccessException("El técnico no puede autorizar su propia solicitud.");
        item.Estado = autorizar ? EstadoInconsistencia.Autorizada : EstadoInconsistencia.Rechazada; item.UsuarioAutorizador = usuario;
        item.FechaAutorizacion = DateTimeOffset.UtcNow; item.ObservacionAutorizacion = dto.Observacion; item.UsuarioModificacion = usuario;
        if(item.LlantaTemporal is not null){item.LlantaTemporal.Estado=item.Estado;item.LlantaTemporal.UsuarioModificacion=usuario;}
        if (autorizar)
        {
            var llantaAprobadaId=dto.LlantaInventarioId??item.LlantaEncontradaId;if(!llantaAprobadaId.HasValue)throw new InvalidOperationException("Para autorizar debe seleccionar la llanta validada del inventario.");
            var nueva = await db.Llantas.SingleOrDefaultAsync(x => x.Id == llantaAprobadaId, ct) ?? throw new KeyNotFoundException("Llanta de inventario no encontrada.");
            var vigente=await db.AsignacionesLlantaPosicion.Include(x=>x.Llanta).SingleOrDefaultAsync(x=>x.PosicionVehiculoId==item.PosicionVehiculoId&&x.EsActiva,ct);var anteriorId=vigente?.LlantaId??item.PosicionVehiculo.LlantaActualId;var otra=await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==nueva.Id&&x.EsActiva&&x.PosicionVehiculoId!=item.PosicionVehiculoId,ct);if(otra)throw new ConflictoException("La llanta reportada tiene otro montaje activo y requiere conciliación previa.");
            var movimiento=new Movimiento{Numero=$"MOV-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..28],Tipo="Corrección por inspección",Motivo=dto.Observacion,CentroId=item.Inspeccion.CentroId,InspeccionId=item.InspeccionId,Usuario=usuario,UsuarioCreacion=usuario,Observaciones=$"Inconsistencia {item.Id}. Valor anterior: {vigente?.Llanta.Codigo??"Vacante"}. Valor aprobado: {nueva.Codigo}."};
            if(vigente is not null&&vigente.LlantaId!=nueva.Id){if(item.Inspeccion.Kilometraje.HasValue&&vigente.KilometrajeMontaje.HasValue&&item.Inspeccion.Kilometraje<vigente.KilometrajeMontaje)throw new ValidacionException("El kilometraje de la inspección es inferior al kilometraje de montaje.");vigente.EsActiva=false;vigente.FechaFin=DateTimeOffset.UtcNow;vigente.KilometrajeDesmontaje=item.Inspeccion.Kilometraje;vigente.KilometrajeRecorrido=item.Inspeccion.Kilometraje.HasValue&&vigente.KilometrajeMontaje.HasValue?item.Inspeccion.Kilometraje-vigente.KilometrajeMontaje:null;vigente.UsuarioModificacion=usuario;if(vigente.KilometrajeRecorrido.HasValue)vigente.Llanta.KilometrajeAcumulado+=vigente.KilometrajeRecorrido.Value;vigente.Llanta.UbicacionActual="Inventario · corrección autorizada";var disponible=await db.EstadosLlanta.Where(x=>x.Codigo=="DISPONIBLE").Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct);if(disponible.HasValue)vigente.Llanta.EstadoLlantaId=disponible.Value;movimiento.Detalles.Add(new(){LlantaId=vigente.LlantaId,PosicionOrigenId=item.PosicionVehiculoId,TipoDestino=TipoDestinoLlanta.Inventario,DestinoDescripcion="Inventario por corrección autorizada",UsuarioCreacion=usuario});}
            if(vigente?.LlantaId!=nueva.Id){movimiento.Detalles.Add(new(){LlantaId=nueva.Id,PosicionDestinoId=item.PosicionVehiculoId,TipoDestino=TipoDestinoLlanta.Posicion,DestinoDescripcion=item.PosicionVehiculo.Codigo,UsuarioCreacion=usuario});db.Movimientos.Add(movimiento);await db.SaveChangesAsync(ct);db.AsignacionesLlantaPosicion.Add(new(){LlantaId=nueva.Id,PosicionVehiculoId=item.PosicionVehiculoId,MovimientoOrigenId=movimiento.Id,KilometrajeMontaje=item.Inspeccion.Kilometraje,UsuarioCreacion=usuario});item.PosicionVehiculo.LlantaActualId=nueva.Id;nueva.CentroId=item.Inspeccion.CentroId;nueva.UbicacionActual=$"{item.Inspeccion.Vehiculo.NumeroInterno} · {item.PosicionVehiculo.Codigo}";var montada=await db.EstadosLlanta.Where(x=>x.Codigo=="MONTADA").Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct);if(montada.HasValue)nueva.EstadoLlantaId=montada.Value;}
            db.MovimientosLlanta.Add(new MovimientoLlanta { InspeccionId = item.InspeccionId, InconsistenciaInspeccionId = item.Id,
                PosicionVehiculoId = item.PosicionVehiculoId, LlantaAnteriorId = anteriorId, LlantaNuevaId = nueva.Id,
                CentroId = item.Inspeccion.CentroId, Motivo = "Regularización de inconsistencia detectada en inspección", TecnicoReporta = item.TecnicoId,
                UsuarioAutoriza = usuario, FechaReporte = item.FechaCreacion, FechaAutorizacion = item.FechaAutorizacion.Value, Observaciones = dto.Observacion, UsuarioCreacion = usuario });
            item.PosicionVehiculo.UsuarioModificacion = usuario; item.PosicionVehiculo.FechaModificacion = DateTimeOffset.UtcNow;
            if(item.LlantaTemporal is not null)item.LlantaTemporal.Estado = EstadoInconsistencia.Regularizada;
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return await MapInconsistencia(id, ct);
    }

    private IQueryable<Inspeccion> ConsultaCompleta() => db.Inspecciones.Include(x => x.Vehiculo).Include(x => x.Centro)
        .Include(x => x.Detalles).ThenInclude(x => x.PosicionVehiculo).Include(x => x.Detalles).ThenInclude(x => x.Llanta);
    private static InspeccionDto Map(Inspeccion x,IReadOnlyList<InconsistenciaPosicionDto> inconsistencias) => new(x.Id, x.VehiculoId, $"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}", x.CentroId, x.Centro.Nombre,
        x.Kilometraje, x.Estado.ToString(), x.TecnicoId, x.Detalles.OrderBy(d => d.PosicionVehiculo.Orden).Select(d => new DetalleInspeccionDto(d.Id,d.PosicionVehiculoId,d.PosicionVehiculo.Codigo,d.LlantaId,d.Llanta?.Codigo,d.ProfundidadExterior,d.ProfundidadCentro,d.ProfundidadInterior,d.CondicionLlantaId,d.CausaLlantaId,d.RecomendacionId,d.Observaciones)).ToList(),inconsistencias);
    private async Task<InconsistenciaDto> MapInconsistencia(Guid id, CancellationToken ct) => await db.InconsistenciasInspeccion.AsNoTracking()
        .Where(x => x.Id == id).Select(x => new InconsistenciaDto(x.Id,x.InspeccionId,x.Inspeccion.Centro.Nombre,$"Interno {x.Inspeccion.Vehiculo.NumeroInterno} - {x.Inspeccion.Vehiculo.Placa}",x.PosicionVehiculo.Codigo,x.LlantaEsperada != null ? x.LlantaEsperada.Codigo : null,x.LlantaEncontradaId,x.IdentificadorEncontrado,x.TecnicoId,x.FechaCreacion,x.Estado.ToString(),x.UsuarioAutorizador,x.ObservacionAutorizacion,x.Observacion,db.EvidenciasInspeccion.Where(e=>e.InconsistenciaInspeccionId==x.Id&&e.Activo).Select(e=>new EvidenciaDto(e.Id,e.NombreArchivo,e.MimeType,e.TamanoBytes,e.Hash,e.FechaCreacion,e.Activo)).ToList())).SingleAsync(ct);
    public async Task<IReadOnlyList<AlertaDto>> AlertasAsync(AlcanceCentros alcance,CancellationToken ct){var ids=await db.AlertasInspeccion.AsNoTracking().Where(x=>alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)).OrderByDescending(x=>x.FechaCreacion).Select(x=>x.Id).ToListAsync(ct);var result=new List<AlertaDto>();foreach(var id in ids)result.Add(await MapAlerta(id,ct));return result;}
    public async Task<AlertaDto> CambiarAlertaAsync(Guid id,CambiarAlertaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct){if(!Enum.TryParse<EstadoAlerta>(dto.Estado,true,out var state))throw new ValidacionException("Estado de alerta no válido.");var item=await db.AlertasInspeccion.SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Alerta no encontrada.");var previous=item.Estado;item.Estado=state;item.UsuarioModificacion=usuario;item.FechaModificacion=DateTimeOffset.UtcNow;db.AlertasHistorial.Add(new(){AlertaInspeccionId=id,EstadoAnterior=previous,EstadoNuevo=state,Observacion=dto.Observacion,UsuarioCreacion=usuario});await db.SaveChangesAsync(ct);return await MapAlerta(id,ct);}
    private Task<AlertaDto> MapAlerta(Guid id,CancellationToken ct)=>db.AlertasInspeccion.AsNoTracking().Where(x=>x.Id==id).Select(x=>new AlertaDto(x.Id,x.Tipo,x.Descripcion,x.Estado.ToString(),x.FechaCreacion,x.InspeccionId,x.Inspeccion.Vehiculo.NumeroInterno+" · "+x.Inspeccion.Vehiculo.Placa,x.Inspeccion.Centro.Nombre,x.InspeccionDetalle.PosicionVehiculo.Codigo,x.InspeccionDetalle.Llanta!=null?x.InspeccionDetalle.Llanta.Codigo:null,x.Historial.OrderBy(h=>h.FechaCreacion).Select(h=>new AlertaEventoDto(h.FechaCreacion,h.EstadoAnterior.ToString(),h.EstadoNuevo.ToString(),h.UsuarioCreacion,h.Observacion)).ToList())).SingleAsync(ct);
    public async Task<ResumenInspeccionesDto> ResumenAsync(string usuario,bool soloPropias,AlcanceCentros alcance,CancellationToken ct){var today=DateTimeOffset.UtcNow.Date;var tomorrow=today.AddDays(1);var q=db.Inspecciones.AsNoTracking().Where(x=>soloPropias?x.TecnicoId==usuario:(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)));return new(await q.CountAsync(x=>x.Estado==EstadoInspeccion.Borrador&&x.FechaCreacion>=today&&x.FechaCreacion<tomorrow,ct),await q.CountAsync(x=>x.Estado==EstadoInspeccion.Finalizada&&x.FechaModificacion>=today&&x.FechaModificacion<tomorrow,ct),await q.CountAsync(x=>x.FechaCreacion>=today&&x.FechaCreacion<tomorrow&&x.Detalles.Any(d=>d.CausaLlantaId!=null||d.Observaciones!=null),ct),await db.AlertasInspeccion.AsNoTracking().CountAsync(x=>x.FechaCreacion>=today&&x.FechaCreacion<tomorrow&&(soloPropias?x.Inspeccion.TecnicoId==usuario:(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId))),ct));}
    public async Task<IReadOnlyList<HistorialInspeccionDto>> HistorialAsync(string usuario,bool soloPropias,AlcanceCentros alcance,CancellationToken ct){var q=db.Inspecciones.AsNoTracking().Where(x=>soloPropias?x.TecnicoId==usuario:(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)));return await q.OrderByDescending(x=>x.FechaCreacion).Take(200).Select(x=>new HistorialInspeccionDto(x.Id,x.FechaCreacion,x.Vehiculo.Placa,x.Vehiculo.NumeroInterno,x.Centro.Nombre,x.TecnicoId,x.Kilometraje,x.Detalles.Count(d=>d.LlantaId!=null&&d.CondicionLlantaId!=null),x.Detalles.Count(d=>d.CausaLlantaId!=null||d.Observaciones!=null),db.AlertasInspeccion.Count(a=>a.InspeccionId==x.Id),x.Estado.ToString())).ToListAsync(ct);}
    public async Task<InspeccionDto> FinalizarAsync(Guid id,string usuario,CancellationToken ct){var item=await db.Inspecciones.Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==id&&x.TecnicoId==usuario,ct)??throw new KeyNotFoundException("Inspección no encontrada.");if(item.Detalles.Any(x=>x.LlantaId!=null&&(!x.CondicionLlantaId.HasValue||!x.RecomendacionId.HasValue)))throw new ValidacionException("Hay posiciones con llanta pendientes por inspeccionar.");item.Estado=EstadoInspeccion.Finalizada;item.UsuarioModificacion=usuario;item.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return (await ObtenerAsync(id,new(true,[]),ct))!;}
    public async Task<IReadOnlyList<LlantaBusquedaInspeccionDto>> BuscarLlantaExactaAsync(string termino,CancellationToken ct){if(string.IsNullOrWhiteSpace(termino)||termino.Trim().Length<2)return[];var value=termino.Trim();var guid=Guid.TryParse(value,out var id)?id:Guid.Empty;return await db.Llantas.AsNoTracking().Where(x=>x.Activo&&(x.Codigo==value||x.Serial==value||x.Id==guid)).Take(5).Select(x=>new LlantaBusquedaInspeccionDto(x.Id,x.Codigo,x.Serial,x.Marca.Nombre,x.Referencia.Nombre,x.Dimension.Nombre,x.EstadoLlanta.Nombre)).ToListAsync(ct);}
}
