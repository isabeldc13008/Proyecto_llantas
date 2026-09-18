using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledMountSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActividadProgramada_GTecnicoUsuarioId_GVehiculoId_STipoActividad_DFechaProgramada",
                schema: "dbo",
                table: "TBL_ActividadProgramada");

            migrationBuilder.AddColumn<Guid>(
                name: "GGrupoOperacionId",
                schema: "dbo",
                table: "TBL_SolicitudOperacion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_SolicitudOperacion_GGrupoOperacionId",
                schema: "dbo",
                table: "TBL_SolicitudOperacion",
                column: "GGrupoOperacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadProgramada_TrabajoPosicion",
                schema: "dbo",
                table: "TBL_ActividadProgramada",
                columns: new[] { "GTecnicoUsuarioId", "GVehiculoId", "STipoActividad", "DFechaProgramada", "GPosicionVehiculoId" },
                unique: true,
                filter: "([BActivo]=(1) AND [NEstado]<>(4) AND [GTecnicoUsuarioId] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TBL_SolicitudOperacion_GGrupoOperacionId",
                schema: "dbo",
                table: "TBL_SolicitudOperacion");

            migrationBuilder.DropIndex(
                name: "IX_ActividadProgramada_TrabajoPosicion",
                schema: "dbo",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "GGrupoOperacionId",
                schema: "dbo",
                table: "TBL_SolicitudOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadProgramada_GTecnicoUsuarioId_GVehiculoId_STipoActividad_DFechaProgramada",
                schema: "dbo",
                table: "TBL_ActividadProgramada",
                columns: new[] { "GTecnicoUsuarioId", "GVehiculoId", "STipoActividad", "DFechaProgramada" },
                unique: true,
                filter: "([BActivo]=(1) AND [NEstado]<>(4) AND [GTecnicoUsuarioId] IS NOT NULL)");
        }
    }
}
