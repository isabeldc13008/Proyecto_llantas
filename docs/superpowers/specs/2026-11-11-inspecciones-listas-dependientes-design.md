# Listas dependientes de novedades en Inspecciones

## Intención

Hacer que Condición, Causa y Recomendación formen una cadena dependiente para que el técnico solo pueda elegir combinaciones parametrizadas y pueda ajustar las reglas sin cambiar código.

## Objetivos

- Mantener los catálogos actuales y agregar relaciones mediante tablas puente.
- Filtrar Causas por Condición y Recomendaciones por Causa sin recargar la pantalla.
- Validar las combinaciones en backend antes de guardar.
- Mantener compatibilidad con inspecciones históricas y no cambiar inventario, montajes ni disposición.
- Entregar migraciones EF Core sin ejecutarlas.

## No objetivos

- No crear una pantalla administrativa nueva para parametrizar relaciones en esta entrega.
- No eliminar ni renombrar catálogos existentes.
- No asignar automáticamente una disposición final.

## Enfoque elegido

Se crearán las tablas `TBL_CondicionCausa` y `TBL_CausaRecomendacion`, con claves foráneas a los catálogos existentes y restricciones únicas. Los endpoints de opciones devolverán condiciones y relaciones; el backend validará que cada combinación activa exista. La interfaz Angular conservará los valores guardados, limpiará dependencias al cambiar una selección y tratará `SIN_NOVEDAD` como condición sin causa.

Las relaciones iniciales se sembrarán en la migración mediante códigos estables. Las recomendaciones existentes se conservarán; las nuevas se agregarán solo si no existen.

## Restricciones

- Rama aislada `codex/inspecciones-dependientes` basada en `develop`.
- No ejecutar migraciones, desplegar QA ni fusionar automáticamente.
- No incluir cambios ajenos de QA, documentación o analítica.
- Pruebas focalizadas para relaciones, validación, limpieza de selección y carga de valores existentes.

## Resultado esperado

La pantalla deshabilita Causa y Recomendación sin selección previa, filtra ambas listas dinámicamente, permite observaciones siempre y conserva el comportamiento de guardar/finalizar inspecciones.
