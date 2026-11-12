using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations;

public partial class AddInspectionFoundTireRetread : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Unknown physical type must not be replaced with an invented catalog entry.
        migrationBuilder.AlterColumn<Guid>(name: "GTipoLlantaId", schema: "dbo", table: "TBL_Llanta",
            type: "uniqueidentifier", nullable: true, oldClrType: typeof(Guid), oldType: "uniqueidentifier");
        migrationBuilder.AddColumn<Guid>(name: "GMarcaReencaucheId", schema: "dbo", table: "TBL_Llanta",
            type: "uniqueidentifier", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Llanta_GMarcaReencaucheId", schema: "dbo", table: "TBL_Llanta", column: "GMarcaReencaucheId");
        migrationBuilder.AddForeignKey(name: "FK_TBL_Llanta_TBL_Marca_GMarcaReencaucheId", schema: "dbo", table: "TBL_Llanta",
            column: "GMarcaReencaucheId", principalSchema: "dbo", principalTable: "TBL_Marca", principalColumn: "GId", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => throw new NotSupportedException("La reversión requiere preservar las marcas de reencauche y clasificar las llantas sin tipo; no se asignan valores ficticios ni se elimina su evidencia automáticamente.");
}
