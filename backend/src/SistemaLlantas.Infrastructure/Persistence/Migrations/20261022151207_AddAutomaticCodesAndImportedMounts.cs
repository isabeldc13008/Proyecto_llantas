using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaLlantas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticCodesAndImportedMounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF DB_NAME() <> N'GDLLSQLDLLO' THROW 51000, 'Migracion exclusiva de Desarrollo GDLLSQLDLLO.', 1;
                IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId=N'20261006162816_AddDisposicionSistemaVerde')
                    THROW 51000, 'Falta checkpoint inicial aprobado.', 1;
                IF (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>26
                    THROW 51000, 'Se esperaban las 26 migraciones declaradas de DLLO; revisar antes de ejecutar.', 1;
                IF EXISTS(SELECT MigrationId FROM dbo.__EFMigrationsHistory EXCEPT SELECT v.Id FROM (VALUES
                    (N'20260806031028_InitialCatalogosLlantas'),
                    (N'20260812034221_AddInspectionWorkflow'),
                    (N'20260812121402_AddOperationsAndActivities'),
                    (N'20260818181414_AddPersistedSecurity'),
                    (N'20260818184029_AddRegionalToCenters'),
                    (N'20260818202852_AddMultiCenterSecurity'),
                    (N'20260818211402_AddVehicleConfigurations'),
                    (N'20260818213949_AddTireLifecycleMileage'),
                    (N'20260818214721_SeedTireLifecycleStates'),
                    (N'20260818215854_AddInspectionAlertsAndEvidenceMetadata'),
                    (N'20260818222817_AddSchedulingPhase5'),
                    (N'20260818232905_AddOperationalWorkflowsPhase6To8'),
                    (N'20260825225140_AddInspectionFoundTireTraceability'),
                    (N'20260826030341_AddSchedulingOriginAndIdempotency'),
                    (N'20260826034111_AddConsolidatedRepairShipments'),
                    (N'20260903172716_SincronizarModeloPostobon'),
                    (N'20260904234601_AddEntraIdentity'),
                    (N'20260907205344_AlignExistingAzureSchema'),
                    (N'20260907211928_AlignExistingAzureSecuritySchema'),
                    (N'20260907212835_AlignExistingAzureSchema_AllEntities'),
                    (N'20260908124228_AlignExistingAzureSchema_AllEntities_V2'),
                    (N'20260910190000_AddDebeCambiarClave'),
                    (N'20260911160835_AddAlertRuleConfiguration'),
                    (N'20260918131215_AddScheduledMountSets'),
                    (N'20260923182748_AddDisposalShipments'),
                    (N'20261006162816_AddDisposicionSistemaVerde')
                )v(Id)) THROW 51000, 'El historial no coincide con los 26 IDs aprobados.', 1;
                IF SCHEMA_NAME()<>N'dbo' THROW 51000, 'Se requiere esquema predeterminado dbo.', 1;
                IF EXISTS(SELECT 1 FROM sys.columns WHERE default_object_id<>0 AND
                    ((object_id=OBJECT_ID(N'dbo.TBL_Llanta') AND name IN(N'SSerial',N'NProfundidadInicial'))
                    OR (object_id=OBJECT_ID(N'dbo.TBL_AsignacionLlantaPosicion') AND name=N'DFechaInicio')))
                    THROW 51000, 'Hay defaults no presentes en el esquema revisado. Detener.', 1;
                IF ISNULL(COL_LENGTH(N'dbo.TBL_Marca',N'SCodigo'),-1)<>60 OR ISNULL(COL_LENGTH(N'dbo.TBL_Llanta',N'SSerial'),-1)<>200
                    THROW 51000, 'Longitud fisica inesperada.', 1;
                IF OBJECT_ID(N'dbo.TBL_ConsecutivoCodigo',N'U') IS NOT NULL
                    OR COL_LENGTH(N'dbo.TBL_Marca',N'SAbreviatura') IS NOT NULL
                    OR COL_LENGTH(N'dbo.TBL_Llanta',N'SIdentificadorOriginal') IS NOT NULL
                    OR COL_LENGTH(N'dbo.TBL_Llanta',N'SDatosImportacionJson') IS NOT NULL
                    THROW 51000, 'Existen objetos de una aplicacion parcial. No continuar.', 1;
                IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TBL_Llanta') AND name=N'IX_Llanta_SSerial' AND is_unique=1 AND is_disabled=0 AND has_filter=0)
                    THROW 51000, 'Indice de serial no coincide con la base revisada.', 1;
                IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.TBL_Llanta') AND name IN(N'SSerial',N'NProfundidadInicial') AND is_nullable=1)
                    OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.TBL_AsignacionLlantaPosicion') AND name=N'DFechaInicio' AND is_nullable=1)
                    THROW 51000, 'Nulabilidad previa inesperada. Revisar aplicacion parcial.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Llanta_SSerial",
                schema: "dbo",
                table: "TBL_Llanta");

            migrationBuilder.AddColumn<string>(
                name: "SAbreviatura",
                schema: "dbo",
                table: "TBL_Marca",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SSerial",
                schema: "dbo",
                table: "TBL_Llanta",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "NProfundidadInicial",
                schema: "dbo",
                table: "TBL_Llanta",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldPrecision: 8,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "SDatosImportacionJson",
                schema: "dbo",
                table: "TBL_Llanta",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SIdentificadorOriginal",
                schema: "dbo",
                table: "TBL_Llanta",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DFechaInicio",
                schema: "dbo",
                table: "TBL_AsignacionLlantaPosicion",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateTable(
                name: "TBL_ConsecutivoCodigo",
                schema: "dbo",
                columns: table => new
                {
                    SAmbito = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NUltimo = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ConsecutivoCodigo", x => x.SAmbito);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Marca_SAbreviatura",
                schema: "dbo",
                table: "TBL_Marca",
                column: "SAbreviatura",
                unique: true,
                filter: "[SAbreviatura] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Llanta_SSerial",
                schema: "dbo",
                table: "TBL_Llanta",
                column: "SSerial",
                unique: true,
                filter: "[SSerial] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Llanta_SIdentificadorOriginal",
                schema: "dbo",
                table: "TBL_Llanta",
                column: "SIdentificadorOriginal",
                unique: true,
                filter: "[SIdentificadorOriginal] IS NOT NULL");

            migrationBuilder.Sql("""
                IF ISNULL(COL_LENGTH(N'dbo.TBL_Marca',N'SAbreviatura'),-1)<>4
                    OR ISNULL(COL_LENGTH(N'dbo.TBL_Llanta',N'SIdentificadorOriginal'),-1)<>200
                    OR OBJECT_ID(N'dbo.TBL_ConsecutivoCodigo',N'U') IS NULL
                    THROW 51000, 'Verificacion posterior de columnas incompleta.', 1;
                IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.TBL_Llanta') AND name IN(N'SSerial',N'NProfundidadInicial') AND is_nullable=1)<>2
                    OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.TBL_AsignacionLlantaPosicion') AND name=N'DFechaInicio' AND is_nullable=1)
                    THROW 51000, 'Verificacion posterior de nulabilidad fallida.', 1;
                IF (SELECT COUNT(*) FROM sys.indexes WHERE name IN(N'UX_Marca_SAbreviatura',N'UX_Llanta_SIdentificadorOriginal',N'IX_Llanta_SSerial') AND object_id IN(OBJECT_ID(N'dbo.TBL_Marca'),OBJECT_ID(N'dbo.TBL_Llanta')) AND is_unique=1 AND has_filter=1 AND is_disabled=0)<>3
                    THROW 51000, 'Verificacion posterior de indices fallida.', 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("No hay reversión destructiva automática. Restaurar respaldo aprobado; no sustituir datos desconocidos por ceros o fechas ficticias.");
        }
    }
}
