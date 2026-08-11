# Arquitectura y alcance de la primera entrega

## Capas

- **Domain**: entidades y reglas invariantes sin dependencias de infraestructura.
- **Application**: contratos, DTO y modelos de paginación.
- **Infrastructure**: EF Core, SQL Server, configuración e implementación de casos de uso.
- **Api**: endpoints, JWT, políticas, CORS y manejo global de excepciones.
- **Angular**: shell, dashboard y módulo de llantas cargado de forma diferida.

## Seguridad

La API valida JWT y permisos del lado servidor. `llantas.consultar`, `llantas.administrar` y `catalogos.administrar` son los permisos iniciales. El claim `centro_id` restringe consultas de llantas. No hay usuarios o contraseñas de demostración en el repositorio.

## Persistencia

Las tablas siguen `TBL_<Nombre>`. Se usan índices únicos para código y serial, restricciones de clave foránea, `decimal` para valores numéricos relevantes, `rowversion` para concurrencia, filtros de baja lógica y una tabla de auditoría. La migración es incremental y no ejecuta cambios automáticamente al arrancar.

## Siguiente fase

Completar usuarios, roles, permisos y emisión de tokens; luego vehículos/posiciones, inventario operacional y movimientos transaccionales.
