# Proyecto Sistema de Gestión de Llantas

Este directorio concentra la información funcional, técnica, operativa y de seguimiento del Sistema de Gestión de Llantas de EDINSA. La documentación se construyó a partir de los levantamientos de requerimientos, el diseño técnico, los lineamientos de base de datos, los compromisos del proyecto y la estructura actual del código.

## Navegación

| Documento | Contenido |
|---|---|
| [01 Contexto y alcance](01-contexto-y-alcance.md) | Objetivo, alcance, usuarios y módulos |
| [02 Requerimientos funcionales](02-requerimientos-funcionales.md) | Requerimientos por módulo y criterios de aceptación |
| [03 Requerimientos no funcionales](03-requerimientos-no-funcionales.md) | Seguridad, rendimiento, disponibilidad, trazabilidad y móvil |
| [04 Diseño técnico](04-diseno-tecnico.md) | Arquitectura, componentes, tecnologías, datos y dependencias |
| [05 Convenciones de base de datos](05-convenciones-base-datos.md) | Estándares de nombramiento SQL Server y documentación T-SQL |
| [06 Compromisos y pendientes](06-compromisos-y-pendientes.md) | Responsables, estados, reglas y temas por validar |
| [07 Guía de operación y QA](07-guia-operacion-y-qa.md) | Perfiles, flujo operativo, pruebas y salida a producción |
| [08 Inventario de la implementación](08-inventario-implementacion.md) | Relación entre módulos documentados y código actual |
| [Diagramas](diagramas/) | Arquitectura actual y arquitectura esperada |

## Módulos del sistema

1. Gestión general, catálogos y parametrización.
2. Inspección de llantas.
3. Alertas y tolerancias.
4. Programación y mantenimiento.
5. Montaje y desmontaje.
6. Movimientos no programados.
7. Control de inventario, reencauche y disposición final.
8. Servicios y reparaciones.
9. Seguridad, roles, permisos y alcance por centros.
10. Analítica y dashboard operativo.

## Fuentes funcionales

- Levantamiento de requerimientos del Sistema de Gestión de Llantas.
- Diseño técnico detallado AE-038.
- App de compromisos y plan de trabajo.
- Lineamiento para nombrar objetos de base de datos.
- Diagramas de arquitectura actual y esperada.

## Estado documental

La documentación debe actualizarse junto con cada cambio relevante del backend, frontend, base de datos, permisos, reglas de negocio o flujo operativo. Los pendientes identificados están separados de las funcionalidades ya observables en el código para facilitar la validación con QA y negocio.
