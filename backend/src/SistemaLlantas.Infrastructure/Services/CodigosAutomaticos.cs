using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Domain.Common;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Application.Common;
namespace SistemaLlantas.Infrastructure.Services;
public static class CodigosAutomaticos
{
    public static async Task Bloquear(LlantasDbContext db,CancellationToken ct)
    {
        if(db.Database.CurrentTransaction is null) throw new InvalidOperationException("Se requiere transacción para reservar códigos.");
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'GLLD-CODIGOS-MONTAJE',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000; IF @r<0 THROW 51000,'No se obtuvo bloqueo de códigos.',1;",ct);
    }
    public static async Task Preparar(LlantasDbContext db,CancellationToken ct)
    {
        var entries=db.ChangeTracker.Entries().Where(x=>x.State==EntityState.Added).ToArray();
        if(!entries.Any(x=>x.Entity is CatalogoBase or Llanta))return;
        await Bloquear(db,ct);
        var existingCodes=new Dictionary<string,List<string>>();
        async Task<string> Next(string scope,string prefix,long min,IQueryable<string> query,int digits=0)
        {
            var row=db.Set<ConsecutivoCodigo>().Local.FirstOrDefault(x=>x.Ambito==scope) ?? await db.Set<ConsecutivoCodigo>().SingleOrDefaultAsync(x=>x.Ambito==scope,ct);
            if(!existingCodes.TryGetValue(scope,out var codes))existingCodes[scope]=codes=await query.ToListAsync(ct);
            var number=Nomenclatura.Siguiente(codes,prefix,min,row?.Ultimo??0);
            if(row is null){row=new(){Ambito=scope};db.Add(row);}row.Ultimo=number;
            return prefix+number.ToString(digits==0?"0":new string('0',digits),System.Globalization.CultureInfo.InvariantCulture);
        }
        foreach(var e in entries.Select(x=>x.Entity).OfType<CatalogoBase>().OrderBy(x=>x is Referencia?1:0))
        {
            e.Codigo=e switch {
                Marca => await Next("MC","MC_",100,db.Marcas.IgnoreQueryFilters().Select(x=>x.Codigo)),
                Dimension => await Next("DM","DM_",100,db.Dimensiones.IgnoreQueryFilters().Select(x=>x.Codigo)),
                TipoLlanta => await Next("TL","TL_",100,db.TiposLlanta.IgnoreQueryFilters().Select(x=>x.Codigo)),
                Referencia rf => Nomenclatura.Referencia((db.Marcas.Local.FirstOrDefault(x=>x.Id==rf.MarcaId)??await db.Marcas.SingleAsync(x=>x.Id==rf.MarcaId,ct)).Codigo,rf.Nombre),
                _=>e.Codigo };
            if(e is Referencia && (await db.Referencias.AnyAsync(x=>x.Codigo==e.Codigo,ct) || entries.Select(x=>x.Entity).OfType<Referencia>().Count(x=>x.Codigo==e.Codigo)>1))
                throw new ConflictoException("Ya existe una referencia con ese diseño normalizado y marca.");
        }
        foreach(var tire in entries.Select(x=>x.Entity).OfType<Llanta>())
        {
            var brand=db.Marcas.Local.FirstOrDefault(x=>x.Id==tire.MarcaId)??await db.Marcas.SingleAsync(x=>x.Id==tire.MarcaId&&x.Activo,ct);
            if(string.IsNullOrWhiteSpace(brand.Abreviatura))throw new ValidacionException("Configure la abreviatura de la marca antes de crear llantas.");
            var abbreviation=Nomenclatura.Abreviatura(brand.Abreviatura);
            var prefix=$"LL_{abbreviation}_";
            tire.CambiarIdentificacion(await Next("LL_"+brand.Id,prefix,1,db.Llantas.IgnoreQueryFilters().Select(x=>x.Codigo),6),tire.Serial);
        }
    }
}
