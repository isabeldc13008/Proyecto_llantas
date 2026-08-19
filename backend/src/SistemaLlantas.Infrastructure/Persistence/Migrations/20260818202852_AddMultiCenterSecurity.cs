using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCenterSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_UsuarioCentro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_TBL_UsuarioCentro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_UsuarioCentro_TBL_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "TBL_Centro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TBL_UsuarioCentro_TBL_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "TBL_Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_UsuarioCentro_CentroId",
                table: "TBL_UsuarioCentro",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_UsuarioCentro_UsuarioId_CentroId",
                table: "TBL_UsuarioCentro",
                columns: new[] { "UsuarioId", "CentroId" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO TBL_UsuarioCentro (Id, UsuarioId, CentroId, FechaCreacion, UsuarioCreacion, Activo)
                SELECT NEWID(), Id, CentroId, SYSDATETIMEOFFSET(), 'migracion-multicentro', 1
                FROM TBL_Usuario
                WHERE CentroId IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_UsuarioCentro");
        }
    }
}
