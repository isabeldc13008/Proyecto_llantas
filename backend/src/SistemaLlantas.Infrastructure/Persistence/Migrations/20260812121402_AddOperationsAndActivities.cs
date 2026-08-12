using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationsAndActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_ActividadProgramada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoActividad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaProgramada = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PosicionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TecnicoId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaInicioReal = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FechaFinReal = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ActividadProgramada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_ActividadProgramada_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_ActividadProgramada_TBL_Vehiculo_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "TBL_Vehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_AsignacionLlantaPosicion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaFin = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EsActiva = table.Column<bool>(type: "bit", nullable: false),
                    MovimientoOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_AsignacionLlantaPosicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_AsignacionLlantaPosicion_TBL_Llanta_LlantaId",
                        column: x => x.LlantaId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_AsignacionLlantaPosicion_TBL_PosicionVehiculo_PosicionVehiculoId",
                        column: x => x.PosicionVehiculoId,
                        principalTable: "TBL_PosicionVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Movimiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Movimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_Movimiento_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_MovimientoDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovimientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PosicionDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoDestino = table.Column<int>(type: "int", nullable: false),
                    CentroDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinoDescripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_MovimientoDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_MovimientoDetalle_TBL_Llanta_LlantaId",
                        column: x => x.LlantaId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_MovimientoDetalle_TBL_Movimiento_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "TBL_Movimiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_CentroId",
                table: "TBL_ActividadProgramada",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_TecnicoId_Estado_FechaProgramada",
                table: "TBL_ActividadProgramada",
                columns: new[] { "TecnicoId", "Estado", "FechaProgramada" });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ActividadProgramada_VehiculoId",
                table: "TBL_ActividadProgramada",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "UX_Asignacion_LlantaActiva",
                table: "TBL_AsignacionLlantaPosicion",
                column: "LlantaId",
                unique: true,
                filter: "[EsActiva] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_Asignacion_PosicionActiva",
                table: "TBL_AsignacionLlantaPosicion",
                column: "PosicionVehiculoId",
                unique: true,
                filter: "[EsActiva] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Movimiento_CentroId",
                table: "TBL_Movimiento",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Movimiento_Numero",
                table: "TBL_Movimiento",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MovimientoDetalle_LlantaId",
                table: "TBL_MovimientoDetalle",
                column: "LlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_MovimientoDetalle_MovimientoId",
                table: "TBL_MovimientoDetalle",
                column: "MovimientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_ActividadProgramada");

            migrationBuilder.DropTable(
                name: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropTable(
                name: "TBL_MovimientoDetalle");

            migrationBuilder.DropTable(
                name: "TBL_Movimiento");
        }
    }
}
