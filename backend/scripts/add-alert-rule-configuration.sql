-- Aplicación manual de 20260911160835_AddAlertRuleConfiguration.
-- Ejecutar conectado a GDLLSQLDLLO. No ejecuta migraciones EF ni modifica su historial.
-- Conserva valores existentes; los defaults solo inicializan columnas nuevas.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'GDLLSQLDLLO'
    THROW 51000, N'Este script requiere la base GDLLSQLDLLO.', 1;
IF @@TRANCOUNT <> 0
    THROW 51000, N'Ejecute el script sin una transacción externa.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lockResult int;
    EXEC @lockResult = sys.sp_getapplock
        @Resource = N'20260911160835_AddAlertRuleConfiguration',
        @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000;
    IF @lockResult < 0
        THROW 51000, N'No fue posible bloquear la preparación de alertas. Reintente.', 1;

    IF OBJECT_ID(N'dbo.TBL_ParametroAlerta', N'U') IS NULL
        OR OBJECT_ID(N'dbo.TBL_Centro', N'U') IS NULL
        OR OBJECT_ID(N'dbo.TBL_Permiso', N'U') IS NULL
        OR OBJECT_ID(N'dbo.TBL_Rol', N'U') IS NULL
        OR OBJECT_ID(N'dbo.TBL_RolPermiso', N'U') IS NULL
        THROW 51000, N'Faltan tablas requeridas. No se ha recreado ninguna tabla.', 1;

    -- Validar el esquema prefijado de seguridad antes de insertar.
    IF EXISTS (
        SELECT 1 FROM (VALUES
            (N'TBL_Permiso', N'GId'), (N'TBL_Permiso', N'SCodigo'),
            (N'TBL_Permiso', N'SNombre'), (N'TBL_Permiso', N'DFechaCreacion'),
            (N'TBL_Permiso', N'SUsuarioCreacion'), (N'TBL_Permiso', N'BActivo'),
            (N'TBL_Rol', N'GId'), (N'TBL_Rol', N'SCodigo'),
            (N'TBL_RolPermiso', N'GRolId'), (N'TBL_RolPermiso', N'GPermisoId')
        ) AS requiredColumns(TableName, ColumnName)
        WHERE COL_LENGTH(N'dbo.' + requiredColumns.TableName, requiredColumns.ColumnName) IS NULL
    ) THROW 51000, N'El esquema de seguridad no coincide con el esquema prefijado esperado.', 1;

    -- Elegir la PK real de una sola columna, incluso si existen Id y GId.
    DECLARE @centerKey sysname;
    SELECT @centerKey = c.name
    FROM sys.indexes AS i
    JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID(N'dbo.TBL_Centro') AND i.is_primary_key = 1
        AND ic.key_ordinal = 1 AND c.name IN (N'GId', N'Id')
        AND c.system_type_id = TYPE_ID(N'uniqueidentifier')
        AND NOT EXISTS (SELECT 1 FROM sys.index_columns AS extra
            WHERE extra.object_id = i.object_id AND extra.index_id = i.index_id AND extra.key_ordinal > 1);
    IF @centerKey IS NULL
        THROW 51000, N'TBL_Centro debe tener una PK uniqueidentifier simple llamada GId o Id.', 1;

    -- Una columna existente incompatible provoca rollback; nunca se cambia su definición ni contenido.
    IF EXISTS (
        SELECT 1 FROM (VALUES
            (N'GCentroId', N'uniqueidentifier', 16, 1),
            (N'SDescripcion', N'nvarchar', 2000, 0),
            (N'SNombre', N'nvarchar', 300, 0),
            (N'SOperador', N'nvarchar', 4, 0),
            (N'SPrioridad', N'nvarchar', 20, 0),
            (N'STipo', N'nvarchar', 100, 0)
        ) AS expected(ColumnName, TypeName, MaxBytes, Nullable)
        JOIN sys.columns AS c ON c.object_id = OBJECT_ID(N'dbo.TBL_ParametroAlerta') AND c.name = expected.ColumnName
        WHERE c.system_type_id <> TYPE_ID(expected.TypeName) OR c.max_length <> expected.MaxBytes
            OR c.is_nullable <> expected.Nullable OR c.is_computed = 1
    ) THROW 51000, N'Una columna de parametrización existente es incompatible con la migración.', 1;

    IF COL_LENGTH(N'dbo.TBL_ParametroAlerta', N'GCentroId') IS NULL
        ALTER TABLE dbo.TBL_ParametroAlerta ADD GCentroId uniqueidentifier NULL;
    IF COL_LENGTH(N'dbo.TBL_ParametroAlerta', N'SDescripcion') IS NULL
        ALTER TABLE dbo.TBL_ParametroAlerta ADD SDescripcion nvarchar(1000) NOT NULL DEFAULT (N'');
    IF COL_LENGTH(N'dbo.TBL_ParametroAlerta', N'SNombre') IS NULL
        ALTER TABLE dbo.TBL_ParametroAlerta ADD SNombre nvarchar(150) NOT NULL DEFAULT (N'Diferencia entre mediciones');
    IF COL_LENGTH(N'dbo.TBL_ParametroAlerta', N'SOperador') IS NULL
        ALTER TABLE dbo.TBL_ParametroAlerta ADD SOperador nvarchar(2) NOT NULL DEFAULT (N'>=');
    IF COL_LENGTH(N'dbo.TBL_ParametroAlerta', N'SPrioridad') IS NULL
        ALTER TABLE dbo.TBL_ParametroAlerta ADD SPrioridad nvarchar(10) NOT NULL DEFAULT (N'Media');
    IF COL_LENGTH(N'dbo.TBL_ParametroAlerta', N'STipo') IS NULL
        ALTER TABLE dbo.TBL_ParametroAlerta ADD STipo nvarchar(50) NOT NULL DEFAULT (N'DIFERENCIA_HOMBROS');

    -- SQL dinámico: resolver la nueva columna después de ALTER TABLE en este mismo lote.
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TBL_ParametroAlerta')
        AND name = N'IX_TBL_ParametroAlerta_GCentroId')
        EXEC sys.sp_executesql N'CREATE INDEX IX_TBL_ParametroAlerta_GCentroId ON dbo.TBL_ParametroAlerta(GCentroId);';

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.TBL_ParametroAlerta')
        AND name = N'FK_TBL_ParametroAlerta_TBL_Centro_GCentroId')
    BEGIN
        DECLARE @fkSql nvarchar(max) = N'ALTER TABLE dbo.TBL_ParametroAlerta WITH CHECK ADD CONSTRAINT '
            + N'FK_TBL_ParametroAlerta_TBL_Centro_GCentroId FOREIGN KEY (GCentroId) REFERENCES dbo.TBL_Centro('
            + QUOTENAME(@centerKey) + N');';
        EXEC sys.sp_executesql @fkSql;
    END;

    -- Solo inserciones faltantes: no se alteran permisos ni asignaciones existentes.
    IF NOT EXISTS (SELECT 1 FROM dbo.TBL_Permiso WITH (UPDLOCK, HOLDLOCK) WHERE SCodigo = N'alertas.parametrizar')
        INSERT dbo.TBL_Permiso (GId, SCodigo, SNombre, DFechaCreacion, SUsuarioCreacion, BActivo)
        VALUES (NEWID(), N'alertas.parametrizar', N'Parametrizar alertas', SYSDATETIMEOFFSET(), N'migration-alert-rules', 1);

    INSERT dbo.TBL_RolPermiso (GRolId, GPermisoId)
    SELECT r.GId, p.GId
    FROM dbo.TBL_Rol AS r
    CROSS JOIN dbo.TBL_Permiso AS p
    WHERE r.SCodigo IN (N'ADMINISTRADOR', N'SUPERVISOR_ADMINISTRADOR')
        AND p.SCodigo = N'alertas.parametrizar'
        AND NOT EXISTS (SELECT 1 FROM dbo.TBL_RolPermiso AS rp WITH (UPDLOCK, HOLDLOCK)
            WHERE rp.GRolId = r.GId AND rp.GPermisoId = p.GId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
