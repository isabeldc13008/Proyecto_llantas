# Profundidad de inspecciones hasta 25 mm

## Intención

Permitir que el técnico registre en Inspecciones profundidades de labrado de hasta 25 mm para las lecturas exterior, centro e interior.

## Alcance

- Mantener los selectores actuales y sus incrementos de 0,5 mm.
- Ampliar las opciones de 0–15 mm a 0–25 mm.
- Actualizar la validación del servidor para aceptar valores entre 0 y 25 mm, inclusive.
- Agregar pruebas que cubran el nuevo máximo y el rechazo de valores superiores.

## Fuera de alcance

- No cambiar la profundidad inicial de la ficha de la llanta.
- No cambiar umbrales de alertas, cálculos analíticos ni reglas de desgaste.
- No cambiar el formato ni la precisión almacenada en la base de datos.

## Enfoques considerados

1. **Selectores hasta 25 mm en pasos de 0,5 (seleccionado):** conserva la interacción actual y evita errores de digitación.
2. **Campos numéricos libres:** permitiría más precisión, pero cambia la experiencia y aumenta el riesgo de valores inválidos.

## Criterios de aceptación

- Los tres selectores muestran `25,0` como valor máximo.
- Una inspección con una lectura de 25 mm puede guardarse.
- Una lectura superior a 25 mm o inferior a 0 sigue siendo rechazada por el servidor.
- Las pruebas existentes continúan aprobando.
