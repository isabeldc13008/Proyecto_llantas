# Proyecto_llantas

Proyecto de gestión de llantas EDINSA.

## Estructura de vistas

La UI principal ahora está dividida por módulos de menú en archivos HTML separados dentro de `views/`:

- `views/dashboard.html`
- `views/vehicles.html`
- `views/inventory.html`
- `views/inspection.html`
- `views/mounting.html`
- `views/movements.html`
- `views/schedule.html`
- `views/alerts.html`

`index.html` conserva el layout base (sidebar + contenedores globales), y `app.js` carga dinámicamente el módulo según la opción seleccionada en el menú.
