import {HttpClient,HttpParams} from '@angular/common/http';
import {inject,Injectable} from '@angular/core';
import {MaintenanceDetail,MaintenanceFilters,MaintenanceQueueData,MaintenanceVehicle,ObservedAlert} from './maintenance-models';

export interface AnalyticsFilters {buscar?:string;centroId:string;marcaId:string;referenciaId:string;dimensionId:string;estadoId:string;tipoVehiculo:string;ingresoDesde:string;ingresoHasta:string;minimoMuestra:number}
export interface AnalyticsOption {id:string;nombre:string}
export interface AnalyticsOptions {centros:AnalyticsOption[];marcas:AnalyticsOption[];referencias:AnalyticsOption[];dimensiones:AnalyticsOption[];estados:AnalyticsOption[];tiposVehiculo:string[]}
export interface KmStats {muestra:number;promedio:number|null;mediana:number|null;desviacion:number|null;minimo:number|null;maximo:number|null}
export interface AnalyticsCount {nombre:string;cantidad:number}
export interface AnalyticsSummary {total:number;montadas:number;disponibles:number;enReparacion:number;enReencauche:number;disposicionFinal:number;conAlertas:number;sinKm:number;conTramosIncompletos:number;profundidadPromedio:number|null;muestraProfundidad:number;movimientosPromedio:number|null;enOperacion:KmStats;finalizadas:KmStats;estados:AnalyticsCount[]}
export interface AnalyticsGroup {id:string;nombre:string;total:number;enOperacionTotal:number;finalizadasTotal:number;enOperacion:KmStats;finalizadas:KmStats;reparacionesPromedio:number;reencauchesPromedio:number;alertas:number;movimientos:number;muestraSuficiente:boolean}
export interface AnalyticsPosition {configuracion:string;tipoVehiculo:string;eje:number;tipoEje:string;posicion:string;llantas:number;muestraKm:number;tramos:number;tramosValidos:number;kmPromedio:number|null;diasPromedio:number|null;muestraSuficiente:boolean}
export interface AnalyticsMovement {id:string;codigo:string;serial:string;marca:string;referencia:string;estado:string;total:number;vehiculos:number;centros:number;posiciones:number;tipos:AnalyticsCount[]}
export interface AnalyticsPage<T> {items:T[];pageNumber:number;pageSize:number;totalItems:number;totalPages:number}
export interface AnalyticsRanking {ranking:AnalyticsPage<AnalyticsMovement>;tipos:AnalyticsCount[]}
export type AnalyticsView='desgaste'|'resumen'|'vida-util'|'marcas-referencias'|'posiciones'|'movimientos'|'centros';

@Injectable({providedIn:'root'})
export class AnalyticsApi {
 private http=inject(HttpClient);
 maintenance(filters:MaintenanceFilters,page=1){return this.http.get<MaintenanceQueueData>('/api/analitica/mantenimiento',{params:this.maintenanceParams(filters).set('pagina',page).set('tamano',20)});}
 maintenanceDetail(id:string){return this.http.get<MaintenanceDetail>('/api/analitica/mantenimiento/llantas/'+encodeURIComponent(id));}
 maintenanceAlerts(id:string,page=1){return this.http.get<AnalyticsPage<ObservedAlert>>('/api/analitica/mantenimiento/llantas/'+encodeURIComponent(id)+'/alertas',{params:{pagina:page,tamano:20}});}
 maintenanceVehicles(buscar='',centroId='',page=1){return this.http.get<AnalyticsPage<MaintenanceVehicle>>('/api/analitica/mantenimiento/vehiculos',{params:this.maintenanceParams({buscar,centroId}).set('pagina',page).set('tamano',20)});}
 private maintenanceParams(filters:object){let params=new HttpParams();for(const[k,v]of Object.entries(filters))if(v!==''&&v!=null)params=params.set(k,v);return params;}
 tires(filters:AnalyticsFilters,page:number,indicator:string){return this.http.get<AnalyticsPage<AnalyticsTire>>('/api/analitica/llantas',{params:this.params(filters,page).set('indicador',indicator)});}
 wear(id:string){return this.http.get<AnalyticsWear>('/api/analitica/llantas/'+encodeURIComponent(id)+'/desgaste');}
 options(){return this.http.get<AnalyticsOptions>('/api/analitica/opciones');}
 params(filters:AnalyticsFilters,page=1,group='marca'){
  let params=new HttpParams().set('pagina',page).set('tamano',20).set('agrupar',group);
  for(const [key,value] of Object.entries(filters))if(value!==''&&value!=null)params=params.set(key,value);
  return params;
 }
 summary(filters:AnalyticsFilters){return this.http.get<AnalyticsSummary>('/api/analitica/resumen',{params:this.params(filters)});}
 groups(view:AnalyticsView,filters:AnalyticsFilters,page:number,group:string){return this.http.get<AnalyticsPage<AnalyticsGroup>>('/api/analitica/'+view,{params:this.params(filters,page,group)});}
 positions(filters:AnalyticsFilters,page:number){return this.http.get<AnalyticsPage<AnalyticsPosition>>('/api/analitica/posiciones',{params:this.params(filters,page)});}
 movements(filters:AnalyticsFilters,page:number){return this.http.get<AnalyticsRanking>('/api/analitica/movimientos',{params:this.params(filters,page)});}
}

export interface AnalyticsTire {id:string;codigo:string;serial:string;centro:string;estado:string;alertas:number;profundidad:number|null;km:number|null}
export interface AnalyticsMeasurement {lectura:{id:string;inspeccionId:string;fecha:string;posicionId:string;posicion:string;odometro:number|null;exterior:number|null;centro:number|null;interior:number|null};minima:number|null;tramoId:string|null;kmDesdeMontaje:number|null;calidad:string}
export interface AnalyticsWear {id:string;codigo:string;totalLecturas:number;lecturasCompletas:number;lecturasConTramo:number;mediciones:AnalyticsMeasurement[];reglas:{codigo:string;operador:string;valor:number;unidad:string;alcance:string}[];pronostico:string;limitaciones:string[]}
