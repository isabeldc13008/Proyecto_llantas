import {CommonModule} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {Component,Input,Output,EventEmitter,OnChanges,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';
import {VehicleDetail,VehiclePosition} from '../vehicles/vehicles-api';
import {DiagramAxle,VehicleAxleDiagram} from '../../shared/vehicle-axle-diagram';
export interface MountAssignment{posicionId:string;llantaId:string;llantaActualId:string|null;codigo?:string|null;serial?:string|null;marcaReferencia?:string|null;dimension?:string|null}
export interface AvailableTire{id:string;codigo:string;serial:string;marca:string;referencia:string;dimension:string;estado:string;centro:string}
@Component({selector:'app-mount-assignment',imports:[CommonModule,FormsModule,VehicleAxleDiagram],template:`
<section class="assignment-layout" aria-label="Asignación de llantas por posición">
 <div><app-vehicle-axle-diagram [axles]="axles()" [vehicleType]="vehicle?.tipo??'Vehículo'" [selectedId]="selectedId" (positionSelected)="selectedId=''+$event.id"/>
 <p>Selecciona una posición para ver la llanta actual y su reemplazo.</p>
 <div class="position-list">@for(p of positions();track p.id){<button type="button" [class.selected]="selectedId===p.id" (click)="selectedId=p.id"><b>{{p.codigo}}</b> {{p.llantaCodigo??'Libre'}} → {{assigned(p.id)?.codigo??(assigned(p.id)?assigned(p.id)!.llantaId:'Mantener')}}</button>}</div></div>
 <div class="assignment-panel">@if(position();as p){
  <h3>{{p.codigo}} · {{p.lado}} {{p.ubicacion}}</h3>
  <article class="current"><small>ACTUALMENTE INSTALADA</small><b>{{p.llantaCodigo??'Posición libre'}}</b><span>{{p.llantaSerial??'Sin serial'}}</span><span>{{p.marcaReferencia??'—'}}</span><span>{{p.estadoLlanta??'Sin llanta'}} · {{p.kilometrajeLlanta===null?'Sin kilometraje':(p.kilometrajeLlanta|number)+' km'}}</span></article>
  <div class="arrow" aria-hidden="true">↓</div>
  @if(assigned(p.id);as a){<article class="assigned"><small>LLANTA ASIGNADA</small><b>{{a.codigo??a.llantaId}}</b><span>{{a.serial}}</span><span>{{a.marcaReferencia}} · {{a.dimension}}</span>@if(!readonly){<button type="button" (click)="keep(p.id)">Mantener posición sin cambio</button>}</article>}@else{<p class="unchanged">Mantener {{p.llantaCodigo??'posición libre'}}</p>}
  @if(!readonly){<label>Buscar llanta disponible<input [(ngModel)]="search" [ngModelOptions]="{standalone:true}" placeholder="Código, serial, marca, referencia o dimensión" (keyup.enter)="find()"></label><button type="button" (click)="find()" [disabled]="loading()">Buscar disponibles</button><small>Hasta 50 resultados del centro. Las llantas comprometidas no se ofrecen.</small>
   <div class="tire-results">@for(t of options();track t.id){<button type="button" (click)="choose(p,t)"><b>{{t.codigo}}</b><span>Serial {{t.serial}}</span><span>{{t.marca}} · {{t.referencia}} · {{t.dimension}}</span><small>Disponible · {{t.centro}}</small></button>}@empty{<p>{{loading()?'Consultando…':'No hay disponibles para esta búsqueda.'}}</p>}</div>
  }
 }@else{<p>Selecciona una posición del vehículo.</p>}
 @if(error()){<p role="alert" class="warning">{{error()}}</p>}</div>
</section>
<p class="assignment-summary" role="status">{{removed()}} llantas serán desmontadas · {{value.length}} serán montadas · {{positions().length-value.length}} posiciones permanecerán iguales. Las retiradas regresan a inventario.</p>
`,styleUrl:'./mount-assignment.scss'})
export class MountAssignmentEditor implements OnChanges{
 private http=inject(HttpClient);@Input() vehicle:VehicleDetail|null=null;@Input() value:MountAssignment[]=[];@Input() readonly=false;@Input() individual=false;@Output() valueChange=new EventEmitter<MountAssignment[]>();
 selectedId='';search='';available=signal<AvailableTire[]>([]);error=signal('');loading=signal(false);private version=0;private lastVehicle='';
 ngOnChanges(){const id=this.vehicle?.id??'';if(id!==this.lastVehicle){this.lastVehicle=id;this.selectedId=this.positions()[0]?.id??'';this.available.set([]);this.search='';++this.version;if(id&&!this.readonly)void this.find();}}
 positions(){return this.vehicle?.ejes.flatMap(e=>e.posiciones)??[];}position(){return this.positions().find(p=>p.id===this.selectedId);}assigned(id:string){return this.value.find(v=>v.posicionId===id);}removed(){return this.value.filter(v=>v.llantaActualId).length;}
 axles():DiagramAxle[]{return this.vehicle?.ejes.map(e=>({id:e.id,name:e.nombre,type:e.tipoEje,positions:e.posiciones.map(p=>({id:p.id,code:p.codigo,side:p.lado+' '+p.ubicacion,tire:p.llantaCodigo??'LIBRE',state:p.llantaId?'normal':'empty'}))}))??[];}
 options(){return this.available().filter(t=>!this.value.some(v=>v.llantaId===t.id));}
 async find(){if(!this.vehicle||this.readonly)return;const version=++this.version;this.loading.set(true);this.error.set('');try{const rows=await firstValueFrom(this.http.get<AvailableTire[]>('/api/operaciones/llantas-disponibles',{params:{vehiculoId:this.vehicle.id,buscar:this.search}}));if(version===this.version)this.available.set(rows);}catch(e:any){if(version===this.version){this.available.set([]);this.error.set(e?.userMessage??'No se pudieron consultar las llantas.');}}finally{if(version===this.version)this.loading.set(false);}}
 choose(p:VehiclePosition,t:AvailableTire){if(this.readonly||this.value.some(v=>v.llantaId===t.id))return;const row:MountAssignment={posicionId:p.id,llantaId:t.id,llantaActualId:p.llantaId,codigo:t.codigo,serial:t.serial,marcaReferencia:t.marca+' · '+t.referencia,dimension:t.dimension};this.value=this.individual?[row]:[...this.value.filter(v=>v.posicionId!==p.id),row];this.valueChange.emit(this.value);}
 keep(id:string){if(this.readonly)return;this.value=this.value.filter(v=>v.posicionId!==id);this.valueChange.emit(this.value);}
}
