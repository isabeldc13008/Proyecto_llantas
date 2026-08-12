using SistemaLlantas.Domain.Common;

namespace SistemaLlantas.Domain.Entities;

public enum EstadoInspeccion { Borrador, Finalizada, Anulada }
public enum EstadoInconsistencia { PendienteAutorizacion, Autorizada, Rechazada, Regularizada }

public sealed class Vehiculo : EntidadAuditable
{
    public string NumeroInterno { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;
    public ICollection<EjeVehiculo> Ejes { get; set; } = [];
}

public sealed class EjeVehiculo : EntidadAuditable
{
    public Guid VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public int Numero { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<PosicionVehiculo> Posiciones { get; set; } = [];
}

public sealed class PosicionVehiculo : EntidadAuditable
{
    public Guid EjeVehiculoId { get; set; }
    public EjeVehiculo EjeVehiculo { get; set; } = null!;
    public string Codigo { get; set; } = string.Empty;
    public string Lado { get; set; } = string.Empty;
    public int Orden { get; set; }
    public Guid? LlantaActualId { get; set; }
    public Llanta? LlantaActual { get; set; }
}

public sealed class CondicionLlanta : CatalogoBase { public bool RequiereCausa { get; set; } }
public sealed class CausaLlanta : CatalogoBase { }
public sealed class RecomendacionInspeccion : CatalogoBase { public bool EsCandidataReencauche { get; set; } }

public sealed class Inspeccion : EntidadAuditable
{
    public Guid VehiculoId { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public Guid CentroId { get; set; }
    public Centro Centro { get; set; } = null!;
    public decimal? Kilometraje { get; set; }
    public EstadoInspeccion Estado { get; set; } = EstadoInspeccion.Borrador;
    public string TecnicoId { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public ICollection<InspeccionDetalle> Detalles { get; set; } = [];
}

public sealed class InspeccionDetalle : EntidadAuditable
{
    public Guid InspeccionId { get; set; }
    public Inspeccion Inspeccion { get; set; } = null!;
    public Guid PosicionVehiculoId { get; set; }
    public PosicionVehiculo PosicionVehiculo { get; set; } = null!;
    public Guid? LlantaId { get; set; }
    public Llanta? Llanta { get; set; }
    public decimal? ProfundidadExterior { get; set; }
    public decimal? ProfundidadCentro { get; set; }
    public decimal? ProfundidadInterior { get; set; }
    public Guid? CondicionLlantaId { get; set; }
    public CondicionLlanta? CondicionLlanta { get; set; }
    public Guid? CausaLlantaId { get; set; }
    public CausaLlanta? CausaLlanta { get; set; }
    public Guid? RecomendacionId { get; set; }
    public RecomendacionInspeccion? Recomendacion { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class InconsistenciaInspeccion : EntidadAuditable
{
    public Guid InspeccionId { get; set; }
    public Inspeccion Inspeccion { get; set; } = null!;
    public Guid PosicionVehiculoId { get; set; }
    public PosicionVehiculo PosicionVehiculo { get; set; } = null!;
    public Guid? LlantaEsperadaId { get; set; }
    public Llanta? LlantaEsperada { get; set; }
    public string IdentificadorEncontrado { get; set; } = string.Empty;
    public string TecnicoId { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
    public EstadoInconsistencia Estado { get; set; } = EstadoInconsistencia.PendienteAutorizacion;
    public string? UsuarioAutorizador { get; set; }
    public DateTimeOffset? FechaAutorizacion { get; set; }
    public string? ObservacionAutorizacion { get; set; }
    public LlantaTemporal? LlantaTemporal { get; set; }
}

public sealed class LlantaTemporal : EntidadAuditable
{
    public Guid InconsistenciaInspeccionId { get; set; }
    public InconsistenciaInspeccion InconsistenciaInspeccion { get; set; } = null!;
    public string IdentificadorTemporal { get; set; } = string.Empty;
    public string IdentificadorFisico { get; set; } = string.Empty;
    public EstadoInconsistencia Estado { get; set; } = EstadoInconsistencia.PendienteAutorizacion;
}

public sealed class MovimientoLlanta : EntidadAuditable
{
    public Guid InspeccionId { get; set; }
    public Guid InconsistenciaInspeccionId { get; set; }
    public Guid PosicionVehiculoId { get; set; }
    public Guid? LlantaAnteriorId { get; set; }
    public Guid? LlantaNuevaId { get; set; }
    public Guid CentroId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string TecnicoReporta { get; set; } = string.Empty;
    public string UsuarioAutoriza { get; set; } = string.Empty;
    public DateTimeOffset FechaReporte { get; set; }
    public DateTimeOffset FechaAutorizacion { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class EvidenciaInspeccion : EntidadAuditable
{
    public Guid? InspeccionId { get; set; }
    public Guid? InconsistenciaInspeccionId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}

public sealed class ParametroReencauche : EntidadAuditable
{
    public Guid? DimensionId { get; set; }
    public int MaximoReencauches { get; set; }
    public decimal ProfundidadMinima { get; set; }
    public DateOnly VigenteDesde { get; set; }
    public DateOnly? VigenteHasta { get; set; }
}
