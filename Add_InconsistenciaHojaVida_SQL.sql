/* Ejecutar manualmente en la base de datos del ambiente correspondiente.
   No se ejecuta desde la aplicación ni desde este commit. */
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'SFichaSerial') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD SFichaSerial nvarchar(100) NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'GFichaMarcaId') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD GFichaMarcaId uniqueidentifier NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'GFichaReferenciaId') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD GFichaReferenciaId uniqueidentifier NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'GFichaDimensionId') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD GFichaDimensionId uniqueidentifier NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'BFichaEsReencauchada') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD BFichaEsReencauchada bit NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'GFichaMarcaReencaucheId') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD GFichaMarcaReencaucheId uniqueidentifier NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'NFichaVida') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD NFichaVida int NULL;
IF COL_LENGTH('dbo.TBL_InconsistenciaInspeccion', 'DFichaProfundidadInicial') IS NULL
    ALTER TABLE dbo.TBL_InconsistenciaInspeccion ADD DFichaProfundidadInicial decimal(18,2) NULL;

-- Validación opcional previa:
-- SELECT TOP (1) * FROM dbo.TBL_InconsistenciaInspeccion ORDER BY DFechaCreacion DESC;
