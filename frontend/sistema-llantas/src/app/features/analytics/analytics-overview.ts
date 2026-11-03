import {CommonModule} from '@angular/common';
import {Component, OnDestroy, OnInit, inject, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {Subscription, firstValueFrom} from 'rxjs';
import {AnalyticsApi, AnalyticsOptions, AnalyticsPage, QualityData} from './analytics-api';
import {MaintenanceFilters, MaintenanceVehicle, maintenanceDefaults} from './maintenance-models';
import {AnalyticsPageComponent} from './analytics-page';

type Section='resumen'|'calidad'|'mantenimiento';
const defaults=():MaintenanceFilters=>({...maintenanceDefaults(),montaje:'TODAS'});

@Component({selector:'app-analytics-overview',imports:[CommonModule,FormsModule,RouterLink,AnalyticsPageComponent],
 templateUrl:'./analytics-overview.html',styleUrl:'./analytics-overview.scss'})
export class AnalyticsOverview implements OnInit, OnDestroy{
 private api=inject(AnalyticsApi);private route=inject(ActivatedRoute);readonly router=inject(Router);
 private subscription?:Subscription;private version=0;private vehicleVersion=0;private lastQuery='';
 section=signal<Section>('resumen');filters=defaults();applied=defaults();page=signal(1);category='todas';
 data=signal<QualityData|null>(null);loading=signal(false);error=signal('');options=signal<AnalyticsOptions|null>(null);optionsError=signal('');
 vehicles=signal<AnalyticsPage<MaintenanceVehicle>|null>(null);vehicleSearch='';vehicleLoading=signal(false);vehicleError=signal('');
 ngOnInit(){void this.loadOptions();this.subscription=this.route.queryParamMap.subscribe(p=>{
  const raw=p.get('seccion');const section:Section=raw==='calidad'||raw==='mantenimiento'?raw:!raw&&(p.has('vista')||p.has('seleccion'))?'mantenimiento':'resumen';
  this.section.set(section);const next=defaults();for(const key of Object.keys(next) as (keyof MaintenanceFilters)[]){const v=p.get(key);if(v!==null)(next as unknown as Record<string,string>)[key]=v;}
  if(!['MONTADAS','TODAS'].includes(next.montaje))next.montaje='TODAS';
  // Legacy queue-only filters must not change the population of the executive/quality views.
  next.vista='TODAS';next.clasificacion='';
  const page=Number(p.get('pagina')??1);this.page.set(Number.isInteger(page)&&page>0&&page<=100000?page:1);
  const category=p.get('categoria')??'todas';this.category=['todas','sin-inspeccion','incompleta','completa'].includes(category)?category:'todas';
  const centerChanged=this.applied.centroId!==next.centroId;this.filters={...next};this.applied={...next};
  if(section==='mantenimiento'){this.version++;this.loading.set(false);this.lastQuery='';return;}
  const query=JSON.stringify([next,this.page(),this.category]);if(query!==this.lastQuery){this.lastQuery=query;void this.load();}
  if(centerChanged||!this.vehicles())void this.findVehicles();
 });}
 ngOnDestroy(){this.subscription?.unsubscribe();this.version++;this.vehicleVersion++;}
 async load(){const v=++this.version;this.loading.set(true);this.error.set('');this.data.set(null);
  try{const data=await firstValueFrom(this.api.quality(this.applied,this.page(),this.category));if(v===this.version)this.data.set(data);}
  catch(e:unknown){if(v===this.version)this.error.set(this.message(e,'No se pudieron consultar los indicadores.'));}
  finally{if(v===this.version)this.loading.set(false);}}
 async loadOptions(){this.optionsError.set('');try{this.options.set(await firstValueFrom(this.api.options()));}catch(e:unknown){this.optionsError.set(this.message(e,'No se pudieron cargar los filtros.'));}}
 async navigate(section:Section){
  // The existing maintenance view owns its live filters while mounted in this shell.
  const filters=this.section()==='mantenimiento'?{}:this.filters;
  await this.router.navigate([],{relativeTo:this.route,queryParams:{...filters,seccion:section,pagina:1,categoria:null,seleccion:null,vista:section==='mantenimiento'?'COLA':'TODAS',clasificacion:null},queryParamsHandling:'merge'});
 }
 async apply(){const changed=JSON.stringify(this.filters)!==JSON.stringify(this.applied)||this.page()!==1;
  await this.router.navigate([],{relativeTo:this.route,queryParams:{...this.filters,seccion:this.section(),pagina:1,seleccion:null},queryParamsHandling:'merge'});if(!changed)await this.load();}
 async clear(){this.filters=defaults();this.vehicleSearch='';await this.apply();}
 async centerChanged(){this.filters.vehiculoId='';this.vehicleSearch='';await this.findVehicles();}
 async findVehicles(page=1){const v=++this.vehicleVersion;this.vehicleLoading.set(true);this.vehicleError.set('');
  try{const data=await firstValueFrom(this.api.maintenanceVehicles(this.vehicleSearch,this.filters.centroId,page));if(v===this.vehicleVersion)this.vehicles.set(data);}
  catch(e:unknown){if(v===this.vehicleVersion)this.vehicleError.set(this.message(e,'No se pudieron buscar vehículos.'));}
  finally{if(v===this.vehicleVersion)this.vehicleLoading.set(false);}}
 async qualityCategory(category:string){await this.router.navigate([],{relativeTo:this.route,queryParams:{seccion:'calidad',categoria:category,pagina:1},queryParamsHandling:'merge'});}
 async changePage(page:number){await this.router.navigate([],{relativeTo:this.route,queryParams:{pagina:page},queryParamsHandling:'merge'});}
 percent(count:number,total:number):number|null{return total>0?100*count/total:null;}
 display(value:number|null|undefined){return value==null?'Sin datos suficientes':String(value);}
 private message(error:unknown,fallback:string){const e=error as {userMessage?:string;error?:{message?:string};status?:number};return e?.userMessage??e?.error?.message??(e?.status===403?'Sin permiso para consultar estos datos.':fallback);}
}
