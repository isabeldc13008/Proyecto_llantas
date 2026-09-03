using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarModeloPostobon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Centro_CentroId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Usuario_TecnicoUsuarioId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Vehiculo_VehiculoId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AlertaHistorial_TBL_AlertaInspeccion_AlertaInspeccionId",
                table: "TBL_AlertaHistorial");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_InspeccionDetalle_InspeccionDetalleId",
                table: "TBL_AlertaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_Inspeccion_InspeccionId",
                table: "TBL_AlertaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_Llanta_LlantaId",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_PosicionVehiculo_PosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Centro_TBL_Regional_RegionalId",
                table: "TBL_Centro");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ConfiguracionEje_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                table: "TBL_ConfiguracionEje");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ConfiguracionPosicion_TBL_ConfiguracionEje_ConfiguracionEjeId",
                table: "TBL_ConfiguracionPosicion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_EjeVehiculo_TBL_Vehiculo_VehiculoId",
                table: "TBL_EjeVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_EvidenciaFlujo_TBL_OrdenServicioLlanta_OrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Inspeccion_InspeccionId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_PosicionVehiculo_PosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Centro_CentroId",
                table: "TBL_Inspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Vehiculo_VehiculoId",
                table: "TBL_Inspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CausaLlanta_CausaLlantaId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CondicionLlanta_CondicionLlantaId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Inspeccion_InspeccionId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Llanta_LlantaId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_PosicionVehiculo_PosicionVehiculoId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_RecomendacionInspeccion_RecomendacionId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Centro_CentroId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Dimension_DimensionId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_EstadoLlanta_EstadoLlantaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Marca_MarcaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Referencia_ReferenciaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_TipoLlanta_TipoLlantaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_LlantaTemporal_TBL_InconsistenciaInspeccion_InconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_Centro_CentroOrigenId",
                table: "TBL_LoteEnvioReparacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_ProveedorServicio_ProveedorId",
                table: "TBL_LoteEnvioReparacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Movimiento_TBL_Centro_CentroId",
                table: "TBL_Movimiento");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Llanta_LlantaId",
                table: "TBL_MovimientoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Movimiento_MovimientoId",
                table: "TBL_MovimientoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Centro_CentroOrigenId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Llanta_LlantaId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_ProveedorServicio_ProveedorId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_EjeVehiculo_EjeVehiculoId",
                table: "TBL_PosicionVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_Llanta_LlantaActualId",
                table: "TBL_PosicionVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Referencia_TBL_Marca_MarcaId",
                table: "TBL_Referencia");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Permiso_PermisoId",
                table: "TBL_RolPermiso");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Rol_RolId",
                table: "TBL_RolPermiso");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Centro_CentroId",
                table: "TBL_SolicitudOperacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Llanta_LlantaId",
                table: "TBL_SolicitudOperacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Usuario_TBL_Centro_CentroId",
                table: "TBL_Usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Usuario_TBL_Rol_RolId",
                table: "TBL_Usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Centro_CentroId",
                table: "TBL_UsuarioCentro");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Usuario_UsuarioId",
                table: "TBL_UsuarioCentro");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Vehiculo_TBL_Centro_CentroId",
                table: "TBL_Vehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo");

  migrationBuilder.Sql("""
    DROP INDEX IF EXISTS [UX_Asignacion_LlantaActiva]
    ON [dbo].[TBL_AsignacionLlantaPosicion];

    DROP INDEX IF EXISTS [UX_Asignacion_PosicionActiva]
    ON [dbo].[TBL_AsignacionLlantaPosicion];
    """);
            migrationBuilder.DropIndex(
                name: "IX_TBL_ActividadProgramada_IdempotencyKey",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_VehiculoId_TipoActividad_FechaProgramada",
                table: "TBL_ActividadProgramada");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Vehiculo",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Vehiculo",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_Vehiculo",
                newName: "STipo");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Vehiculo",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Placa",
                table: "TBL_Vehiculo",
                newName: "SPlaca");

            migrationBuilder.RenameColumn(
                name: "NumeroInterno",
                table: "TBL_Vehiculo",
                newName: "SNumeroInterno");

            migrationBuilder.RenameColumn(
                name: "Kilometraje",
                table: "TBL_Vehiculo",
                newName: "NKilometraje");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Vehiculo",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Vehiculo",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_Vehiculo",
                newName: "SEstado");

            migrationBuilder.RenameColumn(
                name: "ConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                newName: "GConfiguracionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_Vehiculo",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Vehiculo",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Vehiculo",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Vehiculo_NumeroInterno",
                table: "TBL_Vehiculo",
                newName: "IX_Vehiculo_SNumeroInterno");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Vehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                newName: "IX_Vehiculo_GConfiguracionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Vehiculo_CentroId",
                table: "TBL_Vehiculo",
                newName: "IX_Vehiculo_GCentroId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_UsuarioCentro",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "TBL_UsuarioCentro",
                newName: "GUsuarioId");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_UsuarioCentro",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_UsuarioCentro",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_UsuarioCentro",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_UsuarioCentro",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_UsuarioCentro",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_UsuarioCentro",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_UsuarioCentro",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_UsuarioCentro_UsuarioId_CentroId",
                table: "TBL_UsuarioCentro",
                newName: "IX_UsuarioCentro_GUsuarioId_GCentroId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_UsuarioCentro_CentroId",
                table: "TBL_UsuarioCentro",
                newName: "IX_UsuarioCentro_GCentroId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Usuario",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Usuario",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "TBL_Usuario",
                newName: "SUsername");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Usuario",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "RolId",
                table: "TBL_Usuario",
                newName: "GRolId");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "TBL_Usuario",
                newName: "SPasswordHash");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Usuario",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Usuario",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Usuario",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_Usuario",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Usuario",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Usuario",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Usuario_Username",
                table: "TBL_Usuario",
                newName: "IX_Usuario_SUsername");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Usuario_RolId",
                table: "TBL_Usuario",
                newName: "IX_Usuario_GRolId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Usuario_CentroId",
                table: "TBL_Usuario",
                newName: "IX_Usuario_GCentroId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_TipoLlanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_TipoLlanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_TipoLlanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_TipoLlanta",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_TipoLlanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_TipoLlanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_TipoLlanta",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_TipoLlanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_TipoLlanta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TipoLlanta_Codigo",
                table: "TBL_TipoLlanta",
                newName: "IX_TipoLlanta_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_SolicitudOperacion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_SolicitudOperacion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TipoDestino",
                table: "TBL_SolicitudOperacion",
                newName: "STipoDestino");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_SolicitudOperacion",
                newName: "STipo");

            migrationBuilder.RenameColumn(
                name: "Solicitante",
                table: "TBL_SolicitudOperacion",
                newName: "SSolicitante");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_SolicitudOperacion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PosicionOrigenId",
                table: "TBL_SolicitudOperacion",
                newName: "GPosicionOrigenId");

            migrationBuilder.RenameColumn(
                name: "PosicionDestinoId",
                table: "TBL_SolicitudOperacion",
                newName: "GPosicionDestinoId");

            migrationBuilder.RenameColumn(
                name: "PosicionDestinoDesplazadaId",
                table: "TBL_SolicitudOperacion",
                newName: "GPosicionDestinoDesplazadaId");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_SolicitudOperacion",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "MovimientoEjecutadoId",
                table: "TBL_SolicitudOperacion",
                newName: "GMovimientoEjecutadoId");

            migrationBuilder.RenameColumn(
                name: "MotivoRechazo",
                table: "TBL_SolicitudOperacion",
                newName: "SMotivoRechazo");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "TBL_SolicitudOperacion",
                newName: "SMotivo");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_SolicitudOperacion",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "LlantaDesplazadaId",
                table: "TBL_SolicitudOperacion",
                newName: "GLlantaDesplazadaId");

            migrationBuilder.RenameColumn(
                name: "KilometrajeVehiculo",
                table: "TBL_SolicitudOperacion",
                newName: "NKilometrajeVehiculo");

            migrationBuilder.RenameColumn(
                name: "FechaRecepcionDestino",
                table: "TBL_SolicitudOperacion",
                newName: "DFechaRecepcionDestino");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_SolicitudOperacion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaDecision",
                table: "TBL_SolicitudOperacion",
                newName: "DFechaDecision");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_SolicitudOperacion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_SolicitudOperacion",
                newName: "NEstado");

            migrationBuilder.RenameColumn(
                name: "DestinoDesplazada",
                table: "TBL_SolicitudOperacion",
                newName: "SDestinoDesplazada");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_SolicitudOperacion",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "CentroDestinoId",
                table: "TBL_SolicitudOperacion",
                newName: "GCentroDestinoId");

            migrationBuilder.RenameColumn(
                name: "Aprobador",
                table: "TBL_SolicitudOperacion",
                newName: "SAprobador");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_SolicitudOperacion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "ActividadProgramadaId",
                table: "TBL_SolicitudOperacion",
                newName: "GActividadProgramadaId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_SolicitudOperacion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_SolicitudOperacion_LlantaId",
                table: "TBL_SolicitudOperacion",
                newName: "IX_SolicitudOperacion_GLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_SolicitudOperacion_CentroId_Estado_FechaCreacion",
                table: "TBL_SolicitudOperacion",
                newName: "IX_SolicitudOperacion_GCentroId_NEstado_DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "PermisoId",
                table: "TBL_RolPermiso",
                newName: "GPermisoId");

            migrationBuilder.RenameColumn(
                name: "RolId",
                table: "TBL_RolPermiso",
                newName: "GRolId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_RolPermiso_PermisoId",
                table: "TBL_RolPermiso",
                newName: "IX_RolPermiso_GPermisoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Rol",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Rol",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Rol",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Rol",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Rol",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Rol",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Rol",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Rol",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Rol",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Rol_Codigo",
                table: "TBL_Rol",
                newName: "IX_Rol_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Regional",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Regional",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Regional",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Regional",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Regional",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Regional",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Regional",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Regional",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Regional",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_Regional_Codigo",
                table: "TBL_Regional",
                newName: "IX_Regional_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Referencia",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Referencia",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Referencia",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Referencia",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "MarcaId",
                table: "TBL_Referencia",
                newName: "GMarcaId");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Referencia",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Referencia",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Referencia",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Referencia",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Referencia",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Referencia_MarcaId",
                table: "TBL_Referencia",
                newName: "IX_Referencia_GMarcaId");

            migrationBuilder.RenameIndex(
                name: "IX_Referencia_Codigo",
                table: "TBL_Referencia",
                newName: "IX_Referencia_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_RecomendacionInspeccion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_RecomendacionInspeccion",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "EsCandidataReencauche",
                table: "TBL_RecomendacionInspeccion",
                newName: "BEsCandidataReencauche");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_RecomendacionInspeccion",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_RecomendacionInspeccion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_RecomendacionInspeccion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_RecomendacionInspeccion_Codigo",
                table: "TBL_RecomendacionInspeccion",
                newName: "IX_RecomendacionInspeccion_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ProveedorServicio",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ProveedorServicio",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_ProveedorServicio",
                newName: "STipo");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ProveedorServicio",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_ProveedorServicio",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ProveedorServicio",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ProveedorServicio",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_ProveedorServicio",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ProveedorServicio",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ProveedorServicio",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ProveedorServicio_Codigo",
                table: "TBL_ProveedorServicio",
                newName: "IX_ProveedorServicio_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_PosicionVehiculo",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_PosicionVehiculo",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                table: "TBL_PosicionVehiculo",
                newName: "SUbicacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_PosicionVehiculo",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Orden",
                table: "TBL_PosicionVehiculo",
                newName: "NOrden");

            migrationBuilder.RenameColumn(
                name: "LlantaActualId",
                table: "TBL_PosicionVehiculo",
                newName: "GLlantaActualId");

            migrationBuilder.RenameColumn(
                name: "Lado",
                table: "TBL_PosicionVehiculo",
                newName: "SLado");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_PosicionVehiculo",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_PosicionVehiculo",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "EjeVehiculoId",
                table: "TBL_PosicionVehiculo",
                newName: "GEjeVehiculoId");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_PosicionVehiculo",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_PosicionVehiculo",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_PosicionVehiculo",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_PosicionVehiculo_LlantaActualId",
                table: "TBL_PosicionVehiculo",
                newName: "IX_PosicionVehiculo_GLlantaActualId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_PosicionVehiculo_EjeVehiculoId_Codigo",
                table: "TBL_PosicionVehiculo",
                newName: "IX_PosicionVehiculo_GEjeVehiculoId_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Permiso",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Permiso",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Permiso",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Permiso",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Permiso",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Permiso",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Permiso",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Permiso",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Permiso",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Permiso_Codigo",
                table: "TBL_Permiso",
                newName: "IX_Permiso_SCodigo");

            migrationBuilder.RenameColumn(
                name: "VigenteHasta",
                table: "TBL_ParametroReencauche",
                newName: "DVigenteHasta");

            migrationBuilder.RenameColumn(
                name: "VigenteDesde",
                table: "TBL_ParametroReencauche",
                newName: "DVigenteDesde");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ParametroReencauche",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ParametroReencauche",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ParametroReencauche",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "ProfundidadMinima",
                table: "TBL_ParametroReencauche",
                newName: "NProfundidadMinima");

            migrationBuilder.RenameColumn(
                name: "MaximoReencauches",
                table: "TBL_ParametroReencauche",
                newName: "NMaximoReencauches");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ParametroReencauche",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ParametroReencauche",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DimensionId",
                table: "TBL_ParametroReencauche",
                newName: "GDimensionId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ParametroReencauche",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ParametroReencauche",
                newName: "GId");

            migrationBuilder.RenameColumn(
                name: "Valor",
                table: "TBL_ParametroAlerta",
                newName: "NValor");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ParametroAlerta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ParametroAlerta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Unidad",
                table: "TBL_ParametroAlerta",
                newName: "SUnidad");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ParametroAlerta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ParametroAlerta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ParametroAlerta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_ParametroAlerta",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ParametroAlerta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ParametroAlerta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ParametroAlerta_Codigo",
                table: "TBL_ParametroAlerta",
                newName: "IX_ParametroAlerta_SCodigo");

            migrationBuilder.RenameColumn(
                name: "VehiculoOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GVehiculoOrigenId");

            migrationBuilder.RenameColumn(
                name: "UsuarioOpciona",
                table: "TBL_OrdenServicioLlanta",
                newName: "SUsuarioOpciona");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_OrdenServicioLlanta",
                newName: "NTipo");

            migrationBuilder.RenameColumn(
                name: "Solicitante",
                table: "TBL_OrdenServicioLlanta",
                newName: "SSolicitante");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_OrdenServicioLlanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Resultado",
                table: "TBL_OrdenServicioLlanta",
                newName: "SResultado");

            migrationBuilder.RenameColumn(
                name: "ProveedorId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GProveedorId");

            migrationBuilder.RenameColumn(
                name: "PosicionOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GPosicionOrigenId");

            migrationBuilder.RenameColumn(
                name: "OrigenTipo",
                table: "TBL_OrdenServicioLlanta",
                newName: "SOrigenTipo");

            migrationBuilder.RenameColumn(
                name: "OrigenEntidadId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GOrigenEntidadId");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_OrdenServicioLlanta",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "MotivoRechazo",
                table: "TBL_OrdenServicioLlanta",
                newName: "SMotivoRechazo");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "TBL_OrdenServicioLlanta",
                newName: "SMotivo");

            migrationBuilder.RenameColumn(
                name: "LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GLoteEnvioReparacionId");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "FechaRecepcion",
                table: "TBL_OrdenServicioLlanta",
                newName: "DFechaRecepcion");

            migrationBuilder.RenameColumn(
                name: "FechaOpcionada",
                table: "TBL_OrdenServicioLlanta",
                newName: "DFechaOpcionada");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaEnvio",
                table: "TBL_OrdenServicioLlanta",
                newName: "DFechaEnvio");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "FechaAprobacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "DFechaAprobacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_OrdenServicioLlanta",
                newName: "SEstado");

            migrationBuilder.RenameColumn(
                name: "Elegible",
                table: "TBL_OrdenServicioLlanta",
                newName: "BElegible");

            migrationBuilder.RenameColumn(
                name: "CriterioElegibilidad",
                table: "TBL_OrdenServicioLlanta",
                newName: "SCriterioElegibilidad");

            migrationBuilder.RenameColumn(
                name: "Costo",
                table: "TBL_OrdenServicioLlanta",
                newName: "NCosto");

            migrationBuilder.RenameColumn(
                name: "CentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "GCentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "Aprobador",
                table: "TBL_OrdenServicioLlanta",
                newName: "SAprobador");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_OrdenServicioLlanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_OrdenServicioLlanta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_OrdenServicioLlanta_Tipo_Estado_CentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_OrdenServicioLlanta_NTipo_SEstado_GCentroOrigenId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_OrdenServicioLlanta_ProveedorId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_OrdenServicioLlanta_GProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_OrdenServicioLlanta_OrigenTipo_OrigenEntidadId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_OrdenServicioLlanta_SOrigenTipo_GOrigenEntidadId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_OrdenServicioLlanta_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_OrdenServicioLlanta_GLoteEnvioReparacionId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_OrdenServicioLlanta_LlantaId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_OrdenServicioLlanta_GLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_OrdenServicioLlanta_CentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_OrdenServicioLlanta_GCentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_MovimientoLlanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_MovimientoLlanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioAutoriza",
                table: "TBL_MovimientoLlanta",
                newName: "SUsuarioAutoriza");

            migrationBuilder.RenameColumn(
                name: "TecnicoReporta",
                table: "TBL_MovimientoLlanta",
                newName: "STecnicoReporta");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_MovimientoLlanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PosicionVehiculoId",
                table: "TBL_MovimientoLlanta",
                newName: "GPosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_MovimientoLlanta",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "TBL_MovimientoLlanta",
                newName: "SMotivo");

            migrationBuilder.RenameColumn(
                name: "LlantaNuevaId",
                table: "TBL_MovimientoLlanta",
                newName: "GLlantaNuevaId");

            migrationBuilder.RenameColumn(
                name: "LlantaAnteriorId",
                table: "TBL_MovimientoLlanta",
                newName: "GLlantaAnteriorId");

            migrationBuilder.RenameColumn(
                name: "InspeccionId",
                table: "TBL_MovimientoLlanta",
                newName: "GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "InconsistenciaInspeccionId",
                table: "TBL_MovimientoLlanta",
                newName: "GInconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "FechaReporte",
                table: "TBL_MovimientoLlanta",
                newName: "DFechaReporte");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_MovimientoLlanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_MovimientoLlanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "FechaAutorizacion",
                table: "TBL_MovimientoLlanta",
                newName: "DFechaAutorizacion");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_MovimientoLlanta",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_MovimientoLlanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_MovimientoLlanta",
                newName: "GId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_MovimientoDetalle",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_MovimientoDetalle",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TipoDestino",
                table: "TBL_MovimientoDetalle",
                newName: "NTipoDestino");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_MovimientoDetalle",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PosicionOrigenId",
                table: "TBL_MovimientoDetalle",
                newName: "GPosicionOrigenId");

            migrationBuilder.RenameColumn(
                name: "PosicionDestinoId",
                table: "TBL_MovimientoDetalle",
                newName: "GPosicionDestinoId");

            migrationBuilder.RenameColumn(
                name: "MovimientoId",
                table: "TBL_MovimientoDetalle",
                newName: "GMovimientoId");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_MovimientoDetalle",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_MovimientoDetalle",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_MovimientoDetalle",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DestinoDescripcion",
                table: "TBL_MovimientoDetalle",
                newName: "SDestinoDescripcion");

            migrationBuilder.RenameColumn(
                name: "CentroDestinoId",
                table: "TBL_MovimientoDetalle",
                newName: "GCentroDestinoId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_MovimientoDetalle",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_MovimientoDetalle",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_MovimientoDetalle_MovimientoId",
                table: "TBL_MovimientoDetalle",
                newName: "IX_MovimientoDetalle_GMovimientoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_MovimientoDetalle_LlantaId",
                table: "TBL_MovimientoDetalle",
                newName: "IX_MovimientoDetalle_GLlantaId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Movimiento",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Movimiento",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Usuario",
                table: "TBL_Movimiento",
                newName: "SUsuario");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_Movimiento",
                newName: "STipo");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Movimiento",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_Movimiento",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "TBL_Movimiento",
                newName: "SNumero");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "TBL_Movimiento",
                newName: "SMotivo");

            migrationBuilder.RenameColumn(
                name: "InspeccionId",
                table: "TBL_Movimiento",
                newName: "GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Movimiento",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Movimiento",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_Movimiento",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Movimiento",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Movimiento",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Movimiento_Numero",
                table: "TBL_Movimiento",
                newName: "IX_Movimiento_SNumero");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Movimiento_CentroId",
                table: "TBL_Movimiento",
                newName: "IX_Movimiento_GCentroId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Marca",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Marca",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Marca",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Marca",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Marca",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Marca",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Marca",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Marca",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Marca",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_Marca_Codigo",
                table: "TBL_Marca",
                newName: "IX_Marca_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Transportador",
                table: "TBL_LoteEnvioReparacion",
                newName: "STransportador");

            migrationBuilder.RenameColumn(
                name: "Solicitante",
                table: "TBL_LoteEnvioReparacion",
                newName: "SSolicitante");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_LoteEnvioReparacion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Remision",
                table: "TBL_LoteEnvioReparacion",
                newName: "SRemision");

            migrationBuilder.RenameColumn(
                name: "Receptor",
                table: "TBL_LoteEnvioReparacion",
                newName: "SReceptor");

            migrationBuilder.RenameColumn(
                name: "ProveedorId",
                table: "TBL_LoteEnvioReparacion",
                newName: "GProveedorId");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_LoteEnvioReparacion",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "TBL_LoteEnvioReparacion",
                newName: "SIdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "FechaSalida",
                table: "TBL_LoteEnvioReparacion",
                newName: "DFechaSalida");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "FechaCierre",
                table: "TBL_LoteEnvioReparacion",
                newName: "DFechaCierre");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_LoteEnvioReparacion",
                newName: "SEstado");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_LoteEnvioReparacion",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "CentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                newName: "GCentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_LoteEnvioReparacion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_LoteEnvioReparacion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_LoteEnvioReparacion_ProveedorId",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_LoteEnvioReparacion_GProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_LoteEnvioReparacion_IdempotencyKey",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_LoteEnvioReparacion_SIdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_LoteEnvioReparacion_Codigo",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_LoteEnvioReparacion_SCodigo");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_LoteEnvioReparacion_CentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_LoteEnvioReparacion_GCentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_LlantaTemporal",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_LlantaTemporal",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_LlantaTemporal",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "InconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                newName: "GInconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "IdentificadorTemporal",
                table: "TBL_LlantaTemporal",
                newName: "SIdentificadorTemporal");

            migrationBuilder.RenameColumn(
                name: "IdentificadorFisico",
                table: "TBL_LlantaTemporal",
                newName: "SIdentificadorFisico");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_LlantaTemporal",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_LlantaTemporal",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_LlantaTemporal",
                newName: "NEstado");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_LlantaTemporal",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_LlantaTemporal",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_LlantaTemporal_InconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                newName: "IX_LlantaTemporal_GInconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Llanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Llanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "UbicacionActual",
                table: "TBL_Llanta",
                newName: "SUbicacionActual");

            migrationBuilder.RenameColumn(
                name: "TipoLlantaId",
                table: "TBL_Llanta",
                newName: "GTipoLlantaId");

            migrationBuilder.RenameColumn(
                name: "Serial",
                table: "TBL_Llanta",
                newName: "SSerial");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Llanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "ReferenciaId",
                table: "TBL_Llanta",
                newName: "GReferenciaId");

            migrationBuilder.RenameColumn(
                name: "ProfundidadInicial",
                table: "TBL_Llanta",
                newName: "NProfundidadInicial");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_Llanta",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "NumeroReencauches",
                table: "TBL_Llanta",
                newName: "NNumeroReencauches");

            migrationBuilder.RenameColumn(
                name: "MarcaId",
                table: "TBL_Llanta",
                newName: "GMarcaId");

            migrationBuilder.RenameColumn(
                name: "KilometrajeAcumulado",
                table: "TBL_Llanta",
                newName: "NKilometrajeAcumulado");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Llanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaIngreso",
                table: "TBL_Llanta",
                newName: "DFechaIngreso");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Llanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "FechaCompra",
                table: "TBL_Llanta",
                newName: "DFechaCompra");

            migrationBuilder.RenameColumn(
                name: "EstadoLlantaId",
                table: "TBL_Llanta",
                newName: "GEstadoLlantaId");

            migrationBuilder.RenameColumn(
                name: "DimensionId",
                table: "TBL_Llanta",
                newName: "GDimensionId");

            migrationBuilder.RenameColumn(
                name: "Costo",
                table: "TBL_Llanta",
                newName: "NCosto");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Llanta",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_Llanta",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Llanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Llanta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Llanta_TipoLlantaId",
                table: "TBL_Llanta",
                newName: "IX_Llanta_GTipoLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Llanta_ReferenciaId",
                table: "TBL_Llanta",
                newName: "IX_Llanta_GReferenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Llanta_MarcaId",
                table: "TBL_Llanta",
                newName: "IX_Llanta_GMarcaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Llanta_EstadoLlantaId",
                table: "TBL_Llanta",
                newName: "IX_Llanta_GEstadoLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Llanta_DimensionId",
                table: "TBL_Llanta",
                newName: "IX_Llanta_GDimensionId");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_Serial",
                table: "TBL_Llanta",
                newName: "IX_Llanta_SSerial");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_Codigo",
                table: "TBL_Llanta",
                newName: "IX_Llanta_SCodigo");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_CentroEstado",
                table: "TBL_Llanta",
                newName: "IX_Llanta_GCentroId_GEstadoLlantaId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_InspeccionDetalle",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_InspeccionDetalle",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_InspeccionDetalle",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "RecomendacionId",
                table: "TBL_InspeccionDetalle",
                newName: "GRecomendacionId");

            migrationBuilder.RenameColumn(
                name: "ProfundidadInterior",
                table: "TBL_InspeccionDetalle",
                newName: "NProfundidadInterior");

            migrationBuilder.RenameColumn(
                name: "ProfundidadExterior",
                table: "TBL_InspeccionDetalle",
                newName: "NProfundidadExterior");

            migrationBuilder.RenameColumn(
                name: "ProfundidadCentro",
                table: "TBL_InspeccionDetalle",
                newName: "NProfundidadCentro");

            migrationBuilder.RenameColumn(
                name: "PosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                newName: "GPosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_InspeccionDetalle",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "InspeccionId",
                table: "TBL_InspeccionDetalle",
                newName: "GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_InspeccionDetalle",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_InspeccionDetalle",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "CondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "GCondicionLlantaId");

            migrationBuilder.RenameColumn(
                name: "CausaLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "GCausaLlantaId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_InspeccionDetalle",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_InspeccionDetalle",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InspeccionDetalle_RecomendacionId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_InspeccionDetalle_GRecomendacionId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InspeccionDetalle_PosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_InspeccionDetalle_GPosicionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InspeccionDetalle_LlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_InspeccionDetalle_GLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InspeccionDetalle_InspeccionId_PosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_InspeccionDetalle_GInspeccionId_GPosicionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InspeccionDetalle_CondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_InspeccionDetalle_GCondicionLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InspeccionDetalle_CausaLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_InspeccionDetalle_GCausaLlantaId");

            migrationBuilder.RenameColumn(
                name: "VehiculoId",
                table: "TBL_Inspeccion",
                newName: "GVehiculoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Inspeccion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Inspeccion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TecnicoId",
                table: "TBL_Inspeccion",
                newName: "STecnicoId");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Inspeccion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_Inspeccion",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "Kilometraje",
                table: "TBL_Inspeccion",
                newName: "NKilometraje");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Inspeccion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Inspeccion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_Inspeccion",
                newName: "NEstado");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_Inspeccion",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Inspeccion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Inspeccion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Inspeccion_VehiculoId",
                table: "TBL_Inspeccion",
                newName: "IX_Inspeccion_GVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Inspeccion_CentroId",
                table: "TBL_Inspeccion",
                newName: "IX_Inspeccion_GCentroId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioAutorizador",
                table: "TBL_InconsistenciaInspeccion",
                newName: "SUsuarioAutorizador");

            migrationBuilder.RenameColumn(
                name: "TecnicoId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "STecnicoId");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "GPosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "ObservacionAutorizacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "SObservacionAutorizacion");

            migrationBuilder.RenameColumn(
                name: "Observacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "SObservacion");

            migrationBuilder.RenameColumn(
                name: "LlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "GLlantaEsperadaId");

            migrationBuilder.RenameColumn(
                name: "LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "GLlantaEncontradaId");

            migrationBuilder.RenameColumn(
                name: "InspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "IdentificadorEncontrado",
                table: "TBL_InconsistenciaInspeccion",
                newName: "SIdentificadorEncontrado");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "FechaAutorizacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "DFechaAutorizacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_InconsistenciaInspeccion",
                newName: "NEstado");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_InconsistenciaInspeccion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_InconsistenciaInspeccion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InconsistenciaInspeccion_PosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_InconsistenciaInspeccion_GPosicionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InconsistenciaInspeccion_LlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_InconsistenciaInspeccion_GLlantaEsperadaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InconsistenciaInspeccion_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_InconsistenciaInspeccion_GLlantaEncontradaId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_InconsistenciaInspeccion_InspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_InconsistenciaInspeccion_GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "SUbicacion");

            migrationBuilder.RenameColumn(
                name: "TamanoBytes",
                table: "TBL_EvidenciaInspeccion",
                newName: "NTamanoBytes");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_EvidenciaInspeccion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "RetenerHasta",
                table: "TBL_EvidenciaInspeccion",
                newName: "DRetenerHasta");

            migrationBuilder.RenameColumn(
                name: "NombreArchivo",
                table: "TBL_EvidenciaInspeccion",
                newName: "SNombreArchivo");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "TBL_EvidenciaInspeccion",
                newName: "SMimeType");

            migrationBuilder.RenameColumn(
                name: "InspeccionId",
                table: "TBL_EvidenciaInspeccion",
                newName: "GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "InconsistenciaInspeccionId",
                table: "TBL_EvidenciaInspeccion",
                newName: "GInconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "TBL_EvidenciaInspeccion",
                newName: "SHash");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_EvidenciaInspeccion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_EvidenciaInspeccion",
                newName: "GId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_EvidenciaFlujo",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_EvidenciaFlujo",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                table: "TBL_EvidenciaFlujo",
                newName: "SUbicacion");

            migrationBuilder.RenameColumn(
                name: "TamanoBytes",
                table: "TBL_EvidenciaFlujo",
                newName: "NTamanoBytes");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_EvidenciaFlujo",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "OrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                newName: "GOrdenServicioLlantaId");

            migrationBuilder.RenameColumn(
                name: "NombreArchivo",
                table: "TBL_EvidenciaFlujo",
                newName: "SNombreArchivo");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "TBL_EvidenciaFlujo",
                newName: "SMimeType");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "TBL_EvidenciaFlujo",
                newName: "SHash");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_EvidenciaFlujo",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_EvidenciaFlujo",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_EvidenciaFlujo",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_EvidenciaFlujo",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_EvidenciaFlujo_OrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                newName: "IX_EvidenciaFlujo_GOrdenServicioLlantaId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_EstadoLlanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_EstadoLlanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_EstadoLlanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PermiteMontaje",
                table: "TBL_EstadoLlanta",
                newName: "BPermiteMontaje");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_EstadoLlanta",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_EstadoLlanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_EstadoLlanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "EsDisposicionFinal",
                table: "TBL_EstadoLlanta",
                newName: "BEsDisposicionFinal");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_EstadoLlanta",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_EstadoLlanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_EstadoLlanta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_EstadoLlanta_Codigo",
                table: "TBL_EstadoLlanta",
                newName: "IX_EstadoLlanta_SCodigo");

            migrationBuilder.RenameColumn(
                name: "VehiculoId",
                table: "TBL_EjeVehiculo",
                newName: "GVehiculoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_EjeVehiculo",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_EjeVehiculo",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TipoEje",
                table: "TBL_EjeVehiculo",
                newName: "STipoEje");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_EjeVehiculo",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Orden",
                table: "TBL_EjeVehiculo",
                newName: "NOrden");

            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "TBL_EjeVehiculo",
                newName: "NNumero");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_EjeVehiculo",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_EjeVehiculo",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_EjeVehiculo",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_EjeVehiculo",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_EjeVehiculo",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_EjeVehiculo_VehiculoId_Numero",
                table: "TBL_EjeVehiculo",
                newName: "IX_EjeVehiculo_GVehiculoId_NNumero");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Dimension",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Dimension",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Dimension",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Dimension",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Dimension",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Dimension",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Dimension",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Dimension",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Dimension",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_Dimension_Codigo",
                table: "TBL_Dimension",
                newName: "IX_Dimension_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TipoVehiculo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "STipoVehiculo");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_ConfiguracionVehiculo",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ConfiguracionVehiculo",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ConfiguracionVehiculo_Codigo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "IX_ConfiguracionVehiculo_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "SUbicacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ConfiguracionPosicion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Orden",
                table: "TBL_ConfiguracionPosicion",
                newName: "NOrden");

            migrationBuilder.RenameColumn(
                name: "Lado",
                table: "TBL_ConfiguracionPosicion",
                newName: "SLado");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "ConfiguracionEjeId",
                table: "TBL_ConfiguracionPosicion",
                newName: "GConfiguracionEjeId");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_ConfiguracionPosicion",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ConfiguracionPosicion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ConfiguracionPosicion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ConfiguracionPosicion_ConfiguracionEjeId_Orden",
                table: "TBL_ConfiguracionPosicion",
                newName: "IX_ConfiguracionPosicion_GConfiguracionEjeId_NOrden");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ConfiguracionPosicion_ConfiguracionEjeId_Codigo",
                table: "TBL_ConfiguracionPosicion",
                newName: "IX_ConfiguracionPosicion_GConfiguracionEjeId_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ConfiguracionEje",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ConfiguracionEje",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TipoEje",
                table: "TBL_ConfiguracionEje",
                newName: "STipoEje");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ConfiguracionEje",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Orden",
                table: "TBL_ConfiguracionEje",
                newName: "NOrden");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_ConfiguracionEje",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ConfiguracionEje",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ConfiguracionEje",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "ConfiguracionVehiculoId",
                table: "TBL_ConfiguracionEje",
                newName: "GConfiguracionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ConfiguracionEje",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ConfiguracionEje",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ConfiguracionEje_ConfiguracionVehiculoId_Orden",
                table: "TBL_ConfiguracionEje",
                newName: "IX_ConfiguracionEje_GConfiguracionVehiculoId_NOrden");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_CondicionLlanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_CondicionLlanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_CondicionLlanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "RequiereCausa",
                table: "TBL_CondicionLlanta",
                newName: "BRequiereCausa");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_CondicionLlanta",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_CondicionLlanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_CondicionLlanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_CondicionLlanta",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_CondicionLlanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_CondicionLlanta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_CondicionLlanta_Codigo",
                table: "TBL_CondicionLlanta",
                newName: "IX_CondicionLlanta_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_Centro",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_Centro",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_Centro",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Relevancia",
                table: "TBL_Centro",
                newName: "SRelevancia");

            migrationBuilder.RenameColumn(
                name: "RegionalId",
                table: "TBL_Centro",
                newName: "GRegionalId");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_Centro",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_Centro",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_Centro",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_Centro",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_Centro",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Centro",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_Centro_RegionalId",
                table: "TBL_Centro",
                newName: "IX_Centro_GRegionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Centro_Codigo",
                table: "TBL_Centro",
                newName: "IX_Centro_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_CausaLlanta",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_CausaLlanta",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_CausaLlanta",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "TBL_CausaLlanta",
                newName: "SNombre");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_CausaLlanta",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_CausaLlanta",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "TBL_CausaLlanta",
                newName: "SCodigo");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_CausaLlanta",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_CausaLlanta",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_CausaLlanta_Codigo",
                table: "TBL_CausaLlanta",
                newName: "IX_CausaLlanta_SCodigo");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_CargaMasiva",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_CargaMasiva",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Usuario",
                table: "TBL_CargaMasiva",
                newName: "SUsuario");

            migrationBuilder.RenameColumn(
                name: "TotalFilas",
                table: "TBL_CargaMasiva",
                newName: "NTotalFilas");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_CargaMasiva",
                newName: "STipo");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_CargaMasiva",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "NombreArchivo",
                table: "TBL_CargaMasiva",
                newName: "SNombreArchivo");

            migrationBuilder.RenameColumn(
                name: "FilasValidas",
                table: "TBL_CargaMasiva",
                newName: "NFilasValidas");

            migrationBuilder.RenameColumn(
                name: "FilasJson",
                table: "TBL_CargaMasiva",
                newName: "SFilasJson");

            migrationBuilder.RenameColumn(
                name: "FilasConError",
                table: "TBL_CargaMasiva",
                newName: "NFilasConError");

            migrationBuilder.RenameColumn(
                name: "FechaProcesamiento",
                table: "TBL_CargaMasiva",
                newName: "DFechaProcesamiento");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_CargaMasiva",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_CargaMasiva",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_CargaMasiva",
                newName: "SEstado");

            migrationBuilder.RenameColumn(
                name: "ErroresJson",
                table: "TBL_CargaMasiva",
                newName: "SErroresJson");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_CargaMasiva",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_CargaMasiva",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_CargaMasiva_Usuario_FechaCreacion",
                table: "TBL_CargaMasiva",
                newName: "IX_CargaMasiva_SUsuario_DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "ValoresNuevos",
                table: "TBL_Auditoria",
                newName: "SValoresNuevos");

            migrationBuilder.RenameColumn(
                name: "ValoresAnteriores",
                table: "TBL_Auditoria",
                newName: "SValoresAnteriores");

            migrationBuilder.RenameColumn(
                name: "Usuario",
                table: "TBL_Auditoria",
                newName: "SUsuario");

            migrationBuilder.RenameColumn(
                name: "Origen",
                table: "TBL_Auditoria",
                newName: "SOrigen");

            migrationBuilder.RenameColumn(
                name: "Identificador",
                table: "TBL_Auditoria",
                newName: "SIdentificador");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "TBL_Auditoria",
                newName: "DFecha");

            migrationBuilder.RenameColumn(
                name: "Entidad",
                table: "TBL_Auditoria",
                newName: "SEntidad");

            migrationBuilder.RenameColumn(
                name: "DireccionIp",
                table: "TBL_Auditoria",
                newName: "SDireccionIp");

            migrationBuilder.RenameColumn(
                name: "Accion",
                table: "TBL_Auditoria",
                newName: "SAccion");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_Auditoria",
                newName: "NId");

            migrationBuilder.RenameIndex(
                name: "IX_Auditoria_EntidadFecha",
                table: "TBL_Auditoria",
                newName: "IX_Auditoria_SEntidad_SIdentificador_DFecha");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "GPosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "MovimientoOrigenId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "GMovimientoOrigenId");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "KilometrajeRecorrido",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "NKilometrajeRecorrido");

            migrationBuilder.RenameColumn(
                name: "KilometrajeMontaje",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "NKilometrajeMontaje");

            migrationBuilder.RenameColumn(
                name: "KilometrajeDesmontaje",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "NKilometrajeDesmontaje");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "DFechaInicio");

            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "DFechaFin");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "EsActiva",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "BEsActiva");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "GId");


            migrationBuilder.RenameColumn(
                name: "VehiculoId",
                table: "TBL_AlertaInspeccion",
                newName: "GVehiculoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_AlertaInspeccion",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_AlertaInspeccion",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "TBL_AlertaInspeccion",
                newName: "STipo");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_AlertaInspeccion",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "PosicionVehiculoId",
                table: "TBL_AlertaInspeccion",
                newName: "GPosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_AlertaInspeccion",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "InspeccionId",
                table: "TBL_AlertaInspeccion",
                newName: "GInspeccionId");

            migrationBuilder.RenameColumn(
                name: "InspeccionDetalleId",
                table: "TBL_AlertaInspeccion",
                newName: "GInspeccionDetalleId");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_AlertaInspeccion",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_AlertaInspeccion",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_AlertaInspeccion",
                newName: "NEstado");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "TBL_AlertaInspeccion",
                newName: "SDescripcion");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_AlertaInspeccion",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_AlertaInspeccion",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_AlertaInspeccion",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_AlertaInspeccion_InspeccionId",
                table: "TBL_AlertaInspeccion",
                newName: "IX_AlertaInspeccion_GInspeccionId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_AlertaInspeccion_InspeccionDetalleId_Tipo",
                table: "TBL_AlertaInspeccion",
                newName: "IX_AlertaInspeccion_GInspeccionDetalleId_STipo");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_AlertaInspeccion_CentroId_Estado_FechaCreacion",
                table: "TBL_AlertaInspeccion",
                newName: "IX_AlertaInspeccion_GCentroId_NEstado_DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_AlertaHistorial",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_AlertaHistorial",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_AlertaHistorial",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "Observacion",
                table: "TBL_AlertaHistorial",
                newName: "SObservacion");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_AlertaHistorial",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_AlertaHistorial",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "EstadoNuevo",
                table: "TBL_AlertaHistorial",
                newName: "NEstadoNuevo");

            migrationBuilder.RenameColumn(
                name: "EstadoAnterior",
                table: "TBL_AlertaHistorial",
                newName: "NEstadoAnterior");

            migrationBuilder.RenameColumn(
                name: "AlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                newName: "GAlertaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_AlertaHistorial",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_AlertaHistorial",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_AlertaHistorial_AlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                newName: "IX_AlertaHistorial_GAlertaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "VehiculoId",
                table: "TBL_ActividadProgramada",
                newName: "GVehiculoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioModificacion",
                table: "TBL_ActividadProgramada",
                newName: "SUsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "UsuarioCreacion",
                table: "TBL_ActividadProgramada",
                newName: "SUsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "TipoActividad",
                table: "TBL_ActividadProgramada",
                newName: "STipoActividad");

            migrationBuilder.RenameColumn(
                name: "TecnicoUsuarioId",
                table: "TBL_ActividadProgramada",
                newName: "GTecnicoUsuarioId");

            migrationBuilder.RenameColumn(
                name: "TecnicoId",
                table: "TBL_ActividadProgramada",
                newName: "STecnicoId");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "TBL_ActividadProgramada",
                newName: "TRowVersion");

            migrationBuilder.RenameColumn(
                name: "ReasignadoPor",
                table: "TBL_ActividadProgramada",
                newName: "SReasignadoPor");

            migrationBuilder.RenameColumn(
                name: "Prioridad",
                table: "TBL_ActividadProgramada",
                newName: "SPrioridad");

            migrationBuilder.RenameColumn(
                name: "PosicionVehiculoId",
                table: "TBL_ActividadProgramada",
                newName: "GPosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "OrigenEntidadId",
                table: "TBL_ActividadProgramada",
                newName: "GOrigenEntidadId");

            migrationBuilder.RenameColumn(
                name: "Origen",
                table: "TBL_ActividadProgramada",
                newName: "SOrigen");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "TBL_ActividadProgramada",
                newName: "SObservaciones");

            migrationBuilder.RenameColumn(
                name: "MotivoCancelacion",
                table: "TBL_ActividadProgramada",
                newName: "SMotivoCancelacion");

            migrationBuilder.RenameColumn(
                name: "LlantaId",
                table: "TBL_ActividadProgramada",
                newName: "GLlantaId");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "TBL_ActividadProgramada",
                newName: "SIdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "GrupoProgramacionId",
                table: "TBL_ActividadProgramada",
                newName: "GGrupoProgramacionId");

            migrationBuilder.RenameColumn(
                name: "FechaProgramada",
                table: "TBL_ActividadProgramada",
                newName: "DFechaProgramada");

            migrationBuilder.RenameColumn(
                name: "FechaModificacion",
                table: "TBL_ActividadProgramada",
                newName: "DFechaModificacion");

            migrationBuilder.RenameColumn(
                name: "FechaInicioReal",
                table: "TBL_ActividadProgramada",
                newName: "DFechaInicioReal");

            migrationBuilder.RenameColumn(
                name: "FechaFinReal",
                table: "TBL_ActividadProgramada",
                newName: "DFechaFinReal");

            migrationBuilder.RenameColumn(
                name: "FechaFinProgramada",
                table: "TBL_ActividadProgramada",
                newName: "DFechaFinProgramada");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "TBL_ActividadProgramada",
                newName: "DFechaCreacion");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "TBL_ActividadProgramada",
                newName: "NEstado");

            migrationBuilder.RenameColumn(
                name: "CentroId",
                table: "TBL_ActividadProgramada",
                newName: "GCentroId");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "TBL_ActividadProgramada",
                newName: "BActivo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TBL_ActividadProgramada",
                newName: "GId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ActividadProgramada_VehiculoId",
                table: "TBL_ActividadProgramada",
                newName: "IX_ActividadProgramada_GVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_FechaProgramada_FechaFinProgramada",
                table: "TBL_ActividadProgramada",
                newName: "IX_ActividadProgramada_GTecnicoUsuarioId_DFechaProgramada_DFechaFinProgramada");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoId_Estado_FechaProgramada",
                table: "TBL_ActividadProgramada",
                newName: "IX_ActividadProgramada_STecnicoId_NEstado_DFechaProgramada");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ActividadProgramada_Origen_OrigenEntidadId",
                table: "TBL_ActividadProgramada",
                newName: "IX_ActividadProgramada_SOrigen_GOrigenEntidadId");

            migrationBuilder.RenameIndex(
                name: "IX_TBL_ActividadProgramada_CentroId",
                table: "TBL_ActividadProgramada",
                newName: "IX_ActividadProgramada_GCentroId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionLlantaPosicion_GLlantaId",
                table: "TBL_AsignacionLlantaPosicion",
                column: "GLlantaId",
                unique: true,
                filter: "[BEsActiva] = 1");
