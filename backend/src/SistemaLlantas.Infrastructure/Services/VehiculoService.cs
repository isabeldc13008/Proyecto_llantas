using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Vehiculos;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class VehiculoService(LlantasDbContext db) : IVehiculoService
{
    public async Task<Pagina<VehiculoResumenDto>> ConsultarAsync(ConsultaPaginada c,AlcanceCentros alcance,CancellationToken ct)
    {
        var q=db.Vehiculos.AsNoTracking().Where(x=>alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId));
        if(c.CentroId.HasValue){if(!alcance.Autoriza(c.CentroId.Value))q=q.Where(_=>false);else q=q.Where(x=>x.CentroId==c.CentroId);}
        if(c.Activo.HasValue)q=q.Where(x=>x.Activo==c.Activo);
        if(!string.IsNullOrWhiteSpace(c.Search)){var s=c.Search.Trim();q=q.Where(x=>x.NumeroInterno.Contains(s)||x.Placa.Contains(s)||x.Tipo.Contains(s));}
        var total=await q.CountAsync(ct);
        var items=await q.OrderBy(x=>x.NumeroInterno).Skip((c.Pagina-1)*c.Tamano).Take(c.Tamano).Select(x=>new VehiculoResumenDto(x.Id,x.NumeroInterno,x.Placa,x.Tipo,x.CentroId,x.Centro.Nombre,x.ConfiguracionVehiculo!=null?x.ConfiguracionVehiculo.Nombre:null,x.Estado,x.Kilometraje,x.Ejes.Count,x.Ejes.SelectMany(e=>e.Posiciones).Count(),Convert.ToBase64String(x.RowVersion))).ToListAsync(ct);
        return new(items,c.Pagina,c.Tamano,total);
    }

    public async Task<VehiculoDetalleDto?> ObtenerAsync(Guid id,AlcanceCentros alcance,CancellationToken ct)
    {
        var v=await db.Vehiculos.AsNoTracking().Include(x=>x.Centro).Include(x=>x.ConfiguracionVehiculo).Include(x=>x.Ejes).ThenInclude(x=>x.Posiciones).ThenInclude(x=>x.LlantaActual).SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct);
        if(v is null)return null;
        var ids=v.Ejes.SelectMany(x=>x.Posiciones).Select(x=>x.Id).ToArray();
        var active=await db.AsignacionesLlantaPosicion.AsNoTracking().Where(x=>ids.Contains(x.PosicionVehiculoId)&&x.EsActiva).Include(x=>x.Llanta).ToDictionaryAsync(x=>x.PosicionVehiculoId,ct);
        var historial=await db.AsignacionesLlantaPosicion.AsNoTracking().Where(x=>ids.Contains(x.PosicionVehiculoId)).Include(x=>x.Llanta).Include(x=>x.PosicionVehiculo).OrderByDescending(x=>x.FechaInicio).Select(x=>new AsignacionVehiculoDto(x.Id,x.Llanta.Codigo,x.PosicionVehiculo.Codigo,x.FechaInicio,x.FechaFin,x.EsActiva)).ToListAsync(ct);
        return new(v.Id,v.NumeroInterno,v.Placa,v.Tipo,v.CentroId,v.Centro.Nombre,v.ConfiguracionVehiculoId,v.ConfiguracionVehiculo?.Nombre,v.Estado,v.Kilometraje,v.Ejes.OrderBy(x=>x.Orden).Select(e=>new EjeVehiculoDto(e.Id,e.Numero,e.Orden,e.Nombre,e.TipoEje,e.Posiciones.OrderBy(p=>p.Orden).Select(p=>active.TryGetValue(p.Id,out var a)?new PosicionVehiculoDto(p.Id,p.Codigo,p.Lado,p.Ubicacion,p.Orden,a.LlantaId,a.Llanta.Codigo,a.Llanta.Serial):new PosicionVehiculoDto(p.Id,p.Codigo,p.Lado,p.Ubicacion,p.Orden,null,null,null)).ToList())).ToList(),historial,Convert.ToBase64String(v.RowVersion));
    }

    public async Task<VehiculoDetalleDto> CrearAsync(GuardarVehiculoDto d,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        if(!alcance.Autoriza(d.CentroId))throw new UnauthorizedAccessException("El centro no está autorizado para el usuario.");
        await ValidarAsync(d,null,ct); var v=new Vehiculo{NumeroInterno=d.NumeroInterno.Trim().ToUpperInvariant(),Placa=d.Placa.Trim().ToUpperInvariant(),Tipo=d.Tipo.Trim(),CentroId=d.CentroId,ConfiguracionVehiculoId=d.ConfiguracionVehiculoId,Kilometraje=d.Kilometraje,Estado=d.Estado.Trim(),UsuarioCreacion=usuario};
        if(d.ConfiguracionVehiculoId.HasValue){var cfg=await db.ConfiguracionesVehiculo.Include(x=>x.Ejes).ThenInclude(x=>x.Posiciones).SingleAsync(x=>x.Id==d.ConfiguracionVehiculoId,ct);foreach(var ce in cfg.Ejes.OrderBy(x=>x.Orden)){var eje=new EjeVehiculo{Numero=ce.Orden,Orden=ce.Orden,Nombre=ce.Nombre,TipoEje=ce.TipoEje,UsuarioCreacion=usuario};foreach(var cp in ce.Posiciones.OrderBy(x=>x.Orden))eje.Posiciones.Add(new(){Codigo=cp.Codigo,Lado=cp.Lado,Ubicacion=cp.Ubicacion,Orden=cp.Orden,UsuarioCreacion=usuario});v.Ejes.Add(eje);}}
        db.Vehiculos.Add(v);await db.SaveChangesAsync(ct);return (await ObtenerAsync(v.Id,alcance,ct))!;
    }

    public async Task<VehiculoDetalleDto?> ActualizarAsync(Guid id,GuardarVehiculoDto d,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        if(!alcance.Autoriza(d.CentroId))throw new UnauthorizedAccessException("El centro no está autorizado para el usuario.");
        var v=await db.Vehiculos.Include(x=>x.Ejes).ThenInclude(x=>x.Posiciones).SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct);if(v is null)return null;
        var positionIds=v.Ejes.SelectMany(e=>e.Posiciones).Select(p=>p.Id).ToArray();
        if(v.ConfiguracionVehiculoId!=d.ConfiguracionVehiculoId&&await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.EsActiva&&positionIds.Contains(x.PosicionVehiculoId),ct))throw new ConflictoException("Debe desmontar las llantas antes de cambiar la configuración del vehículo.");
        if(v.ConfiguracionVehiculoId!=d.ConfiguracionVehiculoId)throw new ConflictoException("La configuración de un vehículo existente requiere un proceso controlado; cree el vehículo con la configuración homologada correcta.");
        await ValidarAsync(d,id,ct);if(!string.IsNullOrWhiteSpace(d.RowVersion))db.Entry(v).Property(x=>x.RowVersion).OriginalValue=Convert.FromBase64String(d.RowVersion);v.NumeroInterno=d.NumeroInterno.Trim().ToUpperInvariant();v.Placa=d.Placa.Trim().ToUpperInvariant();v.Tipo=d.Tipo.Trim();v.CentroId=d.CentroId;v.Kilometraje=d.Kilometraje;v.Estado=d.Estado.Trim();v.FechaModificacion=DateTimeOffset.UtcNow;v.UsuarioModificacion=usuario;await db.SaveChangesAsync(ct);return await ObtenerAsync(id,alcance,ct);
    }

    public async Task<IReadOnlyList<ConfiguracionVehiculoDto>> ConfiguracionesAsync(CancellationToken ct)=>await db.ConfiguracionesVehiculo.AsNoTracking().Include(x=>x.Ejes).ThenInclude(x=>x.Posiciones).OrderBy(x=>x.Nombre).Select(x=>new ConfiguracionVehiculoDto(x.Id,x.Codigo,x.Nombre,x.TipoVehiculo,x.Ejes.OrderBy(e=>e.Orden).Select(e=>new ConfiguracionEjeDto(e.Orden,e.Nombre,e.TipoEje,e.Posiciones.OrderBy(p=>p.Orden).Select(p=>new ConfiguracionPosicionDto(p.Codigo,p.Lado,p.Ubicacion,p.Orden)).ToList())).ToList(),x.Activo)).ToListAsync(ct);
    public async Task<ConfiguracionVehiculoDto> CrearConfiguracionAsync(GuardarConfiguracionVehiculoDto d,string usuario,CancellationToken ct){ValidarConfiguracion(d);if(await db.ConfiguracionesVehiculo.IgnoreQueryFilters().AnyAsync(x=>x.Codigo==d.Codigo.Trim().ToUpper(),ct))throw new ConflictoException("Ya existe una configuración con ese código.");var e=new ConfiguracionVehiculo{Codigo=d.Codigo.Trim().ToUpperInvariant(),Nombre=d.Nombre.Trim(),TipoVehiculo=d.TipoVehiculo.Trim(),UsuarioCreacion=usuario};foreach(var de in d.Ejes.OrderBy(x=>x.Orden)){var eje=new ConfiguracionEje{Orden=de.Orden,Nombre=de.Nombre.Trim(),TipoEje=de.TipoEje.Trim(),UsuarioCreacion=usuario};foreach(var p in de.Posiciones.OrderBy(x=>x.Orden))eje.Posiciones.Add(new(){Codigo=p.Codigo.Trim().ToUpperInvariant(),Lado=p.Lado.Trim(),Ubicacion=p.Ubicacion.Trim(),Orden=p.Orden,UsuarioCreacion=usuario});e.Ejes.Add(eje);}db.ConfiguracionesVehiculo.Add(e);await db.SaveChangesAsync(ct);return (await ConfiguracionesAsync(ct)).Single(x=>x.Id==e.Id);}
    private async Task ValidarAsync(GuardarVehiculoDto d,Guid? id,CancellationToken ct){if(string.IsNullOrWhiteSpace(d.NumeroInterno)||string.IsNullOrWhiteSpace(d.Estado))throw new ValidacionException("Número interno y estado son obligatorios.");if(await db.Vehiculos.IgnoreQueryFilters().AnyAsync(x=>x.Id!=id&&x.NumeroInterno==d.NumeroInterno.Trim().ToUpper(),ct))throw new ConflictoException("Ya existe un vehículo con ese número interno.");if(!await db.Centros.AnyAsync(x=>x.Id==d.CentroId,ct))throw new ValidacionException("El centro no existe o está inactivo.");if(d.ConfiguracionVehiculoId.HasValue&&!await db.ConfiguracionesVehiculo.AnyAsync(x=>x.Id==d.ConfiguracionVehiculoId,ct))throw new ValidacionException("La configuración no existe o está inactiva.");}
    private static void ValidarConfiguracion(GuardarConfiguracionVehiculoDto d){if(string.IsNullOrWhiteSpace(d.Codigo)||string.IsNullOrWhiteSpace(d.Nombre)||d.Ejes.Count==0)throw new ValidacionException("Código, nombre y al menos un eje son obligatorios.");if(d.Ejes.GroupBy(x=>x.Orden).Any(x=>x.Count()>1)||d.Ejes.Any(x=>x.Posiciones.Count==0||x.Posiciones.GroupBy(p=>p.Orden).Any(g=>g.Count()>1)||x.Posiciones.GroupBy(p=>p.Codigo,StringComparer.OrdinalIgnoreCase).Any(g=>g.Count()>1)))throw new ValidacionException("Los órdenes y códigos de posiciones deben ser únicos dentro de cada eje.");}
}
