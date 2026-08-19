using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionalToCenters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RegionalId",
                table: "TBL_Centro",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TBL_Regional",
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
                    table.PrimaryKey("PK_TBL_Regional", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Centro_RegionalId",
                table: "TBL_Centro",
                column: "RegionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Regional_Codigo",
                table: "TBL_Regional",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TBL_Centro_TBL_Regional_RegionalId",
                table: "TBL_Centro",
                column: "RegionalId",
                principalTable: "TBL_Regional",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_Centro_TBL_Regional_RegionalId",
                table: "TBL_Centro");

            migrationBuilder.DropTable(
                name: "TBL_Regional");

            migrationBuilder.DropIndex(
                name: "IX_TBL_Centro_RegionalId",
                table: "TBL_Centro");

            migrationBuilder.DropColumn(
                name: "RegionalId",
                table: "TBL_Centro");
        }
    }
}
