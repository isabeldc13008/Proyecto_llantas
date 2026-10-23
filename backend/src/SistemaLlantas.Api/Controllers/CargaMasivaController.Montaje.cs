using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Services;

namespace SistemaLlantas.Api.Controllers;

public sealed partial class CargaMasivaController
{
    public static readonly string[] InventarioColumns = ["IdentificadorOriginal","Serial","Dimension","Marca","Referencia","TipoLlanta","Centro","Vida","MarcaReencauche","BandaReencauche","DOT","ProfundidadInicial","ProfundidadInterior","ProfundidadCentral","ProfundidadExterior","Estado"];
    public static readonly string[] MontajeColumns = [..InventarioColumns,"Placa","Interno","Posicion","FechaMontaje","KilometrajeMontaje"];
    public sealed record Ticket(Guid Id,string Usuario,string Modalidad,string Hash,DateTimeOffset Hasta);
    public sealed record Vista(Guid Id,string Token,int Total,int Validas,int ConError,int Montajes,IReadOnlyList<RowError> Errores,IReadOnlyList<Dictionary<string,string>> Filas);
    public sealed record ImportRow(int Fila,Llanta Tire,PosicionVehiculo? Position,DateTimeOffset? Date,decimal? Km);
    private IDataProtector Protector => HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector("GLLD.ImportacionLlantas.v2");

    private async Task<(List<Dictionary<string,string>> Rows,string Hash)> Archivo(IFormFile archivo,CancellationToken ct)
    {
        if(archivo.Length is <=0 or >25_000_000)throw new ValidacionException("Archivo vacío o mayor a 25 MB.");
        var ext=Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if(ext is not ".csv" and not ".xlsx")throw new ValidacionException("Solo CSV o XLSX.");
        await using var stream=archivo.OpenReadStream();
        var hash=Convert.ToHexString(await SHA256.HashDataAsync(stream,ct));
        var rows=await Read(archivo,ext,ct);
        if(rows.Count is 0 or >10000)throw new ValidacionException("Se requieren entre 1 y 10000 filas.");
        return (rows,hash);
    }

    [HttpPost("llantas/previsualizar"),RequestSizeLimit(26_000_000)]
    public async Task<Vista> PreviewLlantas([FromForm]string modalidad,[FromForm]IFormFile archivo,CancellationToken ct)
    {
        if(modalidad is not "inventario" and not "montaje")throw new ValidacionException("Modalidad inválida.");
        var input=await Archivo(archivo,ct);
        var resolved=await ResolveImport(input.Rows,modalidad,ct);
        var id=Guid.NewGuid();
        var ticket=new Ticket(id,User.Username(),modalidad,input.Hash,DateTimeOffset.UtcNow.AddMinutes(30));
        var invalid=resolved.Errors.Select(x=>x.Fila).Distinct().Count();
        return new(id,Protector.Protect(JsonSerializer.Serialize(ticket)),input.Rows.Count,input.Rows.Count-invalid,invalid,
            modalidad=="montaje"?resolved.Rows.Count:0,resolved.Errors,input.Rows);
    }

