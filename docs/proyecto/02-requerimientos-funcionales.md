# Requerimientos funcionales

## RF01 Gestión general

- Registrar y administrar llantas, vehículos, técnicos y centros.
- Administrar marcas, referencias, dimensiones, tipos de eje, posiciones, estados y parámetros.
- Cargar masivamente el inventario inicial y validar duplicados, campos obligatorios y relaciones.
- Configurar usuarios, roles, permisos y alcance de centros.
- Consultar inventario con filtros y búsqueda.

## RF02 Inspección

- Crear una inspección asociada a vehículo, centro, responsable y kilometraje.
- Cargar automáticamente la información de la llanta por identificador o búsqueda.
- Registrar profundidad exterior, centro e interior.
- Seleccionar observaciones predefinidas y registrar notas cuando aplique.
- Registrar banda de reencauche, marca, referencia, dimensión y estado.
- Adjuntar evidencias en formatos permitidos.
- Consultar historial de inspecciones.
- Permitir reportar inconsistencias de identificación y resolverlas mediante autorización.
- Advertir cuando no se inspeccionen todas las posiciones y exigir una razón.

## RF03 Alertas y tolerancias

- Configurar rangos de desgaste por aplicación o eje.
- Alertar cuando el desgaste mensual supere la tolerancia aprobada.
- Alertar por diferencias de profundidad entre hombros.
- Alertar por diferencias de profundidad entre llantas duales.
- Alertar restricciones de instalación, como llantas reencauchadas en tren delantero.
- Alertar llantas o posiciones pendientes de inspección.
- Mostrar la explicación y la acción recomendada para cada alerta.

## RF04 Programación

- Consultar vehículos por placa, equipo, centro y estado.
- Mostrar posiciones, llantas instaladas y estado operativo.
- Crear actividades de inspección o mantenimiento.
- Controlar actividades pendientes, vencidas, próximas, canceladas y ejecutadas.
- Restringir la información al alcance del usuario.

## RF05 Montaje y desmontaje

- Registrar llanta, posición, estado, profundidad y observaciones.
- Registrar destino de la llanta desmontada: inventario, reencauche, reparación o disposición final.
- Actualizar automáticamente el historial y la ubicación de la llanta.
- Impedir operaciones inválidas, como montar en una posición ocupada sin resolver la llanta desplazada.

## RF06 Movimientos no programados

- Registrar montaje, rotación, reparación, traslado y disposición.
- Registrar motivo, observaciones, kilometraje y técnico responsable.
- Asociar el movimiento a una inspección o solicitud cuando corresponda.
- Gestionar operaciones que requieren autorización.
- Mantener el estado de la solicitud: pendiente, aprobada, rechazada, ejecutada o recibida.

## RF07 Inventario, reencauche y disposición

- Consultar llantas disponibles por centro, estado, marca, referencia y búsqueda.
- Registrar destino sugerido y destino final.
- Registrar proveedor, centro de destino y observaciones.
- Controlar envíos y recepciones de reparación o reencauche.
- Evitar que una llanta salga de inventario sin trazabilidad del destino.

## RF08 Analítica

- Mostrar estado de llantas, desgaste, vida útil, movimientos y rotaciones.
- Filtrar por centro, fecha, marca, referencia y tipo de movimiento.
- Mostrar tendencias y alertas operativas.
- Garantizar consistencia entre la transacción operativa y el indicador.

## Criterios transversales de aceptación

- Toda operación debe identificar usuario, fecha, centro y entidad afectada.
- Las validaciones deben ejecutarse tanto en interfaz como en API.
- Las operaciones no autorizadas deben responder con un mensaje comprensible y no modificar datos.
- Los cambios de ciclo de vida deben poder consultarse en el historial.
- Las pantallas deben funcionar en resolución de escritorio y en dispositivos móviles de campo.
