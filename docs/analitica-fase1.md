# Analítica de Llantas — Fase 1

## Diagnóstico del modelo real

La revisión se realizó sobre entidades, DTO, consultas, configuraciones EF y servicios del repositorio. No se pudo certificar la cantidad ni calidad del historial productivo: el entorno local no logra iniciar LocalDB. La presencia de una columna no demuestra que esté completa ni que haya observaciones suficientes para predecir.

| Indicador | Datos disponibles | Fuente | Calculable hoy | Limitaciones |
|---|---|---|---|---|
| Existencias, montadas y disponibles | Estado, centro, asignación activa y posición actual | Llanta, EstadoLlanta, AsignacionLlantaPosicion, PosicionVehiculo; LlantasDisponibles.Consulta | Sí | Totales actuales, no snapshots históricos |
| Reparación/reencauche actuales | EN_REPARACION/REP, EN_REENCAUCHE/REE | Estados y equivalencias reales de OperacionService | Sí | Estado actual no equivale a orden terminada |
| Disposición final | EsDisposicionFinal | EstadoLlanta | Sí | Activo=false es desactivación administrativa, no retiro |
| Km por llanta | Odómetros de montaje/desmontaje y recorrido | AsignacionLlantaPosicion | Sí, recorrido observado | No necesariamente toda la vida; se excluyen tramos abiertos |
| Promedio, mediana, dispersión | Total verificable por llanta | Agregados de asignaciones | Sí | Separar disposición y operación; mostrar muestra |
| Marca/referencia/dimensión | Relaciones de catálogo | Marca, Referencia, Dimension | Sí | Muestras pequeñas no sustentan rankings de calidad |
| Posición/eje/configuración | Vehículo, configuración, eje, código/lado/ubicación | Asignaciones, Vehiculo, ConfiguracionVehiculo, EjeVehiculo, PosicionVehiculo | Sí | Configuración actual, sin snapshot por asignación |
| Centros | Centro actual de llanta y origen de eventos | Llanta, Movimiento, Inspeccion, OrdenServicioLlanta | Sí | No atribuye causalmente todo el recorrido |
| Movimientos y destinos distintos | ID, tipo, posiciones y centros | Movimiento, MovimientoDetalle | Sí | Deduplicar un movimiento repetido en detalles |
| Profundidad conocida | Lecturas exterior/centro/interior | Inspeccion, InspeccionDetalle | Sí | Última finalizada con tres lecturas no negativas; informar cobertura |
| Servicios completados | Tipo y estado de órdenes | OrdenServicioLlanta | Sí | CERRADA; no sumar solicitudes ni no reparables |
| Alertas abiertas | ABIERTA / EN_PROCESO | AlertaInspeccion | Sí | No equivale a falla, retiro o criticidad |
| Costos | Costo de compra y costo opcional de orden | Llanta, OrdenServicioLlanta | Parcial, diferido | Cobertura, moneda y comparabilidad no demostradas |
| Desgaste por km | Inspecciones y odómetros de vehículo | Inspecciones + asignaciones | Fase 2 | Vincular lecturas a llanta/tramo; cambios de vehículo y reencauches rompen series directas |
| Predicción | Mediciones y ParametroAlerta | Inspecciones, parámetros y tramos | Aún no | Verificar volumen, km crecientes, ciclos, reglas y unidades |
| Supervivencia | Recorridos parciales y estado actual | Varias entidades | No certificada | Validar origen, evento/fecha de retiro, historial completo y censura |

KilometrajeAcumulado se incrementa al cerrar asignaciones en OperacionService e InspeccionService. No se suma nuevamente con esas asignaciones: produciría doble conteo. Un cero por defecto en el maestro no demuestra kilometraje observado.

MovimientoLlanta representa regularizaciones de inspección; InspeccionService también registra su movimiento operativo. No se suman ambos registros como eventos independientes.

## Páginas y endpoints

Se sustituyó la pantalla pendiente de /analitica. Ya existían enlace de menú, guard y permiso modulos.analitica.consultar.

| Vista | GET |
|---|---|
| Opciones de filtros | /api/analitica/opciones |
| Resumen ejecutivo | /api/analitica/resumen |
| Vida útil observada | /api/analitica/vida-util |
| Marcas/referencias | /api/analitica/marcas-referencias |
| Centros | /api/analitica/centros |
| Posiciones | /api/analitica/posiciones |
| Llantas con más movimientos | /api/analitica/movimientos |

Todos requieren usuario autenticado + modulos.analitica.consultar. No se crean permisos, roles ni cambios de autenticación. Una política conecta el permiso existente con la API.

Filtros: centroId, marcaId, referenciaId, dimensionId, estadoId, tipoVehiculo, ingresoDesde, ingresoHasta, minimoMuestra. Paginación: pagina y tamano (1–100). Comparativos: agrupar=marca, referencia, marca-referencia, dimension o centro.

**Fechas:** cohorte por FechaIngreso, inclusive. No filtran fecha del movimiento ni calculan una vida dentro de un periodo. La interfaz lo señala. Eventos: historial autorizado de esa cohorte.

**Tipo de vehículo:** llantas con asignaciones autorizadas al tipo indicado; limita los tramos de kilometraje/posición a ese tipo. Servicios, movimientos y estado siguen describiendo la llanta de la cohorte. Una llanta pudo usarse en varios tipos.

## Fórmulas y datos faltantes

