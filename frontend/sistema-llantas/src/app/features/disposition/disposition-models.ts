export interface Ref{id:string;nombre:string}
export interface Page<T>{items:T[];pageNumber:number;pageSize:number;totalItems:number;totalPages:number}
export interface Filter{buscar?:string;centroId?:string;estado?:string;pageNumber?:number;pageSize?:number;centroIds?:string[];llantaIds?:string[];estados?:string[];conEvidencia?:boolean;desde?:string;hasta?:string;ordenarPor?:string;descendente?:boolean;soloPendientes?:boolean;llantaTexto?:string;centroTexto?:string;estadoTexto?:string}
export interface Order{evidencias?:number;ordenId:string;llantaId:string;codigo:string;serial:string;marca:string;referencia:string;dimension:string;centroOrigen:Ref;ubicacionRegistrada:Ref;estado:string;etiqueta:string;fechaPendiente:string|null;responsable:string|null;loteId:string|null;despachoId:string|null}
export interface Event{tipo:string;etiqueta:string;estadoVisual:string;fecha:string|null;responsable:string|null;observacion:string|null;referencia:string|null}
export interface Evidence{id:string;nombreArchivo:string;mimeType:string;tamanoBytes:number;fecha:string;responsable?:string}
export interface Detail{orden:Order;motivo:string;concepto:string|null;resultado:string|null;eventos:Event[];evidencias:Evidence[];accionesPermitidas:string[]}
export interface Summary{generadoEn:string;contadores:{clave:string;etiqueta:string;unidad:string;cantidad:number;ruta:string}[];atencion:{codigo:string;centro:string;motivo:string;fecha:string|null;ruta:string}[]}
export interface Novelty{id:string;ordenId:string|null;observacion:string;fecha:string;responsable:string;fechaResolucion:string|null;resueltaPor:string|null;resolucion:string|null}
export interface Lot{id:string;codigo:string;origen:Ref;destino:Ref;estado:string;fechaSalida:string;transportador:string|null;placa:string|null;remision:string|null;observaciones:string|null;items:Order[];novedades:Novelty[];enviadas:number;recibidas:number;pendientes:number;vistaParcial:boolean;puedeRecibir:boolean}
export interface ActaSnapshot{version:number;codigo:string;fechaEmision:string;fechaSalida:string;centroOrigen:string;centroR1:string;proveedor:string;transportador:string;placa:string;remision:string|null;llantas:{ordenId:string;codigo:string;serial:string;marca:string;dimension:string;loteEntrada:string}[]}
export interface Acta{id:string;codigo:string;centroOrigenId:string;snapshot:ActaSnapshot;soportes:Evidence[]}
export interface Dispatch{id:string;codigo:string;centroR1:Ref;proveedor:Ref;fechaSalida:string;transportador:string;placa:string;remision:string|null;estado:string;rowVersion:string;items:Order[];actas:Acta[];vistaParcial:boolean;puedeCerrar:boolean;motivosBloqueo:string[]}
export interface Provider{id:string;nombre:string;tipo:string}
export interface Center extends Ref{relevancia?:string|null;codigo:string}
export const orderStates=[['PENDIENTE_EVALUACION_TECNICA','Pendiente de evaluación'],['PENDIENTE_APROBACION','Pendiente de aprobación'],['LISTAS_R1','Listas para enviar a R1'],['EN_TRANSITO_DISPOSICION','En tránsito a R1'],['DISPONIBLES_R1','Disponibles en R1'],['DISPOSICION_FINAL','Disposición final cerrada'],['RECHAZADA','Rechazada'],['RETORNADA_INVENTARIO','Retornada a inventario']];
export function stageLabel(state:string){return Object.fromEntries([...orderStates,['APROBADA','Lista para enviar a R1'],['PENDIENTE_DISPOSICION','Recibida; pendiente de disposición'],['EN_TRANSITO','En tránsito'],['RECEPCION_PARCIAL','Recepción parcial'],['RECIBIDO','Recibido'],['EN_DISPOSICION','Disposición parcial'],['ENVIADO','Enviado a Sistema Verde'],['CERRADO','Cerrado']])[state]??state}
export function visibleSelection(selected:string[],visible:string[],checked:boolean){return checked?[...new Set([...selected,...visible])]:selected.filter(id=>!visible.includes(id))}
export function actaCount(rows:{centroOrigen:{id:string}}[]){return new Set(rows.map(x=>x.centroOrigen.id)).size}
export function remainingAfterReceipt(expected:number,previous:number,selected:number){return Math.max(0,expected-previous-selected)}
export function localDate(){const d=new Date();return new Date(d.getTime()-d.getTimezoneOffset()*60000).toISOString().slice(0,16)}
export function failure(e:any){return e?.userMessage??e?.error?.message??e?.message??'No fue posible completar la operación. Intenta nuevamente.'}
export function safeDispositionReturn(value:string|null,fallback='/disposicion-final/bandeja'){return value&&/^\/disposicion-final\/(bandeja|lotes|despachos)(?:\?|$)/.test(value)?value:fallback}

export interface EligibleTire{id:string;codigo:string;serial:string;centro:string;vehiculo:string|null;posicion:string|null}
export interface Facet{valor:string;etiqueta:string}


