using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMountPositionDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DFechaEjecucion",
                schema: "dbo",
                table: "TBL_SolicitudOperacion",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SDestinoSolicitadoJson",
                schema: "dbo",
                table: "TBL_SolicitudOperacion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SEjecutor",
                schema: "dbo",
                table: "TBL_SolicitudOperacion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SHistorialRevisionJson",
                schema: "dbo",
                table: "TBL_SolicitudOperacion",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("La reversión requiere preservar el historial de decisiones y aprobación explícita del DBA.");
        }
    }
}
