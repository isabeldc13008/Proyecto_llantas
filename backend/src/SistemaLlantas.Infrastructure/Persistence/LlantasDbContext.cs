using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Domain.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Persistence;

public sealed class LlantasDbContext(DbContextOptions<LlantasDbContext> options) : DbContext(options)
{
    public DbSet<Llanta> Llantas => Set<Llanta>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Referencia> Referencias => Set<Referencia>();
    public DbSet<Dimension> Dimensiones => Set<Dimension>();
    public DbSet<TipoLlanta> TiposLlanta => Set<TipoLlanta>();
    public DbSet<EstadoLlanta> EstadosLlanta => Set<EstadoLlanta>();
    public DbSet<Centro> Centros => Set<Centro>();
    public DbSet<Regional> Regionales => Set<Regional>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<EjeVehiculo> EjesVehiculo => Set<EjeVehiculo>();
    public DbSet<PosicionVehiculo> PosicionesVehiculo => Set<PosicionVehiculo>();
    public DbSet<ConfiguracionVehiculo> ConfiguracionesVehiculo => Set<ConfiguracionVehiculo>();
    public DbSet<ConfiguracionEje> ConfiguracionesEje => Set<ConfiguracionEje>();
    public DbSet<ConfiguracionPosicion> ConfiguracionesPosicion => Set<ConfiguracionPosicion>();
    public DbSet<Inspeccion> Inspecciones => Set<Inspeccion>();
    public DbSet<InspeccionDetalle> InspeccionesDetalle => Set<InspeccionDetalle>();
    public DbSet<CondicionLlanta> CondicionesLlanta => Set<CondicionLlanta>();
    public DbSet<CausaLlanta> CausasLlanta => Set<CausaLlanta>();
    public DbSet<RecomendacionInspeccion> RecomendacionesInspeccion => Set<RecomendacionInspeccion>();
    public DbSet<InconsistenciaInspeccion> InconsistenciasInspeccion => Set<InconsistenciaInspeccion>();
    public DbSet<LlantaTemporal> LlantasTemporales => Set<LlantaTemporal>();
    public DbSet<MovimientoLlanta> MovimientosLlanta => Set<MovimientoLlanta>();
    public DbSet<EvidenciaInspeccion> EvidenciasInspeccion => Set<EvidenciaInspeccion>();
    public DbSet<ParametroAlerta> ParametrosAlerta => Set<ParametroAlerta>();
    public DbSet<AlertaInspeccion> AlertasInspeccion => Set<AlertaInspeccion>();
    public DbSet<AlertaHistorial> AlertasHistorial => Set<AlertaHistorial>();
    public DbSet<ParametroReencauche> ParametrosReencauche => Set<ParametroReencauche>();
    public DbSet<AsignacionLlantaPosicion> AsignacionesLlantaPosicion => Set<AsignacionLlantaPosicion>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<MovimientoDetalle> MovimientosDetalle => Set<MovimientoDetalle>();
    public DbSet<ActividadProgramada> ActividadesProgramadas => Set<ActividadProgramada>();
    public DbSet<UsuarioSistema> UsuariosSistema => Set<UsuarioSistema>();
    public DbSet<RolSistema> RolesSistema => Set<RolSistema>();
    public DbSet<PermisoSistema> PermisosSistema => Set<PermisoSistema>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
    public DbSet<UsuarioCentro> UsuariosCentros => Set<UsuarioCentro>();
    public DbSet<SolicitudOperacion> SolicitudesOperacion => Set<SolicitudOperacion>();
    public DbSet<ProveedorServicio> ProveedoresServicio => Set<ProveedorServicio>();
    public DbSet<OrdenServicioLlanta> OrdenesServicioLlanta => Set<OrdenServicioLlanta>();
    public DbSet<LoteEnvioReparacion> LotesEnvioReparacion => Set<LoteEnvioReparacion>();
    public DbSet<EvidenciaFlujo> EvidenciasFlujo => Set<EvidenciaFlujo>();
    public DbSet<CargaMasiva> CargasMasivas => Set<CargaMasiva>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LlantasDbContext).Assembly);
        foreach (var type in modelBuilder.Model.GetEntityTypes().Where(x => typeof(EntidadAuditable).IsAssignableFrom(x.ClrType)).Select(x => x.ClrType))
            modelBuilder.Entity(type).Property(nameof(EntidadAuditable.RowVersion)).IsRowVersion();

        modelBuilder.Entity<Llanta>().HasQueryFilter(x => x.Activo);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var cambios = ChangeTracker.Entries<EntidadAuditable>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Select(x => new Auditoria
            {
                Usuario = x.State == EntityState.Added ? x.Entity.UsuarioCreacion : x.Entity.UsuarioModificacion ?? "sistema",
                Accion = x.State.ToString(), Entidad = x.Metadata.ClrType.Name, Identificador = x.Entity.Id.ToString(),
                ValoresAnteriores = x.State == EntityState.Modified ? JsonSerializer.Serialize(x.OriginalValues.Properties.ToDictionary(p => p.Name, p => EsSensible(p.Name) ? "***" : x.OriginalValues[p])) : null,
                ValoresNuevos = JsonSerializer.Serialize(x.CurrentValues.Properties.ToDictionary(p => p.Name, p => EsSensible(p.Name) ? "***" : x.CurrentValues[p]))
            }).ToList();
        Auditorias.AddRange(cambios);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private static bool EsSensible(string nombre) => nombre.Contains("password", StringComparison.OrdinalIgnoreCase) || nombre.Contains("token", StringComparison.OrdinalIgnoreCase);
}
