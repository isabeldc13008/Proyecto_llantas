# Contexto y alcance

## Propósito

El Sistema de Gestión de Llantas centraliza, controla y optimiza la administración del ciclo de vida de las llantas. La solución busca eliminar la dispersión de información y mejorar la trazabilidad de inventarios, inspecciones, mantenimientos, montajes, rotaciones, reparaciones, reencauches y disposición final.

## Objetivo general

Implementar un sistema integral que centralice la información, garantice la trazabilidad completa del ciclo de vida y mejore la toma de decisiones operativas, contribuyendo a la eficiencia y a la reducción de costos en un periodo máximo de ocho meses.

## Objetivos específicos

- Consolidar y validar el inventario inicial de aproximadamente 20.000 llantas, además de vehículos, técnicos, centros y parámetros.
- Registrar inspecciones por llanta y posición, incluyendo profundidades exterior, centro e interior, observaciones y evidencias.
- Generar alertas por desgaste, condiciones críticas, diferencias de profundidad y necesidad de reencauche.
- Registrar el 100% de los movimientos relevantes con fecha, usuario, motivo y tipo de operación.
- Dar seguimiento al estado de las llantas por vehículo, centro, posición e inspección.
- Controlar stock, reencauche, reparaciones y disposición final.
- Integrar analítica operativa con indicadores de desgaste, vida útil, rotación y estado por centro.
- Validar el uso con usuarios finales durante un piloto de tres a cinco días antes de producción.

## Alcance funcional

### Gestión general

Administración de llantas, vehículos, técnicos, centros, marcas, referencias, dimensiones, estados, parámetros, usuarios, roles y permisos. Incluye búsqueda, filtros y carga masiva del inventario.

### Inspección

Registro de inspecciones por vehículo, posición y llanta; profundidades; observaciones; banda de reencauche; evidencias; historial; identificación de inconsistencias y visualización del estado de la llanta.

### Alertas

Identificación de desgaste fuera de rango, llantas no inspeccionadas, restricciones de instalación y otras condiciones operativas. Las reglas deben ser configurables y visibles para el usuario.

### Programación

Consulta por placa, centro, estado y fecha; control de inspecciones vencidas o próximas; actividades de mantenimiento; responsables y seguimiento.

### Montaje y movimientos

Montaje, desmontaje, rotación, reparación, traslado y operaciones no programadas. Cada operación debe conservar el historial, motivo, observaciones, kilometraje y usuario responsable.

### Inventario y servicios

Control de stock por centro, envío a reencauche, recepción, reparación, disposición final, destino sugerido, proveedor y observaciones.

### Analítica

Dashboards y reportes con filtros por centro, fecha, marca y tipo de movimiento. Los indicadores deben apoyar el seguimiento diario y la toma de decisiones operativas y gerenciales.

## Usuarios

| Perfil | Uso principal |
|---|---|
| Técnico | Registrar inspecciones y movimientos autorizados |
| Supervisor | Controlar programación, operación y seguimiento |
| Líder de taller | Gestionar inventario, reencauche y disposición |
| Administrador | Configurar catálogos, parámetros, usuarios, roles y permisos |

## Límites del alcance

El sistema no reemplaza las fuentes corporativas externas ni define por sí solo los criterios técnicos de seguridad de la llanta. Esos criterios deben mantenerse como parámetros aprobados por el negocio y documentarse cuando cambien.
