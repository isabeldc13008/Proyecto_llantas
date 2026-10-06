using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class DisposicionService
{
    private static void Required(string? value,int max,string name){if(string.IsNullOrWhiteSpace(value)||value.Length>max)throw new ValidacionException($"{name} es obligatorio (máximo {max} caracteres).");}
    private static void Selection(IReadOnlyList<Guid>? ids){if(ids is null||ids.Count is <1 or >500||ids.Contains(Guid.Empty)||ids.Distinct().Count()!=ids.Count)throw new ValidacionException("Selecciona entre 1 y 500 órdenes, sin repetir.");}
    public async Task<DespachoDisposicionDto> CrearDespachoAsync(CrearDespachoDisposicionDto dto,string user,AlcanceCentros a,CancellationToken ct)
    {
        Selection(dto.OrdenIds);Required(dto.Transportador,150,"Transportador");Required(dto.Placa,20,"Placa");Required(dto.IdempotencyKey,100,"Clave de operación");
        if(dto.FechaSalida==default||(dto.Remision?.Length??0)>100||(dto.Observaciones?.Length??0)>1000)throw new ValidacionException("Fecha, remisión u observaciones inválidas.");
        if(!a.Autoriza(dto.CentroR1Id))throw new UnauthorizedAccessException("Centro R1 fuera de alcance.");
        var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto with{OrdenIds=dto.OrdenIds.Order().ToArray()}))));
        var id=await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var prior=await db.DespachosDisposicion.SingleOrDefaultAsync(d=>d.IdempotencyKey==dto.IdempotencyKey,ct);
            if(prior is not null){if(prior.CentroR1Id!=dto.CentroR1Id||prior.SolicitudHash!=hash)throw new ConflictoException("La clave corresponde a un despacho distinto.");await tx.CommitAsync(ct);return prior.Id;}
            var r1=await db.Centros.SingleOrDefaultAsync(c=>c.Id==dto.CentroR1Id&&c.Activo&&c.Relevancia=="R1",ct)??throw new ValidacionException("Selecciona un centro R1 activo.");
            var provider=await db.ProveedoresServicio.SingleOrDefaultAsync(p=>p.Id==dto.ProveedorId&&p.Activo&&(p.Tipo=="DisposicionFinal"||p.Tipo==""),ct)??throw new ValidacionException("Selecciona un proveedor receptor activo de disposición final.");
            var orders=await db.OrdenesServicioLlanta.Include(o=>o.Llanta).ThenInclude(t=>t.Marca).Include(o=>o.Llanta).ThenInclude(t=>t.Dimension).Include(o=>o.CentroOrigen).Include(o=>o.LoteDisposicionFinal)
                .Where(o=>dto.OrdenIds.Contains(o.Id)).ToListAsync(ct);
            if(orders.Count!=dto.OrdenIds.Count||orders.Any(o=>!o.Activo||!o.Llanta.Activo||o.Tipo!=TipoServicioLlanta.DisposicionFinal||o.Estado!="PENDIENTE_DISPOSICION"||o.Resultado!="DISPOSICION"||!o.FechaRecepcion.HasValue||o.LoteDisposicionFinal?.CentroDestinoId!=r1.Id||o.Llanta.CentroId!=r1.Id))throw new ConflictoException("Solo pueden enviarse llantas recibidas físicamente y disponibles en este R1.");
            if(orders.Select(o=>o.LlantaId).Distinct().Count()!=orders.Count)throw new ConflictoException("Una llanta no puede repetirse en el despacho.");
            if(await db.DespachosDisposicionItems.AnyAsync(i=>i.Activo&&dto.OrdenIds.Contains(i.OrdenId),ct))throw new ConflictoException("Una orden ya pertenece a un despacho.");
            await ValidarDesmontadas(orders.Select(o=>o.LlantaId).ToArray(),ct);
            var d=new DespachoDisposicion{CentroR1=r1,Proveedor=provider,FechaSalida=dto.FechaSalida,Transportador=dto.Transportador.Trim(),Placa=dto.Placa.Trim(),Remision=dto.Remision,Observaciones=dto.Observaciones,IdempotencyKey=dto.IdempotencyKey,SolicitudHash=hash,UsuarioCreacion=user};
            d.Codigo=$"SV-{DateTimeOffset.UtcNow.Year}-{d.Id:N}";
            foreach(var o in orders)d.Items.Add(new(){Orden=o,LoteEntrada=o.LoteDisposicionFinal!,UsuarioCreacion=user});
            foreach(var group in orders.GroupBy(o=>o.CentroOrigenId))
            {
                var acta=new ActaDisposicion{CentroOrigenId=group.Key,UsuarioCreacion=user};acta.Codigo=$"ACT-{DateTimeOffset.UtcNow.Year}-{acta.Id:N}";
                acta.SnapshotJson=JsonSerializer.Serialize(new ActaSnapshot(1,acta.Codigo,acta.FechaCreacion,d.FechaSalida,group.First().CentroOrigen.Nombre,r1.Nombre,provider.Nombre,d.Transportador,d.Placa,d.Remision,
                    group.OrderBy(o=>o.Llanta.Codigo).Select(o=>new ActaLlanta(o.Id,o.Llanta.Codigo,o.Llanta.Serial,o.Llanta.Marca.Nombre,o.Llanta.Dimension.Nombre,o.LoteDisposicionFinal!.Codigo)).ToArray()));
                d.Actas.Add(acta);
            }
            db.DespachosDisposicion.Add(d);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return d.Id;
        });
        return (await DespachoAsync(id,a,ct))!;
    }
    private Task ValidarDesmontadas(Guid[] tires,CancellationToken ct)=>ValidateMounted(tires,ct);
    // Both representations are checked because a stale active assignment must not authorize dispatch.
    private async Task ValidateMounted(Guid[] tires,CancellationToken ct)
    {
        if(await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.EsActiva&&tires.Contains(x.LlantaId),ct)||await db.PosicionesVehiculo.AnyAsync(p=>p.LlantaActualId.HasValue&&tires.Contains(p.LlantaActualId.Value),ct))throw new ConflictoException("Una llanta continúa montada.");
    }
    private IQueryable<DespachoDisposicion> Despachos(AlcanceCentros a)=>db.DespachosDisposicion.AsNoTracking().Where(d=>d.Activo&&(a.VerTodos||a.CentroIds.Contains(d.CentroR1Id)||d.Actas.Any(x=>a.CentroIds.Contains(x.CentroOrigenId))));
    public async Task<DespachoDisposicionDto?> DespachoAsync(Guid id,AlcanceCentros a,CancellationToken ct)
    {
        var d=await Despachos(a).Include(x=>x.CentroR1).Include(x=>x.Proveedor).SingleOrDefaultAsync(x=>x.Id==id,ct);if(d is null)return null;
        var full=a.Autoriza(d.CentroR1Id);
        var actas=await db.ActasDisposicion.AsNoTracking().Include(x=>x.Soportes).Where(x=>x.Activo&&x.DespachoId==id&&(full||a.CentroIds.Contains(x.CentroOrigenId))).OrderBy(x=>x.Codigo).ToListAsync(ct);
        var items=await Proyectar(Ordenes(a).Where(o=>db.DespachosDisposicionItems.Any(i=>i.Activo&&i.DespachoId==id&&i.OrdenId==o.Id)&&(full||a.CentroIds.Contains(o.CentroOrigenId)))).ToListAsync(ct);
        var reasons=new List<string>();if(!full)reasons.Add("Solo el centro R1 puede gestionar este despacho.");if(d.Estado=="CERRADO")reasons.Add("El despacho ya está cerrado.");
        if(!actas.Any()||actas.Any(x=>!x.Soportes.Any(s=>s.Activo)))reasons.Add("Falta el soporte firmado de una o más actas.");
        return new(d.Id,d.Codigo,new(d.CentroR1Id,d.CentroR1.Nombre),new(d.ProveedorId,d.Proveedor.Nombre),d.FechaSalida,d.Transportador,d.Placa,d.Remision,d.Estado,Convert.ToBase64String(d.RowVersion),items.Select(o=>o with{Etiqueta=Etiqueta(o.Estado)}).ToArray(),
            actas.Select(x=>new ActaDisposicionDto(x.Id,x.Codigo,x.CentroOrigenId,JsonSerializer.Deserialize<ActaSnapshot>(x.SnapshotJson)!,x.Soportes.Where(s=>s.Activo).OrderByDescending(s=>s.FechaCreacion).Select(Soporte).ToArray())).ToArray(),!full,reasons.Count==0,reasons);
    }
    public async Task<Pagina<DespachoDisposicionDto>> DespachosAsync(ConsultaDisposicion f,AlcanceCentros a,CancellationToken ct)
    {
        var q=Despachos(a);if(f.CentroId.HasValue)q=q.Where(d=>d.CentroR1Id==f.CentroId||d.Actas.Any(x=>x.CentroOrigenId==f.CentroId));
        if(!string.IsNullOrWhiteSpace(f.Buscar))q=q.Where(d=>d.Codigo.Contains(f.Buscar));if(!string.IsNullOrWhiteSpace(f.Estado))q=q.Where(d=>d.Estado==f.Estado);
        var total=await q.CountAsync(ct);var ids=await q.OrderByDescending(d=>d.FechaSalida).ThenBy(d=>d.Id).Skip((f.Pagina-1)*f.Tamano).Take(f.Tamano).Select(d=>d.Id).ToListAsync(ct);
        var result=new List<DespachoDisposicionDto>();foreach(var id in ids)result.Add((await DespachoAsync(id,a,ct))!);return new(result,f.Pagina,f.Tamano,total);
    }
    private async Task<DespachoDisposicion> LockDispatch(Guid id,AlcanceCentros a,CancellationToken ct)
    {
        var d=await db.DespachosDisposicion.FromSqlInterpolated($"SELECT * FROM dbo.TBL_DespachoDisposicion WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}").SingleOrDefaultAsync(ct)??throw new KeyNotFoundException("Despacho no encontrado.");
        if(!d.Activo||!a.Autoriza(d.CentroR1Id))throw new UnauthorizedAccessException("Solo el centro R1 puede gestionar el despacho.");return d;
    }
    public async Task<DespachoDisposicionDto> CerrarDespachoAsync(Guid id,CerrarDespachoDisposicionDto dto,string user,AlcanceCentros a,CancellationToken ct)
    {
        Required(dto.Observaciones,1000,"Concepto de cierre");
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var d=await LockDispatch(id,a,ct);
            if(d.Estado=="CERRADO"){await tx.CommitAsync(ct);return;}
            if(Convert.ToBase64String(d.RowVersion)!=dto.RowVersion)throw new ConflictoException("El despacho cambió. Actualiza antes de cerrar.");
            var actas=await db.ActasDisposicion.Include(x=>x.Soportes).Where(x=>x.DespachoId==id&&x.Activo).ToListAsync(ct);
            if(actas.Count==0||actas.Any(x=>!x.Soportes.Any(s=>s.Activo)))throw new ValidacionException("Adjunta el acta firmada de cada centro de origen antes de cerrar.");
            foreach(var s in actas.SelectMany(x=>x.Soportes.Where(s=>s.Activo)))if(!File.Exists(FullPath(s.Ubicacion)))throw new ValidacionException("Un soporte no está disponible; vuelve a adjuntarlo antes de cerrar.");
            var items=await db.DespachosDisposicionItems.Include(x=>x.Orden).ThenInclude(o=>o.Llanta).Include(x=>x.LoteEntrada).Where(x=>x.Activo&&x.DespachoId==id).ToListAsync(ct);
            if(items.Count==0||items.Any(i=>i.Orden.Estado!="PENDIENTE_DISPOSICION"||!i.Orden.FechaRecepcion.HasValue||i.Orden.Llanta.CentroId!=d.CentroR1Id))throw new ConflictoException("La ubicación o estado de las llantas cambió.");
            await ValidarDesmontadas(items.Select(i=>i.Orden.LlantaId).ToArray(),ct);
            foreach(var item in items)
            {
                await operaciones.MoverAsync(new(){LlantaId=item.Orden.LlantaId,TipoDestino="DisposicionFinal",Motivo=$"Disposición final {d.Codigo}",Observaciones=dto.Observaciones},user,a,ct);
                item.Orden.Estado="DISPOSICION_FINAL";item.Orden.ProveedorId=d.ProveedorId;item.Orden.FechaDisposicion=DateTimeOffset.UtcNow;item.Orden.UsuarioModificacion=user;item.Orden.FechaModificacion=DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync(ct);
            foreach(var l in items.Select(i=>i.LoteEntrada).DistinctBy(l=>l.Id))
            {
                l.Estado=await db.OrdenesServicioLlanta.AnyAsync(o=>o.LoteDisposicionFinalId==l.Id&&o.Activo&&o.Estado!="DISPOSICION_FINAL",ct)?"EN_DISPOSICION":"CERRADO";
                if(l.Estado=="CERRADO")l.FechaCierre=DateTimeOffset.UtcNow;l.UsuarioModificacion=user;
            }
            d.Estado="CERRADO";d.FechaCierre=DateTimeOffset.UtcNow;d.CerradoPor=user;d.UsuarioModificacion=user;d.FechaModificacion=DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        });
        return (await DespachoAsync(id,a,ct))!;
    }
}
