using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations;

/// <summary>
/// Registers the manually managed vehicle operational-group column while remaining
/// safe for databases where the column is already present.
/// </summary>
public partial class SyncVehicleOperationalGroup : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.TBL_Vehiculo', N'SGrupoOperativo') IS NULL
            BEGIN
                ALTER TABLE [dbo].[TBL_Vehiculo] ADD [SGrupoOperativo] nvarchar(max) NULL;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The column is managed manually in existing environments. Do not drop it.
    }
}
