# Inventario de la implementación

## Backend

| Área | Evidencia en el código |
|---|---|
| Autenticación | `AuthController`, configuración JWT y handler de desarrollo |
| Catálogos | `CatalogosController`, `CatalogoService` |
| Carga masiva | `CargaMasivaController` |
| Inspección | `InspeccionesController`, `InspeccionService` |
| Alertas | `ParametrosAlertaController`, reglas configurables y servicios de inspección |
| Inventario | `InventarioController`, `LlantaService`, `LlantasDisponibles` |
| Operaciones | `OperacionesController`, `OperacionService` |
| Programación | `ProgramacionController`, `ProgramacionService` |
| Dashboard | `DashboardController`, `DashboardService` |
| Servicios | `ServiciosLlantaController`, `CicloVidaLlantaService` |
| Seguridad | `RolesController`, `UsuariosController`, claims y alcance por centros |
| Auditoría | Entidad `Auditoria`, entidades auditables y migraciones de seguridad |

## Frontend

Las características principales están en `frontend/sistema-llantas/src/app/features`: autenticación, administración, alertas, autorizaciones, carga masiva, dashboard, inspección, inventario, llantas, movimientos, programación, servicios y vehículos.

## Base de datos

Las migraciones registran la evolución de catálogos, inspecciones, operaciones, seguridad, centros, configuraciones de vehículos, ciclo de vida, alertas, programación, trazabilidad, reparaciones, reencauche, identidad y reglas de alerta.

## Verificación

El repositorio declara pruebas para dominio, aplicación, integración y frontend. GitHub Actions ejecuta las suites y genera artefactos sin desplegar automáticamente.

## Documentación relacionada

- `docs/arquitectura.md`
- `docs/mvp-operacion.md`
- `Documentacion_funcional_GLLD_por_modulos.docx`
- `qa_documentacion/`
