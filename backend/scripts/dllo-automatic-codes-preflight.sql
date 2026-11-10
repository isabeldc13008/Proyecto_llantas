-- SOLO LECTURA. Ejecutar completo en GDLLSQLDLLO antes de la migracion.
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
SELECT DB_NAME() AS BaseValidada,COUNT(*) AS Migraciones FROM dbo.__EFMigrationsHistory;
SELECT t.name AS Tabla,SUM(p.rows) AS Filas FROM sys.tables t JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN(0,1) WHERE t.name IN(N'TBL_Marca',N'TBL_Referencia',N'TBL_Dimension',N'TBL_TipoLlanta',N'TBL_Llanta',N'TBL_AsignacionLlantaPosicion') GROUP BY t.name;
