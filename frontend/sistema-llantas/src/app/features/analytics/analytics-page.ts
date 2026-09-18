import {CommonModule} from '@angular/common';
import {Component,OnInit,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';
import {AnalyticsApi,AnalyticsFilters,AnalyticsGroup,AnalyticsOptions,AnalyticsPage,AnalyticsPosition,AnalyticsRanking,AnalyticsSummary,AnalyticsView,KmStats} from './analytics-api';
import {TireMetric} from './tire-metric';

const defaults=():AnalyticsFilters=>({centroId:'',marcaId:'',referenciaId:'',dimensionId:'',estadoId:'',tipoVehiculo:'',ingresoDesde:'',ingresoHasta:'',minimoMuestra:5});
@Component({selector:'app-analytics-page',imports:[CommonModule,FormsModule,TireMetric],templateUrl:'./analytics-page.html',styleUrl:'./analytics-page.scss'})
export class AnalyticsPageComponent implements OnInit {
 private api=inject(AnalyticsApi);
 readonly tabs:{id:AnalyticsView;label:string;subtitle:string}[]=[
  {id:'resumen',label:'Resumen',subtitle:'Una lectura ejecutiva de la cohorte seleccionada'},
  {id:'vida-util',label:'Vida útil',subtitle:'Recorrido observado, con límites y tamaño de muestra visibles'},
  {id:'marcas-referencias',label:'Marcas / referencias',subtitle:'Comparaciones descriptivas, sin promesas de rendimiento'},
  {id:'posiciones',label:'Posiciones',subtitle:'Cada posición en el contexto de su configuración y eje'},
  {id:'movimientos',label:'Movimientos',subtitle:'Intensidad de uso y trazabilidad por llanta'},
  {id:'centros',label:'Centros',subtitle:'Comparación por centro actual de la llanta'}
 ];
 view=signal<AnalyticsView>('resumen');loading=signal(false);error=signal('');optionsError=signal('');
 options=signal<AnalyticsOptions|null>(null);summary=signal<AnalyticsSummary|null>(null);
 groups=signal<AnalyticsPage<AnalyticsGroup>|null>(null);positions=signal<AnalyticsPage<AnalyticsPosition>|null>(null);movements=signal<AnalyticsRanking|null>(null);
 filters=defaults();applied=defaults();group='marca';cohort:'enOperacion'|'finalizadas'='enOperacion';
 showFilters=false;showMethod=false;page=signal(1);private requestVersion=0;
 async ngOnInit(){await Promise.allSettled([this.loadOptions(),this.load()]);}
 async loadOptions(){this.optionsError.set('');try{this.options.set(await firstValueFrom(this.api.options()));}catch(e:any){this.optionsError.set(e?.userMessage??'No fue posible cargar las opciones de filtro.');}}
 async selectView(view:AnalyticsView){this.view.set(view);this.group=view==='marcas-referencias'?'marca-referencia':'marca';await this.load(1);}
 async apply(){
  if(this.filters.ingresoDesde&&this.filters.ingresoHasta&&this.filters.ingresoDesde>this.filters.ingresoHasta){this.error.set('La fecha inicial no puede ser posterior a la final.');return;}
  if(!Number.isInteger(this.filters.minimoMuestra)||this.filters.minimoMuestra<1||this.filters.minimoMuestra>1000){this.error.set('La muestra mínima debe ser un entero entre 1 y 1.000.');return;}
  this.applied={...this.filters};this.showFilters=false;await this.load(1);
 }
 async clear(){this.filters=defaults();await this.apply();}
 async load(page=1){
  const version=++this.requestVersion;this.loading.set(true);this.error.set('');
  this.summary.set(null);this.groups.set(null);this.positions.set(null);this.movements.set(null);
  try{
   const view=this.view();
   if(view==='resumen'){const data=await firstValueFrom(this.api.summary(this.applied));if(version===this.requestVersion)this.summary.set(data);}
   else if(view==='posiciones'){const data=await firstValueFrom(this.api.positions(this.applied,page));if(version===this.requestVersion)this.positions.set(data);}
   else if(view==='movimientos'){const data=await firstValueFrom(this.api.movements(this.applied,page));if(version===this.requestVersion)this.movements.set(data);}
   else{const data=await firstValueFrom(this.api.groups(view,this.applied,page,this.group));if(version===this.requestVersion)this.groups.set(data);}
   if(version===this.requestVersion)this.page.set(page);
  }catch(e:any){if(version===this.requestVersion)this.error.set(e?.userMessage??e?.error?.message??'No fue posible consultar Analítica. Intenta nuevamente.');}
  finally{if(version===this.requestVersion)this.loading.set(false);}
 }
 stats(group:AnalyticsGroup):KmStats{return group[this.cohort];}
 sufficient(group:AnalyticsGroup){return this.stats(group).muestra>=this.applied.minimoMuestra;}
 maxGroup(){return Math.max(1,...(this.groups()?.items??[]).filter(g=>this.sufficient(g)).map(g=>this.stats(g).promedio??0));}
 bar(group:AnalyticsGroup){return this.sufficient(group)?100*(this.stats(group).promedio??0)/this.maxGroup():0;}
 maxPosition(){return Math.max(1,...(this.positions()?.items??[]).filter(p=>p.muestraSuficiente).map(p=>p.kmPromedio??0));}
 heat(position:AnalyticsPosition){return position.muestraSuficiente&&position.kmPromedio!==null ? .08+.4*position.kmPromedio/this.maxPosition() : 0;}
 totalPages(){return this.groups()?.totalPages??this.positions()?.totalPages??this.movements()?.ranking.totalPages??0;}
 totalItems(){return this.groups()?.totalItems??this.positions()?.totalItems??this.movements()?.ranking.totalItems??0;}
 maxCount(values:{cantidad:number}[]){return Math.max(1,...values.map(x=>x.cantidad));}
 subtitle(){return this.tabs.find(x=>x.id===this.view())?.subtitle;}
 filterCount(){return Object.entries(this.applied).filter(([k,v])=>k!=='minimoMuestra'&&v!=='').length;}
}
