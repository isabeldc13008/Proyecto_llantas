-- SOLO GDLLSQLDLLO. No ejecutado en SQL Server por el agente.
-- Ejecutar TODO el archivo, sin seleccionar fragmentos. Backend y otros escritores detenidos.
-- Cuerpo DDL generado por EF; envoltorio de ejecucion atomica para SSMS.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51000,'La sesion debe comenzar sin transaccion abierta.',1;
BEGIN TRY
    BEGIN TRANSACTION;
    -- Fase 1; lote compilado despues de la fase anterior.
    EXEC sys.sp_executesql N'IF DB_NAME() <> N''GDLLSQLDLLO'' THROW 51000, ''Migracion exclusiva de Desarrollo GDLLSQLDLLO.'', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId=N''20261006162816_AddDisposicionSistemaVerde'')
    THROW 51000, ''Falta checkpoint inicial aprobado.'', 1;
IF (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>26
    THROW 51000, ''Se esperaban las 26 migraciones declaradas de DLLO; revisar antes de ejecutar.'', 1;
IF EXISTS(SELECT MigrationId FROM dbo.__EFMigrationsHistory EXCEPT SELECT v.Id FROM (VALUES
    (N''20260806031028_InitialCatalogosLlantas''),
    (N''20260812034221_AddInspectionWorkflow''),
    (N''20260812121402_AddOperationsAndActivities''),
    (N''20260818181414_AddPersistedSecurity''),
    (N''20260818184029_AddRegionalToCenters''),
    (N''20260818202852_AddMultiCenterSecurity''),
    (N''20260818211402_AddVehicleConfigurations''),
    (N''20260818213949_AddTireLifecycleMileage''),
    (N''20260818214721_SeedTireLifecycleStates''),
    (N''20260818215854_AddInspectionAlertsAndEvidenceMetadata''),
    (N''20260818222817_AddSchedulingPhase5''),
    (N''20260818232905_AddOperationalWorkflowsPhase6To8''),
    (N''20260825225140_AddInspectionFoundTireTraceability''),
    (N''20260826030341_AddSchedulingOriginAndIdempotency''),
    (N''20260826034111_AddConsolidatedRepairShipments''),
    (N''20260903172716_SincronizarModeloPostobon''),
    (N''20260904234601_AddEntraIdentity''),
    (N''20260907205344_AlignExistingAzureSchema''),
    (N''20260907211928_AlignExistingAzureSecuritySchema''),
    (N''20260907212835_AlignExistingAzureSchema_AllEntities''),
    (N''20260908124228_AlignExistingAzureSchema_AllEntities_V2''),
    (N''20260910190000_AddDebeCambiarClave''),
    (N''20260911160835_AddAlertRuleConfiguration''),
    (N''20260918131215_AddScheduledMountSets''),
    (N''20260923182748_AddDisposalShipments''),
    (N''20261006162816_AddDisposicionSistemaVerde'')
)v(Id)) THROW 51000, ''El historial no coincide con los 26 IDs aprobados.'', 1;
IF SCHEMA_NAME()<>N''dbo'' THROW 51000, ''Se requiere esquema predeterminado dbo.'', 1;
IF EXISTS(SELECT 1 FROM sys.columns WHERE default_object_id<>0 AND
    ((object_id=OBJECT_ID(N''dbo.TBL_Llanta'') AND name IN(N''SSerial'',N''NProfundidadInicial''))
    OR (object_id=OBJECT_ID(N''dbo.TBL_AsignacionLlantaPosicion'') AND name=N''DFechaInicio'')))
    THROW 51000, ''Hay defaults no presentes en el esquema revisado. Detener.'', 1;
IF ISNULL(COL_LENGTH(N''dbo.TBL_Marca'',N''SCodigo''),-1)<>60 OR ISNULL(COL_LENGTH(N''dbo.TBL_Llanta'',N''SSerial''),-1)<>200
    THROW 51000, ''Longitud fisica inesperada.'', 1;
IF OBJECT_ID(N''dbo.TBL_ConsecutivoCodigo'',N''U'') IS NOT NULL
    OR COL_LENGTH(N''dbo.TBL_Marca'',N''SAbreviatura'') IS NOT NULL
    OR COL_LENGTH(N''dbo.TBL_Llanta'',N''SIdentificadorOriginal'') IS NOT NULL
    OR COL_LENGTH(N''dbo.TBL_Llanta'',N''SDatosImportacionJson'') IS NOT NULL
    THROW 51000, ''Existen objetos de una aplicacion parcial. No continuar.'', 1;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N''dbo.TBL_Llanta'') AND name=N''IX_Llanta_SSerial'' AND is_unique=1 AND is_disabled=0 AND has_filter=0)
    THROW 51000, ''Indice de serial no coincide con la base revisada.'', 1;
IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.TBL_Llanta'') AND name IN(N''SSerial'',N''NProfundidadInicial'') AND is_nullable=1)
    OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.TBL_AsignacionLlantaPosicion'') AND name=N''DFechaInicio'' AND is_nullable=1)
    THROW 51000, ''Nulabilidad previa inesperada. Revisar aplicacion parcial.'', 1;

';
    -- Fase 2; lote compilado despues de la fase anterior.
    EXEC sys.sp_executesql N'DROP INDEX [IX_Llanta_SSerial] ON [dbo].[TBL_Llanta];

ALTER TABLE [dbo].[TBL_Marca] ADD [SAbreviatura] nvarchar(2) NULL;

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N''[dbo].[TBL_Llanta]'') AND [c].[name] = N''SSerial'');
IF @var IS NOT NULL EXEC(N''ALTER TABLE [dbo].[TBL_Llanta] DROP CONSTRAINT '' + @var + '';'');
ALTER TABLE [dbo].[TBL_Llanta] ALTER COLUMN [SSerial] nvarchar(100) NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N''[dbo].[TBL_Llanta]'') AND [c].[name] = N''NProfundidadInicial'');
IF @var1 IS NOT NULL EXEC(N''ALTER TABLE [dbo].[TBL_Llanta] DROP CONSTRAINT '' + @var1 + '';'');
ALTER TABLE [dbo].[TBL_Llanta] ALTER COLUMN [NProfundidadInicial] decimal(8,2) NULL;

ALTER TABLE [dbo].[TBL_Llanta] ADD [SDatosImportacionJson] nvarchar(max) NULL;

ALTER TABLE [dbo].[TBL_Llanta] ADD [SIdentificadorOriginal] nvarchar(100) NULL;

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N''[dbo].[TBL_AsignacionLlantaPosicion]'') AND [c].[name] = N''DFechaInicio'');
IF @var2 IS NOT NULL EXEC(N''ALTER TABLE [dbo].[TBL_AsignacionLlantaPosicion] DROP CONSTRAINT '' + @var2 + '';'');
ALTER TABLE [dbo].[TBL_AsignacionLlantaPosicion] ALTER COLUMN [DFechaInicio] datetimeoffset NULL;

CREATE TABLE [dbo].[TBL_ConsecutivoCodigo] (
    [SAmbito] nvarchar(80) NOT NULL,
    [NUltimo] bigint NOT NULL,
    CONSTRAINT [PK_TBL_ConsecutivoCodigo] PRIMARY KEY ([SAmbito])
);

';
    -- Fase 3; lote compilado despues de la fase anterior.
    EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [UX_Marca_SAbreviatura] ON [dbo].[TBL_Marca] ([SAbreviatura]) WHERE [SAbreviatura] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Llanta_SSerial] ON [dbo].[TBL_Llanta] ([SSerial]) WHERE [SSerial] IS NOT NULL;

CREATE UNIQUE INDEX [UX_Llanta_SIdentificadorOriginal] ON [dbo].[TBL_Llanta] ([SIdentificadorOriginal]) WHERE [SIdentificadorOriginal] IS NOT NULL;

';
    -- Fase 4; lote compilado despues de la fase anterior.
    EXEC sys.sp_executesql N'IF ISNULL(COL_LENGTH(N''dbo.TBL_Marca'',N''SAbreviatura''),-1)<>4
    OR ISNULL(COL_LENGTH(N''dbo.TBL_Llanta'',N''SIdentificadorOriginal''),-1)<>200
    OR OBJECT_ID(N''dbo.TBL_ConsecutivoCodigo'',N''U'') IS NULL
    THROW 51000, ''Verificacion posterior de columnas incompleta.'', 1;
IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.TBL_Llanta'') AND name IN(N''SSerial'',N''NProfundidadInicial'') AND is_nullable=1)<>2
    OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.TBL_AsignacionLlantaPosicion'') AND name=N''DFechaInicio'' AND is_nullable=1)
    THROW 51000, ''Verificacion posterior de nulabilidad fallida.'', 1;
IF (SELECT COUNT(*) FROM sys.indexes WHERE name IN(N''UX_Marca_SAbreviatura'',N''UX_Llanta_SIdentificadorOriginal'',N''IX_Llanta_SSerial'') AND object_id IN(OBJECT_ID(N''dbo.TBL_Marca''),OBJECT_ID(N''dbo.TBL_Llanta'')) AND is_unique=1 AND has_filter=1 AND is_disabled=0)<>3
    THROW 51000, ''Verificacion posterior de indices fallida.'', 1;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N''20261022151207_AddAutomaticCodesAndImportedMounts'', N''10.0.7'');';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
