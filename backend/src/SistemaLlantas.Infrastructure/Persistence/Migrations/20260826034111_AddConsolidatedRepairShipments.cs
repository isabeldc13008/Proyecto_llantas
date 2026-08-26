using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsolidatedRepairShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaAprobacion",
                table: "TBL_OrdenServicioLlanta",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaOpcionada",
                table: "TBL_OrdenServicioLlanta",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "TBL_OrdenServicioLlanta",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrigenEntidadId",
                table: "TBL_OrdenServicioLlanta",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrigenTipo",
                table: "TBL_OrdenServicioLlanta",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "MANUAL");

            migrationBuilder.AddColumn<Guid>(
                name: "PosicionOrigenId",
                table: "TBL_OrdenServicioLlanta",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resultado",
                table: "TBL_OrdenServicioLlanta",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioOpciona",
                table: "TBL_OrdenServicioLlanta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VehiculoOrigenId",
                table: "TBL_OrdenServicioLlanta",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("UPDATE TBL_OrdenServicioLlanta SET FechaOpcionada = FechaCreacion, OrigenTipo = 'MANUAL', UsuarioOpciona = Solicitante");

            migrationBuilder.CreateTable(
                name: "TBL_LoteEnvioReparacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CentroOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaSalida = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Remision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Transportador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Solicitante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Receptor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaCierre = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_LoteEnvioReparacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_LoteEnvioReparacion_TBL_Centro_CentroOrigenId",
                        column: x => x.CentroOrigenId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_LoteEnvioReparacion_TBL_ProveedorServicio_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "TBL_ProveedorServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                column: "LoteEnvioReparacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_OrigenTipo_OrigenEntidadId",
                table: "TBL_OrdenServicioLlanta",
                columns: new[] { "OrigenTipo", "OrigenEntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteEnvioReparacion_CentroOrigenId",
                table: "TBL_LoteEnvioReparacion",
                column: "CentroOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteEnvioReparacion_Codigo",
                table: "TBL_LoteEnvioReparacion",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteEnvioReparacion_IdempotencyKey",
                table: "TBL_LoteEnvioReparacion",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteEnvioReparacion_ProveedorId",
                table: "TBL_LoteEnvioReparacion",
                column: "ProveedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta",
                column: "LoteEnvioReparacionId",
                principalTable: "TBL_LoteEnvioReparacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteEnvioReparacion_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropTable(
                name: "TBL_LoteEnvioReparacion");

            migrationBuilder.DropIndex(
                name: "IX_TBL_OrdenServicioLlanta_LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropIndex(
                name: "IX_TBL_OrdenServicioLlanta_OrigenTipo_OrigenEntidadId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "FechaOpcionada",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "LoteEnvioReparacionId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "OrigenEntidadId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "OrigenTipo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "PosicionOrigenId",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "Resultado",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "UsuarioOpciona",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "VehiculoOrigenId",
                table: "TBL_OrdenServicioLlanta");
        }
    }
}
