using SistemaLlantas.Domain.Common;

namespace SistemaLlantas.Domain.Entities;

public enum TipoDestinoLlanta { Posicion, Inventario, Reparacion, Reencauche, DisposicionFinal, Traslado, Otro }
public enum EstadoActividad { Pendiente, EnEjecucion, Cumplida, Vencida, Cancelada }

public sealed class AsignacionLlantaPosicion : EntidadAuditable
{
    public Guid LlantaId { get; set; }
    public Llanta Llanta { get; set; } = null!;
    public Guid PosicionVehiculoId { get; set; }
    public PosicionVehiculo PosicionVehiculo { get; set; } = null!;
    public DateTimeOffset FechaInicio { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaFin { get; set; }
    public bool EsActiva { get; set; } = true;
    public Guid MovimientoOrigenId { get; set; }
    public decimal? KilometrajeMontaje { get; set; }
    public decimal? KilometrajeDesmontaje { get; set; }
    public decimal? KilometrajeRecorrido { get; set; }
}

public sealed class Movimiento : EntidadAuditable
{
    public string Numero { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;
    public Guid? InspeccionId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public ICollection<MovimientoDetalle> Detalles { get; set; } = [];
}

public sealed class MovimientoDetalle : EntidadAuditable
{
    public Guid MovimientoId { get; set; }
    public Movimiento Movimiento { get; set; } = null!;
    public Guid LlantaId { get; set; }
    public Llanta Llanta { get; set; } = null!;
    public Guid? PosicionOrigenId { get; set; }
    public Guid? PosicionDestinoId { get; set; }
    public TipoDestinoLlanta TipoDestino { get; set; }
    public Guid? CentroDestinoId { get; set; }
    public string? DestinoDescripcion { get; set; }
}

public sealed class ActividadProgramada : EntidadAuditable
{
    public string TipoActividad { get; set; } = string.Empty;
    public DateTimeOffset FechaProgramada { get; set; }
    public DateTimeOffset? FechaFinProgramada { get; set; }
    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;
    public Guid? VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }
    public Guid? PosicionVehiculoId { get; set; }
    public Guid? LlantaId { get; set; }
    public string TecnicoId { get; set; } = string.Empty;
    public Guid? TecnicoUsuarioId { get; set; }
    public UsuarioSistema? TecnicoUsuario { get; set; }
    public string Prioridad { get; set; } = "Media";
    public EstadoActividad Estado { get; set; } = EstadoActividad.Pendiente;
    public DateTimeOffset? FechaInicioReal { get; set; }
    public DateTimeOffset? FechaFinReal { get; set; }
    public string? Observaciones { get; set; }
    public string? MotivoCancelacion { get; set; }
    public Guid? GrupoProgramacionId { get; set; }
    public string? ReasignadoPor { get; set; }
}
