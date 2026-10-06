using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class DisposicionService
{
    private string StorageRoot=>Path.GetFullPath(configuration["Disposicion:StorageRoot"]??Path.Combine(AppContext.BaseDirectory,"App_Data","disposicion"));
    private string FullPath(string name)
    {
        var path=Path.GetFullPath(Path.Combine(StorageRoot,name));if(!path.StartsWith(StorageRoot+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new ValidacionException("Ruta de soporte inválida.");return path;
    }
    private static SoporteActaDto Soporte(SoporteActaDisposicion s)=>new(s.Id,s.NombreArchivo,s.MimeType,s.TamanoBytes,s.Hash,s.FechaCreacion,s.UsuarioCreacion);
    public async Task<ActaDisposicionDto?> ActaAsync(Guid id,AlcanceCentros a,CancellationToken ct)
    {
        var dispatch=await db.ActasDisposicion.AsNoTracking().Where(x=>x.Id==id&&x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.Despacho.CentroR1Id)||a.CentroIds.Contains(x.CentroOrigenId))).Select(x=>(Guid?)x.DespachoId).SingleOrDefaultAsync(ct);
        return dispatch.HasValue?(await DespachoAsync(dispatch.Value,a,ct))?.Actas.SingleOrDefault(x=>x.Id==id):null;
    }
    public async Task<SoporteActaDto> AdjuntarSoporteAsync(Guid actaId,Stream contenido,string nombreArchivo,string mimeType,long tamanoBytes,string user,AlcanceCentros a,CancellationToken ct)
    {
        if(tamanoBytes is <=0 or >10_000_000)throw new ValidacionException("Archivo vacío o superior a 10 MB.");
        var acta=await db.ActasDisposicion.AsNoTracking().Where(x=>x.Id==actaId&&x.Activo&&x.Despacho.Activo&&(a.VerTodos||a.CentroIds.Contains(x.Despacho.CentroR1Id))).Select(x=>new{x.DespachoId}).SingleOrDefaultAsync(ct)??throw new KeyNotFoundException("Acta no disponible para gestión.");
        var ext=Path.GetExtension(nombreArchivo).ToLowerInvariant();
        var expected=ext switch{".pdf"=>"application/pdf",".jpg" or ".jpeg"=>"image/jpeg",".png"=>"image/png",_=>""};
        if(expected.Length==0||mimeType!=expected)throw new ValidacionException("Solo PDF, JPG, JPEG y PNG con formato coincidente.");
        using var bytes=new MemoryStream();var buffer=new byte[81920];int read;
        while((read=await contenido.ReadAsync(buffer,ct))>0){if(bytes.Length+read>10_000_000)throw new ValidacionException("El archivo supera 10 MB.");await bytes.WriteAsync(buffer.AsMemory(0,read),ct);}
        var data=bytes.ToArray();if(data.Length!=tamanoBytes)throw new ValidacionException("Tamaño de archivo incoherente.");
        var valid=expected switch{"application/pdf"=>data.AsSpan().StartsWith("%PDF-"u8),"image/jpeg"=>data.AsSpan().StartsWith(new byte[]{255,216,255}),"image/png"=>data.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10}),_=>false};
        if(!valid)throw new ValidacionException("El contenido no coincide con el formato declarado.");
        var cleanName=Path.GetFileName(nombreArchivo.Replace('\\','/'));if(string.IsNullOrWhiteSpace(cleanName)||cleanName.Length>255)throw new ValidacionException("Nombre de archivo inválido.");
        var support=new SoporteActaDisposicion{ActaId=actaId,NombreArchivo=cleanName,MimeType=expected,TamanoBytes=data.Length,Hash=Convert.ToHexString(SHA256.HashData(data)),UsuarioCreacion=user};
        support.Ubicacion=support.Id.ToString("N")+ext;Directory.CreateDirectory(StorageRoot);
        var path=FullPath(support.Ubicacion);await File.WriteAllBytesAsync(path,data,ct);
        try
        {
            await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
                db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
                var d=await LockDispatch(acta.DespachoId,a,ct);if(d.Estado=="CERRADO")throw new ConflictoException("No se pueden modificar soportes de un despacho cerrado.");
                if(!await db.SoportesActaDisposicion.AnyAsync(s=>s.Id==support.Id,ct))db.SoportesActaDisposicion.Add(support);
                d.UsuarioModificacion=user;d.FechaModificacion=DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
            });
        }
        catch
        {
            // A commit can succeed while its acknowledgment fails; never erase a referenced file.
            db.ChangeTracker.Clear();if(!await db.SoportesActaDisposicion.AnyAsync(s=>s.Id==support.Id,CancellationToken.None))File.Delete(path);throw;
        }
        return Soporte(support);
    }
    public async Task EliminarSoporteAsync(Guid id,string user,AlcanceCentros a,CancellationToken ct)
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            db.ChangeTracker.Clear();await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var reference=await db.SoportesActaDisposicion.AsNoTracking().Where(s=>s.Id==id).Select(s=>new{s.Acta.DespachoId}).SingleOrDefaultAsync(ct)??throw new KeyNotFoundException("Soporte no encontrado.");
            var d=await LockDispatch(reference.DespachoId,a,ct);if(d.Estado=="CERRADO")throw new ConflictoException("El soporte de un cierre se conserva para trazabilidad.");
            var s=await db.SoportesActaDisposicion.SingleAsync(s=>s.Id==id,ct);s.Activo=false;s.UsuarioModificacion=user;s.FechaModificacion=DateTimeOffset.UtcNow;
            d.UsuarioModificacion=user;d.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        });
    }
    public async Task<ArchivoSoporte?> ArchivoAsync(Guid id,AlcanceCentros a,CancellationToken ct)
    {
        var s=await db.SoportesActaDisposicion.AsNoTracking().SingleOrDefaultAsync(s=>s.Id==id&&s.Activo&&s.Acta.Activo&&s.Acta.Despacho.Activo&&(a.VerTodos||a.CentroIds.Contains(s.Acta.Despacho.CentroR1Id)||a.CentroIds.Contains(s.Acta.CentroOrigenId)),ct);
        if(s is null)return null;var path=FullPath(s.Ubicacion);return File.Exists(path)?new(path,s.MimeType,s.NombreArchivo):null;
    }
    public async Task RegistrarNovedadAsync(Guid loteId,CrearNovedadDto dto,string user,AlcanceCentros a,CancellationToken ct)
    {
        Required(dto.Observacion,1000,"Observación");var l=await db.LotesDisposicionFinal.SingleOrDefaultAsync(l=>l.Id==loteId&&l.Activo&&(a.VerTodos||a.CentroIds.Contains(l.CentroOrigenId)||a.CentroIds.Contains(l.CentroDestinoId)),ct)??throw new KeyNotFoundException("Lote no disponible.");
        if(l.Estado=="CERRADO")throw new ConflictoException("El lote está cerrado.");
        if(dto.OrdenId.HasValue&&!await db.OrdenesServicioLlanta.AnyAsync(o=>o.Id==dto.OrdenId&&o.Activo&&o.LoteDisposicionFinalId==loteId,ct))throw new ValidacionException("La orden no pertenece al lote.");
        db.NovedadesDisposicion.Add(new(){LoteId=loteId,OrdenId=dto.OrdenId,Observacion=dto.Observacion.Trim(),UsuarioCreacion=user});await db.SaveChangesAsync(ct);
    }
    public async Task ResolverNovedadAsync(Guid id,ResolverNovedadDto dto,string user,AlcanceCentros a,CancellationToken ct)
    {
        Required(dto.Observacion,1000,"Resolución");var n=await db.NovedadesDisposicion.SingleOrDefaultAsync(n=>n.Id==id&&n.Activo&&(a.VerTodos||a.CentroIds.Contains(n.Lote.CentroDestinoId)||a.CentroIds.Contains(n.Lote.CentroOrigenId)),ct)??throw new KeyNotFoundException("Novedad no disponible.");
        if(n.FechaResolucion.HasValue)throw new ConflictoException("La novedad ya fue resuelta.");n.FechaResolucion=DateTimeOffset.UtcNow;n.ResueltaPor=user;n.Resolucion=dto.Observacion.Trim();n.UsuarioModificacion=user;n.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);
    }
}
