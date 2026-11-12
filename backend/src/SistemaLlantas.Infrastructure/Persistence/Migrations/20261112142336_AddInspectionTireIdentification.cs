using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionTireIdentification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BFichaEsReencauchada",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DFichaProfundidadInicial",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GFichaDimensionId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GFichaMarcaId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GFichaMarcaReencaucheId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GFichaReferenciaId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NFichaVida",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SFichaSerial",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BFichaEsReencauchada",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "DFichaProfundidadInicial",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "GFichaDimensionId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "GFichaMarcaId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "GFichaMarcaReencaucheId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "GFichaReferenciaId",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "NFichaVida",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");

            migrationBuilder.DropColumn(
                name: "SFichaSerial",
                schema: "dbo",
                table: "TBL_InconsistenciaInspeccion");
        }
    }
}
