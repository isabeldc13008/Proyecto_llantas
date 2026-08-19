using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTireLifecycleMileage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "TBL_Movimiento",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KilometrajeDesmontaje",
                table: "TBL_AsignacionLlantaPosicion",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KilometrajeMontaje",
                table: "TBL_AsignacionLlantaPosicion",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KilometrajeRecorrido",
                table: "TBL_AsignacionLlantaPosicion",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "TBL_Movimiento");

            migrationBuilder.DropColumn(
                name: "KilometrajeDesmontaje",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropColumn(
                name: "KilometrajeMontaje",
                table: "TBL_AsignacionLlantaPosicion");

            migrationBuilder.DropColumn(
                name: "KilometrajeRecorrido",
                table: "TBL_AsignacionLlantaPosicion");
        }
    }
}
