using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertRuleConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GCentroId",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SDescripcion",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SNombre",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Diferencia entre mediciones");

            migrationBuilder.AddColumn<string>(
                name: "SOperador",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: ">=");

            migrationBuilder.AddColumn<string>(
                name: "SPrioridad",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Media");

            migrationBuilder.AddColumn<string>(
                name: "STipo",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DIFERENCIA_HOMBROS");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ParametroAlerta_GCentroId",
                schema: "dbo",
                table: "TBL_ParametroAlerta",
                column: "GCentroId");

            // Fresh historical migrations use Id; the existing database uses GId.
            migrationBuilder.Sql("""
                DECLARE @key sysname = CASE WHEN COL_LENGTH('dbo.TBL_Centro','GId') IS NOT NULL THEN 'GId' ELSE 'Id' END;
                DECLARE @fk nvarchar(max)=N'ALTER TABLE dbo.TBL_ParametroAlerta ADD CONSTRAINT FK_TBL_ParametroAlerta_TBL_Centro_GCentroId FOREIGN KEY (GCentroId) REFERENCES dbo.TBL_Centro('+QUOTENAME(@key)+N')';
                EXEC sp_executesql @fk;
                """);
            migrationBuilder.Sql("""
                DECLARE @prefix bit=CASE WHEN COL_LENGTH('dbo.TBL_Permiso','SCodigo') IS NOT NULL THEN 1 ELSE 0 END;
                DECLARE @id sysname=CASE WHEN @prefix=1 THEN 'GId' ELSE 'Id' END,
                        @code sysname=CASE WHEN @prefix=1 THEN 'SCodigo' ELSE 'Codigo' END,
                        @name sysname=CASE WHEN @prefix=1 THEN 'SNombre' ELSE 'Nombre' END,
                        @created sysname=CASE WHEN @prefix=1 THEN 'DFechaCreacion' ELSE 'FechaCreacion' END,
                        @user sysname=CASE WHEN @prefix=1 THEN 'SUsuarioCreacion' ELSE 'UsuarioCreacion' END,
                        @active sysname=CASE WHEN @prefix=1 THEN 'BActivo' ELSE 'Activo' END,
                        @roleId sysname=CASE WHEN @prefix=1 THEN 'GRolId' ELSE 'RolId' END,
                        @permissionId sysname=CASE WHEN @prefix=1 THEN 'GPermisoId' ELSE 'PermisoId' END;
                DECLARE @sql nvarchar(max)=N'
                IF NOT EXISTS(SELECT 1 FROM dbo.TBL_Permiso WHERE '+QUOTENAME(@code)+N'=''alertas.parametrizar'')
                    INSERT dbo.TBL_Permiso('+QUOTENAME(@id)+N','+QUOTENAME(@code)+N','+QUOTENAME(@name)+N','+QUOTENAME(@created)+N','+QUOTENAME(@user)+N','+QUOTENAME(@active)+N')
                    VALUES(NEWID(),''alertas.parametrizar'',N''Parametrizar alertas'',SYSDATETIMEOFFSET(),''migration-alert-rules'',1);
                INSERT dbo.TBL_RolPermiso('+QUOTENAME(@roleId)+N','+QUOTENAME(@permissionId)+N')
                    SELECT r.'+QUOTENAME(@id)+N',p.'+QUOTENAME(@id)+N' FROM dbo.TBL_Rol r CROSS JOIN dbo.TBL_Permiso p
                    WHERE r.'+QUOTENAME(@code)+N' IN (''ADMINISTRADOR'',''SUPERVISOR_ADMINISTRADOR'') AND p.'+QUOTENAME(@code)+N'=''alertas.parametrizar''
                    AND NOT EXISTS(SELECT 1 FROM dbo.TBL_RolPermiso rp WHERE rp.'+QUOTENAME(@roleId)+N'=r.'+QUOTENAME(@id)+N' AND rp.'+QUOTENAME(@permissionId)+N'=p.'+QUOTENAME(@id)+N');';
                EXEC sp_executesql @sql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBL_ParametroAlerta_TBL_Centro_GCentroId",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropIndex(
                name: "IX_TBL_ParametroAlerta_GCentroId",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropColumn(
                name: "GCentroId",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropColumn(
                name: "SDescripcion",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropColumn(
                name: "SNombre",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropColumn(
                name: "SOperador",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropColumn(
                name: "SPrioridad",
                schema: "dbo",
                table: "TBL_ParametroAlerta");

            migrationBuilder.DropColumn(
                name: "STipo",
                schema: "dbo",
                table: "TBL_ParametroAlerta");
        }
    }
}
