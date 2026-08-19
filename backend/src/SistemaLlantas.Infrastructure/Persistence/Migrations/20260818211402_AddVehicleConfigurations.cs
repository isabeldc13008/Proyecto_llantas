using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "TBL_Vehiculo",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Activo");

            migrationBuilder.AddColumn<decimal>(
                name: "Kilometraje",
                table: "TBL_Vehiculo",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion",
                table: "TBL_PosicionVehiculo",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Sin definir");

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "TBL_EjeVehiculo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE TBL_EjeVehiculo SET Orden = Numero WHERE Orden = 0");

            migrationBuilder.AddColumn<string>(
                name: "TipoEje",
                table: "TBL_EjeVehiculo",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Sin definir");

            migrationBuilder.CreateTable(
                name: "TBL_ConfiguracionVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TipoVehiculo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ConfiguracionVehiculo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ConfiguracionEje",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfiguracionVehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoEje = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ConfiguracionEje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_ConfiguracionEje_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                        column: x => x.ConfiguracionVehiculoId,
                        principalTable: "TBL_ConfiguracionVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ConfiguracionPosicion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfiguracionEjeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Lado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ConfiguracionPosicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_ConfiguracionPosicion_TBL_ConfiguracionEje_ConfiguracionEjeId",
                        column: x => x.ConfiguracionEjeId,
                        principalTable: "TBL_ConfiguracionEje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Vehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                column: "ConfiguracionVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ConfiguracionEje_ConfiguracionVehiculoId_Orden",
                table: "TBL_ConfiguracionEje",
                columns: new[] { "ConfiguracionVehiculoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ConfiguracionPosicion_ConfiguracionEjeId_Codigo",
                table: "TBL_ConfiguracionPosicion",
                columns: new[] { "ConfiguracionEjeId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ConfiguracionPosicion_ConfiguracionEjeId_Orden",
                table: "TBL_ConfiguracionPosicion",
                columns: new[] { "ConfiguracionEjeId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ConfiguracionVehiculo_Codigo",
                table: "TBL_ConfiguracionVehiculo",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo",
                column: "ConfiguracionVehiculoId",
                principalTable: "TBL_ConfiguracionVehiculo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Vehiculo_TBL_ConfiguracionVehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo");

            migrationBuilder.DropTable(
                name: "TBL_ConfiguracionPosicion");

            migrationBuilder.DropTable(
                name: "TBL_ConfiguracionEje");

            migrationBuilder.DropTable(
                name: "TBL_ConfiguracionVehiculo");

            migrationBuilder.DropIndex(
                name: "IX_TBL_Vehiculo_ConfiguracionVehiculoId",
                table: "TBL_Vehiculo");

            migrationBuilder.DropColumn(
                name: "ConfiguracionVehiculoId",
                table: "TBL_Vehiculo");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "TBL_Vehiculo");

            migrationBuilder.DropColumn(
                name: "Kilometraje",
                table: "TBL_Vehiculo");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "TBL_PosicionVehiculo");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "TBL_EjeVehiculo");

            migrationBuilder.DropColumn(
                name: "TipoEje",
                table: "TBL_EjeVehiculo");
        }
    }
}
