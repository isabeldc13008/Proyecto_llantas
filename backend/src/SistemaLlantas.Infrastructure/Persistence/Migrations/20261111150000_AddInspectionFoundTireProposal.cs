using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionFoundTireProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GDimensionPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GMarcaPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GMarcaReencauchePropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GReferenciaPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GTipoLlantaPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NNumeroReencauchesPropuesto",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NProfundidadInicialPropuesta",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SCodigoPropuesto",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSerialPropuesto",
                schema: "dbo",
                table: "TBL_LlantaTemporal",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GDimensionPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "GMarcaPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "GMarcaReencauchePropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "GReferenciaPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "GTipoLlantaPropuestaId",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "NNumeroReencauchesPropuesto",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "NProfundidadInicialPropuesta",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "SCodigoPropuesto",
                schema: "dbo",
                table: "TBL_LlantaTemporal");

            migrationBuilder.DropColumn(
                name: "SSerialPropuesto",
                schema: "dbo",
                table: "TBL_LlantaTemporal");
        }
    }
}

