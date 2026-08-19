using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Programacion;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class ProgramacionService(LlantasDbContext db):IProgramacionService
{
    public async Task<IReadOnlyList<ProgramacionDto>> ListarAsync(ProgramacionFiltro f,AlcanceCentros alcance,CancellationToken ct)
    {
        var q=Base(alcance);
        if(f.CentroId.HasValue)q=q.Where(x=>x.CentroId==f.CentroId);
        if(f.VehiculoId.HasValue)q=q.Where(x=>x.VehiculoId==f.VehiculoId);
        if(f.TecnicoUsuarioId.HasValue)q=q.Where(x=>x.TecnicoUsuarioId==f.TecnicoUsuarioId);
        if(!string.IsNullOrWhiteSpace(f.Tipo))q=q.Where(x=>x.TipoActividad.Contains(f.Tipo));
        if(!string.IsNullOrWhiteSpace(f.Prioridad))q=q.Where(x=>x.Prioridad==f.Prioridad);
        if(f.Desde.HasValue)q=q.Where(x=>(x.FechaFinProgramada??x.FechaProgramada)>=f.Desde);
        if(f.Hasta.HasValue)q=q.Where(x=>x.FechaProgramada<=f.Hasta);
        if(!string.IsNullOrWhiteSpace(f.Estado))
        {
            if(f.Estado.Equals("Vencida",StringComparison.OrdinalIgnoreCase))q=q.Where(x=>x.Estado==EstadoActividad.Vencida||(x.Estado==EstadoActividad.Pendiente&&x.FechaProgramada<DateTimeOffset.UtcNow));
            else if(Enum.TryParse<EstadoActividad>(f.Estado.Replace(" ",string.Empty),true,out var state))q=q.Where(x=>x.Estado==state);
        }
        var rows=await q.OrderBy(x=>x.FechaProgramada).ToListAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<TecnicoProgramacionDto>> TecnicosAsync(AlcanceCentros alcance,CancellationToken ct)=>await db.UsuariosSistema.AsNoTracking()
        .Where(x=>x.Activo&&x.Rol.Codigo=="TECNICO"&&(alcance.VerTodos||x.Centros.Any(c=>c.Activo&&alcance.CentroIds.Contains(c.CentroId))))
        .OrderBy(x=>x.Nombre).Select(x=>new TecnicoProgramacionDto(x.Id,x.Username,x.Nombre,x.Centros.Where(c=>c.Activo).Select(c=>c.CentroId).ToList())).ToListAsync(ct);

    public async Task<ProgramacionDto> CrearAsync(GuardarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        await Validar(dto,alcance,null,ct);var item=await Construir(dto,usuario,ct);db.ActividadesProgramadas.Add(item);await db.SaveChangesAsync(ct);return await Obtener(item.Id,alcance,ct);
    }

    public async Task<IReadOnlyList<ProgramacionDto>> CrearMasivaAsync(ProgramacionMasivaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        if(dto.Actividades.Count==0)throw new ValidacionException("Incluya al menos una actividad.");
        var duplicates=dto.Actividades.GroupBy(Key).Where(x=>x.Count()>1).Select(x=>x.Key).ToList();if(duplicates.Count>0)throw new ConflictoException("La carga contiene programaciones duplicadas.");
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;var group=Guid.NewGuid();var ids=new List<Guid>();
        foreach(var input in dto.Actividades){await Validar(input,alcance,null,ct);var item=await Construir(input,usuario,ct);item.GrupoProgramacionId=group;db.ActividadesProgramadas.Add(item);ids.Add(item.Id);}
        await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);var rows=await Base(alcance).Where(x=>ids.Contains(x.Id)).OrderBy(x=>x.FechaProgramada).ToListAsync(ct);return rows.Select(Map).ToList();
    }

    public async Task<ProgramacionDto> ActualizarAsync(Guid id,GuardarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        var item=await db.ActividadesProgramadas.SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Programación no encontrada.");
        if(item.Estado is EstadoActividad.Cumplida or EstadoActividad.Cancelada)throw new ConflictoException("Una actividad cumplida o cancelada no puede editarse.");
        await Validar(dto,alcance,id,ct);if(dto.RowVersion is {Length:>0})db.Entry(item).Property(x=>x.RowVersion).OriginalValue=dto.RowVersion;
        if(item.TecnicoUsuarioId!=dto.TecnicoUsuarioId)item.ReasignadoPor=usuario;
        var tech=await db.UsuariosSistema.SingleAsync(x=>x.Id==dto.TecnicoUsuarioId,ct);item.TipoActividad=dto.Tipo.Trim();item.FechaProgramada=dto.Inicio;item.FechaFinProgramada=dto.Fin;item.CentroId=dto.CentroId;item.VehiculoId=dto.VehiculoId;item.TecnicoUsuarioId=tech.Id;item.TecnicoId=tech.Username;item.Prioridad=dto.Prioridad;item.Observaciones=dto.Observaciones;item.UsuarioModificacion=usuario;item.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return await Obtener(id,alcance,ct);
    }

    public async Task<ProgramacionDto> CancelarAsync(Guid id,CancelarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(dto.Motivo))throw new ValidacionException("El motivo de cancelación es obligatorio.");var item=await db.ActividadesProgramadas.SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Programación no encontrada.");if(item.Estado==EstadoActividad.Cumplida)throw new ConflictoException("Una actividad cumplida no puede cancelarse.");item.Estado=EstadoActividad.Cancelada;item.MotivoCancelacion=dto.Motivo.Trim();item.UsuarioModificacion=usuario;item.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);return await Obtener(id,alcance,ct);
    }

    public async Task EliminarAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct)
    {
        var item=await db.ActividadesProgramadas.SingleOrDefaultAsync(x=>x.Id==id&&(alcance.VerTodos||alcance.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Programación no encontrada.");if(item.Estado!=EstadoActividad.Pendiente||item.FechaInicioReal.HasValue)throw new ConflictoException("Sólo puede eliminarse una actividad pendiente que no haya iniciado.");item.Activo=false;item.UsuarioModificacion=usuario;item.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);
    }

    private IQueryable<ActividadProgramada> Base(AlcanceCentros scope)=>db.ActividadesProgramadas.AsNoTracking().Where(x=>x.Activo&&(scope.VerTodos||scope.CentroIds.Contains(x.CentroId))).Include(x=>x.Centro).Include(x=>x.Vehiculo).Include(x=>x.TecnicoUsuario);
    private async Task<ProgramacionDto> Obtener(Guid id,AlcanceCentros scope,CancellationToken ct)=>Map(await Base(scope).SingleAsync(x=>x.Id==id,ct));
    private async Task Validar(GuardarProgramacionDto dto,AlcanceCentros alcance,Guid? exclude,CancellationToken ct)
    {
        if(dto.Inicio==default||dto.Fin<=dto.Inicio)throw new ValidacionException("La fecha final debe ser posterior a la inicial.");if(!alcance.Autoriza(dto.CentroId))throw new UnauthorizedAccessException("Centro no autorizado.");
        if(!await db.Centros.AnyAsync(x=>x.Id==dto.CentroId&&x.Activo,ct))throw new KeyNotFoundException("Centro no encontrado.");
        if(dto.VehiculoId.HasValue&&!await db.Vehiculos.AnyAsync(x=>x.Id==dto.VehiculoId&&x.CentroId==dto.CentroId,ct))throw new ValidacionException("El vehículo no pertenece al centro seleccionado.");
        if(!dto.VehiculoId.HasValue&&!dto.Tipo.Contains("administr",StringComparison.OrdinalIgnoreCase))throw new ValidacionException("El vehículo es obligatorio para esta actividad.");
        if(!await db.UsuariosSistema.AnyAsync(x=>x.Id==dto.TecnicoUsuarioId&&x.Activo&&x.Rol.Codigo=="TECNICO"&&x.Centros.Any(c=>c.Activo&&c.CentroId==dto.CentroId),ct))throw new ValidacionException("El técnico no está activo o no pertenece al centro.");
        if(await db.ActividadesProgramadas.AnyAsync(x=>x.Activo&&x.Id!=exclude&&x.Estado!=EstadoActividad.Cancelada&&x.TecnicoUsuarioId==dto.TecnicoUsuarioId&&x.TipoActividad==dto.Tipo&&x.VehiculoId==dto.VehiculoId&&x.FechaProgramada==dto.Inicio,ct))throw new ConflictoException("Ya existe una programación igual para el técnico, vehículo y hora.");
    }
    private async Task<ActividadProgramada> Construir(GuardarProgramacionDto dto,string usuario,CancellationToken ct){var tech=await db.UsuariosSistema.SingleAsync(x=>x.Id==dto.TecnicoUsuarioId,ct);return new(){TipoActividad=dto.Tipo.Trim(),FechaProgramada=dto.Inicio,FechaFinProgramada=dto.Fin,CentroId=dto.CentroId,VehiculoId=dto.VehiculoId,TecnicoUsuarioId=tech.Id,TecnicoId=tech.Username,Prioridad=dto.Prioridad,Observaciones=dto.Observaciones,UsuarioCreacion=usuario};}
    private ProgramacionDto Map(ActividadProgramada x){var end=x.FechaFinProgramada??x.FechaProgramada;var overlap=db.ActividadesProgramadas.AsNoTracking().Any(a=>a.Activo&&a.Id!=x.Id&&a.TecnicoUsuarioId==x.TecnicoUsuarioId&&a.Estado!=EstadoActividad.Cancelada&&a.FechaProgramada<end&&(a.FechaFinProgramada??a.FechaProgramada)>x.FechaProgramada);var state=x.Estado==EstadoActividad.Pendiente&&x.FechaProgramada<DateTimeOffset.UtcNow?EstadoActividad.Vencida.ToString():x.Estado.ToString();return new(x.Id,x.TipoActividad,x.FechaProgramada,x.FechaFinProgramada,x.CentroId,x.Centro.Nombre,x.VehiculoId,x.Vehiculo is null?"Sin vehículo":$"Interno {x.Vehiculo.NumeroInterno} - {x.Vehiculo.Placa}",x.TecnicoUsuarioId,x.TecnicoId,x.TecnicoUsuario?.Nombre??x.TecnicoId,x.Prioridad,state,Ruta(x),overlap,x.Observaciones,x.MotivoCancelacion,x.RowVersion);}
    private static string Ruta(ActividadProgramada x){var query=$"actividadId={x.Id}&vehiculoId={x.VehiculoId}";var tipo=x.TipoActividad.ToLowerInvariant();return tipo.Contains("inspe")?$"/inspecciones?{query}":tipo.Contains("mov")?$"/movimientos?{query}":$"/montajes?{query}";}
    private static string Key(GuardarProgramacionDto x)=>$"{x.TecnicoUsuarioId:N}|{x.VehiculoId:N}|{x.Tipo.Trim().ToUpperInvariant()}|{x.Inicio:O}";
}
