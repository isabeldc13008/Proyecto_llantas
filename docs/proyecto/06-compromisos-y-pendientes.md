# Compromisos y pendientes

## Registro de compromisos

| Ítem | Compromiso | Módulo | Responsable | Estado reportado |
|---:|---|---|---|---|
| 1 | Definir rangos de tolerancia para tiempo de registro y desgaste mes a mes | Inspección | Maikol | Pendiente de confirmar |
| 2 | Definir alertas e información de identificación preexistente | Inspección | Maikol | Pendiente de confirmar |
| 3 | Cargar y validar inventario | General | Gisell | Realizado |
| 4 | Entregar información de equipos | General | Gisell | Pendiente de confirmar |
| 5 | Entregar base de datos de técnicos | General | Gisell | Realizado |
| 6 | Mostrar estado, marca, referencia y última inspección en el dibujo de llanta | Inspección | Isa | Pendiente de confirmar |
| 7 | Configurar parámetros | General | Maikol | Pendiente de confirmar |
| 9 | Listar observaciones de inspección | Inspección | Maikol | Pendiente de confirmar |
| 10 | Listar bandas para reencauche | Inspección | Maikol | Pendiente de confirmar |
| 11 | Listar observaciones de montaje | Montaje | Maikol | Pendiente de confirmar |

## Reglas de negocio suministradas

- Desgaste normal de referencia: entre 0 y 2 mm por mes.
- Alerta: desgaste mayor o igual a 3 mm, sujeto a la validación técnica final.
- La misma lógica aplica como referencia a ejes direccional, tracción y remolque.
- Diferencia de profundidad entre hombros mayor o igual a 3 mm: acción preventiva.
- Diferencia de profundidad entre llantas duales mayor o igual a 3 mm: revisar apareamiento.
- Toda llanta debe tener una descripción completa para su administración.
- Las llantas reencauchadas no deben instalarse en tren delantero, salvo que el negocio apruebe una excepción documentada.
- Si no se inspeccionan todas las llantas de un vehículo, se debe solicitar la razón antes de continuar.
- La profundidad debe manejar valores de 0 a 25 según la lista definida por negocio.
- Una llanta no aumenta su profundidad; incrementos anómalos deben generar revisión.

## Catálogos aportados

Las observaciones de inspección incluyen golpes, estallido, exposición de cinturones o cuerdas, desgarres, protuberancias, desgaste irregular, daños de rin, cortes y baja presión. Para desecho se contemplan daños estructurales, separaciones, perforaciones múltiples, deterioro de pestaña, problemas de carcasa, fallas de reparación y otras causas técnicas.

Las bandas de reencauche aportadas incluyen TS, TREVOLUTION, D250, DM2, AVTP, HT3, TM22, EXTR, WH, 943, 945, RRT1 y RZL115L, con sus proveedores, profundidades originales y aplicaciones. Deben validarse antes de usarse como catálogo definitivo.

## Pendientes de cierre

1. Confirmar valores finales de tolerancias y responsable de aprobación.
2. Confirmar inventario de equipos, centros y técnicos contra la base cargada.
3. Validar todas las listas de observaciones con líderes de taller.
4. Validar la regla de reencauche en tren delantero.
5. Ejecutar pruebas con datos reales de inspección, inventario y movimientos.
6. Alinear indicadores del dashboard con los responsables de negocio.
7. Confirmar el procedimiento de despliegue y los ambientes autorizados.
