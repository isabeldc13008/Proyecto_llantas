import {CommonModule} from '@angular/common';
import {Component,Input,OnChanges,OnDestroy,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {AnalyticsApi,AnalyticsPage,AnalyticsWear,FleetWear,WearGroup,WearIntervention} from './analytics-api';
import {MaintenanceFilters,maintenanceDefaults} from './maintenance-models';
import {TireDepthChart} from './tire-depth-chart';

@Component({selector:'app-analytics-wear-page',imports:[CommonModule,FormsModule,RouterLink,TireDepthChart],
 templateUrl:'./analytics-wear-page.html',styleUrls:['./analytics-overview.scss','./analytics-wear-page.scss']})
export class AnalyticsWearPage implements OnChanges,OnDestroy{
 @Input() filters:MaintenanceFilters=maintenanceDefaults();
 private api=inject(AnalyticsApi);readonly router=inject(Router);
 private version=0;private detailVersion=0;private historyVersion=0;
 group='marca';page=1;tirePage=1;metric:'profundidadMedia'|'kmMedio'='profundidadMedia';
 data=signal<FleetWear|null>(null);loading=signal(false);error=signal('');
 selected=signal('');wear=signal<AnalyticsWear|null>(null);detailLoading=signal(false);detailError=signal('');
 history=signal<AnalyticsPage<WearIntervention>|null>(null);historyLoading=signal(false);historyError=signal('');
 ngOnChanges(){this.page=1;this.tirePage=1;this.clearSelection();void this.load();}
 ngOnDestroy(){this.version++;this.detailVersion++;this.historyVersion++;}
 clearSelection(){this.detailVersion++;this.historyVersion++;this.selected.set('');this.wear.set(null);this.history.set(null);this.detailError.set('');this.historyError.set('');this.detailLoading.set(false);this.historyLoading.set(false);}
 async load(){const v=++this.version;this.loading.set(true);this.error.set('');this.data.set(null);
  try{const d=await firstValueFrom(this.api.fleetWear(this.filters,this.group,this.page,this.tirePage));if(v===this.version)this.data.set(d);}
  catch(e){if(v===this.version)this.error.set(this.message(e));}finally{if(v===this.version)this.loading.set(false);}}
 async regroup(){this.page=1;await this.load();}
 async changePage(page:number){this.page=page;await this.load();}
 async changeTirePage(page:number){this.tirePage=page;await this.load();}
 display(n:number|null|undefined){return n==null?'Sin datos suficientes':n.toLocaleString('es-CO',{maximumFractionDigits:3});}
 coverage(n:number,total:number){return total>0?`${this.display(100*n/total)}%`:'Sin datos suficientes';}
 width(g:WearGroup){const n=g[this.metric];const max=Math.max(0,...(this.data()?.grupos.items??[]).map(x=>x[this.metric]??0));return n==null||max===0?0:100*n/max;}
 async select(id:string){this.clearSelection();this.selected.set(id);const v=++this.detailVersion;this.detailLoading.set(true);void this.loadHistory();
  try{const d=await firstValueFrom(this.api.wear(id));if(v===this.detailVersion)this.wear.set(d);}
  catch(e){if(v===this.detailVersion)this.detailError.set(this.message(e));}finally{if(v===this.detailVersion)this.detailLoading.set(false);}}
 async loadHistory(page=1){const id=this.selected();if(!id)return;const v=++this.historyVersion;this.historyLoading.set(true);this.historyError.set('');this.history.set(null);
  try{const d=await firstValueFrom(this.api.interventions(id,page));if(v===this.historyVersion)this.history.set(d);}
  catch(e){if(v===this.historyVersion)this.historyError.set(this.message(e));}finally{if(v===this.historyVersion)this.historyLoading.set(false);}}
 validRates(){return this.wear()?.tasas?.filter(t=>t.mmPorMilKm!==null).length??0;}
 readingDate(id:string){return this.wear()?.mediciones.find(m=>m.lectura.id===id)?.lectura.fecha;}
 private message(e:unknown){const x=e as {userMessage?:string;error?:{message?:string};status?:number};return x?.userMessage??x?.error?.message??(x?.status===403?'Sin permiso para consultar estos datos.':'No se pudo consultar la evidencia. Reintenta.');}
}
