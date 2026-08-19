using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalWorkflowsPhase6To8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_CargaMasiva",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalFilas = table.Column<int>(type: "int", nullable: false),
                    FilasValidas = table.Column<int>(type: "int", nullable: false),
                    FilasConError = table.Column<int>(type: "int", nullable: false),
                    FilasJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErroresJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaProcesamiento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_CargaMasiva", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ProveedorServicio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ProveedorServicio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_SolicitudOperacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PosicionDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoDestino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CentroDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LlantaDesplazadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PosicionDestinoDesplazadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinoDesplazada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KilometrajeVehiculo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActividadProgramadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Solicitante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Aprobador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaDecision = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MovimientoEjecutadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaRecepcionDestino = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_SolicitudOperacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_SolicitudOperacion_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_SolicitudOperacion_TBL_Llanta_LlantaId",
                        column: x => x.LlantaId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_OrdenServicioLlanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Costo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Elegible = table.Column<bool>(type: "bit", nullable: false),
                    CriterioElegibilidad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Solicitante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Aprobador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaEnvio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FechaRecepcion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_OrdenServicioLlanta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_OrdenServicioLlanta_TBL_Centro_CentroOrigenId",
                        column: x => x.CentroOrigenId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_OrdenServicioLlanta_TBL_Llanta_LlantaId",
                        column: x => x.LlantaId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_OrdenServicioLlanta_TBL_ProveedorServicio_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "TBL_ProveedorServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_EvidenciaFlujo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenServicioLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_EvidenciaFlujo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_EvidenciaFlujo_TBL_OrdenServicioLlanta_OrdenServicioLlantaId",
                        column: x => x.OrdenServicioLlantaId,
                        principalTable: "TBL_OrdenServicioLlanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CargaMasiva_Usuario_FechaCreacion",
                table: "TBL_CargaMasiva",
                columns: new[] { "Usuario", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_EvidenciaFlujo_OrdenServicioLlantaId",
                table: "TBL_EvidenciaFlujo",
                column: "OrdenServicioLlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_CentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                column: "CentroOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_LlantaId",
                table: "TBL_OrdenServicioLlanta",
                column: "LlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_ProveedorId",
                table: "TBL_OrdenServicioLlanta",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrdenServicioLlanta_Tipo_Estado_CentroOrigenId",
                table: "TBL_OrdenServicioLlanta",
                columns: new[] { "Tipo", "Estado", "CentroOrigenId" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ProveedorServicio_Codigo",
                table: "TBL_ProveedorServicio",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_SolicitudOperacion_CentroId_Estado_FechaCreacion",
                table: "TBL_SolicitudOperacion",
                columns: new[] { "CentroId", "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_SolicitudOperacion_LlantaId",
                table: "TBL_SolicitudOperacion",
                column: "LlantaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_CargaMasiva");

            migrationBuilder.DropTable(
                name: "TBL_EvidenciaFlujo");

            migrationBuilder.DropTable(
                name: "TBL_SolicitudOperacion");

            migrationBuilder.DropTable(
                name: "TBL_OrdenServicioLlanta");

            migrationBuilder.DropTable(
                name: "TBL_ProveedorServicio");
        }
    }
}
