using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingOriginAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "TBL_ActividadProgramada",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origen",
                table: "TBL_ActividadProgramada",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "MANUAL");

            migrationBuilder.AddColumn<Guid>(
                name: "OrigenEntidadId",
                table: "TBL_ActividadProgramada",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_IdempotencyKey",
                table: "TBL_ActividadProgramada",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_Origen_OrigenEntidadId",
                table: "TBL_ActividadProgramada",
                columns: new[] { "Origen", "OrigenEntidadId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TBL_ActividadProgramada_IdempotencyKey",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropIndex(
                name: "IX_TBL_ActividadProgramada_Origen_OrigenEntidadId",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "TBL_ActividadProgramada");

            migrationBuilder.DropColumn(
                name: "OrigenEntidadId",
                table: "TBL_ActividadProgramada");
        }
    }
}
