import {CommonModule} from '@angular/common';
import {Component,OnInit,OnDestroy,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router} from '@angular/router';
import {firstValueFrom,Subscription} from 'rxjs';
import {AnalyticsApi,AnalyticsOptions,AnalyticsPage} from './analytics-api';
import {MaintenanceFilters,MaintenanceQueueData,MaintenanceVehicle,maintenanceDefaults,evaluationText,classificationLabels} from './maintenance-models';
import {MaintenanceQueue} from './maintenance-queue';
import {TireEvidencePanel} from './tire-evidence-panel';
@Component({selector:'app-analytics-page',imports:[CommonModule,FormsModule,MaintenanceQueue,TireEvidencePanel],templateUrl:'./analytics-page.html',styleUrl:'./analytics-page.scss'})
export class AnalyticsPageComponent implements OnInit,OnDestroy{
 private api=inject(AnalyticsApi);private route=inject(ActivatedRoute);private router=inject(Router);private version=0;private vehicleVersion=0;private routeSubscription?:Subscription;
 filters=maintenanceDefaults();applied=maintenanceDefaults();page=signal(1);selectedId=signal('');data=signal<MaintenanceQueueData|null>(null);loading=signal(false);error=signal('');optionsError=signal('');vehicleError=signal('');options=signal<AnalyticsOptions|null>(null);vehicles=signal<AnalyticsPage<MaintenanceVehicle>|null>(null);showFilters=false;vehicleSearch='';vehicleLoading=signal(false);
 evaluationText=evaluationText;labels=classificationLabels;
 ngOnInit(){void this.loadOptions();this.routeSubscription=this.route.queryParamMap.subscribe(p=>{
  const next=maintenanceDefaults();for(const key of Object.keys(next) as (keyof MaintenanceFilters)[]){const value=p.get(key);if(value!==null)(next as unknown as Record<string,string>)[key]=value;}
  if(!['MONTADAS','TODAS'].includes(next.montaje))next.montaje='MONTADAS';if(!['COLA','TODAS','FUERA_COLA'].includes(next.vista))next.vista='COLA';if(next.clasificacion!=='CONDICION_POR_VERIFICAR')next.clasificacion='';
  const parsed=Number(p.get('pagina')??1);const page=Number.isInteger(parsed)&&parsed>0&&parsed<=100000?parsed:1;
  const changed=JSON.stringify(next)!==JSON.stringify(this.applied)||page!==this.page()||this.data()===null;
  const centerChanged=next.centroId!==this.applied.centroId;this.filters={...next};this.applied={...next};this.page.set(page);this.selectedId.set(p.get('seleccion')??'');
  if(changed)void this.load(page);if(centerChanged||!this.vehicles())void this.findVehicles();
 });}
 ngOnDestroy(){this.routeSubscription?.unsubscribe();this.version++;this.vehicleVersion++;}
 async loadOptions(){this.optionsError.set('');try{this.options.set(await firstValueFrom(this.api.options()))}catch(e:any){this.optionsError.set(e?.userMessage??'No se pudieron cargar los filtros')}}
 async load(page=this.page()){const version=++this.version;this.loading.set(true);this.error.set('');try{const data=await firstValueFrom(this.api.maintenance(this.applied,page));if(version===this.version)this.data.set(data)}catch(e:any){if(version===this.version){this.data.set(null);this.error.set(e?.userMessage??e?.error?.message??'No fue posible consultar mantenimiento')}}finally{if(version===this.version)this.loading.set(false)}}
 async apply(){this.showFilters=false;const changed=JSON.stringify(this.filters)!==JSON.stringify(this.applied)||this.page()!==1||!!this.selectedId();await this.router.navigate([],{relativeTo:this.route,queryParams:{...this.filters,pagina:1,seleccion:null}});if(!changed)await this.load(1)}
 async setView(view:MaintenanceFilters['vista']){this.filters.vista=view;this.filters.clasificacion='';await this.apply()}
 async verifyOnly(){this.filters.vista='COLA';this.filters.clasificacion=this.filters.clasificacion?'':'CONDICION_POR_VERIFICAR';await this.apply()}
 select(id:string){void this.router.navigate([],{relativeTo:this.route,queryParams:{seleccion:id},queryParamsHandling:'merge'});}
 changePage(page:number){void this.router.navigate([],{relativeTo:this.route,queryParams:{pagina:page,seleccion:null},queryParamsHandling:'merge'});}
 async clear(){this.filters=maintenanceDefaults();this.vehicleSearch='';await this.apply()}
 async centerChanged(){this.filters.vehiculoId='';this.vehicleSearch='';await this.findVehicles()}
 async findVehicles(page=1){const v=++this.vehicleVersion;this.vehicleLoading.set(true);this.vehicleError.set('');try{const data=await firstValueFrom(this.api.maintenanceVehicles(this.vehicleSearch,this.filters.centroId,page));if(v===this.vehicleVersion)this.vehicles.set(data)}catch(e:any){if(v===this.vehicleVersion)this.vehicleError.set(e?.userMessage??'No se pudieron consultar los vehículos')}finally{if(v===this.vehicleVersion)this.vehicleLoading.set(false)}}
}