migrationBuilder.CreateIndex(
    name: "IX_AsignacionLlantaPosicion_GPosicionVehiculoId",
    table: "TBL_AsignacionLlantaPosicion",
    column: "GPosicionVehiculoId",
    unique: true,
    filter: "[BEsActiva] = 1");
            migrationBuilder.CreateIndex(
                name: "IX_ActividadProgramada_GTecnicoUsuarioId_GVehiculoId_STipoActividad_DFechaProgramada",
                table: "TBL_ActividadProgramada",
                columns: new[] { "GTecnicoUsuarioId", "GVehiculoId", "STipoActividad", "DFechaProgramada" },
                unique: true,
                filter: "[BActivo] = 1 AND [NEstado] <> 4 AND [GTecnicoUsuarioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadProgramada_SIdempotencyKey",
                table: "TBL_ActividadProgramada",
                column: "SIdempotencyKey",
                unique: true,
                filter: "[SIdempotencyKey] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Centro_GCentroId",
                table: "TBL_ActividadProgramada",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Usuario_GTecnicoUsuarioId",
                table: "TBL_ActividadProgramada",
                column: "GTecnicoUsuarioId",
                principalTable: "TBL_Usuario",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Vehiculo_GVehiculoId",
                table: "TBL_ActividadProgramada",
                column: "GVehiculoId",
                principalTable: "TBL_Vehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AlertaHistorial_TBL_AlertaInspeccion_GAlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                column: "GAlertaInspeccionId",
                principalTable: "TBL_AlertaInspeccion",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_InspeccionDetalle_GInspeccionDetalleId",
                table: "TBL_AlertaInspeccion",
                column: "GInspeccionDetalleId",
                principalTable: "TBL_InspeccionDetalle",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_Inspeccion_GInspeccionId",
                table: "TBL_AlertaInspeccion",
                column: "GInspeccionId",
                principalTable: "TBL_Inspeccion",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_Llanta_GLlantaId",
                table: "TBL_AsignacionLlantaPosicion",
                column: "GLlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_PosicionVehiculo_GPosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion",
                column: "GPosicionVehiculoId",
                principalTable: "TBL_PosicionVehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Centro_TBL_Regional_GRegionalId",
                table: "TBL_Centro",
                column: "GRegionalId",
                principalTable: "TBL_Regional",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ConfiguracionEje_TBL_ConfiguracionVehiculo_GConfiguracionVehiculoId",
                table: "TBL_ConfiguracionEje",
                column: "GConfiguracionVehiculoId",
                principalTable: "TBL_ConfiguracionVehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ConfiguracionPosicion_TBL_ConfiguracionEje_GConfiguracionEjeId",
                table: "TBL_ConfiguracionPosicion",
                column: "GConfiguracionEjeId",
                principalTable: "TBL_ConfiguracionEje",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_EjeVehiculo_TBL_Vehiculo_GVehiculoId",
                table: "TBL_EjeVehiculo",
                column: "GVehiculoId",
                principalTable: "TBL_Vehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_EvidenciaFlujo_TBL_OrdenServicioLlanta_GOrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                column: "GOrdenServicioLlantaId",
                principalTable: "TBL_OrdenServicioLlanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Inspeccion_GInspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                column: "GInspeccionId",
                principalTable: "TBL_Inspeccion",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_GLlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "GLlantaEncontradaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_GLlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "GLlantaEsperadaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_PosicionVehiculo_GPosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                column: "GPosicionVehiculoId",
                principalTable: "TBL_PosicionVehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Centro_GCentroId",
                table: "TBL_Inspeccion",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Vehiculo_GVehiculoId",
                table: "TBL_Inspeccion",
                column: "GVehiculoId",
                principalTable: "TBL_Vehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CausaLlanta_GCausaLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "GCausaLlantaId",
                principalTable: "TBL_CausaLlanta",
                principalColumn: "GId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CondicionLlanta_GCondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "GCondicionLlantaId",
                principalTable: "TBL_CondicionLlanta",
                principalColumn: "GId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Inspeccion_GInspeccionId",
                table: "TBL_InspeccionDetalle",
                column: "GInspeccionId",
                principalTable: "TBL_Inspeccion",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Llanta_GLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "GLlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_PosicionVehiculo_GPosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                column: "GPosicionVehiculoId",
                principalTable: "TBL_PosicionVehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_RecomendacionInspeccion_GRecomendacionId",
                table: "TBL_InspeccionDetalle",
                column: "GRecomendacionId",
                principalTable: "TBL_RecomendacionInspeccion",
                principalColumn: "GId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Centro_GCentroId",
                table: "TBL_Llanta",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Dimension_GDimensionId",
                table: "TBL_Llanta",
                column: "GDimensionId",
                principalTable: "TBL_Dimension",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_EstadoLlanta_GEstadoLlantaId",
                table: "TBL_Llanta",
                column: "GEstadoLlantaId",
                principalTable: "TBL_EstadoLlanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Marca_GMarcaId",
                table: "TBL_Llanta",
                column: "GMarcaId",
                principalTable: "TBL_Marca",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Referencia_GReferenciaId",
                table: "TBL_Llanta",
                column: "GReferenciaId",
                principalTable: "TBL_Referencia",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_TipoLlanta_GTipoLlantaId",
                table: "TBL_Llanta",
                column: "GTipoLlantaId",
                principalTable: "TBL_TipoLlanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_LlantaTemporal_TBL_InconsistenciaInspeccion_GInconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                column: "GInconsistenciaInspeccionId",
                principalTable: "TBL_InconsistenciaInspeccion",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_Centro_GCentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                column: "GCentroOrigenId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_ProveedorServicio_GProveedorId",
                table: "TBL_LoteEnvioReparacion",
                column: "GProveedorId",
                principalTable: "TBL_ProveedorServicio",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Movimiento_TBL_Centro_GCentroId",
                table: "TBL_Movimiento",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Llanta_GLlantaId",
                table: "TBL_MovimientoDetalle",
                column: "GLlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Movimiento_GMovimientoId",
                table: "TBL_MovimientoDetalle",
                column: "GMovimientoId",
                principalTable: "TBL_Movimiento",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Centro_GCentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                column: "GCentroOrigenId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Llanta_GLlantaId",
                table: "TBL_OrdenServicioLlanta",
                column: "GLlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_GLoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                column: "GLoteEnvioReparacionId",
                principalTable: "TBL_LoteEnvioReparacion",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_ProveedorServicio_GProveedorId",
                table: "TBL_OrdenServicioLlanta",
                column: "GProveedorId",
                principalTable: "TBL_ProveedorServicio",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_EjeVehiculo_GEjeVehiculoId",
                table: "TBL_PosicionVehiculo",
                column: "GEjeVehiculoId",
                principalTable: "TBL_EjeVehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_Llanta_GLlantaActualId",
                table: "TBL_PosicionVehiculo",
                column: "GLlantaActualId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Referencia_TBL_Marca_GMarcaId",
                table: "TBL_Referencia",
                column: "GMarcaId",
                principalTable: "TBL_Marca",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Permiso_GPermisoId",
                table: "TBL_RolPermiso",
                column: "GPermisoId",
                principalTable: "TBL_Permiso",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Rol_GRolId",
                table: "TBL_RolPermiso",
                column: "GRolId",
                principalTable: "TBL_Rol",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Centro_GCentroId",
                table: "TBL_SolicitudOperacion",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Llanta_GLlantaId",
                table: "TBL_SolicitudOperacion",
                column: "GLlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Usuario_TBL_Centro_GCentroId",
                table: "TBL_Usuario",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Usuario_TBL_Rol_GRolId",
                table: "TBL_Usuario",
                column: "GRolId",
                principalTable: "TBL_Rol",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Centro_GCentroId",
                table: "TBL_UsuarioCentro",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Usuario_GUsuarioId",
                table: "TBL_UsuarioCentro",
                column: "GUsuarioId",
                principalTable: "TBL_Usuario",
                principalColumn: "GId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Vehiculo_TBL_Centro_GCentroId",
                table: "TBL_Vehiculo",
                column: "GCentroId",
                principalTable: "TBL_Centro",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_GConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                column: "GConfiguracionVehiculoId",
                principalTable: "TBL_ConfiguracionVehiculo",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Centro_GCentroId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Usuario_GTecnicoUsuarioId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Vehiculo_GVehiculoId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AlertaHistorial_TBL_AlertaInspeccion_GAlertaInspeccionId",
                table: "TBL_AlertaHistorial");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_InspeccionDetalle_GInspeccionDetalleId",
                table: "TBL_AlertaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_Inspeccion_GInspeccionId",
                table: "TBL_AlertaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_Llanta_GLlantaId",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_PosicionVehiculo_GPosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Centro_TBL_Regional_GRegionalId",
                table: "TBL_Centro");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ConfiguracionEje_TBL_ConfiguracionVehiculo_GConfiguracionVehiculoId",
                table: "TBL_ConfiguracionEje");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ConfiguracionPosicion_TBL_ConfiguracionEje_GConfiguracionEjeId",
                table: "TBL_ConfiguracionPosicion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_EjeVehiculo_TBL_Vehiculo_GVehiculoId",
                table: "TBL_EjeVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_EvidenciaFlujo_TBL_OrdenServicioLlanta_GOrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Inspeccion_GInspeccionId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_GLlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_GLlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_PosicionVehiculo_GPosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Centro_GCentroId",
                table: "TBL_Inspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Vehiculo_GVehiculoId",
                table: "TBL_Inspeccion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CausaLlanta_GCausaLlantaId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CondicionLlanta_GCondicionLlantaId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Inspeccion_GInspeccionId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Llanta_GLlantaId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_PosicionVehiculo_GPosicionVehiculoId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_RecomendacionInspeccion_GRecomendacionId",
                table: "TBL_InspeccionDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Centro_GCentroId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Dimension_GDimensionId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_EstadoLlanta_GEstadoLlantaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Marca_GMarcaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_Referencia_GReferenciaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Llanta_TBL_TipoLlanta_GTipoLlantaId",
                table: "TBL_Llanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_LlantaTemporal_TBL_InconsistenciaInspeccion_GInconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_Centro_GCentroOrigenId",
                table: "TBL_LoteEnvioReparacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_ProveedorServicio_GProveedorId",
                table: "TBL_LoteEnvioReparacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Movimiento_TBL_Centro_GCentroId",
                table: "TBL_Movimiento");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Llanta_GLlantaId",
                table: "TBL_MovimientoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Movimiento_GMovimientoId",
                table: "TBL_MovimientoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Centro_GCentroOrigenId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Llanta_GLlantaId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_GLoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_ProveedorServicio_GProveedorId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_EjeVehiculo_GEjeVehiculoId",
                table: "TBL_PosicionVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_Llanta_GLlantaActualId",
                table: "TBL_PosicionVehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Referencia_TBL_Marca_GMarcaId",
                table: "TBL_Referencia");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Permiso_GPermisoId",
                table: "TBL_RolPermiso");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Rol_GRolId",
                table: "TBL_RolPermiso");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Centro_GCentroId",
                table: "TBL_SolicitudOperacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Llanta_GLlantaId",
                table: "TBL_SolicitudOperacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Usuario_TBL_Centro_GCentroId",
                table: "TBL_Usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Usuario_TBL_Rol_GRolId",
                table: "TBL_Usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Centro_GCentroId",
                table: "TBL_UsuarioCentro");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Usuario_GUsuarioId",
                table: "TBL_UsuarioCentro");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Vehiculo_TBL_Centro_GCentroId",
                table: "TBL_Vehiculo");

            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_GConfiguracionVehiculoId",
                table: "TBL_Vehiculo");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionLlantaPosicion_GLlantaId",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropIndex(
                name: "IX_ActividadProgramada_GTecnicoUsuarioId_GVehiculoId_STipoActividad_DFechaProgramada",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropIndex(
                name: "IX_ActividadProgramada_SIdempotencyKey",
                table: "TBL_ActividadProgramada");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Vehiculo",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Vehiculo",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Vehiculo",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipo",
                table: "TBL_Vehiculo",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SPlaca",
                table: "TBL_Vehiculo",
                newName: "Placa");

            migrationBuilder.RenameColumn(
                name: "SNumeroInterno",
                table: "TBL_Vehiculo",
                newName: "NumeroInterno");

            migrationBuilder.RenameColumn(
                name: "SEstado",
                table: "TBL_Vehiculo",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "NKilometraje",
                table: "TBL_Vehiculo",
                newName: "Kilometraje");

            migrationBuilder.RenameColumn(
                name: "GConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                newName: "ConfiguracionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_Vehiculo",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Vehiculo",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Vehiculo",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Vehiculo",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Vehiculo",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Vehiculo_SNumeroInterno",
                table: "TBL_Vehiculo",
                newName: "IX_TBL_Vehiculo_NumeroInterno");

            migrationBuilder.RenameIndex(
                name: "IX_Vehiculo_GConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                newName: "IX_TBL_Vehiculo_ConfiguracionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehiculo_GCentroId",
                table: "TBL_Vehiculo",
                newName: "IX_TBL_Vehiculo_CentroId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_UsuarioCentro",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_UsuarioCentro",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_UsuarioCentro",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "GUsuarioId",
                table: "TBL_UsuarioCentro",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_UsuarioCentro",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_UsuarioCentro",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_UsuarioCentro",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_UsuarioCentro",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_UsuarioCentro",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioCentro_GUsuarioId_GCentroId",
                table: "TBL_UsuarioCentro",
                newName: "IX_TBL_UsuarioCentro_UsuarioId_CentroId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioCentro_GCentroId",
                table: "TBL_UsuarioCentro",
                newName: "IX_TBL_UsuarioCentro_CentroId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Usuario",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Usuario",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Usuario",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUsername",
                table: "TBL_Usuario",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "SPasswordHash",
                table: "TBL_Usuario",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Usuario",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "GRolId",
                table: "TBL_Usuario",
                newName: "RolId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_Usuario",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Usuario",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Usuario",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Usuario",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Usuario",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_SUsername",
                table: "TBL_Usuario",
                newName: "IX_TBL_Usuario_Username");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_GRolId",
                table: "TBL_Usuario",
                newName: "IX_TBL_Usuario_RolId");

            migrationBuilder.RenameIndex(
                name: "IX_Usuario_GCentroId",
                table: "TBL_Usuario",
                newName: "IX_TBL_Usuario_CentroId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_TipoLlanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_TipoLlanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_TipoLlanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_TipoLlanta",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_TipoLlanta",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_TipoLlanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_TipoLlanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_TipoLlanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_TipoLlanta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TipoLlanta_SCodigo",
                table: "TBL_TipoLlanta",
                newName: "IX_TipoLlanta_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_SolicitudOperacion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_SolicitudOperacion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_SolicitudOperacion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipoDestino",
                table: "TBL_SolicitudOperacion",
                newName: "TipoDestino");

            migrationBuilder.RenameColumn(
                name: "STipo",
                table: "TBL_SolicitudOperacion",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SSolicitante",
                table: "TBL_SolicitudOperacion",
                newName: "Solicitante");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_SolicitudOperacion",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SMotivoRechazo",
                table: "TBL_SolicitudOperacion",
                newName: "MotivoRechazo");

            migrationBuilder.RenameColumn(
                name: "SMotivo",
                table: "TBL_SolicitudOperacion",
                newName: "Motivo");

            migrationBuilder.RenameColumn(
                name: "SDestinoDesplazada",
                table: "TBL_SolicitudOperacion",
                newName: "DestinoDesplazada");

            migrationBuilder.RenameColumn(
                name: "SAprobador",
                table: "TBL_SolicitudOperacion",
                newName: "Aprobador");

            migrationBuilder.RenameColumn(
                name: "NKilometrajeVehiculo",
                table: "TBL_SolicitudOperacion",
                newName: "KilometrajeVehiculo");

            migrationBuilder.RenameColumn(
                name: "NEstado",
                table: "TBL_SolicitudOperacion",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "GPosicionOrigenId",
                table: "TBL_SolicitudOperacion",
                newName: "PosicionOrigenId");

            migrationBuilder.RenameColumn(
                name: "GPosicionDestinoId",
                table: "TBL_SolicitudOperacion",
                newName: "PosicionDestinoId");

            migrationBuilder.RenameColumn(
                name: "GPosicionDestinoDesplazadaId",
                table: "TBL_SolicitudOperacion",
                newName: "PosicionDestinoDesplazadaId");

            migrationBuilder.RenameColumn(
                name: "GMovimientoEjecutadoId",
                table: "TBL_SolicitudOperacion",
                newName: "MovimientoEjecutadoId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_SolicitudOperacion",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "GLlantaDesplazadaId",
                table: "TBL_SolicitudOperacion",
                newName: "LlantaDesplazadaId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_SolicitudOperacion",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "GCentroDestinoId",
                table: "TBL_SolicitudOperacion",
                newName: "CentroDestinoId");

            migrationBuilder.RenameColumn(
                name: "GActividadProgramadaId",
                table: "TBL_SolicitudOperacion",
                newName: "ActividadProgramadaId");

            migrationBuilder.RenameColumn(
                name: "DFechaRecepcionDestino",
                table: "TBL_SolicitudOperacion",
                newName: "FechaRecepcionDestino");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_SolicitudOperacion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaDecision",
                table: "TBL_SolicitudOperacion",
                newName: "FechaDecision");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_SolicitudOperacion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_SolicitudOperacion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_SolicitudOperacion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitudOperacion_GLlantaId",
                table: "TBL_SolicitudOperacion",
                newName: "IX_TBL_SolicitudOperacion_LlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitudOperacion_GCentroId_NEstado_DFechaCreacion",
                table: "TBL_SolicitudOperacion",
                newName: "IX_TBL_SolicitudOperacion_CentroId_Estado_FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "GPermisoId",
                table: "TBL_RolPermiso",
                newName: "PermisoId");

            migrationBuilder.RenameColumn(
                name: "GRolId",
                table: "TBL_RolPermiso",
                newName: "RolId");

            migrationBuilder.RenameIndex(
                name: "IX_RolPermiso_GPermisoId",
                table: "TBL_RolPermiso",
                newName: "IX_TBL_RolPermiso_PermisoId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Rol",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Rol",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Rol",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Rol",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Rol",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Rol",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Rol",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Rol",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Rol",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Rol_SCodigo",
                table: "TBL_Rol",
                newName: "IX_TBL_Rol_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Regional",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Regional",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Regional",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Regional",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Regional",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Regional",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Regional",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Regional",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Regional",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Regional_SCodigo",
                table: "TBL_Regional",
                newName: "IX_Regional_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Referencia",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Referencia",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Referencia",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Referencia",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Referencia",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "GMarcaId",
                table: "TBL_Referencia",
                newName: "MarcaId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Referencia",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Referencia",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Referencia",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Referencia",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Referencia_SCodigo",
                table: "TBL_Referencia",
                newName: "IX_Referencia_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Referencia_GMarcaId",
                table: "TBL_Referencia",
                newName: "IX_TBL_Referencia_MarcaId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_RecomendacionInspeccion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_RecomendacionInspeccion",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_RecomendacionInspeccion",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_RecomendacionInspeccion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BEsCandidataReencauche",
                table: "TBL_RecomendacionInspeccion",
                newName: "EsCandidataReencauche");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_RecomendacionInspeccion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_RecomendacionInspeccion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_RecomendacionInspeccion_SCodigo",
                table: "TBL_RecomendacionInspeccion",
                newName: "IX_TBL_RecomendacionInspeccion_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ProveedorServicio",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ProveedorServicio",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ProveedorServicio",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipo",
                table: "TBL_ProveedorServicio",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_ProveedorServicio",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_ProveedorServicio",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ProveedorServicio",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ProveedorServicio",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ProveedorServicio",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ProveedorServicio",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ProveedorServicio_SCodigo",
                table: "TBL_ProveedorServicio",
                newName: "IX_TBL_ProveedorServicio_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_PosicionVehiculo",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_PosicionVehiculo",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_PosicionVehiculo",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUbicacion",
                table: "TBL_PosicionVehiculo",
                newName: "Ubicacion");

            migrationBuilder.RenameColumn(
                name: "SLado",
                table: "TBL_PosicionVehiculo",
                newName: "Lado");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_PosicionVehiculo",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "NOrden",
                table: "TBL_PosicionVehiculo",
                newName: "Orden");

            migrationBuilder.RenameColumn(
                name: "GLlantaActualId",
                table: "TBL_PosicionVehiculo",
                newName: "LlantaActualId");

            migrationBuilder.RenameColumn(
                name: "GEjeVehiculoId",
                table: "TBL_PosicionVehiculo",
                newName: "EjeVehiculoId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_PosicionVehiculo",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_PosicionVehiculo",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_PosicionVehiculo",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_PosicionVehiculo",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_PosicionVehiculo_GLlantaActualId",
                table: "TBL_PosicionVehiculo",
                newName: "IX_TBL_PosicionVehiculo_LlantaActualId");

            migrationBuilder.RenameIndex(
                name: "IX_PosicionVehiculo_GEjeVehiculoId_SCodigo",
                table: "TBL_PosicionVehiculo",
                newName: "IX_TBL_PosicionVehiculo_EjeVehiculoId_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Permiso",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Permiso",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Permiso",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Permiso",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Permiso",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Permiso",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Permiso",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Permiso",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Permiso",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Permiso_SCodigo",
                table: "TBL_Permiso",
                newName: "IX_TBL_Permiso_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ParametroReencauche",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ParametroReencauche",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ParametroReencauche",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "NProfundidadMinima",
                table: "TBL_ParametroReencauche",
                newName: "ProfundidadMinima");

            migrationBuilder.RenameColumn(
                name: "NMaximoReencauches",
                table: "TBL_ParametroReencauche",
                newName: "MaximoReencauches");

            migrationBuilder.RenameColumn(
                name: "GDimensionId",
                table: "TBL_ParametroReencauche",
                newName: "DimensionId");

            migrationBuilder.RenameColumn(
                name: "DVigenteHasta",
                table: "TBL_ParametroReencauche",
                newName: "VigenteHasta");

            migrationBuilder.RenameColumn(
                name: "DVigenteDesde",
                table: "TBL_ParametroReencauche",
                newName: "VigenteDesde");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ParametroReencauche",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ParametroReencauche",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ParametroReencauche",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ParametroReencauche",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ParametroAlerta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ParametroAlerta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ParametroAlerta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUnidad",
                table: "TBL_ParametroAlerta",
                newName: "Unidad");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_ParametroAlerta",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "NValor",
                table: "TBL_ParametroAlerta",
                newName: "Valor");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ParametroAlerta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ParametroAlerta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ParametroAlerta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ParametroAlerta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ParametroAlerta_SCodigo",
                table: "TBL_ParametroAlerta",
                newName: "IX_TBL_ParametroAlerta_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_OrdenServicioLlanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioOpciona",
                table: "TBL_OrdenServicioLlanta",
                newName: "UsuarioOpciona");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SSolicitante",
                table: "TBL_OrdenServicioLlanta",
                newName: "Solicitante");

            migrationBuilder.RenameColumn(
                name: "SResultado",
                table: "TBL_OrdenServicioLlanta",
                newName: "Resultado");

            migrationBuilder.RenameColumn(
                name: "SOrigenTipo",
                table: "TBL_OrdenServicioLlanta",
                newName: "OrigenTipo");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_OrdenServicioLlanta",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SMotivoRechazo",
                table: "TBL_OrdenServicioLlanta",
                newName: "MotivoRechazo");

            migrationBuilder.RenameColumn(
                name: "SMotivo",
                table: "TBL_OrdenServicioLlanta",
                newName: "Motivo");

            migrationBuilder.RenameColumn(
                name: "SEstado",
                table: "TBL_OrdenServicioLlanta",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "SCriterioElegibilidad",
                table: "TBL_OrdenServicioLlanta",
                newName: "CriterioElegibilidad");

            migrationBuilder.RenameColumn(
                name: "SAprobador",
                table: "TBL_OrdenServicioLlanta",
                newName: "Aprobador");

            migrationBuilder.RenameColumn(
                name: "NTipo",
                table: "TBL_OrdenServicioLlanta",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "NCosto",
                table: "TBL_OrdenServicioLlanta",
                newName: "Costo");

            migrationBuilder.RenameColumn(
                name: "GVehiculoOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "VehiculoOrigenId");

            migrationBuilder.RenameColumn(
                name: "GProveedorId",
                table: "TBL_OrdenServicioLlanta",
                newName: "ProveedorId");

            migrationBuilder.RenameColumn(
                name: "GPosicionOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "PosicionOrigenId");

            migrationBuilder.RenameColumn(
                name: "GOrigenEntidadId",
                table: "TBL_OrdenServicioLlanta",
                newName: "OrigenEntidadId");

            migrationBuilder.RenameColumn(
                name: "GLoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                newName: "LoteEnvioReparacionId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_OrdenServicioLlanta",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "GCentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "CentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "DFechaRecepcion",
                table: "TBL_OrdenServicioLlanta",
                newName: "FechaRecepcion");

            migrationBuilder.RenameColumn(
                name: "DFechaOpcionada",
                table: "TBL_OrdenServicioLlanta",
                newName: "FechaOpcionada");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaEnvio",
                table: "TBL_OrdenServicioLlanta",
                newName: "FechaEnvio");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DFechaAprobacion",
                table: "TBL_OrdenServicioLlanta",
                newName: "FechaAprobacion");

            migrationBuilder.RenameColumn(
                name: "BElegible",
                table: "TBL_OrdenServicioLlanta",
                newName: "Elegible");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_OrdenServicioLlanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_OrdenServicioLlanta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenServicioLlanta_SOrigenTipo_GOrigenEntidadId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_TBL_OrdenServicioLlanta_OrigenTipo_OrigenEntidadId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenServicioLlanta_NTipo_SEstado_GCentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_TBL_OrdenServicioLlanta_Tipo_Estado_CentroOrigenId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenServicioLlanta_GProveedorId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_TBL_OrdenServicioLlanta_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenServicioLlanta_GLoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_TBL_OrdenServicioLlanta_LoteEnvioReparacionId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenServicioLlanta_GLlantaId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_TBL_OrdenServicioLlanta_LlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_OrdenServicioLlanta_GCentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                newName: "IX_TBL_OrdenServicioLlanta_CentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_MovimientoLlanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_MovimientoLlanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_MovimientoLlanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioAutoriza",
                table: "TBL_MovimientoLlanta",
                newName: "UsuarioAutoriza");

            migrationBuilder.RenameColumn(
                name: "STecnicoReporta",
                table: "TBL_MovimientoLlanta",
                newName: "TecnicoReporta");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_MovimientoLlanta",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SMotivo",
                table: "TBL_MovimientoLlanta",
                newName: "Motivo");

            migrationBuilder.RenameColumn(
                name: "GPosicionVehiculoId",
                table: "TBL_MovimientoLlanta",
                newName: "PosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GLlantaNuevaId",
                table: "TBL_MovimientoLlanta",
                newName: "LlantaNuevaId");

            migrationBuilder.RenameColumn(
                name: "GLlantaAnteriorId",
                table: "TBL_MovimientoLlanta",
                newName: "LlantaAnteriorId");

            migrationBuilder.RenameColumn(
                name: "GInspeccionId",
                table: "TBL_MovimientoLlanta",
                newName: "InspeccionId");

            migrationBuilder.RenameColumn(
                name: "GInconsistenciaInspeccionId",
                table: "TBL_MovimientoLlanta",
                newName: "InconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_MovimientoLlanta",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaReporte",
                table: "TBL_MovimientoLlanta",
                newName: "FechaReporte");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_MovimientoLlanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_MovimientoLlanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DFechaAutorizacion",
                table: "TBL_MovimientoLlanta",
                newName: "FechaAutorizacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_MovimientoLlanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_MovimientoLlanta",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_MovimientoDetalle",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_MovimientoDetalle",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_MovimientoDetalle",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SDestinoDescripcion",
                table: "TBL_MovimientoDetalle",
                newName: "DestinoDescripcion");

            migrationBuilder.RenameColumn(
                name: "NTipoDestino",
                table: "TBL_MovimientoDetalle",
                newName: "TipoDestino");

            migrationBuilder.RenameColumn(
                name: "GPosicionOrigenId",
                table: "TBL_MovimientoDetalle",
                newName: "PosicionOrigenId");

            migrationBuilder.RenameColumn(
                name: "GPosicionDestinoId",
                table: "TBL_MovimientoDetalle",
                newName: "PosicionDestinoId");

            migrationBuilder.RenameColumn(
                name: "GMovimientoId",
                table: "TBL_MovimientoDetalle",
                newName: "MovimientoId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_MovimientoDetalle",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "GCentroDestinoId",
                table: "TBL_MovimientoDetalle",
                newName: "CentroDestinoId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_MovimientoDetalle",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_MovimientoDetalle",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_MovimientoDetalle",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_MovimientoDetalle",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoDetalle_GMovimientoId",
                table: "TBL_MovimientoDetalle",
                newName: "IX_TBL_MovimientoDetalle_MovimientoId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoDetalle_GLlantaId",
                table: "TBL_MovimientoDetalle",
                newName: "IX_TBL_MovimientoDetalle_LlantaId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Movimiento",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Movimiento",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Movimiento",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUsuario",
                table: "TBL_Movimiento",
                newName: "Usuario");

            migrationBuilder.RenameColumn(
                name: "STipo",
                table: "TBL_Movimiento",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_Movimiento",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SNumero",
                table: "TBL_Movimiento",
                newName: "Numero");

            migrationBuilder.RenameColumn(
                name: "SMotivo",
                table: "TBL_Movimiento",
                newName: "Motivo");

            migrationBuilder.RenameColumn(
                name: "GInspeccionId",
                table: "TBL_Movimiento",
                newName: "InspeccionId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_Movimiento",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Movimiento",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Movimiento",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Movimiento",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Movimiento",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Movimiento_SNumero",
                table: "TBL_Movimiento",
                newName: "IX_TBL_Movimiento_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_Movimiento_GCentroId",
                table: "TBL_Movimiento",
                newName: "IX_TBL_Movimiento_CentroId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Marca",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Marca",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Marca",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Marca",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Marca",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Marca",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Marca",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Marca",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Marca",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Marca_SCodigo",
                table: "TBL_Marca",
                newName: "IX_Marca_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_LoteEnvioReparacion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STransportador",
                table: "TBL_LoteEnvioReparacion",
                newName: "Transportador");

            migrationBuilder.RenameColumn(
                name: "SSolicitante",
                table: "TBL_LoteEnvioReparacion",
                newName: "Solicitante");

            migrationBuilder.RenameColumn(
                name: "SRemision",
                table: "TBL_LoteEnvioReparacion",
                newName: "Remision");

            migrationBuilder.RenameColumn(
                name: "SReceptor",
                table: "TBL_LoteEnvioReparacion",
                newName: "Receptor");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_LoteEnvioReparacion",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SIdempotencyKey",
                table: "TBL_LoteEnvioReparacion",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "SEstado",
                table: "TBL_LoteEnvioReparacion",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_LoteEnvioReparacion",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "GProveedorId",
                table: "TBL_LoteEnvioReparacion",
                newName: "ProveedorId");

            migrationBuilder.RenameColumn(
                name: "GCentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                newName: "CentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "DFechaSalida",
                table: "TBL_LoteEnvioReparacion",
                newName: "FechaSalida");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_LoteEnvioReparacion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCierre",
                table: "TBL_LoteEnvioReparacion",
                newName: "FechaCierre");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_LoteEnvioReparacion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_LoteEnvioReparacion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_LoteEnvioReparacion_SIdempotencyKey",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_TBL_LoteEnvioReparacion_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_LoteEnvioReparacion_SCodigo",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_TBL_LoteEnvioReparacion_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_LoteEnvioReparacion_GProveedorId",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_TBL_LoteEnvioReparacion_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_LoteEnvioReparacion_GCentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                newName: "IX_TBL_LoteEnvioReparacion_CentroOrigenId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_LlantaTemporal",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_LlantaTemporal",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_LlantaTemporal",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SIdentificadorTemporal",
                table: "TBL_LlantaTemporal",
                newName: "IdentificadorTemporal");

            migrationBuilder.RenameColumn(
                name: "SIdentificadorFisico",
                table: "TBL_LlantaTemporal",
                newName: "IdentificadorFisico");

            migrationBuilder.RenameColumn(
                name: "NEstado",
                table: "TBL_LlantaTemporal",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "GInconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                newName: "InconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_LlantaTemporal",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_LlantaTemporal",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_LlantaTemporal",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_LlantaTemporal",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_LlantaTemporal_GInconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                newName: "IX_TBL_LlantaTemporal_InconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Llanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Llanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Llanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUbicacionActual",
                table: "TBL_Llanta",
                newName: "UbicacionActual");

            migrationBuilder.RenameColumn(
                name: "SSerial",
                table: "TBL_Llanta",
                newName: "Serial");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_Llanta",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Llanta",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "NProfundidadInicial",
                table: "TBL_Llanta",
                newName: "ProfundidadInicial");

            migrationBuilder.RenameColumn(
                name: "NNumeroReencauches",
                table: "TBL_Llanta",
                newName: "NumeroReencauches");

            migrationBuilder.RenameColumn(
                name: "NKilometrajeAcumulado",
                table: "TBL_Llanta",
                newName: "KilometrajeAcumulado");

            migrationBuilder.RenameColumn(
                name: "NCosto",
                table: "TBL_Llanta",
                newName: "Costo");

            migrationBuilder.RenameColumn(
                name: "GTipoLlantaId",
                table: "TBL_Llanta",
                newName: "TipoLlantaId");

            migrationBuilder.RenameColumn(
                name: "GReferenciaId",
                table: "TBL_Llanta",
                newName: "ReferenciaId");

            migrationBuilder.RenameColumn(
                name: "GMarcaId",
                table: "TBL_Llanta",
                newName: "MarcaId");

            migrationBuilder.RenameColumn(
                name: "GEstadoLlantaId",
                table: "TBL_Llanta",
                newName: "EstadoLlantaId");

            migrationBuilder.RenameColumn(
                name: "GDimensionId",
                table: "TBL_Llanta",
                newName: "DimensionId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_Llanta",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Llanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaIngreso",
                table: "TBL_Llanta",
                newName: "FechaIngreso");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Llanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCompra",
                table: "TBL_Llanta",
                newName: "FechaCompra");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Llanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Llanta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_SSerial",
                table: "TBL_Llanta",
                newName: "IX_Llanta_Serial");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_SCodigo",
                table: "TBL_Llanta",
                newName: "IX_Llanta_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_GTipoLlantaId",
                table: "TBL_Llanta",
                newName: "IX_TBL_Llanta_TipoLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_GReferenciaId",
                table: "TBL_Llanta",
                newName: "IX_TBL_Llanta_ReferenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_GMarcaId",
                table: "TBL_Llanta",
                newName: "IX_TBL_Llanta_MarcaId");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_GEstadoLlantaId",
                table: "TBL_Llanta",
                newName: "IX_TBL_Llanta_EstadoLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_GDimensionId",
                table: "TBL_Llanta",
                newName: "IX_TBL_Llanta_DimensionId");

            migrationBuilder.RenameIndex(
                name: "IX_Llanta_GCentroId_GEstadoLlantaId",
                table: "TBL_Llanta",
                newName: "IX_Llanta_CentroEstado");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_InspeccionDetalle",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_InspeccionDetalle",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_InspeccionDetalle",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_InspeccionDetalle",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "NProfundidadInterior",
                table: "TBL_InspeccionDetalle",
                newName: "ProfundidadInterior");

            migrationBuilder.RenameColumn(
                name: "NProfundidadExterior",
                table: "TBL_InspeccionDetalle",
                newName: "ProfundidadExterior");

            migrationBuilder.RenameColumn(
                name: "NProfundidadCentro",
                table: "TBL_InspeccionDetalle",
                newName: "ProfundidadCentro");

            migrationBuilder.RenameColumn(
                name: "GRecomendacionId",
                table: "TBL_InspeccionDetalle",
                newName: "RecomendacionId");

            migrationBuilder.RenameColumn(
                name: "GPosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                newName: "PosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "GInspeccionId",
                table: "TBL_InspeccionDetalle",
                newName: "InspeccionId");

            migrationBuilder.RenameColumn(
                name: "GCondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "CondicionLlantaId");

            migrationBuilder.RenameColumn(
                name: "GCausaLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "CausaLlantaId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_InspeccionDetalle",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_InspeccionDetalle",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_InspeccionDetalle",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_InspeccionDetalle",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_InspeccionDetalle_GRecomendacionId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_TBL_InspeccionDetalle_RecomendacionId");

            migrationBuilder.RenameIndex(
                name: "IX_InspeccionDetalle_GPosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_TBL_InspeccionDetalle_PosicionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_InspeccionDetalle_GLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_TBL_InspeccionDetalle_LlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_InspeccionDetalle_GInspeccionId_GPosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_TBL_InspeccionDetalle_InspeccionId_PosicionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_InspeccionDetalle_GCondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_TBL_InspeccionDetalle_CondicionLlantaId");

            migrationBuilder.RenameIndex(
                name: "IX_InspeccionDetalle_GCausaLlantaId",
                table: "TBL_InspeccionDetalle",
                newName: "IX_TBL_InspeccionDetalle_CausaLlantaId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Inspeccion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Inspeccion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Inspeccion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STecnicoId",
                table: "TBL_Inspeccion",
                newName: "TecnicoId");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_Inspeccion",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "NKilometraje",
                table: "TBL_Inspeccion",
                newName: "Kilometraje");

            migrationBuilder.RenameColumn(
                name: "NEstado",
                table: "TBL_Inspeccion",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "GVehiculoId",
                table: "TBL_Inspeccion",
                newName: "VehiculoId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_Inspeccion",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Inspeccion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Inspeccion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Inspeccion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Inspeccion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Inspeccion_GVehiculoId",
                table: "TBL_Inspeccion",
                newName: "IX_TBL_Inspeccion_VehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_Inspeccion_GCentroId",
                table: "TBL_Inspeccion",
                newName: "IX_TBL_Inspeccion_CentroId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioAutorizador",
                table: "TBL_InconsistenciaInspeccion",
                newName: "UsuarioAutorizador");

            migrationBuilder.RenameColumn(
                name: "STecnicoId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "TecnicoId");

            migrationBuilder.RenameColumn(
                name: "SObservacionAutorizacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "ObservacionAutorizacion");

            migrationBuilder.RenameColumn(
                name: "SObservacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "Observacion");

            migrationBuilder.RenameColumn(
                name: "SIdentificadorEncontrado",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IdentificadorEncontrado");

            migrationBuilder.RenameColumn(
                name: "NEstado",
                table: "TBL_InconsistenciaInspeccion",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "GPosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "PosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GLlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "LlantaEsperadaId");

            migrationBuilder.RenameColumn(
                name: "GLlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "LlantaEncontradaId");

            migrationBuilder.RenameColumn(
                name: "GInspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "InspeccionId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "DFechaAutorizacion",
                table: "TBL_InconsistenciaInspeccion",
                newName: "FechaAutorizacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_InconsistenciaInspeccion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_InconsistenciaInspeccion_GPosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_TBL_InconsistenciaInspeccion_PosicionVehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_InconsistenciaInspeccion_GLlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_TBL_InconsistenciaInspeccion_LlantaEsperadaId");

            migrationBuilder.RenameIndex(
                name: "IX_InconsistenciaInspeccion_GLlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_TBL_InconsistenciaInspeccion_LlantaEncontradaId");

            migrationBuilder.RenameIndex(
                name: "IX_InconsistenciaInspeccion_GInspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                newName: "IX_TBL_InconsistenciaInspeccion_InspeccionId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_EvidenciaInspeccion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUbicacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "Ubicacion");

            migrationBuilder.RenameColumn(
                name: "SNombreArchivo",
                table: "TBL_EvidenciaInspeccion",
                newName: "NombreArchivo");

            migrationBuilder.RenameColumn(
                name: "SMimeType",
                table: "TBL_EvidenciaInspeccion",
                newName: "MimeType");

            migrationBuilder.RenameColumn(
                name: "SHash",
                table: "TBL_EvidenciaInspeccion",
                newName: "Hash");

            migrationBuilder.RenameColumn(
                name: "NTamanoBytes",
                table: "TBL_EvidenciaInspeccion",
                newName: "TamanoBytes");

            migrationBuilder.RenameColumn(
                name: "GInspeccionId",
                table: "TBL_EvidenciaInspeccion",
                newName: "InspeccionId");

            migrationBuilder.RenameColumn(
                name: "GInconsistenciaInspeccionId",
                table: "TBL_EvidenciaInspeccion",
                newName: "InconsistenciaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "DRetenerHasta",
                table: "TBL_EvidenciaInspeccion",
                newName: "RetenerHasta");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_EvidenciaInspeccion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_EvidenciaInspeccion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_EvidenciaInspeccion",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_EvidenciaFlujo",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_EvidenciaFlujo",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_EvidenciaFlujo",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUbicacion",
                table: "TBL_EvidenciaFlujo",
                newName: "Ubicacion");

            migrationBuilder.RenameColumn(
                name: "SNombreArchivo",
                table: "TBL_EvidenciaFlujo",
                newName: "NombreArchivo");

            migrationBuilder.RenameColumn(
                name: "SMimeType",
                table: "TBL_EvidenciaFlujo",
                newName: "MimeType");

            migrationBuilder.RenameColumn(
                name: "SHash",
                table: "TBL_EvidenciaFlujo",
                newName: "Hash");

            migrationBuilder.RenameColumn(
                name: "NTamanoBytes",
                table: "TBL_EvidenciaFlujo",
                newName: "TamanoBytes");

            migrationBuilder.RenameColumn(
                name: "GOrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                newName: "OrdenServicioLlantaId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_EvidenciaFlujo",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_EvidenciaFlujo",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_EvidenciaFlujo",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_EvidenciaFlujo",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_EvidenciaFlujo_GOrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                newName: "IX_TBL_EvidenciaFlujo_OrdenServicioLlantaId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_EstadoLlanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_EstadoLlanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_EstadoLlanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_EstadoLlanta",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_EstadoLlanta",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_EstadoLlanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_EstadoLlanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BPermiteMontaje",
                table: "TBL_EstadoLlanta",
                newName: "PermiteMontaje");

            migrationBuilder.RenameColumn(
                name: "BEsDisposicionFinal",
                table: "TBL_EstadoLlanta",
                newName: "EsDisposicionFinal");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_EstadoLlanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_EstadoLlanta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_EstadoLlanta_SCodigo",
                table: "TBL_EstadoLlanta",
                newName: "IX_EstadoLlanta_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_EjeVehiculo",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_EjeVehiculo",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_EjeVehiculo",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipoEje",
                table: "TBL_EjeVehiculo",
                newName: "TipoEje");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_EjeVehiculo",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "NOrden",
                table: "TBL_EjeVehiculo",
                newName: "Orden");

            migrationBuilder.RenameColumn(
                name: "NNumero",
                table: "TBL_EjeVehiculo",
                newName: "Numero");

            migrationBuilder.RenameColumn(
                name: "GVehiculoId",
                table: "TBL_EjeVehiculo",
                newName: "VehiculoId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_EjeVehiculo",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_EjeVehiculo",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_EjeVehiculo",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_EjeVehiculo",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_EjeVehiculo_GVehiculoId_NNumero",
                table: "TBL_EjeVehiculo",
                newName: "IX_TBL_EjeVehiculo_VehiculoId_Numero");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Dimension",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Dimension",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Dimension",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Dimension",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Dimension",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Dimension",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Dimension",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Dimension",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Dimension",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Dimension_SCodigo",
                table: "TBL_Dimension",
                newName: "IX_Dimension_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipoVehiculo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "TipoVehiculo");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_ConfiguracionVehiculo",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ConfiguracionVehiculo",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ConfiguracionVehiculo",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ConfiguracionVehiculo_SCodigo",
                table: "TBL_ConfiguracionVehiculo",
                newName: "IX_TBL_ConfiguracionVehiculo_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ConfiguracionPosicion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUbicacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "Ubicacion");

            migrationBuilder.RenameColumn(
                name: "SLado",
                table: "TBL_ConfiguracionPosicion",
                newName: "Lado");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_ConfiguracionPosicion",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "NOrden",
                table: "TBL_ConfiguracionPosicion",
                newName: "Orden");

            migrationBuilder.RenameColumn(
                name: "GConfiguracionEjeId",
                table: "TBL_ConfiguracionPosicion",
                newName: "ConfiguracionEjeId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ConfiguracionPosicion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ConfiguracionPosicion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ConfiguracionPosicion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ConfiguracionPosicion_GConfiguracionEjeId_SCodigo",
                table: "TBL_ConfiguracionPosicion",
                newName: "IX_TBL_ConfiguracionPosicion_ConfiguracionEjeId_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_ConfiguracionPosicion_GConfiguracionEjeId_NOrden",
                table: "TBL_ConfiguracionPosicion",
                newName: "IX_TBL_ConfiguracionPosicion_ConfiguracionEjeId_Orden");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ConfiguracionEje",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ConfiguracionEje",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ConfiguracionEje",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipoEje",
                table: "TBL_ConfiguracionEje",
                newName: "TipoEje");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_ConfiguracionEje",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "NOrden",
                table: "TBL_ConfiguracionEje",
                newName: "Orden");

            migrationBuilder.RenameColumn(
                name: "GConfiguracionVehiculoId",
                table: "TBL_ConfiguracionEje",
                newName: "ConfiguracionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ConfiguracionEje",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ConfiguracionEje",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ConfiguracionEje",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ConfiguracionEje",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ConfiguracionEje_GConfiguracionVehiculoId_NOrden",
                table: "TBL_ConfiguracionEje",
                newName: "IX_TBL_ConfiguracionEje_ConfiguracionVehiculoId_Orden");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_CondicionLlanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_CondicionLlanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_CondicionLlanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_CondicionLlanta",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_CondicionLlanta",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_CondicionLlanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_CondicionLlanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BRequiereCausa",
                table: "TBL_CondicionLlanta",
                newName: "RequiereCausa");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_CondicionLlanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_CondicionLlanta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_CondicionLlanta_SCodigo",
                table: "TBL_CondicionLlanta",
                newName: "IX_TBL_CondicionLlanta_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_Centro",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_Centro",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_Centro",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SRelevancia",
                table: "TBL_Centro",
                newName: "Relevancia");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_Centro",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_Centro",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "GRegionalId",
                table: "TBL_Centro",
                newName: "RegionalId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_Centro",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_Centro",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_Centro",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_Centro",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Centro_SCodigo",
                table: "TBL_Centro",
                newName: "IX_Centro_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Centro_GRegionalId",
                table: "TBL_Centro",
                newName: "IX_TBL_Centro_RegionalId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_CausaLlanta",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_CausaLlanta",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_CausaLlanta",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SNombre",
                table: "TBL_CausaLlanta",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "SCodigo",
                table: "TBL_CausaLlanta",
                newName: "Codigo");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_CausaLlanta",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_CausaLlanta",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_CausaLlanta",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_CausaLlanta",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_CausaLlanta_SCodigo",
                table: "TBL_CausaLlanta",
                newName: "IX_TBL_CausaLlanta_Codigo");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_CargaMasiva",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_CargaMasiva",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_CargaMasiva",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SUsuario",
                table: "TBL_CargaMasiva",
                newName: "Usuario");

            migrationBuilder.RenameColumn(
                name: "STipo",
                table: "TBL_CargaMasiva",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SNombreArchivo",
                table: "TBL_CargaMasiva",
                newName: "NombreArchivo");

            migrationBuilder.RenameColumn(
                name: "SFilasJson",
                table: "TBL_CargaMasiva",
                newName: "FilasJson");

            migrationBuilder.RenameColumn(
                name: "SEstado",
                table: "TBL_CargaMasiva",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "SErroresJson",
                table: "TBL_CargaMasiva",
                newName: "ErroresJson");

            migrationBuilder.RenameColumn(
                name: "NTotalFilas",
                table: "TBL_CargaMasiva",
                newName: "TotalFilas");

            migrationBuilder.RenameColumn(
                name: "NFilasValidas",
                table: "TBL_CargaMasiva",
                newName: "FilasValidas");

            migrationBuilder.RenameColumn(
                name: "NFilasConError",
                table: "TBL_CargaMasiva",
                newName: "FilasConError");

            migrationBuilder.RenameColumn(
                name: "DFechaProcesamiento",
                table: "TBL_CargaMasiva",
                newName: "FechaProcesamiento");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_CargaMasiva",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_CargaMasiva",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_CargaMasiva",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_CargaMasiva",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_CargaMasiva_SUsuario_DFechaCreacion",
                table: "TBL_CargaMasiva",
                newName: "IX_TBL_CargaMasiva_Usuario_FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "SValoresNuevos",
                table: "TBL_Auditoria",
                newName: "ValoresNuevos");

            migrationBuilder.RenameColumn(
                name: "SValoresAnteriores",
                table: "TBL_Auditoria",
                newName: "ValoresAnteriores");

            migrationBuilder.RenameColumn(
                name: "SUsuario",
                table: "TBL_Auditoria",
                newName: "Usuario");

            migrationBuilder.RenameColumn(
                name: "SOrigen",
                table: "TBL_Auditoria",
                newName: "Origen");

            migrationBuilder.RenameColumn(
                name: "SIdentificador",
                table: "TBL_Auditoria",
                newName: "Identificador");

            migrationBuilder.RenameColumn(
                name: "SEntidad",
                table: "TBL_Auditoria",
                newName: "Entidad");

            migrationBuilder.RenameColumn(
                name: "SDireccionIp",
                table: "TBL_Auditoria",
                newName: "DireccionIp");

            migrationBuilder.RenameColumn(
                name: "SAccion",
                table: "TBL_Auditoria",
                newName: "Accion");

            migrationBuilder.RenameColumn(
                name: "DFecha",
                table: "TBL_Auditoria",
                newName: "Fecha");

            migrationBuilder.RenameColumn(
                name: "NId",
                table: "TBL_Auditoria",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Auditoria_SEntidad_SIdentificador_DFecha",
                table: "TBL_Auditoria",
                newName: "IX_Auditoria_EntidadFecha");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "NKilometrajeRecorrido",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "KilometrajeRecorrido");

            migrationBuilder.RenameColumn(
                name: "NKilometrajeMontaje",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "KilometrajeMontaje");

            migrationBuilder.RenameColumn(
                name: "NKilometrajeDesmontaje",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "KilometrajeDesmontaje");

            migrationBuilder.RenameColumn(
                name: "GPosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "PosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GMovimientoOrigenId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "MovimientoOrigenId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaInicio",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "FechaInicio");

            migrationBuilder.RenameColumn(
                name: "DFechaFin",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "FechaFin");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BEsActiva",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "EsActiva");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_AsignacionLlantaPosicion_GPosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion",
                newName: "UX_Asignacion_PosicionActiva");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_AlertaInspeccion",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_AlertaInspeccion",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_AlertaInspeccion",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipo",
                table: "TBL_AlertaInspeccion",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "SDescripcion",
                table: "TBL_AlertaInspeccion",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "NEstado",
                table: "TBL_AlertaInspeccion",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "GVehiculoId",
                table: "TBL_AlertaInspeccion",
                newName: "VehiculoId");

            migrationBuilder.RenameColumn(
                name: "GPosicionVehiculoId",
                table: "TBL_AlertaInspeccion",
                newName: "PosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_AlertaInspeccion",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "GInspeccionId",
                table: "TBL_AlertaInspeccion",
                newName: "InspeccionId");

            migrationBuilder.RenameColumn(
                name: "GInspeccionDetalleId",
                table: "TBL_AlertaInspeccion",
                newName: "InspeccionDetalleId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_AlertaInspeccion",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_AlertaInspeccion",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_AlertaInspeccion",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_AlertaInspeccion",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_AlertaInspeccion",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_AlertaInspeccion_GInspeccionId",
                table: "TBL_AlertaInspeccion",
                newName: "IX_TBL_AlertaInspeccion_InspeccionId");

            migrationBuilder.RenameIndex(
                name: "IX_AlertaInspeccion_GInspeccionDetalleId_STipo",
                table: "TBL_AlertaInspeccion",
                newName: "IX_TBL_AlertaInspeccion_InspeccionDetalleId_Tipo");

            migrationBuilder.RenameIndex(
                name: "IX_AlertaInspeccion_GCentroId_NEstado_DFechaCreacion",
                table: "TBL_AlertaInspeccion",
                newName: "IX_TBL_AlertaInspeccion_CentroId_Estado_FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_AlertaHistorial",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_AlertaHistorial",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_AlertaHistorial",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "SObservacion",
                table: "TBL_AlertaHistorial",
                newName: "Observacion");

            migrationBuilder.RenameColumn(
                name: "NEstadoNuevo",
                table: "TBL_AlertaHistorial",
                newName: "EstadoNuevo");

            migrationBuilder.RenameColumn(
                name: "NEstadoAnterior",
                table: "TBL_AlertaHistorial",
                newName: "EstadoAnterior");

            migrationBuilder.RenameColumn(
                name: "GAlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                newName: "AlertaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_AlertaHistorial",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_AlertaHistorial",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_AlertaHistorial",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_AlertaHistorial",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_AlertaHistorial_GAlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                newName: "IX_TBL_AlertaHistorial_AlertaInspeccionId");

            migrationBuilder.RenameColumn(
                name: "TRowVersion",
                table: "TBL_ActividadProgramada",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioModificacion",
                table: "TBL_ActividadProgramada",
                newName: "UsuarioModificacion");

            migrationBuilder.RenameColumn(
                name: "SUsuarioCreacion",
                table: "TBL_ActividadProgramada",
                newName: "UsuarioCreacion");

            migrationBuilder.RenameColumn(
                name: "STipoActividad",
                table: "TBL_ActividadProgramada",
                newName: "TipoActividad");

            migrationBuilder.RenameColumn(
                name: "STecnicoId",
                table: "TBL_ActividadProgramada",
                newName: "TecnicoId");

            migrationBuilder.RenameColumn(
                name: "SReasignadoPor",
                table: "TBL_ActividadProgramada",
                newName: "ReasignadoPor");

            migrationBuilder.RenameColumn(
                name: "SPrioridad",
                table: "TBL_ActividadProgramada",
                newName: "Prioridad");

            migrationBuilder.RenameColumn(
                name: "SOrigen",
                table: "TBL_ActividadProgramada",
                newName: "Origen");

            migrationBuilder.RenameColumn(
                name: "SObservaciones",
                table: "TBL_ActividadProgramada",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "SMotivoCancelacion",
                table: "TBL_ActividadProgramada",
                newName: "MotivoCancelacion");

            migrationBuilder.RenameColumn(
                name: "SIdempotencyKey",
                table: "TBL_ActividadProgramada",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "NEstado",
                table: "TBL_ActividadProgramada",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "GVehiculoId",
                table: "TBL_ActividadProgramada",
                newName: "VehiculoId");

            migrationBuilder.RenameColumn(
                name: "GTecnicoUsuarioId",
                table: "TBL_ActividadProgramada",
                newName: "TecnicoUsuarioId");

            migrationBuilder.RenameColumn(
                name: "GPosicionVehiculoId",
                table: "TBL_ActividadProgramada",
                newName: "PosicionVehiculoId");

            migrationBuilder.RenameColumn(
                name: "GOrigenEntidadId",
                table: "TBL_ActividadProgramada",
                newName: "OrigenEntidadId");

            migrationBuilder.RenameColumn(
                name: "GLlantaId",
                table: "TBL_ActividadProgramada",
                newName: "LlantaId");

            migrationBuilder.RenameColumn(
                name: "GGrupoProgramacionId",
                table: "TBL_ActividadProgramada",
                newName: "GrupoProgramacionId");

            migrationBuilder.RenameColumn(
                name: "GCentroId",
                table: "TBL_ActividadProgramada",
                newName: "CentroId");

            migrationBuilder.RenameColumn(
                name: "DFechaProgramada",
                table: "TBL_ActividadProgramada",
                newName: "FechaProgramada");

            migrationBuilder.RenameColumn(
                name: "DFechaModificacion",
                table: "TBL_ActividadProgramada",
                newName: "FechaModificacion");

            migrationBuilder.RenameColumn(
                name: "DFechaInicioReal",
                table: "TBL_ActividadProgramada",
                newName: "FechaInicioReal");

            migrationBuilder.RenameColumn(
                name: "DFechaFinReal",
                table: "TBL_ActividadProgramada",
                newName: "FechaFinReal");

            migrationBuilder.RenameColumn(
                name: "DFechaFinProgramada",
                table: "TBL_ActividadProgramada",
                newName: "FechaFinProgramada");

            migrationBuilder.RenameColumn(
                name: "DFechaCreacion",
                table: "TBL_ActividadProgramada",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "BActivo",
                table: "TBL_ActividadProgramada",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "GId",
                table: "TBL_ActividadProgramada",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_ActividadProgramada_STecnicoId_NEstado_DFechaProgramada",
                table: "TBL_ActividadProgramada",
                newName: "IX_TBL_ActividadProgramada_TecnicoId_Estado_FechaProgramada");

            migrationBuilder.RenameIndex(
                name: "IX_ActividadProgramada_SOrigen_GOrigenEntidadId",
                table: "TBL_ActividadProgramada",
                newName: "IX_TBL_ActividadProgramada_Origen_OrigenEntidadId");

            migrationBuilder.RenameIndex(
                name: "IX_ActividadProgramada_GVehiculoId",
                table: "TBL_ActividadProgramada",
                newName: "IX_TBL_ActividadProgramada_VehiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_ActividadProgramada_GTecnicoUsuarioId_DFechaProgramada_DFechaFinProgramada",
                table: "TBL_ActividadProgramada",
                newName: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_FechaProgramada_FechaFinProgramada");

            migrationBuilder.RenameIndex(
                name: "IX_ActividadProgramada_GCentroId",
                table: "TBL_ActividadProgramada",
                newName: "IX_TBL_ActividadProgramada_CentroId");

            migrationBuilder.CreateIndex(
                name: "UX_Asignacion_LlantaActiva",
                table: "TBL_AsignacionLlantaPosicion",
                column: "LlantaId",
                unique: true,
                filter: "[EsActiva] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_IdempotencyKey",
                table: "TBL_ActividadProgramada",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_VehiculoId_TipoActividad_FechaProgramada",
                table: "TBL_ActividadProgramada",
                columns: new[] { "TecnicoUsuarioId", "VehiculoId", "TipoActividad", "FechaProgramada" },
                unique: true,
                filter: "[Activo] = 1 AND [Estado] <> 4 AND [TecnicoUsuarioId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Centro_CentroId",
                table: "TBL_ActividadProgramada",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Usuario_TecnicoUsuarioId",
                table: "TBL_ActividadProgramada",
                column: "TecnicoUsuarioId",
                principalTable: "TBL_Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Vehiculo_VehiculoId",
                table: "TBL_ActividadProgramada",
                column: "VehiculoId",
                principalTable: "TBL_Vehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AlertaHistorial_TBL_AlertaInspeccion_AlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                column: "AlertaInspeccionId",
                principalTable: "TBL_AlertaInspeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_InspeccionDetalle_InspeccionDetalleId",
                table: "TBL_AlertaInspeccion",
                column: "InspeccionDetalleId",
                principalTable: "TBL_InspeccionDetalle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AlertaInspeccion_TBL_Inspeccion_InspeccionId",
                table: "TBL_AlertaInspeccion",
                column: "InspeccionId",
                principalTable: "TBL_Inspeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_Llanta_LlantaId",
                table: "TBL_AsignacionLlantaPosicion",
                column: "LlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_AsignacionLlantaPosicion_TBL_PosicionVehiculo_PosicionVehiculoId",
                table: "TBL_AsignacionLlantaPosicion",
                column: "PosicionVehiculoId",
                principalTable: "TBL_PosicionVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Centro_TBL_Regional_RegionalId",
                table: "TBL_Centro",
                column: "RegionalId",
                principalTable: "TBL_Regional",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ConfiguracionEje_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                table: "TBL_ConfiguracionEje",
                column: "ConfiguracionVehiculoId",
                principalTable: "TBL_ConfiguracionVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ConfiguracionPosicion_TBL_ConfiguracionEje_ConfiguracionEjeId",
                table: "TBL_ConfiguracionPosicion",
                column: "ConfiguracionEjeId",
                principalTable: "TBL_ConfiguracionEje",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_EjeVehiculo_TBL_Vehiculo_VehiculoId",
                table: "TBL_EjeVehiculo",
                column: "VehiculoId",
                principalTable: "TBL_Vehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_EvidenciaFlujo_TBL_OrdenServicioLlanta_OrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                column: "OrdenServicioLlantaId",
                principalTable: "TBL_OrdenServicioLlanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Inspeccion_InspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                column: "InspeccionId",
                principalTable: "TBL_Inspeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "LlantaEncontradaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "LlantaEsperadaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_PosicionVehiculo_PosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                column: "PosicionVehiculoId",
                principalTable: "TBL_PosicionVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Centro_CentroId",
                table: "TBL_Inspeccion",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Inspeccion_TBL_Vehiculo_VehiculoId",
                table: "TBL_Inspeccion",
                column: "VehiculoId",
                principalTable: "TBL_Vehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CausaLlanta_CausaLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "CausaLlantaId",
                principalTable: "TBL_CausaLlanta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_CondicionLlanta_CondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "CondicionLlantaId",
                principalTable: "TBL_CondicionLlanta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Inspeccion_InspeccionId",
                table: "TBL_InspeccionDetalle",
                column: "InspeccionId",
                principalTable: "TBL_Inspeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_Llanta_LlantaId",
                table: "TBL_InspeccionDetalle",
                column: "LlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_PosicionVehiculo_PosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                column: "PosicionVehiculoId",
                principalTable: "TBL_PosicionVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InspeccionDetalle_TBL_RecomendacionInspeccion_RecomendacionId",
                table: "TBL_InspeccionDetalle",
                column: "RecomendacionId",
                principalTable: "TBL_RecomendacionInspeccion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Centro_CentroId",
                table: "TBL_Llanta",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Dimension_DimensionId",
                table: "TBL_Llanta",
                column: "DimensionId",
                principalTable: "TBL_Dimension",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_EstadoLlanta_EstadoLlantaId",
                table: "TBL_Llanta",
                column: "EstadoLlantaId",
                principalTable: "TBL_EstadoLlanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Marca_MarcaId",
                table: "TBL_Llanta",
                column: "MarcaId",
                principalTable: "TBL_Marca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_Referencia_ReferenciaId",
                table: "TBL_Llanta",
                column: "ReferenciaId",
                principalTable: "TBL_Referencia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Llanta_TBL_TipoLlanta_TipoLlantaId",
                table: "TBL_Llanta",
                column: "TipoLlantaId",
                principalTable: "TBL_TipoLlanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_LlantaTemporal_TBL_InconsistenciaInspeccion_InconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                column: "InconsistenciaInspeccionId",
                principalTable: "TBL_InconsistenciaInspeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_Centro_CentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                column: "CentroOrigenId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_LoteEnvioReparacion_TBL_ProveedorServicio_ProveedorId",
                table: "TBL_LoteEnvioReparacion",
                column: "ProveedorId",
                principalTable: "TBL_ProveedorServicio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Movimiento_TBL_Centro_CentroId",
                table: "TBL_Movimiento",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Llanta_LlantaId",
                table: "TBL_MovimientoDetalle",
                column: "LlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_MovimientoDetalle_TBL_Movimiento_MovimientoId",
                table: "TBL_MovimientoDetalle",
                column: "MovimientoId",
                principalTable: "TBL_Movimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Centro_CentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                column: "CentroOrigenId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_Llanta_LlantaId",
                table: "TBL_OrdenServicioLlanta",
                column: "LlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                column: "LoteEnvioReparacionId",
                principalTable: "TBL_LoteEnvioReparacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_ProveedorServicio_ProveedorId",
                table: "TBL_OrdenServicioLlanta",
                column: "ProveedorId",
                principalTable: "TBL_ProveedorServicio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_EjeVehiculo_EjeVehiculoId",
                table: "TBL_PosicionVehiculo",
                column: "EjeVehiculoId",
                principalTable: "TBL_EjeVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_PosicionVehiculo_TBL_Llanta_LlantaActualId",
                table: "TBL_PosicionVehiculo",
                column: "LlantaActualId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Referencia_TBL_Marca_MarcaId",
                table: "TBL_Referencia",
                column: "MarcaId",
                principalTable: "TBL_Marca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Permiso_PermisoId",
                table: "TBL_RolPermiso",
                column: "PermisoId",
                principalTable: "TBL_Permiso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_RolPermiso_TBL_Rol_RolId",
                table: "TBL_RolPermiso",
                column: "RolId",
                principalTable: "TBL_Rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Centro_CentroId",
                table: "TBL_SolicitudOperacion",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_SolicitudOperacion_TBL_Llanta_LlantaId",
                table: "TBL_SolicitudOperacion",
                column: "LlantaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Usuario_TBL_Centro_CentroId",
                table: "TBL_Usuario",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Usuario_TBL_Rol_RolId",
                table: "TBL_Usuario",
                column: "RolId",
                principalTable: "TBL_Rol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Centro_CentroId",
                table: "TBL_UsuarioCentro",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_UsuarioCentro_TBL_Usuario_UsuarioId",
                table: "TBL_UsuarioCentro",
                column: "UsuarioId",
                principalTable: "TBL_Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Vehiculo_TBL_Centro_CentroId",
                table: "TBL_Vehiculo",
                column: "CentroId",
                principalTable: "TBL_Centro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                column: "ConfiguracionVehiculoId",
                principalTable: "TBL_ConfiguracionVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
