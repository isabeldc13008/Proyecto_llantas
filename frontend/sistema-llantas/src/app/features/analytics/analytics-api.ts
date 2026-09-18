import {HttpClient,HttpParams} from '@angular/common/http';
import {inject,Injectable} from '@angular/core';

export interface AnalyticsFilters {centroId:string;marcaId:string;referenciaId:string;dimensionId:string;estadoId:string;tipoVehiculo:string;ingresoDesde:string;ingresoHasta:string;minimoMuestra:number}
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
export type AnalyticsView='resumen'|'vida-util'|'marcas-referencias'|'posiciones'|'movimientos'|'centros';

@Injectable({providedIn:'root'})
export class AnalyticsApi {
 private http=inject(HttpClient);
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
