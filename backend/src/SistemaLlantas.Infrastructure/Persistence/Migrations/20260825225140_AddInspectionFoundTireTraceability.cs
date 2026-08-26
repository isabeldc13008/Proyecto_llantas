using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionFoundTireTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InconsistenciaInspeccion_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "LlantaEncontradaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "LlantaEncontradaId",
                principalTable: "TBL_Llanta",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropIndex(
                name: "IX_TBL_InconsistenciaInspeccion_LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "LlantaEncontradaId",
                table: "TBL_InconsistenciaInspeccion");
        }
    }
}