- Tramo cerrado válido: EsActiva=false; FechaFin presente y no anterior a FechaInicio; odómetro inicial >=0; final >= inicial; recorrido >=0 e igual a final-inicial.
- Recorrido por llanta: suma de tramos cerrados autorizados solo si todos son válidos y hay al menos uno. Con algún tramo incompleto/inconsistente, el total es null.
- No se incluyen tramos montados actuales ni se infiere vida completa.
- Se separan disposición final (EsDisposicionFinal) y todos los demás estados. Desactivación administrativa no significa retiro.
- Promedio y mediana: llantas con total verificable. Cero medido se conserva; faltantes y negativos no se imputan.
- Desviación poblacional: raíz de la media de desviaciones cuadráticas. No es un intervalo de confianza.
- Muestra mínima: 5 por defecto, configurable entre 1 y 1.000; preferencia de presentación, no regla de negocio ni garantía estadística. Se ocultan barras comparativas debajo del mínimo; el detalle conserva datos y n.
- No se ordena por el mayor promedio: se prioriza cobertura y se separan cohortes.
- Profundidad: mínimo de tres lecturas no negativas de la última inspección finalizada completa; promedio entre llantas con dato y muestra visible. ProfundidadInicial no se presenta como inspección actual.
- Reparaciones/reencauches promedio: órdenes activas CERRADA de cada tipo / todas las llantas del grupo, ambas cohortes. La UI identifica el denominador.
- Alertas: activas ABIERTA/EN_PROCESO; sin inventar criticidad.
- Posiciones: ID de configuración + tipo de vehículo + número/tipo de eje + código/lado/ubicación. Sin configuración se separa por vehículo.
- Heatmap: km medios por tramo cerrado y duración con fechas válidas. Intensidad = kilometraje, no desgaste. Mínimo basado en llantas distintas con km válidos; se muestran también tramos válidos/totales.
- Ranking: movimientos distintos por llanta; centros y posiciones origen/destino unidos sin duplicados; tipos reales de Movimiento.Tipo.
- Gráfico por tipo: movimientos distintos globales de la cohorte. No suma necesariamente igual al ranking si un movimiento afecta varias llantas.
- Visual tipo llanta: **cobertura de medición**, no vida consumida ni km restantes.

## Seguridad, rendimiento e índices

El alcance se aplica en IQueryable antes de proyectar/agrupar. Se autoriza el centro actual de la llanta y cada fuente histórica:
- Movimiento: centro de origen; en traslados entre centros ambos deben ser autorizados.
- Asignación: movimiento de origen autorizado y centro actual del vehículo autorizado.
- Inspección: centro propio autorizado.
- Orden: CentroOrigenId autorizado.
- Alerta: CentroId autorizado.

Se excluyen eventos fuera del alcance incluso si la llanta ahora está en un centro permitido. Puede reducir cobertura; no se rellenan lagunas. Las opciones se derivan de la cohorte autorizada.

AsNoTracking en todas las lecturas. Resumen/comparación: número fijo de consultas para proyección del maestro y agregados SQL de tramos, servicios, alertas y movimientos. Sin entidades completas ni historial de eventos cargado a memoria.

Mediana exacta y desviación sobre hasta 5.000 proyecciones por llanta. Si se supera el límite, se devuelve validación para acotar filtros. No se toma un subconjunto silencioso. Posiciones y ranking se paginan en SQL.

Índices revisados: Llanta(CentroId,EstadoLlantaId), relaciones de llanta/posición e índices filtrados de asignación activa; relaciones MovimientoDetalle(MovimientoId/LlantaId); InspeccionDetalle(InspeccionId,PosicionVehiculoId); AlertaInspeccion(CentroId,Estado,FechaCreacion); OrdenServicioLlanta(Tipo,Estado,CentroOrigenId). Se respeta el mapeo físico existente.

Requieren medir planes reales: agregado de tramos por llanta, búsqueda de movimiento de origen, última lectura y DISTINCT de posiciones/movimientos. Evaluar después de medir índices de cobertura en asignación(LlantaId,EsActiva) y detalle de inspección(LlantaId,InspeccionId), sin asumir que faltan equivalentes en producción.

Sin migraciones, índices nuevos, caché adicional, dependencias gráficas ni infraestructura ML.

## Riesgos y evolución

No se certificó continuidad/cantidad/calidad productiva. Configuraciones y centros actuales no son snapshots históricos. La disposición final no garantiza historial desde nueva. Cero servicios describe ausencia de registros autorizados, no garantiza ausencia de servicios reales. Datos desactivados/fuera de alcance reducen cobertura.

**Fase 2:** observaciones por ciclo/tramo, continuidad de odómetros, profundidad, desgaste/1.000 km, rotaciones, reencauche, anomalías y regresión interpretable. Auditar suficientes observaciones, km crecientes, ciclos y saltos de profundidad. Usar ParametroAlerta activo, PROFUNDIDAD_MINIMA, operador/unidad coherentes y centro aplicable; resolver conflictos de reglas. ParametroReencauche es elegibilidad, no retiro universal. Sin umbral compatible, exigir parámetro explícito. No se crearon endpoints predictivos vacíos ni scores arbitrarios.

**Fase 3:** supervivencia con censura y origen/evento fiables; después modelos avanzados y recomendaciones. Sin motor ML antes de demostrar valor.

## Pruebas

Estadísticas con cero/null/negativos, media/mediana/desviación/cobertura; validación de muestra/paginación/fechas/alcance; compilación SQL sin conexión para filtrado, agregación, límite, vacío, tracking y cantidad de consultas; integración SQL con cohortes, centros, tramos inconsistentes y deduplicación; política API; frontend con filtros, paginación, pestañas, carreras, errores, vacíos, muestras, heatmap y guard.

Las pruebas de traducción usan lectores sustituidos: certifican traducción de consultas, no resultados ni planes del motor SQL. El resultado de ejecución se entrega separado.
