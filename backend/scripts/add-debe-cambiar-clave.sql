-- Aplicar antes de iniciar la versión nueva en bases administradas sin migraciones.
-- No modifica usuarios existentes: todos conservan false.
-- Si luego se usan migraciones EF, Up detecta la columna existente y no la duplica.
IF COL_LENGTH(N'dbo.TBL_Usuario', N'BDebeCambiarClave') IS NULL
    ALTER TABLE dbo.TBL_Usuario ADD BDebeCambiarClave bit NOT NULL
        CONSTRAINT DF_TBL_Usuario_BDebeCambiarClave DEFAULT (0) WITH VALUES;
