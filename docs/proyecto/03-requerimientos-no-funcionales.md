# Requerimientos no funcionales

## Rendimiento y volumen

- Soportar un inventario superior a 20.000 llantas.
- Mantener tiempos de respuesta adecuados en búsquedas, filtros, registros y consultas históricas.
- Evitar cargas completas innecesarias y paginar consultas de alto volumen.
- Validar rendimiento con datos representativos y no únicamente con datos de prueba pequeños.

## Seguridad

- Autenticar mediante usuario y contraseña o identidad corporativa según el ambiente.
- Aplicar autorización por roles, permisos y alcance de centros.
- Validar permisos en el servidor, no solo en la interfaz.
- Proteger evidencias y restringir su consulta al alcance autorizado.
- No almacenar secretos ni contraseñas en el repositorio.
- Forzar cambio de contraseña cuando la configuración del ambiente lo requiera.

## Integridad y trazabilidad

- Mantener claves y relaciones consistentes.
- Registrar auditoría de altas, cambios, movimientos, autorizaciones y anulaciones.
- Conservar el historial de la llanta desde su ingreso hasta su disposición final.
- Usar operaciones transaccionales para cambios que involucren inventario y posición.
- Evitar duplicidad mediante reglas de idempotencia cuando aplique.

## Disponibilidad y mantenibilidad

- Separar frontend, API, aplicación, dominio e infraestructura.
- Mantener migraciones controladas y reproducibles.
- Ejecutar pruebas automatizadas en cada cambio relevante.
- Mantener documentación sincronizada con el código.
- Registrar errores con mensajes funcionales para el usuario y detalles técnicos en logs.

## Usabilidad

- Interfaz clara para operación en taller y campo.
- Búsqueda por placa, código, serial o identificador de llanta.
- Listas predefinidas para observaciones, profundidades y destinos.
- Confirmaciones visibles antes de operaciones que cambien inventario.
- Mensajes de validación concretos y accionables.
