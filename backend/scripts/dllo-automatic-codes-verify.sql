-- SOLO LECTURA. Ejecutar despues de la migracion, ANTES de arrancar el backend.
IF DB_NAME()<>N'GDLLSQLDLLO' THROW 51000, 'Destino no autorizado.',1;
IF ISNULL(COL_LENGTH(N'dbo.TBL_Marca',N'SAbreviatura'),-1)<>4
    OR ISNULL(COL_LENGTH(N'dbo.TBL_Llanta',N'SIdentificadorOriginal'),-1)<>200
    OR OBJECT_ID(N'dbo.TBL_ConsecutivoCodigo',N'U') IS NULL
    THROW 51000, 'Verificacion posterior de columnas incompleta.', 1;
IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.TBL_Llanta') AND name IN(N'SSerial',N'NProfundidadInicial') AND is_nullable=1)<>2
    OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.TBL_AsignacionLlantaPosicion') AND name=N'DFechaInicio' AND is_nullable=1)
    THROW 51000, 'Verificacion posterior de nulabilidad fallida.', 1;
IF (SELECT COUNT(*) FROM sys.indexes WHERE name IN(N'UX_Marca_SAbreviatura',N'UX_Llanta_SIdentificadorOriginal',N'IX_Llanta_SSerial') AND object_id IN(OBJECT_ID(N'dbo.TBL_Marca'),OBJECT_ID(N'dbo.TBL_Llanta')) AND is_unique=1 AND has_filter=1 AND is_disabled=0)<>3
    THROW 51000, 'Verificacion posterior de indices fallida.', 1;
IF (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)<>27 OR NOT EXISTS(SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId=N'20261022151207_AddAutomaticCodesAndImportedMounts') THROW 51000,'Historial posterior inesperado.',1;
SELECT MigrationId,ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
SELECT t.name AS Tabla,SUM(p.rows) AS Filas FROM sys.tables t JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN(0,1) WHERE t.name IN(N'TBL_Marca',N'TBL_Referencia',N'TBL_Dimension',N'TBL_TipoLlanta',N'TBL_Llanta',N'TBL_AsignacionLlantaPosicion') GROUP BY t.name;
SELECT GId,SCodigo,SNombre,SAbreviatura FROM dbo.TBL_Marca WHERE BActivo=1 ORDER BY SNombre;
-- Abreviaturas NULL en marcas existentes son esperadas: configurarlas en la interfaz antes de altas de llantas.
