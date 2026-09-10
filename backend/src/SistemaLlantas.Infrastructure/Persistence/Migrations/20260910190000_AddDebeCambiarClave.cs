using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SistemaLlantas.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LlantasDbContext))]
[Migration("20260910190000_AddDebeCambiarClave")]
public partial class AddDebeCambiarClave : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        IF COL_LENGTH(N'dbo.TBL_Usuario', N'BDebeCambiarClave') IS NULL
            ALTER TABLE dbo.TBL_Usuario ADD BDebeCambiarClave bit NOT NULL
                CONSTRAINT DF_TBL_Usuario_BDebeCambiarClave DEFAULT (0) WITH VALUES;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "BDebeCambiarClave", schema: "dbo", table: "TBL_Usuario");
}
