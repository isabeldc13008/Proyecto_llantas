import {MountAssignment,MountAssignmentEditor} from './mount-assignment';
import {HttpClient} from '@angular/common/http';
import {CommonModule} from '@angular/common';
import {Component,OnInit,computed,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {AuthService} from '../../core/auth/auth.service';
import {DiagramAxle,DiagramPosition,VehicleAxleDiagram} from '../../shared/vehicle-axle-diagram';
import {VehicleDetail,VehicleSummary} from '../vehicles/vehicles-api';
interface RequestRow{id:string;tipo:string;estado:string;centro:string;llanta:string;motivo:string;centroDestinoId:string|null;fechaRecepcionDestino:string|null}
@Component({host:{class:'edinsa-workflow'},styleUrls:['../../shared/operational-workflow.scss'],selector:'app-movements-page',imports:[CommonModule,FormsModule,VehicleAxleDiagram,MountAssignmentEditor],template:`
<main class="ops"><header class="workflow-header"><div><p>EJECUCIÓN FÍSICA GUIADA</p><h1>Montajes</h1><span>Ejecuta sobre el vehículo; consulta la trazabilidad consolidada en Movimientos.</span></div></header>
@if(activityId){<section class="panel"><h2>{{planned()?.tipo??'Montaje programado'}}</h2><p>Ejecuta las llantas asignadas. No se permite sustituirlas.</p>@if(planned();as plan){<p>{{plan.motivo}}</p><p>{{plan.observaciones}}</p><app-mount-assignment [vehicle]="detail()" [value]="plan.asignaciones" [readonly]="true"/><p class="selection">Odómetro registrado: {{detail()?.kilometraje==null?'Sin registro':(detail()!.kilometraje|number)+' km'}}</p><label>Kilometraje actual<input type="number" [min]="detail()?.kilometraje??0" [(ngModel)]="mileage"></label><button class="primary" [disabled]="busy()||!detail()" (click)="executePlanned()">{{plan.tipo==='Cambio de juego'?'Confirmar cambio de juego':'Ejecutar montaje'}}</button>}@if(message()){<p role="status">{{message()}}</p>}</section>}@else{
<nav aria-label="Tipo de montaje"><button (click)="setMode(false)" [attr.aria-pressed]="!setModeActive">Operación individual</button><button (click)="setMode(true)" [attr.aria-pressed]="setModeActive">Cambio de juego</button></nav>
@if(setModeActive){<section class="panel"><h2>Cambio de juego</h2><p>Reemplaza varias o todas las llantas del vehículo en una sola operación.</p><label>Buscar vehículo<input [(ngModel)]="vehicleSearch" (keyup.enter)="searchVehicles()"><button (click)="searchVehicles()">Buscar</button></label><label>Vehículo<select [(ngModel)]="vehicleId" (change)="loadVehicle()">@for(v of vehicles();track v.id){<option [value]="v.id">{{v.numeroInterno}} · {{v.placa}}</option>}</select></label><app-mount-assignment [vehicle]="detail()" [(value)]="assignments"/><p class="selection">Odómetro registrado: {{detail()?.kilometraje==null?'Sin registro':(detail()!.kilometraje|number)+' km'}}</p><label>Kilometraje actual<input type="number" [min]="detail()?.kilometraje??0" [(ngModel)]="mileage"></label><label>Motivo<textarea [(ngModel)]="reason" maxlength="500"></textarea></label><label>Observaciones<textarea [(ngModel)]="notes" maxlength="1000"></textarea></label><p>La solicitud completa requiere autorización. No se ejecutarán posiciones por separado.</p><button class="primary" [disabled]="busy()||!assignments.length" (click)="submitSet()">Confirmar cambio de juego · enviar a autorización</button>@if(message()){<p role="status">{{message()}}</p>}</section>}@else{
<section class="grid"><article class="panel"><label>Buscar vehículo<input [(ngModel)]="vehicleSearch" (keyup.enter)="searchVehicles()"><button (click)="searchVehicles()">Buscar</button></label><label>Vehículo<select [(ngModel)]="vehicleId" (change)="loadVehicle()">@for(v of vehicles();track v.id){<option [value]="v.id">{{v.numeroInterno}} · {{v.placa}}</option>}</select></label>@if(detail()){<app-vehicle-axle-diagram [axles]="axles()" [selectedId]="selectedPosition()?.id??null" [vehicleType]="detail()!.tipo" (positionSelected)="select($event)"/>}<small>Selecciona una posición ocupada como origen o una libre como destino.</small></article>
<article class="panel form"><label>Operación<select [(ngModel)]="type" (change)="resetSelection()"><option value="Montaje">Montaje</option><option value="Desmontaje">Desmontaje</option><option value="Rotación">Rotación / cambio de posición</option><option value="Traslado">Traslado entre centros</option><option value="Inventario">Enviar a inventario</option><option value="Reparación">Enviar a reparación</option><option value="Reencauche">Enviar a reencauche</option><option value="Disposición final">Enviar a disposición</option></select></label>
@if(type==='Montaje'){<label>Buscar llanta por código o serial<input [(ngModel)]="tireSearch" (keyup.enter)="loadAvailable()"><button (click)="loadAvailable()">Buscar</button></label><small>Hasta 50 disponibles del centro del vehículo. Busca por código o serial para precisar.</small><label>Llanta disponible<select [(ngModel)]="tireId"><option value="">Seleccionar</option>@for(t of available();track t.id){<option [value]="t.id">{{t.codigo}} · {{t.serial}} · {{t.dimension}} · {{t.estado}}</option>}</select></label><p class="selection">Destino: {{selectedPosition()?.code??'Selecciona una posición libre'}}</p>}@else{<p class="selection">Origen: {{selectedPosition()?.code??'Selecciona una posición ocupada'}} · {{selectedPosition()?.tire??'—'}}</p>}
@if(type==='Rotación'){<label>Posición destino<select [(ngModel)]="destinationPositionId"><option value="">Seleccionar</option>@for(p of positions();track p.id){<option [value]="p.id">{{p.codigo}} · {{p.llantaCodigo??'Libre'}}</option>}</select></label>@if(destinationOccupant()){<div class="warning">La posición está ocupada por {{destinationOccupant()!.llantaCodigo}}.<label>Destino de la llanta ocupante<select [(ngModel)]="displacedDestination"><option value="">Obligatorio</option><option>Inventario</option><option>Reparacion</option><option>Reencauche</option><option>DisposicionFinal</option></select></label></div>}}
@if(type==='Traslado'){<label>Centro destino<select [(ngModel)]="destinationCenterId"><option value="">Seleccionar</option>@for(c of centers();track c.id){<option [value]="c.id">{{c.codigo}} · {{c.nombre}}</option>}</select></label>}
@if(detail()){<p class="selection">Odómetro registrado: {{detail()!.kilometraje==null?'Sin registro':(detail()!.kilometraje|number)+' km'}}</p>}<label>Kilometraje actual obligatorio<input type="number" inputmode="numeric" [min]="detail()?.kilometraje??0" [(ngModel)]="mileage"></label><label>Motivo<textarea rows="3" [(ngModel)]="reason"></textarea></label><label>Observaciones<textarea rows="2" [(ngModel)]="notes"></textarea></label><button class="primary" [disabled]="busy()" (click)="submit()">{{busy()?'Procesando…':'Enviar solicitud / ejecutar programación'}}</button>@if(message()){<p class="message" [class.error]="messageKind()==='error'" role="status">{{message()}}</p>}@if(refreshWarning()){<p class="warning" role="alert">{{refreshWarning()}} <button [disabled]="busy()" (click)="refresh()">Reintentar actualización</button></p>}</article></section>
}}
</main>`,styles:[`.ops{display:grid;gap:1rem}.ops>header p{color:var(--edinsa-blue);font-weight:900}.ops>header span{color:#667b84}.grid{display:grid;grid-template-columns:1.1fr .9fr;gap:1rem}.panel{background:#fff;border:1px solid #d9e4e8;border-radius:14px;padding:1rem}.panel label{display:grid;gap:.3rem;margin-bottom:.8rem;font-size:.72rem;font-weight:700;color:#526a75}select,input,textarea{max-width:100%;min-width:0;padding:.65rem;border:1px solid #cbd9de;border-radius:8px;background:#fff}.selection{background:#eaf5f8;padding:.75rem;border-radius:8px}.warning{padding:.8rem;background:#fff1d9;border-left:4px solid #ff962e}.primary{width:100%;padding:.75rem;border:0;border-radius:9px;color:#fff;font-weight:800;background:linear-gradient(110deg,var(--edinsa-blue),var(--edinsa-green))}.message{padding:.7rem;background:#eef7e9}@media(max-width:850px){.grid{grid-template-columns:1fr}}`]} )
export class MovementsPage implements OnInit {
 private http=inject(HttpClient);private catalogs=inject(CatalogsApi);private route=inject(ActivatedRoute);
 readonly auth=inject(AuthService);
 vehicles=signal<VehicleSummary[]>([]);detail=signal<VehicleDetail|null>(null);available=signal<any[]>([]);
 centers=signal<any[]>([]);requests=signal<RequestRow[]>([]);
 busy=signal(false);messageKind=signal('info');message=signal('');refreshWarning=signal('');
 vehicleSearch='';tireSearch='';vehicleId='';type='Montaje';tireId='';destinationPositionId='';
 destinationCenterId='';displacedDestination='';reason='';notes='';mileage:number|null=null;
 selectedPosition=signal<DiagramPosition|null>(null);
 activityId='';setModeActive=false;assignments:MountAssignment[]=[];planned=signal<{tipo:string;vehiculoId:string;motivo:string;observaciones:string;asignaciones:MountAssignment[]}|null>(null);
 private mileageVehicleId='';
 acceptDetail(detail:VehicleDetail){const minimum=detail.kilometraje??0;this.mileage=this.mileageVehicleId===detail.id&&this.mileage!==null&&Number.isFinite(this.mileage)?Math.max(this.mileage,minimum):minimum;this.mileageVehicleId=detail.id;this.detail.set(detail);}
 private mileageValid(){const minimum=this.detail()?.kilometraje??0;if(this.mileage===null||!Number.isFinite(this.mileage)||this.mileage<minimum){this.messageKind.set('error');this.message.set('El kilometraje actual debe ser igual o superior a '+minimum.toLocaleString('es-CO')+' km.');return false;}return true;}
 private refreshVersion=0;private availableVersion=0;
 positions=computed(()=>this.detail()?.ejes.flatMap(e=>e.posiciones)??[]);
 axles=computed<DiagramAxle[]>(()=>this.detail()?.ejes.map(e=>({id:e.id,name:e.nombre,type:e.tipoEje,positions:e.posiciones.map(p=>({id:p.id,code:p.codigo,side:p.lado+' '+p.ubicacion,tire:p.llantaCodigo??'LIBRE',state:p.llantaId?'normal':'empty'}))}))??[]);
 destinationOccupant(){return this.type==='Rotación'?this.positions().find(p=>p.id===this.destinationPositionId&&p.llantaId)??null:null;}
 async ngOnInit(){this.activityId=this.route.snapshot.queryParamMap.get('actividadId')??'';if(this.activityId){try{const plan=await firstValueFrom(this.http.get<NonNullable<ReturnType<typeof this.planned>>>('/api/actividades/'+this.activityId+'/montaje'));this.planned.set(plan);this.vehicleId=plan.vehiculoId;this.acceptDetail(await firstValueFrom(this.http.get<VehicleDetail>('/api/operaciones/vehiculos/'+this.vehicleId)));}catch(e:any){this.message.set(e?.userMessage??'No se pudo cargar la asignación. Solicita revisar la programación.');}return;}this.vehicleId=this.route.snapshot.queryParamMap.get('vehiculoId')??'';await this.refresh();}
 async searchVehicles(){
  try{const page=await firstValueFrom(this.http.get<{items:VehicleSummary[]}>('/api/operaciones/vehiculos',{params:{buscar:this.vehicleSearch,tamano:100}}));this.vehicles.set(page.items);}
  catch(e:any){this.refreshWarning.set(e?.userMessage??'No fue posible buscar vehículos.');}
 }
 async loadVehicle(){this.assignments=[];this.resetSelection();this.detail.set(null);this.available.set([]);await this.refresh();}
 private async updateAvailable(vehicleId:string){
  const version=++this.availableVersion;
  const tires=await firstValueFrom(this.http.get<any[]>('/api/operaciones/llantas-disponibles',{params:{vehiculoId:vehicleId,buscar:this.tireSearch}}));
  if(version===this.availableVersion&&vehicleId===this.vehicleId)this.available.set(tires);
 }
 async loadAvailable(){
  if(!this.vehicleId)return;
  try{await this.updateAvailable(this.vehicleId)}catch(e:any){this.refreshWarning.set(e?.userMessage??'No se pudieron consultar las llantas disponibles.');}
 }
 select(p:DiagramPosition){
  const raw=this.positions().find(x=>x.id===p.id);
  if(this.type==='Montaje'&&raw?.llantaId){this.message.set('Para montar, selecciona una posición libre.');return;}
  if(this.type!=='Montaje'&&!raw?.llantaId){this.message.set('Selecciona una posición que tenga llanta.');return;}
  this.selectedPosition.set(p);this.tireId=raw?.llantaId??this.tireId;
 }
 resetSelection(){this.selectedPosition.set(null);this.tireId='';this.destinationPositionId='';this.destinationCenterId='';this.displacedDestination='';}
 private resetForm(){this.resetSelection();this.reason='';this.notes='';this.mileage=null;}
 async submit(){
  if(this.busy())return;
  const source=this.selectedPosition();
  if(!this.tireId||this.type!=='Montaje'&&!source)return this.message.set('Selecciona la llanta o posición origen.');
  if(this.type==='Montaje'&&!source)return this.message.set('Selecciona la posición destino libre.');
  if(!this.mileageValid())return;
  if(!this.reason.trim())return this.message.set('El motivo es obligatorio.');
  const destination=this.type==='Montaje'?source?.id:this.type==='Rotación'?this.destinationPositionId:null;
  const occupied=this.destinationOccupant();
  if(occupied&&!this.displacedDestination)return this.message.set('Indica el destino de la llanta ocupante.');
  const body={tipo:this.type,llantaId:this.tireId,posicionOrigenId:this.type==='Montaje'?null:source?.id,posicionDestinoId:destination,tipoDestino:this.destinationType(),centroDestinoId:this.type==='Traslado'?this.destinationCenterId:null,llantaDesplazadaId:occupied?.llantaId??null,posicionDestinoDesplazadaId:null,destinoDesplazada:this.displacedDestination||null,motivo:this.reason,observaciones:this.notes||null,kilometrajeVehiculo:this.mileage,actividadProgramadaId:this.route.snapshot.queryParamMap.get('actividadId')};
  this.busy.set(true);
  try{
   const result=await firstValueFrom(this.http.post<RequestRow>('/api/operaciones/solicitudes',body));
   this.messageKind.set('success');
   this.message.set(result.estado==='EJECUTADO'?'Operación ejecutada.':'Solicitud enviada para autorización. No se ha modificado el inventario.');
   this.resetForm();await this.refresh();
  }catch(e:any){this.messageKind.set('error');this.message.set(e?.userMessage??'No fue posible registrar la solicitud.');}
  finally{this.busy.set(false);}
 }
 destinationType(){return this.type==='Montaje'||this.type==='Rotación'?'Posicion':this.type==='Reparación'?'Reparacion':this.type==='Disposición final'?'DisposicionFinal':this.type;}
 async refresh(){
  const version=++this.refreshVersion;const vehicleId=this.vehicleId;
  const current=()=>version===this.refreshVersion&&vehicleId===this.vehicleId;
  const tasks:{name:string;run:()=>Promise<void>}[]=[
   {name:'lista de vehículos',run:async()=>{const page=await firstValueFrom(this.http.get<{items:VehicleSummary[]}>('/api/operaciones/vehiculos',{params:{buscar:this.vehicleSearch,tamano:100}}));if(current())this.vehicles.set(page.items);}},
   {name:'solicitudes',run:async()=>{const rows=await firstValueFrom(this.http.get<RequestRow[]>('/api/operaciones/solicitudes'));if(current())this.requests.set(rows);}},
  ];
  if(!this.centers().length)tasks.push({name:'centros',run:async()=>{const centers=await firstValueFrom(this.catalogs.all('centros',true));if(current())this.centers.set(centers);}});
  if(vehicleId)tasks.push(
   {name:'vehículo, diagrama y trazabilidad',run:async()=>{const detail=await firstValueFrom(this.http.get<VehicleDetail>('/api/operaciones/vehiculos/'+vehicleId));if(current())this.acceptDetail(detail);}},
   {name:'llantas disponibles',run:()=>this.updateAvailable(vehicleId)}
  );
  const results=await Promise.allSettled(tasks.map(task=>task.run()));
  if(!current())return;
  const failed=tasks.filter((_,index)=>results[index].status==='rejected').map(task=>task.name);
  this.refreshWarning.set(failed.length?'No se pudo actualizar: '+failed.join('; ')+'. Los datos de esas secciones pueden estar desactualizados. Puedes reintentar sin repetir la operación.':'');
  if(!vehicleId&&this.vehicles().length){this.vehicleId=this.vehicles()[0].id;await this.refresh();}
 }
 setMode(active:boolean){this.setModeActive=active;this.assignments=[];this.message.set('');}
 async submitSet(){if(this.busy())return;if(!this.assignments.length||!this.reason.trim())return this.message.set('Asigna llantas e ingresa el motivo.');if(!this.mileageValid())return;this.busy.set(true);try{await firstValueFrom(this.http.post('/api/operaciones/solicitudes',{tipo:'Cambio de juego',vehiculoId:this.vehicleId,asignaciones:this.assignments,motivo:this.reason,observaciones:this.notes,kilometrajeVehiculo:this.mileage}));this.message.set('Cambio de juego enviado para autorización conjunta.');this.assignments=[];await this.refresh();}catch(e:any){this.message.set(e?.userMessage??'No fue posible enviar el cambio de juego.');}finally{this.busy.set(false);}}
 async executePlanned(){if(this.busy()||!this.planned())return;if(!this.mileageValid())return;this.busy.set(true);try{await firstValueFrom(this.http.post('/api/actividades/'+this.activityId+'/montaje',{kilometraje:this.mileage}));this.planned.set(null);this.message.set('Trabajo completo ejecutado. Movimientos registrados.');}catch(e:any){this.message.set(e?.userMessage??'No se ejecutó el trabajo. Actualiza la programación.');}finally{this.busy.set(false);}}
 async resolve(r:RequestRow,approve:boolean){
  const motivo=approve?null:prompt('Motivo obligatorio del rechazo:');if(!approve&&!motivo)return;
  await this.perform('/api/operaciones/solicitudes/'+r.id+'/resolver',{aprobar:approve,motivo},'Solicitud procesada.');
 }
 async receive(r:RequestRow){await this.perform('/api/operaciones/solicitudes/'+r.id+'/recibir',{},'Traslado recibido.');}
 private async perform(url:string,body:unknown,success:string){
  if(this.busy())return;this.busy.set(true);
  try{await firstValueFrom(this.http.post(url,body));this.messageKind.set('success');this.message.set(success);this.resetForm();await this.refresh();}
  catch(e:any){this.messageKind.set('error');this.message.set(e?.userMessage??'No fue posible procesar la operación.');}
  finally{this.busy.set(false);}
 }
}
