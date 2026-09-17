# Convenciones de base de datos

Estas convenciones se derivan del lineamiento corporativo para objetos SQL Server y deben aplicarse a los nuevos objetos del sistema.

## Bases de datos

- On premises: `BDS_<NombreBaseDeDatos>`.
- SQL Azure: abreviatura de aplicación + proyecto Postobón + motor + uso + ambiente, en mayúsculas sostenidas. Ejemplo: `CCVPSQLSEGADLLO`.

## Objetos

| Objeto | Convención | Ejemplo |
|---|---|---|
| Tabla | `TBL_<NombreTabla>` | `TBL_LoteEncabezado` |
| Tabla temporal | `TBL_TMP_<NombreTabla>` | `TBL_TMP_Importacion` |
| Nodo Graph | `GPH_<NombreTabla>` | `GPH_Vehiculo` |
| Edge Graph | `EDG_<NombreTabla>` | `EDG_Movimiento` |
| Función | `FN_<NombreFuncion>` | `FN_CalcularPorcentaje` |
| Procedimiento | `USP_<NombreProcedimiento>` | `USP_ValidarUsuario` |
| Vista | `VW_<NombreVista>` | `VW_LlantasDisponibles` |
| Trigger | `TRG_<NombreTrigger>` | `TRG_ActualizarHistorial` |
| Paquete | `PKG_<NombrePaquete>` | `PKG_CargarMaestras` |
| Job | `JOB_<NombreJob>` | `JOB_ActualizarMaestras` |
| Índice | `IX_<Tabla>_<Campo>` | `IX_LoteDetalle_CodigoCliente` |

## Campos y claves

- Texto: prefijo `S`.
- Fecha: prefijo `D`.
- Número o decimal: prefijo `N`.
- Booleano: prefijo `B`.
- Timestamp: prefijo `T`.
- GUID: prefijo `G`.
- Clave primaria: `PK_<Tabla>` o `PK_<Campo>`.
- Clave foránea: `FK_<TablaForanea>`.

## Documentación T-SQL

Todo procedimiento, función o trigger debe incluir sistema, compañía, fecha de creación, desarrollador, descripción, bitácora de cambios, control de cambio y ejemplo de ejecución. Las excepciones y cambios de reglas deben quedar registrados en el historial del proyecto.
