using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed partial class DisposicionService(LlantasDbContext db,IOperacionService operaciones,IConfiguration configuration):IDisposicionService
{
    private IQueryable<OrdenServicioLlanta> Ordenes(AlcanceCentros a)=>db.OrdenesServicioLlanta.AsNoTracking()
        .Where(o=>o.Activo&&o.Tipo==TipoServicioLlanta.DisposicionFinal&&(a.VerTodos||a.CentroIds.Contains(o.CentroOrigenId)||a.CentroIds.Contains(o.Llanta.CentroId)));
    private IQueryable<OrdenServicioLlanta> Filtrar(IQueryable<OrdenServicioLlanta> q,ConsultaDisposicion f)
    {
        if(f.LlantaTexto is not null)q=q.Where(o=>(o.Llanta.Codigo+" · "+o.Llanta.Serial).Contains(f.LlantaTexto));
        if(f.CentroTexto is not null)q=q.Where(o=>o.CentroOrigen.Nombre.Contains(f.CentroTexto));
        if(f.EstadoTexto is not null){var term=f.EstadoTexto.Replace(" ","_");q=q.Where(o=>o.Estado.Contains(term));}
        if(f.CentroIds?.Length>0)q=q.Where(o=>f.CentroIds.Contains(o.CentroOrigenId));
        if(f.LlantaIds?.Length>0)q=q.Where(o=>f.LlantaIds.Contains(o.LlantaId));
        if(f.Estados?.Length>0)q=q.Where(o=>f.Estados.Contains(o.Estado));
        if(f.ConEvidencia.HasValue)q=q.Where(o=>o.Evidencias.Any(e=>e.Activo)==f.ConEvidencia.Value);
        if(f.SoloPendientes)q=q.Where(o=>o.Estado!="RECHAZADA"&&o.Estado!="RETORNADA_INVENTARIO"&&o.Estado!="DISPOSICION_FINAL");
        if(f.CentroId.HasValue)q=q.Where(o=>o.CentroOrigenId==f.CentroId||o.Llanta.CentroId==f.CentroId);
        if(!string.IsNullOrWhiteSpace(f.Buscar)){var term=f.Buscar.Trim();q=q.Where(o=>o.Llanta.Codigo.Contains(term)||o.Llanta.Serial.Contains(term)||o.CentroOrigen.Nombre.Contains(term)||db.AsignacionesLlantaPosicion.Any(m=>m.LlantaId==o.LlantaId&&m.EsActiva&&(m.PosicionVehiculo.EjeVehiculo.Vehiculo.Placa.Contains(term)||m.PosicionVehiculo.EjeVehiculo.Vehiculo.NumeroInterno.Contains(term))));}
        if(f.Desde.HasValue)q=q.Where(o=>(o.FechaModificacion??o.FechaCreacion)>=f.Desde);
        if(f.Hasta.HasValue){var end=f.Hasta.Value.TimeOfDay==TimeSpan.Zero?f.Hasta.Value.AddDays(1).AddTicks(-1):f.Hasta.Value;q=q.Where(o=>(o.FechaModificacion??o.FechaCreacion)<=end);}
        if(f.Estado=="LISTAS_R1")q=q.Where(o=>o.Estado=="APROBADA"&&!o.LoteDisposicionFinalId.HasValue);
        else if(f.Estado=="DISPONIBLES_R1")q=Disponibles(q);
        else if(!string.IsNullOrWhiteSpace(f.Estado))q=q.Where(o=>o.Estado==f.Estado);
        return q;
    }
    private IQueryable<OrdenServicioLlanta> Disponibles(IQueryable<OrdenServicioLlanta> q)=>q.Where(o=>o.Estado=="PENDIENTE_DISPOSICION"&&o.FechaRecepcion.HasValue&&o.LoteDisposicionFinalId.HasValue
        &&o.LoteDisposicionFinal!.RelevanciaDestino=="R1"&&o.Llanta.CentroId==o.LoteDisposicionFinal.CentroDestinoId
        &&!db.DespachosDisposicionItems.Any(i=>i.Activo&&i.OrdenId==o.Id)
        &&!db.AsignacionesLlantaPosicion.Any(m=>m.EsActiva&&m.LlantaId==o.LlantaId)&&!db.PosicionesVehiculo.Any(p=>p.LlantaActualId==o.LlantaId));
    private IQueryable<OrdenDisposicionDto> Proyectar(IQueryable<OrdenServicioLlanta> q)=>q.Select(o=>new OrdenDisposicionDto(o.Id,o.LlantaId,o.Llanta.Codigo,o.Llanta.Serial,o.Llanta.Marca.Nombre,o.Llanta.Referencia.Nombre,o.Llanta.Dimension.Nombre,
        new(o.CentroOrigenId,o.CentroOrigen.Nombre),new(o.Llanta.CentroId,o.Llanta.Centro.Nombre),o.Estado,o.Estado,o.FechaModificacion??o.FechaCreacion,o.EvaluadoPor??o.UsuarioCreacion,o.LoteDisposicionFinalId,
        db.DespachosDisposicionItems.Where(i=>i.Activo&&i.OrdenId==o.Id).Select(i=>(Guid?)i.DespachoId).FirstOrDefault(),o.Evidencias.Count(e=>e.Activo)));
    public static string Etiqueta(string state)=>state switch{
        "PENDIENTE_EVALUACION_TECNICA"=>"Pendiente de evaluación", "PENDIENTE_APROBACION"=>"Pendiente de aprobación", "APROBADA"=>"Lista para enviar a R1",
        "EN_TRANSITO_DISPOSICION"=>"En tránsito a R1", "PENDIENTE_DISPOSICION"=>"Recibida; pendiente de disposición", "DISPOSICION_FINAL"=>"Disposición final cerrada",
        "RECHAZADA"=>"Rechazada", "RETORNADA_INVENTARIO"=>"Retornada a inventario", _=>state};
    private async Task<Pagina<OrdenDisposicionDto>> Paginar(IQueryable<OrdenServicioLlanta> q,ConsultaDisposicion f,CancellationToken ct)
    {
        var total=await q.CountAsync(ct);var rows=await Proyectar(Ordenar(q,f).ThenBy(o=>o.Id).Skip((f.Pagina-1)*f.Tamano).Take(f.Tamano)).ToListAsync(ct);
        return new(rows.Select(o=>o with{Etiqueta=o.DespachoId.HasValue&&o.Estado!="DISPOSICION_FINAL"?"Enviada a Sistema Verde":Etiqueta(o.Estado)}).ToArray(),f.Pagina,f.Tamano,total);
    }
    public Task<Pagina<OrdenDisposicionDto>> OrdenesAsync(ConsultaDisposicion f,AlcanceCentros a,CancellationToken ct)=>Paginar(Filtrar(Ordenes(a),f),f,ct);
    public Task<Pagina<OrdenDisposicionDto>> DisponiblesAsync(ConsultaDisposicion f,Guid centroR1Id,AlcanceCentros a,CancellationToken ct)
        =>Paginar(Disponibles(Filtrar(Ordenes(a),f with{Estado=null})).Where(o=>o.Llanta.CentroId==centroR1Id),f,ct);
    public async Task<ResumenDisposicionDto> ResumenAsync(ConsultaDisposicion f,AlcanceCentros a,CancellationToken ct)
    {
        var q=Filtrar(Ordenes(a),f with{Estado=null});var states=await q.GroupBy(o=>o.Estado).Select(g=>new{Estado=g.Key,Total=g.Count()}).ToListAsync(ct);
        int Count(string s)=>states.FirstOrDefault(x=>x.Estado==s)?.Total??0;
        var available=await Disponibles(q).CountAsync(ct);
        var lots=await db.LotesDisposicionFinal.CountAsync(l=>l.Activo&&l.Ordenes.Any(o=>o.Activo&&o.FechaRecepcion.HasValue)&&l.Ordenes.Any(o=>o.Activo&&!o.FechaRecepcion.HasValue)&&q.Any(o=>o.LoteDisposicionFinalId==l.Id),ct);
        var supports=await db.ActasDisposicion.CountAsync(x=>x.Activo&&x.Despacho.Estado!="CERRADO"&&!x.Soportes.Any(s=>s.Activo)&&x.Despacho.Items.Any(i=>i.Orden.CentroOrigenId==x.CentroOrigenId&&q.Any(o=>o.Id==i.OrdenId)),ct);
        ContadorDisposicion C(string key,string label,string unit,int n,string route)=>new(key,label,unit,n,route);
        var counters=new[]{C("EVALUACION","Pendientes de evaluación","llantas",Count("PENDIENTE_EVALUACION_TECNICA"),"/disposicion-final/bandeja?estado=PENDIENTE_EVALUACION_TECNICA"),
            C("APROBACION","Pendientes de aprobación","llantas",Count("PENDIENTE_APROBACION"),"/disposicion-final/bandeja?estado=PENDIENTE_APROBACION"),
            C("LISTAS_R1","Listas para enviar a R1","llantas",await q.CountAsync(o=>o.Estado=="APROBADA"&&!o.LoteDisposicionFinalId.HasValue,ct),"/disposicion-final/lotes/nuevo"),
            C("TRANSITO","En tránsito a R1","llantas",Count("EN_TRANSITO_DISPOSICION"),"/disposicion-final/bandeja?estado=EN_TRANSITO_DISPOSICION"),
            C("PARCIAL","Con recepción parcial","lotes",lots,"/disposicion-final/lotes?estado=RECEPCION_PARCIAL"),
            C("DISPONIBLES","Disponibles en R1","llantas",available,"/disposicion-final/bandeja?estado=DISPONIBLES_R1"),
            C("DESPACHO","Pendientes de enviar a Sistema Verde","llantas",available,"/disposicion-final/despachos/nuevo"),
            C("SOPORTE","Pendientes de soporte firmado","actas",supports,"/disposicion-final/despachos")};
        var pending=await Proyectar(q.Where(o=>o.Estado!="RECHAZADA"&&o.Estado!="RETORNADA_INVENTARIO"&&o.Estado!="DISPOSICION_FINAL").OrderBy(o=>o.FechaModificacion??o.FechaCreacion).ThenBy(o=>o.Id).Take(12)).ToListAsync(ct);
        return new(DateTimeOffset.UtcNow,counters,pending.Select(o=>new AtencionDisposicion(o.Codigo,o.CentroOrigen.Nombre,Etiqueta(o.Estado),o.FechaPendiente,$"/disposicion-final/ordenes/{o.OrdenId}")).ToArray());
    }
    public async Task<DetalleDisposicionDto?> DetalleAsync(Guid id,AlcanceCentros a,CancellationToken ct)
    {
        var row=await Proyectar(Ordenes(a).Where(o=>o.Id==id)).SingleOrDefaultAsync(ct);if(row is null)return null;
        var o=await Ordenes(a).Include(x=>x.Evidencias).SingleAsync(x=>x.Id==id,ct);
        var events=new List<EventoDisposicion>();
        void E(string type,string label,DateTimeOffset? date,string? user,string? note=null,string? reference=null)=>events.Add(new(type,label,date.HasValue?"COMPLETADO":"PENDIENTE",date,user,note,reference));
        E("PROPUESTA","Propuesta",o.FechaOpcionada,o.UsuarioOpciona,o.Motivo);
        E("EVALUACION","Evaluación",o.FechaEvaluacion,o.EvaluadoPor,o.Observaciones);
        E("APROBACION","Aprobación",o.FechaAprobacion,o.Aprobador);
        E("ENVIO","Envío R1",o.FechaEnvio,null,null,o.LoteDisposicionFinalId?.ToString());
        E("RECEPCION","Recepción",o.FechaRecepcion,null);
        var dispatch=row.DespachoId.HasValue?await DespachoAsync(row.DespachoId.Value,a,ct):null;
        E("SISTEMA_VERDE","Sistema Verde",dispatch?.FechaSalida,null,null,dispatch?.Codigo);
        E("CIERRE","Cierre",o.FechaDisposicion,null,o.Estado=="DISPOSICION_FINAL"&&dispatch is null?"Cierre histórico; soporte firmado no registrado":null);
        if(o.Estado is "RECHAZADA" or "RETORNADA_INVENTARIO")events.Add(new("DESENLACE",Etiqueta(o.Estado),o.Estado=="RECHAZADA"?"RECHAZADO":"COMPLETADO",o.FechaModificacion,o.UsuarioModificacion,o.MotivoRechazo??o.Observaciones,null));
        else
        {
            var currentType=o.Estado switch{"PENDIENTE_EVALUACION_TECNICA"=>"EVALUACION","PENDIENTE_APROBACION"=>"APROBACION","APROBADA"=>"ENVIO","EN_TRANSITO_DISPOSICION"=>"RECEPCION","PENDIENTE_DISPOSICION"=>row.DespachoId.HasValue?"CIERRE":"SISTEMA_VERDE",_=>null};
            var current=events.FindIndex(e=>e.Tipo==currentType);
            for(var i=0;i<events.Count;i++)if(!events[i].Fecha.HasValue&&(o.Estado=="DISPOSICION_FINAL"||i<current))events[i]=events[i] with{EstadoVisual="SIN_REGISTRO"};
            if(current>=0)events[current]=events[current] with{EstadoVisual="ACTUAL"};
        }
        var actions=o.Estado switch{"PENDIENTE_EVALUACION_TECNICA"=>new[]{"EVALUAR"},"PENDIENTE_APROBACION"=>["APROBAR","RECHAZAR"],"APROBADA"=>["ENVIAR"],"EN_TRANSITO_DISPOSICION"=>["RECIBIR"],"PENDIENTE_DISPOSICION"=>row.DespachoId.HasValue?["VER_DESPACHO"]:["DESPACHAR"],_=>Array.Empty<string>()};
        return new(row with{Etiqueta=row.DespachoId.HasValue&&o.Estado!="DISPOSICION_FINAL"?"Enviada a Sistema Verde":Etiqueta(o.Estado)},o.Motivo,o.Observaciones,o.Resultado,events,o.Evidencias.Where(e=>e.Activo).Select(e=>new EvidenciaDisposicion(e.Id,e.NombreArchivo,e.MimeType,e.TamanoBytes,e.FechaCreacion)).ToArray(),actions);
    }
    public async Task<LoteDisposicionDetalleDto?> LoteAsync(Guid id,AlcanceCentros a,CancellationToken ct)
    {
        var l=await db.LotesDisposicionFinal.AsNoTracking().Include(x=>x.CentroOrigen).Include(x=>x.CentroDestino).SingleOrDefaultAsync(x=>x.Id==id&&x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.CentroOrigenId)||a.CentroIds.Contains(x.CentroDestinoId)),ct);if(l is null)return null;
        var q=Ordenes(a).Where(o=>o.LoteDisposicionFinalId==id);var items=await Proyectar(q.OrderBy(o=>o.Llanta.Codigo)).ToListAsync(ct);
        var received=await q.CountAsync(o=>o.FechaRecepcion.HasValue,ct);
        var notes=await db.NovedadesDisposicion.AsNoTracking().Where(n=>n.Activo&&n.LoteId==id).OrderByDescending(n=>n.FechaCreacion).Select(n=>new NovedadDisposicionDto(n.Id,n.OrdenId,n.Observacion,n.FechaCreacion,n.UsuarioCreacion,n.FechaResolucion,n.ResueltaPor,n.Resolucion)).ToListAsync(ct);
        return new(l.Id,l.Codigo,new(l.CentroOrigenId,l.CentroOrigen.Nombre),new(l.CentroDestinoId,l.CentroDestino.Nombre),l.Estado,l.FechaSalida,l.Transportador,l.Placa,l.Remision,l.Observaciones,items.Select(o=>o with{Etiqueta=o.DespachoId.HasValue&&o.Estado!="DISPOSICION_FINAL"?"Enviada a Sistema Verde":Etiqueta(o.Estado)}).ToArray(),notes,items.Count,received,items.Count-received,false,a.Autoriza(l.CentroDestinoId)&&items.Any(o=>o.Estado=="EN_TRANSITO_DISPOSICION"));
    }
    public async Task<Pagina<LoteDisposicionDetalleDto>> LotesAsync(ConsultaDisposicion f,AlcanceCentros a,CancellationToken ct)
    {
        var q=db.LotesDisposicionFinal.AsNoTracking().Where(l=>l.Activo&&(a.VerTodos||a.CentroIds.Contains(l.CentroOrigenId)||a.CentroIds.Contains(l.CentroDestinoId)));
        if(f.CentroId.HasValue)q=q.Where(l=>l.CentroOrigenId==f.CentroId||l.CentroDestinoId==f.CentroId);
        if(f.Estado=="RECEPCION_PARCIAL")q=q.Where(l=>l.Ordenes.Any(o=>o.Activo&&o.FechaRecepcion.HasValue)&&l.Ordenes.Any(o=>o.Activo&&!o.FechaRecepcion.HasValue));
        else if(!string.IsNullOrWhiteSpace(f.Estado))q=q.Where(l=>l.Estado==f.Estado);
        if(!string.IsNullOrWhiteSpace(f.Buscar))q=q.Where(l=>l.Codigo.Contains(f.Buscar));
        var total=await q.CountAsync(ct);var ids=await q.OrderByDescending(l=>l.FechaSalida).ThenBy(l=>l.Id).Skip((f.Pagina-1)*f.Tamano).Take(f.Tamano).Select(l=>l.Id).ToListAsync(ct);
        var rows=new List<LoteDisposicionDetalleDto>();foreach(var id in ids)rows.Add((await LoteAsync(id,a,ct))!);return new(rows,f.Pagina,f.Tamano,total);
    }
}



