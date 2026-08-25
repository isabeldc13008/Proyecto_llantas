using System.ComponentModel.DataAnnotations;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Programacion;

public sealed record ProgramacionDto(Guid Id,string Tipo,DateTimeOffset Inicio,DateTimeOffset? Fin,Guid CentroId,string Centro,Guid? VehiculoId,string Vehiculo,Guid? TecnicoUsuarioId,string Tecnico,string TecnicoNombre,string Prioridad,string Estado,string RutaInicio,bool TieneSolapamiento,string? Observaciones,string? MotivoCancelacion,DateTimeOffset? FechaCumplimiento,byte[] RowVersion);
public sealed record TecnicoProgramacionDto(Guid Id,string Username,string Nombre,IReadOnlyList<Guid> CentroIds);
public sealed record ProgramacionFiltro(Guid? CentroId,Guid? VehiculoId,Guid? TecnicoUsuarioId,string? Tipo,string? Estado,DateTimeOffset? Desde,DateTimeOffset? Hasta,string? Prioridad);
public sealed class GuardarProgramacionDto
{
    [Required,StringLength(50)] public string Tipo {get;init;}=string.Empty;
    public DateTimeOffset Inicio {get;init;}
    public DateTimeOffset Fin {get;init;}
    [Required] public Guid CentroId {get;init;}
    public Guid? VehiculoId {get;init;}
    [Required] public Guid TecnicoUsuarioId {get;init;}
    [Required,StringLength(20)] public string Prioridad {get;init;}="Media";
    [StringLength(1000)] public string? Observaciones {get;init;}
    public byte[]? RowVersion {get;init;}
}
public sealed record ProgramacionMasivaDto(IReadOnlyList<GuardarProgramacionDto> Actividades);
public sealed record CancelarProgramacionDto([Required,StringLength(500)] string Motivo);

public interface IProgramacionService
{
    Task<IReadOnlyList<ProgramacionDto>> ListarAsync(ProgramacionFiltro filtro,AlcanceCentros alcance,CancellationToken ct);
    Task<IReadOnlyList<TecnicoProgramacionDto>> TecnicosAsync(AlcanceCentros alcance,CancellationToken ct);
    Task<ProgramacionDto> CrearAsync(GuardarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<IReadOnlyList<ProgramacionDto>> CrearMasivaAsync(ProgramacionMasivaDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<ProgramacionDto> ActualizarAsync(Guid id,GuardarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task<ProgramacionDto> CancelarAsync(Guid id,CancelarProgramacionDto dto,string usuario,AlcanceCentros alcance,CancellationToken ct);
    Task EliminarAsync(Guid id,string usuario,AlcanceCentros alcance,CancellationToken ct);
}
