import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ActivitiesApi, Activity } from './activities-api';

@Component({selector:'app-activities-page',template:`
<section class="page"><header><div><p class="eyebrow">Agenda del técnico</p><h1>Mis actividades</h1><p>Trabajo asignado desde la programación operativa.</p></div><span>{{pending()}} pendientes</span></header>
@if(loading()){<div class="notice">Cargando actividades desde el servidor…</div>}@else if(error()){<div class="notice error" role="alert">{{error()}} <button (click)="load()">Reintentar</button></div>}@else if(!activities().length){<div class="notice">No tienes actividades asignadas.</div>}@else{
<div class="activities">@for(a of activities();track a.id){<article [class.high]="a.prioridad==='Alta'"><div class="date"><b>{{time(a.fecha)}}</b><small>{{day(a.fecha)}}</small></div><div class="info"><span>{{a.tipo}}</span><h2>{{a.vehiculo}}</h2><p>{{a.centro}} · Prioridad {{a.prioridad}}</p></div><span class="status">{{a.estado}}</span>@if(a.estado==='EnEjecucion'){<button [disabled]="startingId()===a.id" (click)="complete(a)">MARCAR CUMPLIDA</button>}@else{<button [disabled]="startingId()===a.id||a.estado==='Cumplida'" (click)="start(a)">{{startingId()===a.id?'INICIANDO…':'INICIAR '+a.tipo.toUpperCase()}}</button>}</article>}</div>}
</section>`,styles:[`
header{display:flex;justify-content:space-between;align-items:end;margin-bottom:1.5rem}header p{color:#6b808b}header>span{background:#eaf4dd;color:#50791c;padding:.55rem .8rem;border-radius:99px;font-weight:800}.activities{display:grid;gap:.8rem}.activities article{display:grid;grid-template-columns:90px 1fr auto auto;align-items:center;gap:1rem;background:white;border:1px solid #dce7eb;border-left:5px solid #79ad38;border-radius:14px;padding:1rem}.activities article.high{border-left-color:#e15b35}.date{display:grid;text-align:center;border-right:1px solid #e2eaed}.date b{font-size:1.25rem;color:#0b4a78}.date small,.info p{color:#748791}.info span{font-size:.62rem;text-transform:uppercase;font-weight:900;color:#719d39}.info h2{margin:.15rem 0;font-size:1rem}.info p{margin:0;font-size:.7rem}.status{font-size:.65rem;background:#eef4f6;padding:.35rem .55rem;border-radius:99px}.activities button,.notice button{border:0;background:#0b4a78;color:white;padding:.7rem;border-radius:9px;font-weight:800}.activities button:disabled{opacity:.55}.notice{padding:2rem;text-align:center;background:#fff;border:1px solid #dce7eb;border-radius:14px;color:#657b86}.notice.error{color:#9d3d28}@media(max-width:700px){header{display:grid}.activities article{grid-template-columns:65px 1fr}.activities button{grid-column:1/-1}.status{justify-self:start}}
`]})
export class ActivitiesPage implements OnInit {
 private readonly api=inject(ActivitiesApi);private readonly router=inject(Router);
 activities=signal<Activity[]>([]);loading=signal(true);error=signal('');startingId=signal('');
 pending=computed(()=>this.activities().filter(a=>a.estado==='Pendiente'||a.estado==='Vencida').length);
 ngOnInit(){void this.load()}
 async load(){this.loading.set(true);this.error.set('');try{this.activities.set(await firstValueFrom(this.api.list()))}catch{this.error.set('No fue posible consultar tus actividades. Verifica la conexión con el servidor.')}finally{this.loading.set(false)}}
 async start(a:Activity){this.startingId.set(a.id);this.error.set('');try{const updated=await firstValueFrom(this.api.start(a.id));this.activities.update(all=>all.map(item=>item.id===updated.id?updated:item));await this.router.navigateByUrl(updated.rutaInicio)}catch{this.error.set('No fue posible iniciar la actividad.')}finally{this.startingId.set('')}}
 async complete(a:Activity){this.startingId.set(a.id);this.error.set('');try{const updated=await firstValueFrom(this.api.complete(a.id));this.activities.update(all=>all.map(item=>item.id===updated.id?updated:item))}catch{this.error.set('No fue posible completar la actividad.')}finally{this.startingId.set('')}}
 time(value:string){return new Intl.DateTimeFormat('es-CO',{hour:'2-digit',minute:'2-digit',hour12:false}).format(new Date(value))}
 day(value:string){return new Intl.DateTimeFormat('es-CO',{day:'2-digit',month:'short'}).format(new Date(value))}
}
