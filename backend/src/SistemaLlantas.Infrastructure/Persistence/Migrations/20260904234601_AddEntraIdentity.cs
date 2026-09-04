using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntraIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EntraObjectId",
                table: "TBL_Usuario",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Usuario_EntraObjectId",
                table: "TBL_Usuario",
                column: "EntraObjectId",
                unique: true,
                filter: "[EntraObjectId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TBL_Usuario_EntraObjectId",
                table: "TBL_Usuario");

            migrationBuilder.DropColumn(
                name: "EntraObjectId",
                table: "TBL_Usuario");
        }
    }
}
