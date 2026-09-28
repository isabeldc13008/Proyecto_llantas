using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisposalShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DFechaDisposicion",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DFechaEvaluacion",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GLoteDisposicionFinalId",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SEvaluadoPor",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TBL_LoteDisposicionFinal",
                schema: "dbo",
                columns: table => new
                {
                    GId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SCodigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GCentroOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GCentroDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SRelevanciaDestino = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    SEstado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DFechaSalida = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DFechaRecepcion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SRemision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    STransportador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SObservaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SReceptor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DFechaCierre = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SIdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DFechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SUsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DFechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SUsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BActivo = table.Column<bool>(type: "bit", nullable: false),
                    TRowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_LoteDisposicionFinal", x => x.GId);
                    table.ForeignKey(
                        name: "FK_TBL_LoteDisposicionFinal_TBL_Centro_GCentroDestinoId",
                        column: x => x.GCentroDestinoId,
                        principalSchema: "dbo",
                        principalTable: "TBL_Centro",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_LoteDisposicionFinal_TBL_Centro_GCentroOrigenId",
                        column: x => x.GCentroOrigenId,
                        principalSchema: "dbo",
                        principalTable: "TBL_Centro",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_GLoteDisposicionFinalId",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta",
                column: "GLoteDisposicionFinalId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteDisposicionFinal_GCentroDestinoId",
                schema: "dbo",
                table: "TBL_LoteDisposicionFinal",
                column: "GCentroDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteDisposicionFinal_GCentroOrigenId",
                schema: "dbo",
                table: "TBL_LoteDisposicionFinal",
                column: "GCentroOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteDisposicionFinal_SCodigo",
                schema: "dbo",
                table: "TBL_LoteDisposicionFinal",
                column: "SCodigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LoteDisposicionFinal_SIdempotencyKey",
                schema: "dbo",
                table: "TBL_LoteDisposicionFinal",
                column: "SIdempotencyKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteDisposicionFinal_GLoteDisposicionFinalId",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta",
                column: "GLoteDisposicionFinalId",
                principalSchema: "dbo",
                principalTable: "TBL_LoteDisposicionFinal",
                principalColumn: "GId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_OrdenServicioLlanta_TBL_LoteDisposicionFinal_GLoteDisposicionFinalId",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropTable(
                name: "TBL_LoteDisposicionFinal",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_TBL_OrdenServicioLlanta_GLoteDisposicionFinalId",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "DFechaDisposicion",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "DFechaEvaluacion",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "GLoteDisposicionFinalId",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropColumn(
                name: "SEvaluadoPor",
                schema: "dbo",
                table: "TBL_OrdenServicioLlanta");
        }
    }
}
