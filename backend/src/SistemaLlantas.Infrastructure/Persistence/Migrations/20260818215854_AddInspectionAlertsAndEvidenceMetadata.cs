using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionAlertsAndEvidenceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "TBL_EvidenciaInspeccion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetenerHasta",
                table: "TBL_EvidenciaInspeccion",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TamanoBytes",
                table: "TBL_EvidenciaInspeccion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "TBL_AlertaInspeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    InspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspeccionDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_AlertaInspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_AlertaInspeccion_TBL_InspeccionDetalle_InspeccionDetalleId",
                        column: x => x.InspeccionDetalleId,
                        principalTable: "TBL_InspeccionDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_AlertaInspeccion_TBL_Inspeccion_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "TBL_Inspeccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ParametroAlerta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ParametroAlerta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_AlertaHistorial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertaInspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "int", nullable: false),
                    EstadoNuevo = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_AlertaHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_AlertaHistorial_TBL_AlertaInspeccion_AlertaInspeccionId",
                        column: x => x.AlertaInspeccionId,
                        principalTable: "TBL_AlertaInspeccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_AlertaHistorial_AlertaInspeccionId",
                table: "TBL_AlertaHistorial",
                column: "AlertaInspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_AlertaInspeccion_CentroId_Estado_FechaCreacion",
                table: "TBL_AlertaInspeccion",
                columns: new[] { "CentroId", "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_AlertaInspeccion_InspeccionDetalleId_Tipo",
                table: "TBL_AlertaInspeccion",
                columns: new[] { "InspeccionDetalleId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_AlertaInspeccion_InspeccionId",
                table: "TBL_AlertaInspeccion",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ParametroAlerta_Codigo",
                table: "TBL_ParametroAlerta",
                column: "Codigo",
                unique: true);
            migrationBuilder.Sql("INSERT INTO TBL_ParametroAlerta (Id,Codigo,Valor,Unidad,FechaCreacion,UsuarioCreacion,Activo) VALUES (NEWID(),'DIFERENCIA_HOMBROS_MM',3,'mm',SYSDATETIMEOFFSET(),'migration-phase4',1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_AlertaHistorial");

            migrationBuilder.DropTable(
                name: "TBL_ParametroAlerta");

            migrationBuilder.DropTable(
                name: "TBL_AlertaInspeccion");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "TBL_EvidenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "RetenerHasta",
                table: "TBL_EvidenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "TamanoBytes",
                table: "TBL_EvidenciaInspeccion");
        }
    }
}
