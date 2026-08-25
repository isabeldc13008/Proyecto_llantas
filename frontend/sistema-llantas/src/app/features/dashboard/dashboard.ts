import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ActivitiesApi, Activity } from '../activities/activities-api';
import { DashboardApi, DashboardSummary } from './dashboard-api';
@Component({selector:'app-dashboard',imports:[RouterLink],templateUrl:'./dashboard.html',styleUrl:'./dashboard.scss'})
export class Dashboard implements OnInit{
 readonly auth=inject(AuthService);private readonly activitiesApi=inject(ActivitiesApi);private readonly dashboardApi=inject(DashboardApi);private readonly router=inject(Router);
 activities=signal<Activity[]>([]);summary=signal<DashboardSummary|null>(null);loading=signal(true);error=signal('');selectedCenter=signal('');
 pending=computed(()=>this.activities().filter(a=>a.estado==='Pendiente'||a.estado==='Vencida').length);running=computed(()=>this.activities().filter(a=>a.estado==='EnEjecucion'||a.estado==='En ejecución').length);urgent=computed(()=>this.activities().filter(a=>['Alta','Crítica','Critica'].includes(a.prioridad)&&a.estado!=='Cumplida').length);
 attentionCounts=computed(()=>({critical:this.summary()?.attention.filter(x=>x.prioridad==='CRITICA').length??0,high:this.summary()?.attention.filter(x=>x.prioridad==='ALTA').length??0,medium:this.summary()?.attention.filter(x=>x.prioridad==='MEDIA').length??0}));
 ngOnInit(){void this.load()} async load(){this.loading.set(true);this.error.set('');try{if(this.auth.user()?.role==='TECNICO')this.activities.set(await firstValueFrom(this.activitiesApi.list()));else this.summary.set(await firstValueFrom(this.dashboardApi.get(this.selectedCenter()||undefined)))}catch{this.error.set('No fue posible cargar el resumen operativo.')}finally{this.loading.set(false)}}
 changeCenter(event:Event){this.selectedCenter.set((event.target as HTMLSelectElement).value);void this.load()} firstName(){return this.auth.user()?.name.trim().split(/\s+/)[0]??''} greeting(){const h=new Date().getHours();return h<12?'Buenos días':h<18?'Buenas tardes':'Buenas noches'}
 date(){return new Intl.DateTimeFormat('es-CO',{weekday:'long',day:'numeric',month:'long',year:'numeric'}).format(new Date())} subtitle(){const u=this.auth.user();if(u?.role!=='SUPERVISOR')return 'Estado general de la operación de llantas';const centers=this.summary()?.centers??[];return centers.length===1?`Operación de ${centers[0].nombre}`:'Operación de tus centros asignados'}
 open(route:string){void this.router.navigateByUrl(route)} time(value:string){return new Intl.DateTimeFormat('es-CO',{hour:'2-digit',minute:'2-digit',hour12:false}).format(new Date(value))} shortDate(value:string){return new Intl.DateTimeFormat('es-CO',{day:'2-digit',month:'short'}).format(new Date(value))}
 distribution(value:number){const d=this.summary()?.tireDistribution;const total=d?d.montadas+d.disponibles+d.reparacion+d.reencauche+d.otros:0;return total?Math.max(2,value*100/total):0}
}
