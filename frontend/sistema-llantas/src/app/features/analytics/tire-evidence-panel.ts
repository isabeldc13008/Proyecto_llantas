import {CommonModule} from '@angular/common';
import {Component,Input,OnChanges,OnDestroy,inject,signal} from '@angular/core';
import {Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {AnalyticsApi,AnalyticsWear} from './analytics-api';
import {MaintenanceDetail,evaluationText,classificationLabels} from './maintenance-models';
import {MaintenanceNavigation} from './maintenance-navigation';
import {TireDepthChart} from './tire-depth-chart';
@Component({selector:'app-tire-evidence-panel',imports:[CommonModule,RouterLink,TireDepthChart],templateUrl:'./tire-evidence-panel.html',styleUrl:'./tire-evidence-panel.scss'})
export class TireEvidencePanel implements OnChanges,OnDestroy{
 @Input() tireId='';@Input() full=false;
 private api=inject(AnalyticsApi);readonly navigation=inject(MaintenanceNavigation);readonly router=inject(Router);
 detail=signal<MaintenanceDetail|null>(null);wear=signal<AnalyticsWear|null>(null);loading=signal(false);wearLoading=signal(false);error=signal('');wearError=signal('');alertsError=signal('');alertsLoading=signal(false);
 evaluationText=evaluationText;labels=classificationLabels;private version=0;private alertVersion=0;
 ngOnChanges(){void this.load()}ngOnDestroy(){this.version++;this.alertVersion++;}
 async load(){const v=++this.version;this.alertVersion++;this.alertsLoading.set(false);this.detail.set(null);this.wear.set(null);this.error.set('');this.wearError.set('');this.alertsError.set('');if(!this.tireId){this.loading.set(false);this.wearLoading.set(false);return}
  this.loading.set(true);this.wearLoading.set(true);
  await Promise.allSettled([
   firstValueFrom(this.api.maintenanceDetail(this.tireId)).then(x=>{if(v===this.version)this.detail.set(x)}).catch(e=>{if(v===this.version)this.error.set(e?.userMessage??e?.error?.message??'No fue posible consultar la evidencia')}).finally(()=>{if(v===this.version)this.loading.set(false)}),
   firstValueFrom(this.api.wear(this.tireId)).then(x=>{if(v===this.version)this.wear.set(x)}).catch(e=>{if(v===this.version)this.wearError.set(e?.userMessage??e?.error?.message??'No fue posible consultar el historial de profundidad')}).finally(()=>{if(v===this.version)this.wearLoading.set(false)})
  ]);
 }
 async alerts(page:number){const v=this.version,a=++this.alertVersion;this.alertsError.set('');this.alertsLoading.set(true);try{const data=await firstValueFrom(this.api.maintenanceAlerts(this.tireId,page));if(v===this.version&&a===this.alertVersion)this.detail.update(d=>d?{...d,alertas:data}:d)}catch(e:any){if(v===this.version&&a===this.alertVersion)this.alertsError.set(e?.userMessage??'No fue posible consultar alertas')}finally{if(v===this.version&&a===this.alertVersion)this.alertsLoading.set(false)}}
}
