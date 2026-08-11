using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalogosLlantas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_Auditoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Identificador = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Centro",
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
                    table.PrimaryKey("PK_TBL_Centro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Dimension",
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
                    table.PrimaryKey("PK_TBL_Dimension", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_EstadoLlanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EsDisposicionFinal = table.Column<bool>(type: "bit", nullable: false),
                    PermiteMontaje = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TBL_EstadoLlanta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Marca",
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
                    table.PrimaryKey("PK_TBL_Marca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_TipoLlanta",
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
                    table.PrimaryKey("PK_TBL_TipoLlanta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Referencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarcaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_TBL_Referencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_Referencia_TBL_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "TBL_Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Llanta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MarcaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DimensionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoLlantaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UbicacionActual = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaCompra = table.Column<DateOnly>(type: "date", nullable: true),
                    Costo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KilometrajeAcumulado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroReencauches = table.Column<int>(type: "int", nullable: false),
                    ProfundidadInicial = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_TBL_Llanta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_Llanta_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_Llanta_TBL_Dimension_DimensionId",
                        column: x => x.DimensionId,
                        principalTable: "TBL_Dimension",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_Llanta_TBL_EstadoLlanta_EstadoLlantaId",
                        column: x => x.EstadoLlantaId,
                        principalTable: "TBL_EstadoLlanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_Llanta_TBL_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "TBL_Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_Llanta_TBL_Referencia_ReferenciaId",
                        column: x => x.ReferenciaId,
                        principalTable: "TBL_Referencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_Llanta_TBL_TipoLlanta_TipoLlantaId",
                        column: x => x.TipoLlantaId,
                        principalTable: "TBL_TipoLlanta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_EntidadFecha",
                table: "TBL_Auditoria",
                columns: new[] { "Entidad", "Identificador", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Centro_Codigo",
                table: "TBL_Centro",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dimension_Codigo",
                table: "TBL_Dimension",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadoLlanta_Codigo",
                table: "TBL_EstadoLlanta",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Llanta_CentroEstado",
                table: "TBL_Llanta",
                columns: new[] { "CentroId", "EstadoLlantaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Llanta_Codigo",
                table: "TBL_Llanta",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Llanta_Serial",
                table: "TBL_Llanta",
                column: "Serial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Llanta_DimensionId",
                table: "TBL_Llanta",
                column: "DimensionId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Llanta_EstadoLlantaId",
                table: "TBL_Llanta",
                column: "EstadoLlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Llanta_MarcaId",
                table: "TBL_Llanta",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Llanta_ReferenciaId",
                table: "TBL_Llanta",
                column: "ReferenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Llanta_TipoLlantaId",
                table: "TBL_Llanta",
                column: "TipoLlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_Marca_Codigo",
                table: "TBL_Marca",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referencia_Codigo",
                table: "TBL_Referencia",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Referencia_MarcaId",
                table: "TBL_Referencia",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoLlanta_Codigo",
                table: "TBL_TipoLlanta",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_Auditoria");

            migrationBuilder.DropTable(
                name: "TBL_Llanta");

            migrationBuilder.DropTable(
                name: "TBL_Centro");

            migrationBuilder.DropTable(
                name: "TBL_Dimension");

            migrationBuilder.DropTable(
                name: "TBL_EstadoLlanta");

            migrationBuilder.DropTable(
                name: "TBL_Referencia");

            migrationBuilder.DropTable(
                name: "TBL_TipoLlanta");

            migrationBuilder.DropTable(
                name: "TBL_Marca");
        }
    }
}
