using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Relevancia",
                table: "TBL_Centro",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TBL_CausaLlanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_CausaLlanta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_CondicionLlanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequiereCausa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_CondicionLlanta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_EvidenciaInspeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InconsistenciaInspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                    table.PrimaryKey("PK_TBL_EvidenciaInspeccion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_MovimientoLlanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InconsistenciaInspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaAnteriorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LlantaNuevaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TecnicoReporta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UsuarioAutoriza = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaReporte = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaAutorizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_TBL_MovimientoLlanta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ParametroReencauche",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DimensionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaximoReencauches = table.Column<int>(type: "int", nullable: false),
                    ProfundidadMinima = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ParametroReencauche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_RecomendacionInspeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EsCandidataReencauche = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_RecomendacionInspeccion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Vehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroInterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Vehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_Vehiculo_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_EjeVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_EjeVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_EjeVehiculo_TBL_Vehiculo_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "TBL_Vehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Inspeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kilometraje = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    TecnicoId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_TBL_Inspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_Inspeccion_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_Inspeccion_TBL_Vehiculo_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "TBL_Vehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_PosicionVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EjeVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Lado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    LlantaActualId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_PosicionVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_PosicionVehiculo_TBL_EjeVehiculo_EjeVehiculoId",
                        column: x => x.EjeVehiculoId,
                        principalTable: "TBL_EjeVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBL_PosicionVehiculo_TBL_Llanta_LlantaActualId",
                        column: x => x.LlantaActualId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_InconsistenciaInspeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaEsperadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdentificadorEncontrado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TecnicoId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    UsuarioAutorizador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaAutorizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ObservacionAutorizacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_InconsistenciaInspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_InconsistenciaInspeccion_TBL_Inspeccion_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "TBL_Inspeccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_InconsistenciaInspeccion_TBL_Llanta_LlantaEsperadaId",
                        column: x => x.LlantaEsperadaId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TBL_InconsistenciaInspeccion_TBL_PosicionVehiculo_PosicionVehiculoId",
                        column: x => x.PosicionVehiculoId,
                        principalTable: "TBL_PosicionVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_InspeccionDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosicionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProfundidadExterior = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    ProfundidadCentro = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    ProfundidadInterior = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    CondicionLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CausaLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecomendacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_TBL_InspeccionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_InspeccionDetalle_TBL_CausaLlanta_CausaLlantaId",
                        column: x => x.CausaLlantaId,
                        principalTable: "TBL_CausaLlanta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TBL_InspeccionDetalle_TBL_CondicionLlanta_CondicionLlantaId",
                        column: x => x.CondicionLlantaId,
                        principalTable: "TBL_CondicionLlanta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TBL_InspeccionDetalle_TBL_Inspeccion_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "TBL_Inspeccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBL_InspeccionDetalle_TBL_Llanta_LlantaId",
                        column: x => x.LlantaId,
                        principalTable: "TBL_Llanta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TBL_InspeccionDetalle_TBL_PosicionVehiculo_PosicionVehiculoId",
                        column: x => x.PosicionVehiculoId,
                        principalTable: "TBL_PosicionVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_InspeccionDetalle_TBL_RecomendacionInspeccion_RecomendacionId",
                        column: x => x.RecomendacionId,
                        principalTable: "TBL_RecomendacionInspeccion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TBL_LlantaTemporal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InconsistenciaInspeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentificadorTemporal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdentificadorFisico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_LlantaTemporal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_LlantaTemporal_TBL_InconsistenciaInspeccion_InconsistenciaInspeccionId",
                        column: x => x.InconsistenciaInspeccionId,
                        principalTable: "TBL_InconsistenciaInspeccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CausaLlanta_Codigo",
                table: "TBL_CausaLlanta",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CondicionLlanta_Codigo",
                table: "TBL_CondicionLlanta",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_EjeVehiculo_VehiculoId_Numero",
                table: "TBL_EjeVehiculo",
                columns: new[] { "VehiculoId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InconsistenciaInspeccion_InspeccionId",
                table: "TBL_InconsistenciaInspeccion",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InconsistenciaInspeccion_LlantaEsperadaId",
                table: "TBL_InconsistenciaInspeccion",
                column: "LlantaEsperadaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InconsistenciaInspeccion_PosicionVehiculoId",
                table: "TBL_InconsistenciaInspeccion",
                column: "PosicionVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Inspeccion_CentroId",
                table: "TBL_Inspeccion",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Inspeccion_VehiculoId",
                table: "TBL_Inspeccion",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InspeccionDetalle_CausaLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "CausaLlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InspeccionDetalle_CondicionLlantaId",
                table: "TBL_InspeccionDetalle",
                column: "CondicionLlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InspeccionDetalle_InspeccionId_PosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                columns: new[] { "InspeccionId", "PosicionVehiculoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InspeccionDetalle_LlantaId",
                table: "TBL_InspeccionDetalle",
                column: "LlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InspeccionDetalle_PosicionVehiculoId",
                table: "TBL_InspeccionDetalle",
                column: "PosicionVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_InspeccionDetalle_RecomendacionId",
                table: "TBL_InspeccionDetalle",
                column: "RecomendacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_LlantaTemporal_InconsistenciaInspeccionId",
                table: "TBL_LlantaTemporal",
                column: "InconsistenciaInspeccionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_PosicionVehiculo_EjeVehiculoId_Codigo",
                table: "TBL_PosicionVehiculo",
                columns: new[] { "EjeVehiculoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_PosicionVehiculo_LlantaActualId",
                table: "TBL_PosicionVehiculo",
                column: "LlantaActualId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_RecomendacionInspeccion_Codigo",
                table: "TBL_RecomendacionInspeccion",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Vehiculo_CentroId",
                table: "TBL_Vehiculo",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Vehiculo_NumeroInterno",
                table: "TBL_Vehiculo",
                column: "NumeroInterno",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_EvidenciaInspeccion");

            migrationBuilder.DropTable(
                name: "TBL_InspeccionDetalle");

            migrationBuilder.DropTable(
                name: "TBL_LlantaTemporal");

            migrationBuilder.DropTable(
                name: "TBL_MovimientoLlanta");

            migrationBuilder.DropTable(
                name: "TBL_ParametroReencauche");

            migrationBuilder.DropTable(
                name: "TBL_CausaLlanta");

            migrationBuilder.DropTable(
                name: "TBL_CondicionLlanta");

            migrationBuilder.DropTable(
                name: "TBL_RecomendacionInspeccion");

            migrationBuilder.DropTable(
                name: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropTable(
                name: "TBL_Inspeccion");

            migrationBuilder.DropTable(
                name: "TBL_PosicionVehiculo");

            migrationBuilder.DropTable(
                name: "TBL_EjeVehiculo");

            migrationBuilder.DropTable(
                name: "TBL_Vehiculo");

            migrationBuilder.DropColumn(
                name: "Relevancia",
                table: "TBL_Centro");
        }
    }
}
