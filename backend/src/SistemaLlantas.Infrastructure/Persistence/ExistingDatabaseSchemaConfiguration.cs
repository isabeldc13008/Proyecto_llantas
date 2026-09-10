using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Persistence;

// Physical names verified against the user-provided GDLLSQLDLLO schema (2026-09-09).
internal static class ExistingDatabaseSchemaConfiguration
{
    internal static void Configure(ModelBuilder modelBuilder)
    {
        {
            var b = modelBuilder.Entity<ActividadProgramada>();
            b.ToTable("TBL_ActividadProgramada", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.Estado).HasColumnName("NEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaFinProgramada).HasColumnName("DFechaFinProgramada");
            b.Property(x => x.FechaFinReal).HasColumnName("DFechaFinReal");
            b.Property(x => x.FechaInicioReal).HasColumnName("DFechaInicioReal");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.FechaProgramada).HasColumnName("DFechaProgramada");
            b.Property(x => x.GrupoProgramacionId).HasColumnName("GGrupoProgramacionId");
            b.Property(x => x.IdempotencyKey).HasColumnName("SIdempotencyKey");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.MotivoCancelacion).HasColumnName("SMotivoCancelacion");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.Origen).HasColumnName("SOrigen");
            b.Property(x => x.OrigenEntidadId).HasColumnName("GOrigenEntidadId");
            b.Property(x => x.PosicionVehiculoId).HasColumnName("GPosicionVehiculoId");
            b.Property(x => x.Prioridad).HasColumnName("SPrioridad");
            b.Property(x => x.ReasignadoPor).HasColumnName("SReasignadoPor");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TecnicoId).HasColumnName("STecnicoId");
            b.Property(x => x.TecnicoUsuarioId).HasColumnName("GTecnicoUsuarioId");
            b.Property(x => x.TipoActividad).HasColumnName("STipoActividad");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.VehiculoId).HasColumnName("GVehiculoId");
            b.HasIndex(x => x.CentroId).HasDatabaseName("IX_ActividadProgramada_GCentroId").HasFilter(null);
            b.HasIndex(x => new { x.TecnicoUsuarioId, x.FechaProgramada, x.FechaFinProgramada }).HasDatabaseName("IX_ActividadProgramada_GTecnicoUsuarioId_DFechaProgramada_DFechaFinProgramada").HasFilter(null);
            b.HasIndex(x => new { x.TecnicoUsuarioId, x.VehiculoId, x.TipoActividad, x.FechaProgramada }).HasDatabaseName("IX_ActividadProgramada_GTecnicoUsuarioId_GVehiculoId_STipoActividad_DFechaProgramada").HasFilter("([BActivo]=(1) AND [NEstado]<>(4) AND [GTecnicoUsuarioId] IS NOT NULL)");
            b.HasIndex(x => x.VehiculoId).HasDatabaseName("IX_ActividadProgramada_GVehiculoId").HasFilter(null);
            b.HasIndex(x => x.IdempotencyKey).HasDatabaseName("IX_ActividadProgramada_SIdempotencyKey").HasFilter("([SIdempotencyKey] IS NOT NULL)");
            b.HasIndex(x => new { x.Origen, x.OrigenEntidadId }).HasDatabaseName("IX_ActividadProgramada_SOrigen_GOrigenEntidadId").HasFilter(null);
            b.HasIndex(x => new { x.TecnicoId, x.Estado, x.FechaProgramada }).HasDatabaseName("IX_ActividadProgramada_STecnicoId_NEstado_DFechaProgramada").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_ActividadProgramada");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_ActividadProgramada_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "TecnicoUsuarioId" })).SetConstraintName("FK_TBL_ActividadProgramada_TBL_Usuario_GTecnicoUsuarioId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "VehiculoId" })).SetConstraintName("FK_TBL_ActividadProgramada_TBL_Vehiculo_GVehiculoId");
        }
        {
            var b = modelBuilder.Entity<AlertaHistorial>();
            b.ToTable("TBL_AlertaHistorial", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.AlertaInspeccionId).HasColumnName("GAlertaInspeccionId");
            b.Property(x => x.EstadoAnterior).HasColumnName("NEstadoAnterior");
            b.Property(x => x.EstadoNuevo).HasColumnName("NEstadoNuevo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Observacion).HasColumnName("SObservacion");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.AlertaInspeccionId).HasDatabaseName("IX_AlertaHistorial_GAlertaInspeccionId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_AlertaHistorial");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "AlertaInspeccionId" })).SetConstraintName("FK_TBL_AlertaHistorial_TBL_AlertaInspeccion_GAlertaInspeccionId");
        }
        {
            var b = modelBuilder.Entity<AlertaInspeccion>();
            b.ToTable("TBL_AlertaInspeccion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.Descripcion).HasColumnName("SDescripcion");
            b.Property(x => x.Estado).HasColumnName("NEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.InspeccionDetalleId).HasColumnName("GInspeccionDetalleId");
            b.Property(x => x.InspeccionId).HasColumnName("GInspeccionId");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.PosicionVehiculoId).HasColumnName("GPosicionVehiculoId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Tipo).HasColumnName("STipo");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.VehiculoId).HasColumnName("GVehiculoId");
            b.HasIndex(x => new { x.CentroId, x.Estado, x.FechaCreacion }).HasDatabaseName("IX_AlertaInspeccion_GCentroId_NEstado_DFechaCreacion").HasFilter(null);
            b.HasIndex(x => new { x.InspeccionDetalleId, x.Tipo }).HasDatabaseName("IX_AlertaInspeccion_GInspeccionDetalleId_STipo").HasFilter(null);
            b.HasIndex(x => x.InspeccionId).HasDatabaseName("IX_AlertaInspeccion_GInspeccionId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_AlertaInspeccion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "InspeccionDetalleId" })).SetConstraintName("FK_TBL_AlertaInspeccion_TBL_InspeccionDetalle_GInspeccionDetalleId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "InspeccionId" })).SetConstraintName("FK_TBL_AlertaInspeccion_TBL_Inspeccion_GInspeccionId");
        }
        {
            var b = modelBuilder.Entity<AsignacionLlantaPosicion>();
            b.ToTable("TBL_AsignacionLlantaPosicion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.EsActiva).HasColumnName("BEsActiva");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaFin).HasColumnName("DFechaFin");
            b.Property(x => x.FechaInicio).HasColumnName("DFechaInicio");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.KilometrajeDesmontaje).HasColumnName("NKilometrajeDesmontaje");
            b.Property(x => x.KilometrajeMontaje).HasColumnName("NKilometrajeMontaje");
            b.Property(x => x.KilometrajeRecorrido).HasColumnName("NKilometrajeRecorrido");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.MovimientoOrigenId).HasColumnName("GMovimientoOrigenId");
            b.Property(x => x.PosicionVehiculoId).HasColumnName("GPosicionVehiculoId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.LlantaId).HasDatabaseName("IX_AsignacionLlantaPosicion_GLlantaId").HasFilter("([BEsActiva]=(1))");
            b.HasIndex(x => x.PosicionVehiculoId).HasDatabaseName("IX_AsignacionLlantaPosicion_GPosicionVehiculoId").HasFilter("([BEsActiva]=(1))");
            b.HasKey(x => x.Id).HasName("PK_TBL_AsignacionLlantaPosicion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaId" })).SetConstraintName("FK_TBL_AsignacionLlantaPosicion_TBL_Llanta_GLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "PosicionVehiculoId" })).SetConstraintName("FK_TBL_AsignacionLlantaPosicion_TBL_PosicionVehiculo_GPosicionVehiculoId");
        }
        {
            var b = modelBuilder.Entity<Auditoria>();
            b.ToTable("TBL_Auditoria", "dbo");
            b.Property(x => x.Id).HasColumnName("NId");
            b.Property(x => x.Accion).HasColumnName("SAccion");
            b.Property(x => x.DireccionIp).HasColumnName("SDireccionIp");
            b.Property(x => x.Entidad).HasColumnName("SEntidad");
            b.Property(x => x.Fecha).HasColumnName("DFecha");
            b.Property(x => x.Identificador).HasColumnName("SIdentificador");
            b.Property(x => x.Origen).HasColumnName("SOrigen");
            b.Property(x => x.Usuario).HasColumnName("SUsuario");
            b.Property(x => x.ValoresAnteriores).HasColumnName("SValoresAnteriores");
            b.Property(x => x.ValoresNuevos).HasColumnName("SValoresNuevos");
            b.HasIndex(x => new { x.Entidad, x.Identificador, x.Fecha }).HasDatabaseName("IX_Auditoria_SEntidad_SIdentificador_DFecha").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Auditoria");
        }
        {
            var b = modelBuilder.Entity<CargaMasiva>();
            b.ToTable("TBL_CargaMasiva", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.ErroresJson).HasColumnName("SErroresJson");
            b.Property(x => x.Estado).HasColumnName("SEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.FechaProcesamiento).HasColumnName("DFechaProcesamiento");
            b.Property(x => x.FilasConError).HasColumnName("NFilasConError");
            b.Property(x => x.FilasJson).HasColumnName("SFilasJson");
            b.Property(x => x.FilasValidas).HasColumnName("NFilasValidas");
            b.Property(x => x.NombreArchivo).HasColumnName("SNombreArchivo");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Tipo).HasColumnName("STipo");
            b.Property(x => x.TotalFilas).HasColumnName("NTotalFilas");
            b.Property(x => x.Usuario).HasColumnName("SUsuario");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => new { x.Usuario, x.FechaCreacion }).HasDatabaseName("IX_CargaMasiva_SUsuario_DFechaCreacion").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_CargaMasiva");
        }
        {
            var b = modelBuilder.Entity<CausaLlanta>();
            b.ToTable("TBL_CausaLlanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_CausaLlanta_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_CausaLlanta");
        }
        {
            var b = modelBuilder.Entity<Centro>();
            b.ToTable("TBL_Centro", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RegionalId).HasColumnName("GRegionalId");
            b.Property(x => x.Relevancia).HasColumnName("SRelevancia");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.RegionalId).HasDatabaseName("IX_Centro_GRegionalId").HasFilter(null);
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Centro_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Centro");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "RegionalId" })).SetConstraintName("FK_TBL_Centro_TBL_Regional_GRegionalId");
        }
        {
            var b = modelBuilder.Entity<CondicionLlanta>();
            b.ToTable("TBL_CondicionLlanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RequiereCausa).HasColumnName("BRequiereCausa");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_CondicionLlanta_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_CondicionLlanta");
        }
        {
            var b = modelBuilder.Entity<ConfiguracionEje>();
            b.ToTable("TBL_ConfiguracionEje", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.ConfiguracionVehiculoId).HasColumnName("GConfiguracionVehiculoId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.Orden).HasColumnName("NOrden");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TipoEje).HasColumnName("STipoEje");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => new { x.ConfiguracionVehiculoId, x.Orden }).HasDatabaseName("IX_ConfiguracionEje_GConfiguracionVehiculoId_NOrden").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_ConfiguracionEje");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "ConfiguracionVehiculoId" })).SetConstraintName("FK_TBL_ConfiguracionEje_TBL_ConfiguracionVehiculo_GConfiguracionVehiculoId");
        }
        {
            var b = modelBuilder.Entity<ConfiguracionPosicion>();
            b.ToTable("TBL_ConfiguracionPosicion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.ConfiguracionEjeId).HasColumnName("GConfiguracionEjeId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Lado).HasColumnName("SLado");
            b.Property(x => x.Orden).HasColumnName("NOrden");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Ubicacion).HasColumnName("SUbicacion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => new { x.ConfiguracionEjeId, x.Orden }).HasDatabaseName("IX_ConfiguracionPosicion_GConfiguracionEjeId_NOrden").HasFilter(null);
            b.HasIndex(x => new { x.ConfiguracionEjeId, x.Codigo }).HasDatabaseName("IX_ConfiguracionPosicion_GConfiguracionEjeId_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_ConfiguracionPosicion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "ConfiguracionEjeId" })).SetConstraintName("FK_TBL_ConfiguracionPosicion_TBL_ConfiguracionEje_GConfiguracionEjeId");
        }
        {
            var b = modelBuilder.Entity<ConfiguracionVehiculo>();
            b.ToTable("TBL_ConfiguracionVehiculo", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TipoVehiculo).HasColumnName("STipoVehiculo");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_ConfiguracionVehiculo_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_ConfiguracionVehiculo");
        }
        {
            var b = modelBuilder.Entity<Dimension>();
            b.ToTable("TBL_Dimension", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Dimension_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Dimension");
        }
        {
            var b = modelBuilder.Entity<EjeVehiculo>();
            b.ToTable("TBL_EjeVehiculo", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.Numero).HasColumnName("NNumero");
            b.Property(x => x.Orden).HasColumnName("NOrden");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TipoEje).HasColumnName("STipoEje");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.VehiculoId).HasColumnName("GVehiculoId");
            b.HasIndex(x => new { x.VehiculoId, x.Numero }).HasDatabaseName("IX_EjeVehiculo_GVehiculoId_NNumero").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_EjeVehiculo");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "VehiculoId" })).SetConstraintName("FK_TBL_EjeVehiculo_TBL_Vehiculo_GVehiculoId");
        }
        {
            var b = modelBuilder.Entity<EstadoLlanta>();
            b.ToTable("TBL_EstadoLlanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.EsDisposicionFinal).HasColumnName("BEsDisposicionFinal");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.PermiteMontaje).HasColumnName("BPermiteMontaje");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_EstadoLlanta_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_EstadoLlanta");
        }
        {
            var b = modelBuilder.Entity<EvidenciaFlujo>();
            b.ToTable("TBL_EvidenciaFlujo", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Hash).HasColumnName("SHash");
            b.Property(x => x.MimeType).HasColumnName("SMimeType");
            b.Property(x => x.NombreArchivo).HasColumnName("SNombreArchivo");
            b.Property(x => x.OrdenServicioLlantaId).HasColumnName("GOrdenServicioLlantaId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TamanoBytes).HasColumnName("NTamanoBytes");
            b.Property(x => x.Ubicacion).HasColumnName("SUbicacion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.OrdenServicioLlantaId).HasDatabaseName("IX_EvidenciaFlujo_GOrdenServicioLlantaId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_EvidenciaFlujo");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "OrdenServicioLlantaId" })).SetConstraintName("FK_TBL_EvidenciaFlujo_TBL_OrdenServicioLlanta_GOrdenServicioLlantaId");
        }
        {
            var b = modelBuilder.Entity<EvidenciaInspeccion>();
            b.ToTable("TBL_EvidenciaInspeccion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Hash).HasColumnName("SHash");
            b.Property(x => x.InconsistenciaInspeccionId).HasColumnName("GInconsistenciaInspeccionId");
            b.Property(x => x.InspeccionId).HasColumnName("GInspeccionId");
            b.Property(x => x.MimeType).HasColumnName("SMimeType");
            b.Property(x => x.NombreArchivo).HasColumnName("SNombreArchivo");
            b.Property(x => x.RetenerHasta).HasColumnName("DRetenerHasta");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TamanoBytes).HasColumnName("NTamanoBytes");
            b.Property(x => x.Ubicacion).HasColumnName("SUbicacion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasKey(x => x.Id).HasName("PK_TBL_EvidenciaInspeccion");
        }
        {
            var b = modelBuilder.Entity<InconsistenciaInspeccion>();
            b.ToTable("TBL_InconsistenciaInspeccion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Estado).HasColumnName("NEstado");
            b.Property(x => x.FechaAutorizacion).HasColumnName("DFechaAutorizacion");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.IdentificadorEncontrado).HasColumnName("SIdentificadorEncontrado");
            b.Property(x => x.InspeccionId).HasColumnName("GInspeccionId");
            b.Property(x => x.LlantaEncontradaId).HasColumnName("GLlantaEncontradaId");
            b.Property(x => x.LlantaEsperadaId).HasColumnName("GLlantaEsperadaId");
            b.Property(x => x.Observacion).HasColumnName("SObservacion");
            b.Property(x => x.ObservacionAutorizacion).HasColumnName("SObservacionAutorizacion");
            b.Property(x => x.PosicionVehiculoId).HasColumnName("GPosicionVehiculoId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TecnicoId).HasColumnName("STecnicoId");
            b.Property(x => x.UsuarioAutorizador).HasColumnName("SUsuarioAutorizador");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.InspeccionId).HasDatabaseName("IX_InconsistenciaInspeccion_GInspeccionId").HasFilter(null);
            b.HasIndex(x => x.LlantaEncontradaId).HasDatabaseName("IX_InconsistenciaInspeccion_GLlantaEncontradaId").HasFilter(null);
            b.HasIndex(x => x.LlantaEsperadaId).HasDatabaseName("IX_InconsistenciaInspeccion_GLlantaEsperadaId").HasFilter(null);
            b.HasIndex(x => x.PosicionVehiculoId).HasDatabaseName("IX_InconsistenciaInspeccion_GPosicionVehiculoId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_InconsistenciaInspeccion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "InspeccionId" })).SetConstraintName("FK_TBL_InconsistenciaInspeccion_TBL_Inspeccion_GInspeccionId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaEncontradaId" })).SetConstraintName("FK_TBL_InconsistenciaInspeccion_TBL_Llanta_GLlantaEncontradaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaEsperadaId" })).SetConstraintName("FK_TBL_InconsistenciaInspeccion_TBL_Llanta_GLlantaEsperadaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "PosicionVehiculoId" })).SetConstraintName("FK_TBL_InconsistenciaInspeccion_TBL_PosicionVehiculo_GPosicionVehiculoId");
        }
        {
            var b = modelBuilder.Entity<Inspeccion>();
            b.ToTable("TBL_Inspeccion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.Estado).HasColumnName("NEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Kilometraje).HasColumnName("NKilometraje");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TecnicoId).HasColumnName("STecnicoId");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.VehiculoId).HasColumnName("GVehiculoId");
            b.HasIndex(x => x.CentroId).HasDatabaseName("IX_Inspeccion_GCentroId").HasFilter(null);
            b.HasIndex(x => x.VehiculoId).HasDatabaseName("IX_Inspeccion_GVehiculoId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Inspeccion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_Inspeccion_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "VehiculoId" })).SetConstraintName("FK_TBL_Inspeccion_TBL_Vehiculo_GVehiculoId");
        }
        {
            var b = modelBuilder.Entity<InspeccionDetalle>();
            b.ToTable("TBL_InspeccionDetalle", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CausaLlantaId).HasColumnName("GCausaLlantaId");
            b.Property(x => x.CondicionLlantaId).HasColumnName("GCondicionLlantaId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.InspeccionId).HasColumnName("GInspeccionId");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.PosicionVehiculoId).HasColumnName("GPosicionVehiculoId");
            b.Property(x => x.ProfundidadCentro).HasColumnName("NProfundidadCentro");
            b.Property(x => x.ProfundidadExterior).HasColumnName("NProfundidadExterior");
            b.Property(x => x.ProfundidadInterior).HasColumnName("NProfundidadInterior");
            b.Property(x => x.RecomendacionId).HasColumnName("GRecomendacionId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.CausaLlantaId).HasDatabaseName("IX_InspeccionDetalle_GCausaLlantaId").HasFilter(null);
            b.HasIndex(x => x.CondicionLlantaId).HasDatabaseName("IX_InspeccionDetalle_GCondicionLlantaId").HasFilter(null);
            b.HasIndex(x => new { x.InspeccionId, x.PosicionVehiculoId }).HasDatabaseName("IX_InspeccionDetalle_GInspeccionId_GPosicionVehiculoId").HasFilter(null);
            b.HasIndex(x => x.LlantaId).HasDatabaseName("IX_InspeccionDetalle_GLlantaId").HasFilter(null);
            b.HasIndex(x => x.PosicionVehiculoId).HasDatabaseName("IX_InspeccionDetalle_GPosicionVehiculoId").HasFilter(null);
            b.HasIndex(x => x.RecomendacionId).HasDatabaseName("IX_InspeccionDetalle_GRecomendacionId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_InspeccionDetalle");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CausaLlantaId" })).SetConstraintName("FK_TBL_InspeccionDetalle_TBL_CausaLlanta_GCausaLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CondicionLlantaId" })).SetConstraintName("FK_TBL_InspeccionDetalle_TBL_CondicionLlanta_GCondicionLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "InspeccionId" })).SetConstraintName("FK_TBL_InspeccionDetalle_TBL_Inspeccion_GInspeccionId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaId" })).SetConstraintName("FK_TBL_InspeccionDetalle_TBL_Llanta_GLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "PosicionVehiculoId" })).SetConstraintName("FK_TBL_InspeccionDetalle_TBL_PosicionVehiculo_GPosicionVehiculoId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "RecomendacionId" })).SetConstraintName("FK_TBL_InspeccionDetalle_TBL_RecomendacionInspeccion_GRecomendacionId");
        }
        {
            var b = modelBuilder.Entity<Llanta>();
            b.ToTable("TBL_Llanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.Costo).HasColumnName("NCosto");
            b.Property(x => x.DimensionId).HasColumnName("GDimensionId");
            b.Property(x => x.EstadoLlantaId).HasColumnName("GEstadoLlantaId");
            b.Property(x => x.FechaCompra).HasColumnName("DFechaCompra");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaIngreso).HasColumnName("DFechaIngreso");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.KilometrajeAcumulado).HasColumnName("NKilometrajeAcumulado");
            b.Property(x => x.MarcaId).HasColumnName("GMarcaId");
            b.Property(x => x.NumeroReencauches).HasColumnName("NNumeroReencauches");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.ProfundidadInicial).HasColumnName("NProfundidadInicial");
            b.Property(x => x.ReferenciaId).HasColumnName("GReferenciaId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Serial).HasColumnName("SSerial");
            b.Property(x => x.TipoLlantaId).HasColumnName("GTipoLlantaId");
            b.Property(x => x.UbicacionActual).HasColumnName("SUbicacionActual");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => new { x.CentroId, x.EstadoLlantaId }).HasDatabaseName("IX_Llanta_GCentroId_GEstadoLlantaId").HasFilter(null);
            b.HasIndex(x => x.DimensionId).HasDatabaseName("IX_Llanta_GDimensionId").HasFilter(null);
            b.HasIndex(x => x.EstadoLlantaId).HasDatabaseName("IX_Llanta_GEstadoLlantaId").HasFilter(null);
            b.HasIndex(x => x.MarcaId).HasDatabaseName("IX_Llanta_GMarcaId").HasFilter(null);
            b.HasIndex(x => x.ReferenciaId).HasDatabaseName("IX_Llanta_GReferenciaId").HasFilter(null);
            b.HasIndex(x => x.TipoLlantaId).HasDatabaseName("IX_Llanta_GTipoLlantaId").HasFilter(null);
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Llanta_SCodigo").HasFilter(null);
            b.HasIndex(x => x.Serial).HasDatabaseName("IX_Llanta_SSerial").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Llanta");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_Llanta_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "DimensionId" })).SetConstraintName("FK_TBL_Llanta_TBL_Dimension_GDimensionId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "EstadoLlantaId" })).SetConstraintName("FK_TBL_Llanta_TBL_EstadoLlanta_GEstadoLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "MarcaId" })).SetConstraintName("FK_TBL_Llanta_TBL_Marca_GMarcaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "ReferenciaId" })).SetConstraintName("FK_TBL_Llanta_TBL_Referencia_GReferenciaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "TipoLlantaId" })).SetConstraintName("FK_TBL_Llanta_TBL_TipoLlanta_GTipoLlantaId");
        }
        {
            var b = modelBuilder.Entity<LlantaTemporal>();
            b.ToTable("TBL_LlantaTemporal", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Estado).HasColumnName("NEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.IdentificadorFisico).HasColumnName("SIdentificadorFisico");
            b.Property(x => x.IdentificadorTemporal).HasColumnName("SIdentificadorTemporal");
            b.Property(x => x.InconsistenciaInspeccionId).HasColumnName("GInconsistenciaInspeccionId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.InconsistenciaInspeccionId).HasDatabaseName("IX_LlantaTemporal_GInconsistenciaInspeccionId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_LlantaTemporal");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "InconsistenciaInspeccionId" })).SetConstraintName("FK_TBL_LlantaTemporal_TBL_InconsistenciaInspeccion_GInconsistenciaInspeccionId");
        }
        {
            var b = modelBuilder.Entity<LoteEnvioReparacion>();
            b.ToTable("TBL_LoteEnvioReparacion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroOrigenId).HasColumnName("GCentroOrigenId");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.Estado).HasColumnName("SEstado");
            b.Property(x => x.FechaCierre).HasColumnName("DFechaCierre");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.FechaSalida).HasColumnName("DFechaSalida");
            b.Property(x => x.IdempotencyKey).HasColumnName("SIdempotencyKey");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.ProveedorId).HasColumnName("GProveedorId");
            b.Property(x => x.Receptor).HasColumnName("SReceptor");
            b.Property(x => x.Remision).HasColumnName("SRemision");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Solicitante).HasColumnName("SSolicitante");
            b.Property(x => x.Transportador).HasColumnName("STransportador");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.CentroOrigenId).HasDatabaseName("IX_LoteEnvioReparacion_GCentroOrigenId").HasFilter(null);
            b.HasIndex(x => x.ProveedorId).HasDatabaseName("IX_LoteEnvioReparacion_GProveedorId").HasFilter(null);
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_LoteEnvioReparacion_SCodigo").HasFilter(null);
            b.HasIndex(x => x.IdempotencyKey).HasDatabaseName("IX_LoteEnvioReparacion_SIdempotencyKey").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_LoteEnvioReparacion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroOrigenId" })).SetConstraintName("FK_TBL_LoteEnvioReparacion_TBL_Centro_GCentroOrigenId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProveedorId" })).SetConstraintName("FK_TBL_LoteEnvioReparacion_TBL_ProveedorServicio_GProveedorId");
        }
        {
            var b = modelBuilder.Entity<Marca>();
            b.ToTable("TBL_Marca", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Marca_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Marca");
        }
        {
            var b = modelBuilder.Entity<Movimiento>();
            b.ToTable("TBL_Movimiento", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.InspeccionId).HasColumnName("GInspeccionId");
            b.Property(x => x.Motivo).HasColumnName("SMotivo");
            b.Property(x => x.Numero).HasColumnName("SNumero");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Tipo).HasColumnName("STipo");
            b.Property(x => x.Usuario).HasColumnName("SUsuario");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.CentroId).HasDatabaseName("IX_Movimiento_GCentroId").HasFilter(null);
            b.HasIndex(x => x.Numero).HasDatabaseName("IX_Movimiento_SNumero").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Movimiento");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_Movimiento_TBL_Centro_GCentroId");
        }
        {
            var b = modelBuilder.Entity<MovimientoDetalle>();
            b.ToTable("TBL_MovimientoDetalle", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroDestinoId).HasColumnName("GCentroDestinoId");
            b.Property(x => x.DestinoDescripcion).HasColumnName("SDestinoDescripcion");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.MovimientoId).HasColumnName("GMovimientoId");
            b.Property(x => x.PosicionDestinoId).HasColumnName("GPosicionDestinoId");
            b.Property(x => x.PosicionOrigenId).HasColumnName("GPosicionOrigenId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TipoDestino).HasColumnName("NTipoDestino");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.LlantaId).HasDatabaseName("IX_MovimientoDetalle_GLlantaId").HasFilter(null);
            b.HasIndex(x => x.MovimientoId).HasDatabaseName("IX_MovimientoDetalle_GMovimientoId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_MovimientoDetalle");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaId" })).SetConstraintName("FK_TBL_MovimientoDetalle_TBL_Llanta_GLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "MovimientoId" })).SetConstraintName("FK_TBL_MovimientoDetalle_TBL_Movimiento_GMovimientoId");
        }
        {
            var b = modelBuilder.Entity<MovimientoLlanta>();
            b.ToTable("TBL_MovimientoLlanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.FechaAutorizacion).HasColumnName("DFechaAutorizacion");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.FechaReporte).HasColumnName("DFechaReporte");
            b.Property(x => x.InconsistenciaInspeccionId).HasColumnName("GInconsistenciaInspeccionId");
            b.Property(x => x.InspeccionId).HasColumnName("GInspeccionId");
            b.Property(x => x.LlantaAnteriorId).HasColumnName("GLlantaAnteriorId");
            b.Property(x => x.LlantaNuevaId).HasColumnName("GLlantaNuevaId");
            b.Property(x => x.Motivo).HasColumnName("SMotivo");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.PosicionVehiculoId).HasColumnName("GPosicionVehiculoId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.TecnicoReporta).HasColumnName("STecnicoReporta");
            b.Property(x => x.UsuarioAutoriza).HasColumnName("SUsuarioAutoriza");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasKey(x => x.Id).HasName("PK_TBL_MovimientoLlanta");
        }
        {
            var b = modelBuilder.Entity<OrdenServicioLlanta>();
            b.ToTable("TBL_OrdenServicioLlanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Aprobador).HasColumnName("SAprobador");
            b.Property(x => x.CentroOrigenId).HasColumnName("GCentroOrigenId");
            b.Property(x => x.Costo).HasColumnName("NCosto");
            b.Property(x => x.CriterioElegibilidad).HasColumnName("SCriterioElegibilidad");
            b.Property(x => x.Elegible).HasColumnName("BElegible");
            b.Property(x => x.Estado).HasColumnName("SEstado");
            b.Property(x => x.FechaAprobacion).HasColumnName("DFechaAprobacion");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaEnvio).HasColumnName("DFechaEnvio");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.FechaOpcionada).HasColumnName("DFechaOpcionada");
            b.Property(x => x.FechaRecepcion).HasColumnName("DFechaRecepcion");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.LoteEnvioReparacionId).HasColumnName("GLoteEnvioReparacionId");
            b.Property(x => x.Motivo).HasColumnName("SMotivo");
            b.Property(x => x.MotivoRechazo).HasColumnName("SMotivoRechazo");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.OrigenEntidadId).HasColumnName("GOrigenEntidadId");
            b.Property(x => x.OrigenTipo).HasColumnName("SOrigenTipo");
            b.Property(x => x.PosicionOrigenId).HasColumnName("GPosicionOrigenId");
            b.Property(x => x.ProveedorId).HasColumnName("GProveedorId");
            b.Property(x => x.Resultado).HasColumnName("SResultado");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Solicitante).HasColumnName("SSolicitante");
            b.Property(x => x.Tipo).HasColumnName("NTipo");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.UsuarioOpciona).HasColumnName("SUsuarioOpciona");
            b.Property(x => x.VehiculoOrigenId).HasColumnName("GVehiculoOrigenId");
            b.HasIndex(x => x.CentroOrigenId).HasDatabaseName("IX_OrdenServicioLlanta_GCentroOrigenId").HasFilter(null);
            b.HasIndex(x => x.LlantaId).HasDatabaseName("IX_OrdenServicioLlanta_GLlantaId").HasFilter(null);
            b.HasIndex(x => x.LoteEnvioReparacionId).HasDatabaseName("IX_OrdenServicioLlanta_GLoteEnvioReparacionId").HasFilter(null);
            b.HasIndex(x => x.ProveedorId).HasDatabaseName("IX_OrdenServicioLlanta_GProveedorId").HasFilter(null);
            b.HasIndex(x => new { x.Tipo, x.Estado, x.CentroOrigenId }).HasDatabaseName("IX_OrdenServicioLlanta_NTipo_SEstado_GCentroOrigenId").HasFilter(null);
            b.HasIndex(x => new { x.OrigenTipo, x.OrigenEntidadId }).HasDatabaseName("IX_OrdenServicioLlanta_SOrigenTipo_GOrigenEntidadId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_OrdenServicioLlanta");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroOrigenId" })).SetConstraintName("FK_TBL_OrdenServicioLlanta_TBL_Centro_GCentroOrigenId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaId" })).SetConstraintName("FK_TBL_OrdenServicioLlanta_TBL_Llanta_GLlantaId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LoteEnvioReparacionId" })).SetConstraintName("FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_GLoteEnvioReparacionId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProveedorId" })).SetConstraintName("FK_TBL_OrdenServicioLlanta_TBL_ProveedorServicio_GProveedorId");
        }
        {
            var b = modelBuilder.Entity<ParametroAlerta>();
            b.ToTable("TBL_ParametroAlerta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Unidad).HasColumnName("SUnidad");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.Valor).HasColumnName("NValor");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_ParametroAlerta_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_ParametroAlerta");
        }
        {
            var b = modelBuilder.Entity<ParametroReencauche>();
            b.ToTable("TBL_ParametroReencauche", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.DimensionId).HasColumnName("GDimensionId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.MaximoReencauches).HasColumnName("NMaximoReencauches");
            b.Property(x => x.ProfundidadMinima).HasColumnName("NProfundidadMinima");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.Property(x => x.VigenteDesde).HasColumnName("DVigenteDesde");
            b.Property(x => x.VigenteHasta).HasColumnName("DVigenteHasta");
            b.HasKey(x => x.Id).HasName("PK_TBL_ParametroReencauche");
        }
        {
            var b = modelBuilder.Entity<PermisoSistema>();
            b.ToTable("TBL_Permiso", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Permiso_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Permiso");
        }
        {
            var b = modelBuilder.Entity<PosicionVehiculo>();
            b.ToTable("TBL_PosicionVehiculo", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.EjeVehiculoId).HasColumnName("GEjeVehiculoId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Lado).HasColumnName("SLado");
            b.Property(x => x.LlantaActualId).HasColumnName("GLlantaActualId");
            b.Property(x => x.Orden).HasColumnName("NOrden");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Ubicacion).HasColumnName("SUbicacion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => new { x.EjeVehiculoId, x.Codigo }).HasDatabaseName("IX_PosicionVehiculo_GEjeVehiculoId_SCodigo").HasFilter(null);
            b.HasIndex(x => x.LlantaActualId).HasDatabaseName("IX_PosicionVehiculo_GLlantaActualId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_PosicionVehiculo");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "EjeVehiculoId" })).SetConstraintName("FK_TBL_PosicionVehiculo_TBL_EjeVehiculo_GEjeVehiculoId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaActualId" })).SetConstraintName("FK_TBL_PosicionVehiculo_TBL_Llanta_GLlantaActualId");
        }
        {
            var b = modelBuilder.Entity<ProveedorServicio>();
            b.ToTable("TBL_ProveedorServicio", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Tipo).HasColumnName("STipo");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_ProveedorServicio_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_ProveedorServicio");
        }
        {
            var b = modelBuilder.Entity<RecomendacionInspeccion>();
            b.ToTable("TBL_RecomendacionInspeccion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.EsCandidataReencauche).HasColumnName("BEsCandidataReencauche");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_RecomendacionInspeccion_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_RecomendacionInspeccion");
        }
        {
            var b = modelBuilder.Entity<Referencia>();
            b.ToTable("TBL_Referencia", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.MarcaId).HasColumnName("GMarcaId");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.MarcaId).HasDatabaseName("IX_Referencia_GMarcaId").HasFilter(null);
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Referencia_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Referencia");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "MarcaId" })).SetConstraintName("FK_TBL_Referencia_TBL_Marca_GMarcaId");
        }
        {
            var b = modelBuilder.Entity<Regional>();
            b.ToTable("TBL_Regional", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Regional_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Regional");
        }
        {
            var b = modelBuilder.Entity<RolPermiso>();
            b.ToTable("TBL_RolPermiso", "dbo");
            b.Property(x => x.RolId).HasColumnName("GRolId");
            b.Property(x => x.PermisoId).HasColumnName("GPermisoId");
            b.HasIndex(x => x.PermisoId).HasDatabaseName("IX_RolPermiso_GPermisoId").HasFilter(null);
            b.HasKey(x => new { x.RolId, x.PermisoId }).HasName("PK_TBL_RolPermiso");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "PermisoId" })).SetConstraintName("FK_TBL_RolPermiso_TBL_Permiso_GPermisoId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "RolId" })).SetConstraintName("FK_TBL_RolPermiso_TBL_Rol_GRolId");
        }
        {
            var b = modelBuilder.Entity<RolSistema>();
            b.ToTable("TBL_Rol", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_Rol_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Rol");
        }
        {
            var b = modelBuilder.Entity<SolicitudOperacion>();
            b.ToTable("TBL_SolicitudOperacion", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.ActividadProgramadaId).HasColumnName("GActividadProgramadaId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Aprobador).HasColumnName("SAprobador");
            b.Property(x => x.CentroDestinoId).HasColumnName("GCentroDestinoId");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.DestinoDesplazada).HasColumnName("SDestinoDesplazada");
            b.Property(x => x.Estado).HasColumnName("NEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaDecision).HasColumnName("DFechaDecision");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.FechaRecepcionDestino).HasColumnName("DFechaRecepcionDestino");
            b.Property(x => x.KilometrajeVehiculo).HasColumnName("NKilometrajeVehiculo");
            b.Property(x => x.LlantaDesplazadaId).HasColumnName("GLlantaDesplazadaId");
            b.Property(x => x.LlantaId).HasColumnName("GLlantaId");
            b.Property(x => x.Motivo).HasColumnName("SMotivo");
            b.Property(x => x.MotivoRechazo).HasColumnName("SMotivoRechazo");
            b.Property(x => x.MovimientoEjecutadoId).HasColumnName("GMovimientoEjecutadoId");
            b.Property(x => x.Observaciones).HasColumnName("SObservaciones");
            b.Property(x => x.PosicionDestinoDesplazadaId).HasColumnName("GPosicionDestinoDesplazadaId");
            b.Property(x => x.PosicionDestinoId).HasColumnName("GPosicionDestinoId");
            b.Property(x => x.PosicionOrigenId).HasColumnName("GPosicionOrigenId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Solicitante).HasColumnName("SSolicitante");
            b.Property(x => x.Tipo).HasColumnName("STipo");
            b.Property(x => x.TipoDestino).HasColumnName("STipoDestino");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => new { x.CentroId, x.Estado, x.FechaCreacion }).HasDatabaseName("IX_SolicitudOperacion_GCentroId_NEstado_DFechaCreacion").HasFilter(null);
            b.HasIndex(x => x.LlantaId).HasDatabaseName("IX_SolicitudOperacion_GLlantaId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_SolicitudOperacion");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_SolicitudOperacion_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "LlantaId" })).SetConstraintName("FK_TBL_SolicitudOperacion_TBL_Llanta_GLlantaId");
        }
        {
            var b = modelBuilder.Entity<TipoLlanta>();
            b.ToTable("TBL_TipoLlanta", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.Codigo).HasColumnName("SCodigo");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.Codigo).HasDatabaseName("IX_TipoLlanta_SCodigo").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_TipoLlanta");
        }
        {
            var b = modelBuilder.Entity<UsuarioCentro>();
            b.ToTable("TBL_UsuarioCentro", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioId).HasColumnName("GUsuarioId");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.CentroId).HasDatabaseName("IX_UsuarioCentro_GCentroId").HasFilter(null);
            b.HasIndex(x => new { x.UsuarioId, x.CentroId }).HasDatabaseName("IX_UsuarioCentro_GUsuarioId_GCentroId").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_UsuarioCentro");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_UsuarioCentro_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "UsuarioId" })).SetConstraintName("FK_TBL_UsuarioCentro_TBL_Usuario_GUsuarioId");
        }
        {
            var b = modelBuilder.Entity<UsuarioSistema>();
            b.Property(x => x.DebeCambiarClave).HasColumnName("BDebeCambiarClave").HasDefaultValue(false);
            b.ToTable("TBL_Usuario", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Nombre).HasColumnName("SNombre");
            b.Property(x => x.PasswordHash).HasColumnName("SPasswordHash");
            b.Property(x => x.RolId).HasColumnName("GRolId");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Username).HasColumnName("SUsername");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.CentroId).HasDatabaseName("IX_Usuario_GCentroId").HasFilter(null);
            b.HasIndex(x => x.RolId).HasDatabaseName("IX_Usuario_GRolId").HasFilter(null);
            b.HasIndex(x => x.Username).HasDatabaseName("IX_Usuario_SUsername").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Usuario");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_Usuario_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "RolId" })).SetConstraintName("FK_TBL_Usuario_TBL_Rol_GRolId");
        }
        {
            var b = modelBuilder.Entity<Vehiculo>();
            b.ToTable("TBL_Vehiculo", "dbo");
            b.Property(x => x.Id).HasColumnName("GId");
            b.Property(x => x.Activo).HasColumnName("BActivo");
            b.Property(x => x.CentroId).HasColumnName("GCentroId");
            b.Property(x => x.ConfiguracionVehiculoId).HasColumnName("GConfiguracionVehiculoId");
            b.Property(x => x.Estado).HasColumnName("SEstado");
            b.Property(x => x.FechaCreacion).HasColumnName("DFechaCreacion");
            b.Property(x => x.FechaModificacion).HasColumnName("DFechaModificacion");
            b.Property(x => x.Kilometraje).HasColumnName("NKilometraje");
            b.Property(x => x.NumeroInterno).HasColumnName("SNumeroInterno");
            b.Property(x => x.Placa).HasColumnName("SPlaca");
            b.Property(x => x.RowVersion).HasColumnName("TRowVersion");
            b.Property(x => x.Tipo).HasColumnName("STipo");
            b.Property(x => x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");
            b.Property(x => x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");
            b.HasIndex(x => x.CentroId).HasDatabaseName("IX_Vehiculo_GCentroId").HasFilter(null);
            b.HasIndex(x => x.ConfiguracionVehiculoId).HasDatabaseName("IX_Vehiculo_GConfiguracionVehiculoId").HasFilter(null);
            b.HasIndex(x => x.NumeroInterno).HasDatabaseName("IX_Vehiculo_SNumeroInterno").HasFilter(null);
            b.HasKey(x => x.Id).HasName("PK_TBL_Vehiculo");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "CentroId" })).SetConstraintName("FK_TBL_Vehiculo_TBL_Centro_GCentroId");
            b.Metadata.GetForeignKeys().Single(f => f.Properties.Select(p => p.Name).SequenceEqual(new[] { "ConfiguracionVehiculoId" })).SetConstraintName("FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_GConfiguracionVehiculoId");
        }
    }
}
