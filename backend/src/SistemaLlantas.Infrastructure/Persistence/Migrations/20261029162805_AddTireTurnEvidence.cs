using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTireTurnEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DFechaGiro",
                schema: "dbo",
                table: "TBL_Movimiento",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GTecnicoGiroId",
                schema: "dbo",
                table: "TBL_Movimiento",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NKilometrajeGiro",
                schema: "dbo",
                table: "TBL_Movimiento",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DFechaGiro",
                schema: "dbo",
                table: "TBL_Movimiento");

            migrationBuilder.DropColumn(
                name: "GTecnicoGiroId",
                schema: "dbo",
                table: "TBL_Movimiento");

            migrationBuilder.DropColumn(
                name: "NKilometrajeGiro",
                schema: "dbo",
                table: "TBL_Movimiento");
        }
    }
}
