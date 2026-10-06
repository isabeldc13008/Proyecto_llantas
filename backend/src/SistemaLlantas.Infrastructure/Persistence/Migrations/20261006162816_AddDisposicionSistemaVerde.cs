using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisposicionSistemaVerde : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SPlaca",
                schema: "dbo",
                table: "TBL_LoteDisposicionFinal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TBL_DespachoDisposicion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CentroR1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaSalida = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Transportador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SolicitudHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaCierre = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CerradoPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_DespachoDisposicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_DespachoDisposicion_TBL_Centro_CentroR1Id",
                        column: x => x.CentroR1Id,
                        principalSchema: "dbo",
                        principalTable: "TBL_Centro",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_DespachoDisposicion_TBL_ProveedorServicio_ProveedorId",
                        column: x => x.ProveedorId,
                        principalSchema: "dbo",
                        principalTable: "TBL_ProveedorServicio",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_NovedadDisposicion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaResolucion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResueltaPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Resolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_NovedadDisposicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_NovedadDisposicion_TBL_LoteDisposicionFinal_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "dbo",
                        principalTable: "TBL_LoteDisposicionFinal",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_NovedadDisposicion_TBL_OrdenServicioLlanta_OrdenId",
                        column: x => x.OrdenId,
                        principalSchema: "dbo",
                        principalTable: "TBL_OrdenServicioLlanta",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ActaDisposicion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ActaDisposicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_ActaDisposicion_TBL_Centro_CentroOrigenId",
                        column: x => x.CentroOrigenId,
                        principalSchema: "dbo",
                        principalTable: "TBL_Centro",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_ActaDisposicion_TBL_DespachoDisposicion_DespachoId",
                        column: x => x.DespachoId,
                        principalSchema: "dbo",
                        principalTable: "TBL_DespachoDisposicion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_DespachoDisposicionItem",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteEntradaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_DespachoDisposicionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_DespachoDisposicionItem_TBL_DespachoDisposicion_DespachoId",
                        column: x => x.DespachoId,
                        principalSchema: "dbo",
                        principalTable: "TBL_DespachoDisposicion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_DespachoDisposicionItem_TBL_LoteDisposicionFinal_LoteEntradaId",
                        column: x => x.LoteEntradaId,
                        principalSchema: "dbo",
                        principalTable: "TBL_LoteDisposicionFinal",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_DespachoDisposicionItem_TBL_OrdenServicioLlanta_OrdenId",
                        column: x => x.OrdenId,
                        principalSchema: "dbo",
                        principalTable: "TBL_OrdenServicioLlanta",
                        principalColumn: "GId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_SoporteActaDisposicion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_SoporteActaDisposicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_SoporteActaDisposicion_TBL_ActaDisposicion_ActaId",
                        column: x => x.ActaId,
                        principalSchema: "dbo",
                        principalTable: "TBL_ActaDisposicion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActaDisposicion_CentroOrigenId",
                schema: "dbo",
                table: "TBL_ActaDisposicion",
                column: "CentroOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActaDisposicion_Codigo",
                schema: "dbo",
                table: "TBL_ActaDisposicion",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActaDisposicion_DespachoId_CentroOrigenId",
                schema: "dbo",
                table: "TBL_ActaDisposicion",
                columns: new[] { "DespachoId", "CentroOrigenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicion_CentroR1Id",
                schema: "dbo",
                table: "TBL_DespachoDisposicion",
                column: "CentroR1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicion_Codigo",
                schema: "dbo",
                table: "TBL_DespachoDisposicion",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicion_IdempotencyKey",
                schema: "dbo",
                table: "TBL_DespachoDisposicion",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicion_ProveedorId",
                schema: "dbo",
                table: "TBL_DespachoDisposicion",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicionItem_DespachoId",
                schema: "dbo",
                table: "TBL_DespachoDisposicionItem",
                column: "DespachoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicionItem_LoteEntradaId",
                schema: "dbo",
                table: "TBL_DespachoDisposicionItem",
                column: "LoteEntradaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_DespachoDisposicionItem_OrdenId",
                schema: "dbo",
                table: "TBL_DespachoDisposicionItem",
                column: "OrdenId",
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_NovedadDisposicion_LoteId",
                schema: "dbo",
                table: "TBL_NovedadDisposicion",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_NovedadDisposicion_OrdenId",
                schema: "dbo",
                table: "TBL_NovedadDisposicion",
                column: "OrdenId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_SoporteActaDisposicion_ActaId",
                schema: "dbo",
                table: "TBL_SoporteActaDisposicion",
                column: "ActaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_DespachoDisposicionItem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TBL_NovedadDisposicion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TBL_SoporteActaDisposicion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TBL_ActaDisposicion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TBL_DespachoDisposicion",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "SPlaca",
                schema: "dbo",
                table: "TBL_LoteDisposicionFinal");
        }
    }
}
