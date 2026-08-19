using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingPhase5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaFinProgramada",
                table: "TBL_ActividadProgramada",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GrupoProgramacionId",
                table: "TBL_ActividadProgramada",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelacion",
                table: "TBL_ActividadProgramada",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasignadoPor",
                table: "TBL_ActividadProgramada",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TecnicoUsuarioId",
                table: "TBL_ActividadProgramada",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE actividad
                SET TecnicoUsuarioId = usuario.Id
                FROM TBL_ActividadProgramada actividad
                INNER JOIN TBL_Usuario usuario
                    ON LOWER(actividad.TecnicoId) = LOWER(usuario.Username)
                    OR LOWER(actividad.TecnicoId) = LOWER(usuario.Username + '.local')
                WHERE actividad.TecnicoUsuarioId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_FechaProgramada_FechaFinProgramada",
                table: "TBL_ActividadProgramada",
                columns: new[] { "TecnicoUsuarioId", "FechaProgramada", "FechaFinProgramada" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_VehiculoId_TipoActividad_FechaProgramada",
                table: "TBL_ActividadProgramada",
                columns: new[] { "TecnicoUsuarioId", "VehiculoId", "TipoActividad", "FechaProgramada" },
                unique: true,
                filter: "[Activo] = 1 AND [Estado] <> 4 AND [TecnicoUsuarioId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Usuario_TecnicoUsuarioId",
                table: "TBL_ActividadProgramada",
                column: "TecnicoUsuarioId",
                principalTable: "TBL_Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ActividadProgramada_TBL_Usuario_TecnicoUsuarioId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_FechaProgramada_FechaFinProgramada",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoUsuarioId_VehiculoId_TipoActividad_FechaProgramada",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "FechaFinProgramada",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "GrupoProgramacionId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "MotivoCancelacion",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "ReasignadoPor",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "TecnicoUsuarioId",
                table: "TBL_ActividadProgramada");
        }
    }
}
