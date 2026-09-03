using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaLlantas.Domain.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Persistence;

public sealed class LlantasDbContext(DbContextOptions<LlantasDbContext> options)
    : DbContext(options)
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
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // CONFIGURACIONES DE LAS ENTIDADES
        // ============================================================

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LlantasDbContext).Assembly
        );


        // ============================================================
        // ROWVERSION PARA TODAS LAS ENTIDADES AUDITABLES
        // ============================================================

        foreach (var type in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(x =>
                         typeof(EntidadAuditable)
                             .IsAssignableFrom(x.ClrType))
                     .Select(x => x.ClrType))
        {
            modelBuilder
                .Entity(type)
                .Property(nameof(EntidadAuditable.RowVersion))
                .IsRowVersion();
        }


        // ============================================================
        // FILTROS GLOBALES
        // ============================================================

        modelBuilder.Entity<Llanta>()
            .HasQueryFilter(x => x.Activo);


        // ============================================================
        // ESTÁNDAR DE NOMBRAMIENTO POSTOBÓN
        // IMPORTANTE: DEBE EJECUTARSE AL FINAL
        // ============================================================

        AplicarEstandarPostobon(modelBuilder);
    }


    // =================================================================
    // CONVENCIÓN CORPORATIVA POSTOBÓN
    // =================================================================

    private static void AplicarEstandarPostobon(
        ModelBuilder modelBuilder)
    {
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            var nombreTabla = entidad.GetTableName();

            if (string.IsNullOrWhiteSpace(nombreTabla))
                continue;


            // =========================================================
            // 1. CAMPOS
            // =========================================================

            foreach (var propiedad in entidad.GetProperties())
            {
                var nombreActual = propiedad.Name;

                var tipo = Nullable.GetUnderlyingType(
                               propiedad.ClrType)
                           ?? propiedad.ClrType;

                var prefijo =
                    ObtenerPrefijoPostobon(
                        tipo,
                        propiedad);

                if (string.IsNullOrWhiteSpace(prefijo))
                    continue;

                propiedad.SetColumnName(
                    $"{prefijo}{nombreActual}");
            }


            // =========================================================
            // 2. CLAVE PRIMARIA
            // PK_<NombreTabla>
            //
            // Ejemplo:
            // PK_TBL_Llanta
            // =========================================================

            var clavePrimaria = entidad.FindPrimaryKey();

            if (clavePrimaria != null)
            {
                clavePrimaria.SetName(
                    $"PK_{nombreTabla}");
            }


            // =========================================================
            // 3. ÍNDICES
            // IX_<NombreTablaSinTBL>_<Campos>
            //
            // Ejemplo:
            // IX_Llanta_SCodigo
            // IX_Llanta_GCentroId_GEstadoLlantaId
            // =========================================================

            foreach (var indice in entidad.GetIndexes())
            {
                var tablaIndice =
                    nombreTabla.StartsWith(
                        "TBL_",
                        StringComparison.OrdinalIgnoreCase)
                        ? nombreTabla[4..]
                        : nombreTabla;

                var campos = indice.Properties
                    .Select(p =>
                        p.GetColumnName(
                            StoreObjectIdentifier.Table(
                                nombreTabla,
                                entidad.GetSchema()))
                        ?? p.Name)
                    .ToList();

                var nombreIndice =
                    $"IX_{tablaIndice}_{string.Join("_", campos)}";

                indice.SetDatabaseName(nombreIndice);
            }


            // =========================================================
            // 4. CLAVES FORÁNEAS
            //
            // El documento establece FK_<TablaForanea>.
            //
            // Sin embargo, SQL Server exige nombres únicos para los
            // constraints dentro de la base de datos.
            //
            // Para evitar colisiones:
            //
            // FK_<TablaActual>_<TablaForanea>_<Campo>
            //
            // Ejemplo:
            // FK_TBL_Llanta_TBL_Centro_GCentroId
            // =========================================================

            foreach (var fk in entidad.GetForeignKeys())
            {
                var tablaPrincipal =
                    fk.PrincipalEntityType.GetTableName();

                if (string.IsNullOrWhiteSpace(tablaPrincipal))
                    continue;

                var camposFk = fk.Properties
                    .Select(p =>
                        p.GetColumnName(
                            StoreObjectIdentifier.Table(
                                nombreTabla,
                                entidad.GetSchema()))
                        ?? p.Name)
                    .ToList();

                var nombreFk =
                    $"FK_{nombreTabla}_{tablaPrincipal}_{string.Join("_", camposFk)}";

                fk.SetConstraintName(nombreFk);
            }
        }
    }


    // =================================================================
    // PREFIJO DE CAMPO SEGÚN TIPO DE DATO
    // =================================================================

    private static string ObtenerPrefijoPostobon(
        Type tipo,
        IMutableProperty propiedad)
    {
        // -------------------------------------------------------------
        // TIMESTAMP / ROWVERSION
        // -------------------------------------------------------------

        if (tipo == typeof(byte[])
            && propiedad.IsConcurrencyToken)
        {
            return "T";
        }


        // -------------------------------------------------------------
        // GUID
        // -------------------------------------------------------------

        if (tipo == typeof(Guid))
        {
            return "G";
        }


        // -------------------------------------------------------------
        // CARÁCTER
        // -------------------------------------------------------------

        if (tipo == typeof(string)
            || tipo == typeof(char))
        {
            return "S";
        }


        // -------------------------------------------------------------
        // FECHA
        // -------------------------------------------------------------

        if (tipo == typeof(DateTime)
            || tipo == typeof(DateTimeOffset)
            || tipo == typeof(DateOnly)
            || tipo == typeof(TimeOnly)
            || tipo == typeof(TimeSpan))
        {
            return "D";
        }


        // -------------------------------------------------------------
        // BOOLEANO
        // -------------------------------------------------------------

        if (tipo == typeof(bool))
        {
            return "B";
        }


        // -------------------------------------------------------------
        // NUMÉRICO
        // -------------------------------------------------------------

        if (tipo == typeof(byte)
            || tipo == typeof(sbyte)
            || tipo == typeof(short)
            || tipo == typeof(ushort)
            || tipo == typeof(int)
            || tipo == typeof(uint)
            || tipo == typeof(long)
            || tipo == typeof(ulong)
            || tipo == typeof(float)
            || tipo == typeof(double)
            || tipo == typeof(decimal))
        {
            return "N";
        }


        // -------------------------------------------------------------
        // ENUM
        // EF normalmente los guarda como número.
        // -------------------------------------------------------------

        if (tipo.IsEnum)
        {
            return "N";
        }


        return "";
    }


    // =================================================================
    // AUDITORÍA
    // =================================================================

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var cambios =
            ChangeTracker
                .Entries<EntidadAuditable>()
                .Where(x =>
                    x.State is EntityState.Added
                        or EntityState.Modified)
                .Select(x => new Auditoria
                {
                    Usuario =
                        x.State == EntityState.Added
                            ? x.Entity.UsuarioCreacion
                            : x.Entity.UsuarioModificacion
                              ?? "sistema",

                    Accion =
                        x.State.ToString(),

                    Entidad =
                        x.Metadata.ClrType.Name,

                    Identificador =
                        x.Entity.Id.ToString(),

                    ValoresAnteriores =
                        x.State == EntityState.Modified
                            ? JsonSerializer.Serialize(
                                x.OriginalValues
                                    .Properties
                                    .ToDictionary(
                                        p => p.Name,
                                        p => EsSensible(p.Name)
                                            ? "***"
                                            : x.OriginalValues[p]))
                            : null,

                    ValoresNuevos =
                        JsonSerializer.Serialize(
                            x.CurrentValues
                                .Properties
                                .ToDictionary(
                                    p => p.Name,
                                    p => EsSensible(p.Name)
                                        ? "***"
                                        : x.CurrentValues[p]))
                })
                .ToList();

        Auditorias.AddRange(cambios);

        return await base.SaveChangesAsync(
            cancellationToken);
    }


    private static bool EsSensible(string nombre)
        => nombre.Contains(
               "password",
               StringComparison.OrdinalIgnoreCase)
           || nombre.Contains(
               "token",
               StringComparison.OrdinalIgnoreCase);
}