    [HttpPost("llantas/confirmar"),RequestSizeLimit(26_000_000)]
    public async Task<ResultadoDto> ConfirmLlantas([FromForm]string token,[FromForm]IFormFile archivo,CancellationToken ct)
    {
        Ticket ticket;
        try { ticket=JsonSerializer.Deserialize<Ticket>(Protector.Unprotect(token))!; }
        catch { throw new ValidacionException("Previsualización inválida; vuelve a cargar el archivo."); }
        if(ticket is null || ticket.Usuario!=User.Username() || ticket.Hasta<DateTimeOffset.UtcNow || ticket.Modalidad is not "inventario" and not "montaje")throw new ValidacionException("Previsualización vencida o no autorizada.");
        var input=await Archivo(archivo,ct);
        if(input.Hash!=ticket.Hash)throw new ValidacionException("El archivo cambió; vuelve a previsualizar.");
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>
        {
            db.ChangeTracker.Clear();
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            await CodigosAutomaticos.Bloquear(db,ct);
            var done=await db.CargasMasivas.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==ticket.Id,ct);
            if(done is not null)
            {
                if(done.Usuario!=ticket.Usuario || done.Estado!="PROCESADA")throw new ConflictoException("Identificador de carga no disponible.");
                await tx.CommitAsync(ct);return new ResultadoDto(done.Id,done.FilasValidas,0);
            }
            var result=await ResolveImport(input.Rows,ticket.Modalidad,ct);
            if(result.Errors.Count>0)throw new ValidacionException("El lote completo fue rechazado.",result.Errors.GroupBy(e=>$"Fila {e.Fila}: {e.Campo}").ToDictionary(g=>g.Key,g=>g.Select(e=>e.Error).ToArray()));
            var load=new CargaMasiva{Tipo="llantas-"+ticket.Modalidad,NombreArchivo=Path.GetFileName(archivo.FileName),TotalFilas=input.Rows.Count,FilasValidas=input.Rows.Count,Estado="PROCESADA",Usuario=User.Username(),UsuarioCreacion=User.Username(),FechaProcesamiento=DateTimeOffset.UtcNow,FilasJson=JsonSerializer.Serialize(input.Rows)};
            db.Entry(load).Property(x=>x.Id).CurrentValue=ticket.Id;
            db.Add(load);
            foreach(var row in result.Rows)
            {
                db.Llantas.Add(row.Tire);
                row.Tire.DatosImportacionJson=JsonSerializer.Serialize(new{Lote=ticket.Id,row.Fila,Valores=input.Rows[row.Fila-2]});
                if(row.Position is null)continue;
                var position=await db.PosicionesVehiculo.SingleAsync(x=>x.Id==row.Position.Id,ct);
                var movement=new Movimiento{Numero="IMP-"+Guid.NewGuid().ToString("N")[..26],Tipo="IMPORTACION_MONTAJE",Motivo="Registro de montaje declarado en carga masiva",CentroId=row.Tire.CentroId,Usuario=User.Username(),UsuarioCreacion=User.Username()};
                db.Movimientos.Add(movement);
                db.MovimientosDetalle.Add(new(){MovimientoId=movement.Id,LlantaId=row.Tire.Id,PosicionDestinoId=position.Id,TipoDestino=TipoDestinoLlanta.Posicion,UsuarioCreacion=User.Username()});
                db.AsignacionesLlantaPosicion.Add(new(){LlantaId=row.Tire.Id,PosicionVehiculoId=position.Id,MovimientoOrigenId=movement.Id,FechaInicio=row.Date,KilometrajeMontaje=row.Km,UsuarioCreacion=User.Username()});
                position.LlantaActualId=row.Tire.Id;position.UsuarioModificacion=User.Username();position.FechaModificacion=DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new ResultadoDto(ticket.Id,input.Rows.Count,0);
        });
    }

    private async Task<(List<ImportRow> Rows,List<RowError> Errors)> ResolveImport(List<Dictionary<string,string>> rows,string mode,CancellationToken ct)
    {
        var centers=await db.Centros.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
        var brands=await db.Marcas.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
        var refs=await db.Referencias.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
        var dims=await db.Dimensiones.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
        var types=await db.TiposLlanta.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
        var states=await db.EstadosLlanta.AsNoTracking().Where(x=>x.Activo).ToListAsync(ct);
        var existing=await db.Llantas.IgnoreQueryFilters().AsNoTracking().Select(x=>new{x.IdentificadorOriginal,x.Serial}).ToListAsync(ct);
        var ids=existing.Where(x=>x.IdentificadorOriginal!=null).Select(x=>x.IdentificadorOriginal!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var serials=existing.Where(x=>x.Serial!=null).Select(x=>x.Serial!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var positions=mode=="montaje"?await db.PosicionesVehiculo.AsNoTracking().Include(x=>x.EjeVehiculo).ThenInclude(x=>x.Vehiculo).Where(x=>x.Activo&&x.EjeVehiculo.Activo&&x.EjeVehiculo.Vehiculo.Activo).ToListAsync(ct):[];
        var occupied=mode=="montaje"?(await db.AsignacionesLlantaPosicion.AsNoTracking().Where(x=>x.EsActiva).Select(x=>x.PosicionVehiculoId).ToListAsync(ct)).ToHashSet():[];
        return ValidateImportRows(rows,mode,centers,brands,refs,dims,types,states,ids,serials,positions,occupied,User.AlcanceCentros(),User.Username());
    }

    // Pure validation shared by preview and transactional confirmation; no persistence or code reservation.
    public static (List<ImportRow> Rows,List<RowError> Errors) ValidateImportRows(
        List<Dictionary<string,string>> rows,string mode,IReadOnlyList<Centro> centers,IReadOnlyList<Marca> brands,
        IReadOnlyList<Referencia> refs,IReadOnlyList<Dimension> dims,IReadOnlyList<TipoLlanta> types,IReadOnlyList<EstadoLlanta> states,
        IEnumerable<string> originalIds,IEnumerable<string> originalSerials,IReadOnlyList<PosicionVehiculo> positions,
        IEnumerable<Guid> occupiedIds,AlcanceCentros scope,string username)
    {
        var result=new List<ImportRow>();var errors=new List<RowError>();
        var ids=originalIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var serials=originalSerials.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var occupied=occupiedIds.ToHashSet();
        for(int i=0;i<rows.Count;i++)
        {
            var row=rows[i];int before=errors.Count;string V(string f)=>(row.GetValueOrDefault(f)??"").Trim();
            void E(string f,string m)=>errors.Add(new(i+2,f,m));
            T? Find<T>(IEnumerable<T> list,string field) where T:CatalogoBase
            {
                var matches=list.Where(x=>string.Equals(x.Codigo,V(field),StringComparison.OrdinalIgnoreCase)||string.Equals(x.Nombre,V(field),StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
                if(matches.Count!=1){E(field,"Catálogo inexistente o ambiguo.");return null;}return matches[0];
            }
            decimal? Number(string field,decimal max)
            {
                if(V(field)=="")return null;
                if(!decimal.TryParse(V(field),NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out var n)||n<0||n>max||decimal.Round(n,2)!=n){E(field,"Número fuera de rango; máximo dos decimales y punto decimal.");return null;}return n;
            }
            var brand=Find(brands,"Marca");var rf=Find(refs.Where(x=>x.MarcaId==brand?.Id),"Referencia");var dim=Find(dims,"Dimension");var type=Find(types,"TipoLlanta");var center=Find(centers,"Centro");var state=Find(states,"Estado");
            if(brand?.Abreviatura is not {Length:2} abbr || abbr.Any(c=>c is < 'A' or > 'Z'))E("Marca","La marca requiere abreviatura de dos letras A-Z configurada.");
            if(center is not null&&!scope.Autoriza(center.Id))E("Centro","Centro fuera del alcance autorizado.");
            var original=V("IdentificadorOriginal").ToUpperInvariant();
            if(original.Length is 0 or >100)E("IdentificadorOriginal","Obligatorio; máximo 100 caracteres.");
            else if(!ids.Add(original))E("IdentificadorOriginal","Duplicado en el archivo o en el sistema.");
            var serial=V("Serial").ToUpperInvariant();if(serial.Length>100)E("Serial","Máximo 100 caracteres.");
            if(serial!=""&&!serials.Add(serial))E("Serial","Serial duplicado.");
            var depth=Number("ProfundidadInicial",100);Number("ProfundidadInterior",100);Number("ProfundidadCentral",100);Number("ProfundidadExterior",100);
            int? life=null;
            if(V("Vida")!=""){if(!int.TryParse(V("Vida"),out var n)||n<1)E("Vida","Entero mayor o igual a 1.");else life=n;}
            PosicionVehiculo? position=null;DateTimeOffset? date=null;decimal? km=null;
            if(mode=="montaje")
            {
                if(V("Placa")==""&&V("Interno")=="")E("Placa","Indique placa o número interno; el vehículo debe existir.");
                var matches=positions.Where(x=>(V("Placa")==""||string.Equals(x.EjeVehiculo.Vehiculo.Placa,V("Placa"),StringComparison.OrdinalIgnoreCase))&&(V("Interno")==""||string.Equals(x.EjeVehiculo.Vehiculo.NumeroInterno,V("Interno"),StringComparison.OrdinalIgnoreCase))&&string.Equals(x.Codigo,V("Posicion"),StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
                if(matches.Count!=1)E("Posicion","Vehículo/posición inexistente o ambiguo; registre el vehículo previamente.");
                else
                {
                    position=matches[0];
                    if(!string.Equals(position.EjeVehiculo.Vehiculo.Estado,"Activo",StringComparison.OrdinalIgnoreCase))E("Placa","El vehículo no está en estado Activo.");
                    if(position.EjeVehiculo.Vehiculo.CentroId!=center?.Id)E("Centro","Vehículo y llanta deben pertenecer al mismo centro.");
                    if(position.LlantaActualId.HasValue||!occupied.Add(position.Id))E("Posicion","Posición ocupada o repetida en el lote.");
                }
                if(state?.Codigo!="MONTADA")E("Estado","La modalidad con montaje requiere estado MONTADA.");
                km=Number("KilometrajeMontaje",9999999999999999.99m);
                if(V("FechaMontaje")!="")
                {
                    if(!DateTimeOffset.TryParseExact(V("FechaMontaje"),"yyyy-MM-dd'T'HH:mm:sszzz",CultureInfo.InvariantCulture,DateTimeStyles.None,out var d)||d>DateTimeOffset.UtcNow)E("FechaMontaje","Use yyyy-MM-ddTHH:mm:ss±HH:mm, no futura, o deje vacío.");else date=d;
                }
            }
            else if(state?.Codigo=="MONTADA")E("Estado","Solo inventario no puede declarar un montaje.");
            if(errors.Count!=before)continue;
            var tire=new Llanta("PENDIENTE",serial){IdentificadorOriginal=original,MarcaId=brand!.Id,ReferenciaId=rf!.Id,DimensionId=dim!.Id,TipoLlantaId=type!.Id,CentroId=center!.Id,EstadoLlantaId=state!.Id,ProfundidadInicial=depth,NumeroReencauches=life.HasValue?life.Value-1:0,UsuarioCreacion=username,UbicacionActual=position is null?"Inventario":$"{position.EjeVehiculo.Vehiculo.Placa} / {position.Codigo}"};
            result.Add(new(i+2,tire,position,date,km));
        }
        return (result,errors);
    }
}
