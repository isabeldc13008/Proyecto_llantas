using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaLlantas.Domain.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Persistence;

public sealed class LlantaConfiguration : IEntityTypeConfiguration<Llanta>
{
    public void Configure(EntityTypeBuilder<Llanta> b)
    {
        b.ToTable("TBL_Llanta"); b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Serial).HasMaxLength(100).IsRequired();
        b.Property(x => x.UbicacionActual).HasMaxLength(150).IsRequired();
        b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.Property(x => x.Costo).HasPrecision(18, 2);
        b.Property(x => x.ProfundidadInicial).HasPrecision(8, 2);
        b.Property(x => x.KilometrajeAcumulado).HasPrecision(18, 2);
        b.HasIndex(x => x.Codigo).IsUnique().HasDatabaseName("IX_Llanta_Codigo");
        b.HasIndex(x => x.Serial).IsUnique().HasDatabaseName("IX_Llanta_Serial");
        b.HasIndex(x => new { x.CentroId, x.EstadoLlantaId }).HasDatabaseName("IX_Llanta_CentroEstado");
        b.HasOne(x => x.Marca).WithMany().HasForeignKey(x => x.MarcaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Referencia).WithMany().HasForeignKey(x => x.ReferenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Dimension).WithMany().HasForeignKey(x => x.DimensionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoLlanta).WithMany().HasForeignKey(x => x.TipoLlantaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EstadoLlanta).WithMany().HasForeignKey(x => x.EstadoLlantaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Centro).WithMany().HasForeignKey(x => x.CentroId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CatalogosConfiguration :
    IEntityTypeConfiguration<Marca>, IEntityTypeConfiguration<Referencia>, IEntityTypeConfiguration<Dimension>,
    IEntityTypeConfiguration<TipoLlanta>, IEntityTypeConfiguration<EstadoLlanta>, IEntityTypeConfiguration<Centro>, IEntityTypeConfiguration<Regional>
{
    public void Configure(EntityTypeBuilder<Marca> b) { Base(b, "Marca"); }
    public void Configure(EntityTypeBuilder<Referencia> b) { Base(b, "Referencia"); b.HasOne(x => x.Marca).WithMany(x => x.Referencias).HasForeignKey(x => x.MarcaId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<Dimension> b) => Base(b, "Dimension");
    public void Configure(EntityTypeBuilder<TipoLlanta> b) => Base(b, "TipoLlanta");
    public void Configure(EntityTypeBuilder<EstadoLlanta> b) => Base(b, "EstadoLlanta");
    public void Configure(EntityTypeBuilder<Regional> b) => Base(b,"Regional");
    public void Configure(EntityTypeBuilder<Centro> b) { Base(b, "Centro"); b.Property(x => x.Relevancia).HasMaxLength(2); b.HasOne(x=>x.Regional).WithMany(x=>x.Centros).HasForeignKey(x=>x.RegionalId).OnDelete(DeleteBehavior.Restrict); }
    private static void Base<T>(EntityTypeBuilder<T> b, string nombre) where T : CatalogoBase
    {
        b.ToTable($"TBL_{nombre}"); b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(30).IsRequired(); b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique().HasDatabaseName($"IX_{nombre}_Codigo");
    }
}

public sealed class InspeccionesConfiguration :
    IEntityTypeConfiguration<Vehiculo>, IEntityTypeConfiguration<ConfiguracionVehiculo>, IEntityTypeConfiguration<ConfiguracionEje>, IEntityTypeConfiguration<ConfiguracionPosicion>, IEntityTypeConfiguration<EjeVehiculo>, IEntityTypeConfiguration<PosicionVehiculo>,
    IEntityTypeConfiguration<Inspeccion>, IEntityTypeConfiguration<InspeccionDetalle>, IEntityTypeConfiguration<InconsistenciaInspeccion>,
    IEntityTypeConfiguration<LlantaTemporal>, IEntityTypeConfiguration<MovimientoLlanta>, IEntityTypeConfiguration<EvidenciaInspeccion>,
    IEntityTypeConfiguration<ParametroReencauche>, IEntityTypeConfiguration<CondicionLlanta>, IEntityTypeConfiguration<CausaLlanta>,
    IEntityTypeConfiguration<RecomendacionInspeccion>
{
    public void Configure(EntityTypeBuilder<Vehiculo> b) { Base(b,"Vehiculo"); b.Property(x=>x.NumeroInterno).HasMaxLength(50).IsRequired(); b.Property(x=>x.Placa).HasMaxLength(20); b.Property(x=>x.Tipo).HasMaxLength(100); b.Property(x=>x.Estado).HasMaxLength(30).IsRequired(); b.Property(x=>x.Kilometraje).HasPrecision(18,2); b.HasIndex(x=>x.NumeroInterno).IsUnique(); b.HasOne(x=>x.Centro).WithMany().HasForeignKey(x=>x.CentroId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.ConfiguracionVehiculo).WithMany(x=>x.Vehiculos).HasForeignKey(x=>x.ConfiguracionVehiculoId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<ConfiguracionVehiculo> b) { Base(b,"ConfiguracionVehiculo"); b.Property(x=>x.Codigo).HasMaxLength(50).IsRequired(); b.Property(x=>x.Nombre).HasMaxLength(150).IsRequired(); b.Property(x=>x.TipoVehiculo).HasMaxLength(100).IsRequired(); b.HasIndex(x=>x.Codigo).IsUnique(); }
    public void Configure(EntityTypeBuilder<ConfiguracionEje> b) { Base(b,"ConfiguracionEje"); b.Property(x=>x.Nombre).HasMaxLength(100).IsRequired(); b.Property(x=>x.TipoEje).HasMaxLength(30).IsRequired(); b.HasIndex(x=>new{x.ConfiguracionVehiculoId,x.Orden}).IsUnique(); b.HasOne(x=>x.ConfiguracionVehiculo).WithMany(x=>x.Ejes).HasForeignKey(x=>x.ConfiguracionVehiculoId); }
    public void Configure(EntityTypeBuilder<ConfiguracionPosicion> b) { Base(b,"ConfiguracionPosicion"); b.Property(x=>x.Codigo).HasMaxLength(20).IsRequired(); b.Property(x=>x.Lado).HasMaxLength(20).IsRequired(); b.Property(x=>x.Ubicacion).HasMaxLength(20).IsRequired(); b.HasIndex(x=>new{x.ConfiguracionEjeId,x.Codigo}).IsUnique(); b.HasIndex(x=>new{x.ConfiguracionEjeId,x.Orden}).IsUnique(); b.HasOne(x=>x.ConfiguracionEje).WithMany(x=>x.Posiciones).HasForeignKey(x=>x.ConfiguracionEjeId); }
    public void Configure(EntityTypeBuilder<EjeVehiculo> b) { Base(b,"EjeVehiculo"); b.Property(x=>x.Nombre).HasMaxLength(100); b.Property(x=>x.TipoEje).HasMaxLength(30).IsRequired(); b.HasIndex(x=>new{x.VehiculoId,x.Numero}).IsUnique(); b.HasOne(x=>x.Vehiculo).WithMany(x=>x.Ejes).HasForeignKey(x=>x.VehiculoId); }
    public void Configure(EntityTypeBuilder<PosicionVehiculo> b) { Base(b,"PosicionVehiculo"); b.Property(x=>x.Codigo).HasMaxLength(20).IsRequired(); b.Property(x=>x.Lado).HasMaxLength(20); b.Property(x=>x.Ubicacion).HasMaxLength(20).IsRequired(); b.HasIndex(x=>new{x.EjeVehiculoId,x.Codigo}).IsUnique(); b.HasOne(x=>x.EjeVehiculo).WithMany(x=>x.Posiciones).HasForeignKey(x=>x.EjeVehiculoId); b.HasOne(x=>x.LlantaActual).WithMany().HasForeignKey(x=>x.LlantaActualId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<Inspeccion> b) { Base(b,"Inspeccion"); b.Property(x=>x.Kilometraje).HasPrecision(18,2); b.Property(x=>x.TecnicoId).HasMaxLength(150).IsRequired(); b.Property(x=>x.Observaciones).HasMaxLength(1000); b.HasOne(x=>x.Vehiculo).WithMany().HasForeignKey(x=>x.VehiculoId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.Centro).WithMany().HasForeignKey(x=>x.CentroId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<InspeccionDetalle> b) { Base(b,"InspeccionDetalle"); b.Property(x=>x.ProfundidadExterior).HasPrecision(8,2); b.Property(x=>x.ProfundidadCentro).HasPrecision(8,2); b.Property(x=>x.ProfundidadInterior).HasPrecision(8,2); b.Property(x=>x.Observaciones).HasMaxLength(1000); b.HasIndex(x=>new{x.InspeccionId,x.PosicionVehiculoId}).IsUnique(); b.HasOne(x=>x.Inspeccion).WithMany(x=>x.Detalles).HasForeignKey(x=>x.InspeccionId); b.HasOne(x=>x.PosicionVehiculo).WithMany().HasForeignKey(x=>x.PosicionVehiculoId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<InconsistenciaInspeccion> b) { Base(b,"InconsistenciaInspeccion"); b.Property(x=>x.IdentificadorEncontrado).HasMaxLength(100).IsRequired(); b.Property(x=>x.TecnicoId).HasMaxLength(150).IsRequired(); b.Property(x=>x.Observacion).HasMaxLength(1000); b.Property(x=>x.UsuarioAutorizador).HasMaxLength(150); b.Property(x=>x.ObservacionAutorizacion).HasMaxLength(1000); b.HasOne(x=>x.Inspeccion).WithMany().HasForeignKey(x=>x.InspeccionId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<LlantaTemporal> b) { Base(b,"LlantaTemporal"); b.Property(x=>x.IdentificadorTemporal).HasMaxLength(50).IsRequired(); b.Property(x=>x.IdentificadorFisico).HasMaxLength(100).IsRequired(); b.HasIndex(x=>x.InconsistenciaInspeccionId).IsUnique(); }
    public void Configure(EntityTypeBuilder<MovimientoLlanta> b) { Base(b,"MovimientoLlanta"); b.Property(x=>x.Motivo).HasMaxLength(300); b.Property(x=>x.TecnicoReporta).HasMaxLength(150); b.Property(x=>x.UsuarioAutoriza).HasMaxLength(150); b.Property(x=>x.Observaciones).HasMaxLength(1000); }
    public void Configure(EntityTypeBuilder<EvidenciaInspeccion> b) { Base(b,"EvidenciaInspeccion"); b.Property(x=>x.NombreArchivo).HasMaxLength(255); b.Property(x=>x.Ubicacion).HasMaxLength(1000); b.Property(x=>x.Hash).HasMaxLength(128); b.Property(x=>x.MimeType).HasMaxLength(100); }
    public void Configure(EntityTypeBuilder<ParametroReencauche> b) { Base(b,"ParametroReencauche"); b.Property(x=>x.ProfundidadMinima).HasPrecision(8,2); }
    public void Configure(EntityTypeBuilder<CondicionLlanta> b) => Catalogo(b,"CondicionLlanta");
    public void Configure(EntityTypeBuilder<CausaLlanta> b) => Catalogo(b,"CausaLlanta");
    public void Configure(EntityTypeBuilder<RecomendacionInspeccion> b) => Catalogo(b,"RecomendacionInspeccion");
    private static void Base<T>(EntityTypeBuilder<T> b,string tabla) where T:EntidadAuditable { b.ToTable($"TBL_{tabla}"); b.HasKey(x=>x.Id); }
    private static void Catalogo<T>(EntityTypeBuilder<T> b,string tabla) where T:CatalogoBase { Base(b,tabla); b.Property(x=>x.Codigo).HasMaxLength(30).IsRequired(); b.Property(x=>x.Nombre).HasMaxLength(150).IsRequired(); b.HasIndex(x=>x.Codigo).IsUnique(); }
}

public sealed class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> b)
    {
        b.ToTable("TBL_Auditoria"); b.HasKey(x => x.Id); b.Property(x => x.Usuario).HasMaxLength(150);
        b.Property(x => x.Accion).HasMaxLength(30); b.Property(x => x.Entidad).HasMaxLength(150);
        b.Property(x => x.Identificador).HasMaxLength(100); b.HasIndex(x => new { x.Entidad, x.Identificador, x.Fecha }).HasDatabaseName("IX_Auditoria_EntidadFecha");
    }
}

public sealed class AlertasConfiguration:IEntityTypeConfiguration<ParametroAlerta>,IEntityTypeConfiguration<AlertaInspeccion>,IEntityTypeConfiguration<AlertaHistorial>
{
 public void Configure(EntityTypeBuilder<ParametroAlerta> b){b.ToTable("TBL_ParametroAlerta");b.HasKey(x=>x.Id);b.Property(x=>x.Codigo).HasMaxLength(80).IsRequired();b.Property(x=>x.Valor).HasPrecision(10,2);b.Property(x=>x.Unidad).HasMaxLength(30);b.HasIndex(x=>x.Codigo).IsUnique();}
 public void Configure(EntityTypeBuilder<AlertaInspeccion> b){b.ToTable("TBL_AlertaInspeccion");b.HasKey(x=>x.Id);b.Property(x=>x.Tipo).HasMaxLength(80).IsRequired();b.Property(x=>x.Descripcion).HasMaxLength(1000).IsRequired();b.HasIndex(x=>new{x.CentroId,x.Estado,x.FechaCreacion});b.HasIndex(x=>new{x.InspeccionDetalleId,x.Tipo}).IsUnique();b.HasOne(x=>x.Inspeccion).WithMany().HasForeignKey(x=>x.InspeccionId).OnDelete(DeleteBehavior.Restrict);b.HasOne(x=>x.InspeccionDetalle).WithMany().HasForeignKey(x=>x.InspeccionDetalleId).OnDelete(DeleteBehavior.Restrict);}
 public void Configure(EntityTypeBuilder<AlertaHistorial> b){b.ToTable("TBL_AlertaHistorial");b.HasKey(x=>x.Id);b.Property(x=>x.Observacion).HasMaxLength(1000);b.HasOne(x=>x.Alerta).WithMany(x=>x.Historial).HasForeignKey(x=>x.AlertaInspeccionId);}
}

public sealed class SeguridadConfiguration : IEntityTypeConfiguration<UsuarioSistema>, IEntityTypeConfiguration<RolSistema>, IEntityTypeConfiguration<PermisoSistema>, IEntityTypeConfiguration<RolPermiso>, IEntityTypeConfiguration<UsuarioCentro>
{
    public void Configure(EntityTypeBuilder<UsuarioSistema> b){b.ToTable("TBL_Usuario");b.HasKey(x=>x.Id);b.HasIndex(x=>x.EntraObjectId).IsUnique().HasFilter("[EntraObjectId] IS NOT NULL"); b.Property(x=>x.Username).HasMaxLength(80).IsRequired();b.HasIndex(x=>x.Username).IsUnique();b.Property(x=>x.Nombre).HasMaxLength(150).IsRequired();b.Property(x=>x.PasswordHash).HasMaxLength(500).IsRequired();b.HasOne(x=>x.Rol).WithMany(x=>x.Usuarios).HasForeignKey(x=>x.RolId).OnDelete(DeleteBehavior.Restrict);b.HasOne(x=>x.Centro).WithMany().HasForeignKey(x=>x.CentroId).OnDelete(DeleteBehavior.Restrict);}
    public void Configure(EntityTypeBuilder<RolSistema> b){b.ToTable("TBL_Rol");b.HasKey(x=>x.Id);b.Property(x=>x.Codigo).HasMaxLength(60).IsRequired();b.HasIndex(x=>x.Codigo).IsUnique();b.Property(x=>x.Nombre).HasMaxLength(100).IsRequired();}
    public void Configure(EntityTypeBuilder<PermisoSistema> b){b.ToTable("TBL_Permiso");b.HasKey(x=>x.Id);b.Property(x=>x.Codigo).HasMaxLength(120).IsRequired();b.HasIndex(x=>x.Codigo).IsUnique();b.Property(x=>x.Nombre).HasMaxLength(160).IsRequired();}
    public void Configure(EntityTypeBuilder<RolPermiso> b){b.ToTable("TBL_RolPermiso");b.HasKey(x=>new{x.RolId,x.PermisoId});b.HasOne(x=>x.Rol).WithMany(x=>x.Permisos).HasForeignKey(x=>x.RolId);b.HasOne(x=>x.Permiso).WithMany(x=>x.Roles).HasForeignKey(x=>x.PermisoId);}
    public void Configure(EntityTypeBuilder<UsuarioCentro> b){b.ToTable("TBL_UsuarioCentro");b.HasKey(x=>x.Id);b.HasIndex(x=>new{x.UsuarioId,x.CentroId}).IsUnique();b.HasOne(x=>x.Usuario).WithMany(x=>x.Centros).HasForeignKey(x=>x.UsuarioId).OnDelete(DeleteBehavior.Cascade);b.HasOne(x=>x.Centro).WithMany().HasForeignKey(x=>x.CentroId).OnDelete(DeleteBehavior.Restrict);}
}

public sealed class OperacionesConfiguration : IEntityTypeConfiguration<AsignacionLlantaPosicion>, IEntityTypeConfiguration<Movimiento>, IEntityTypeConfiguration<MovimientoDetalle>, IEntityTypeConfiguration<ActividadProgramada>
{
    public void Configure(EntityTypeBuilder<AsignacionLlantaPosicion> b) { Base(b,"AsignacionLlantaPosicion"); b.Property(x=>x.KilometrajeMontaje).HasPrecision(18,2); b.Property(x=>x.KilometrajeDesmontaje).HasPrecision(18,2); b.Property(x=>x.KilometrajeRecorrido).HasPrecision(18,2); b.HasIndex(x=>x.LlantaId).IsUnique().HasFilter("[EsActiva] = 1").HasDatabaseName("UX_Asignacion_LlantaActiva"); b.HasIndex(x=>x.PosicionVehiculoId).IsUnique().HasFilter("[EsActiva] = 1").HasDatabaseName("UX_Asignacion_PosicionActiva"); b.HasOne(x=>x.Llanta).WithMany().HasForeignKey(x=>x.LlantaId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.PosicionVehiculo).WithMany().HasForeignKey(x=>x.PosicionVehiculoId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<Movimiento> b) { Base(b,"Movimiento"); b.Property(x=>x.Numero).HasMaxLength(30).IsRequired(); b.Property(x=>x.Tipo).HasMaxLength(50).IsRequired(); b.Property(x=>x.Motivo).HasMaxLength(500).IsRequired(); b.Property(x=>x.Observaciones).HasMaxLength(1000); b.Property(x=>x.Usuario).HasMaxLength(150).IsRequired(); b.HasIndex(x=>x.Numero).IsUnique(); b.HasOne(x=>x.Centro).WithMany().HasForeignKey(x=>x.CentroId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<MovimientoDetalle> b) { Base(b,"MovimientoDetalle"); b.Property(x=>x.DestinoDescripcion).HasMaxLength(300); b.HasOne(x=>x.Movimiento).WithMany(x=>x.Detalles).HasForeignKey(x=>x.MovimientoId); b.HasOne(x=>x.Llanta).WithMany().HasForeignKey(x=>x.LlantaId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<ActividadProgramada> b) { Base(b,"ActividadProgramada"); b.Property(x=>x.TipoActividad).HasMaxLength(50).IsRequired(); b.Property(x=>x.TecnicoId).HasMaxLength(150).IsRequired(); b.Property(x=>x.Prioridad).HasMaxLength(20); b.Property(x=>x.Observaciones).HasMaxLength(1000);b.Property(x=>x.MotivoCancelacion).HasMaxLength(500);b.Property(x=>x.ReasignadoPor).HasMaxLength(150);b.Property(x=>x.Origen).HasMaxLength(30).IsRequired();b.Property(x=>x.IdempotencyKey).HasMaxLength(100); b.HasIndex(x=>new{x.TecnicoId,x.Estado,x.FechaProgramada});b.HasIndex(x=>new{x.TecnicoUsuarioId,x.FechaProgramada,x.FechaFinProgramada});b.HasIndex(x=>x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");b.HasIndex(x=>new{x.Origen,x.OrigenEntidadId});b.HasIndex(x=>new{x.TecnicoUsuarioId,x.VehiculoId,x.TipoActividad,x.FechaProgramada}).IsUnique().HasFilter("[Activo] = 1 AND [Estado] <> 4 AND [TecnicoUsuarioId] IS NOT NULL"); b.HasOne(x=>x.Centro).WithMany().HasForeignKey(x=>x.CentroId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.Vehiculo).WithMany().HasForeignKey(x=>x.VehiculoId).OnDelete(DeleteBehavior.Restrict);b.HasOne(x=>x.TecnicoUsuario).WithMany().HasForeignKey(x=>x.TecnicoUsuarioId).OnDelete(DeleteBehavior.Restrict); }
    private static void Base<T>(EntityTypeBuilder<T> b,string tabla) where T:EntidadAuditable { b.ToTable($"TBL_{tabla}"); b.HasKey(x=>x.Id); }
}
