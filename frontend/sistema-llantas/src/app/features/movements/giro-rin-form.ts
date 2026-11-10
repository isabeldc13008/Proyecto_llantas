import {Component,Input,Output,EventEmitter,OnChanges,inject,signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';
import {VehicleDetail} from '../vehicles/vehicles-api';
import {VehicleAxleDiagram,DiagramAxle} from '../../shared/vehicle-axle-diagram';
import {AuthService} from '../../core/auth/auth.service';

@Component({selector:'app-giro-rin-form',imports:[FormsModule,VehicleAxleDiagram],template:`<section aria-label="Giro en el mismo rin">
 <p>La intervención conserva la llanta en el mismo vehículo, rin y posición.</p>
 <app-vehicle-axle-diagram [axles]="axles()" [vehicleType]="vehicle?.tipo??'Vehículo'" [selectedId]="positionId" (positionSelected)="selectPosition(''+$event.id)"/>
 <div class="fields">
 <label>Llanta instalada<select [(ngModel)]="positionId" [disabled]="busy()"><option value="">Seleccionar llanta y posición</option>@for(p of positions();track p.id){<option [value]="p.id">{{p.codigo}} · {{p.llantaCodigo??p.llantaId}}</option>}</select></label>
 @if(isTechnician()){<label>Técnico<input [value]="auth.user()?.name??''" readonly aria-describedby="current-technician-note"></label><small id="current-technician-note">Se registra automáticamente tu usuario autenticado.</small>}@else{<label>Técnico<select [(ngModel)]="technicianId" [disabled]="busy()||loading()"><option value="">Seleccionar técnico</option>@for(t of technicians();track t.id){<option [value]="t.id">{{t.nombre}}</option>}</select></label>}
 <label>Fecha y hora de intervención<input type="datetime-local" [(ngModel)]="date" [disabled]="busy()" required></label>
 <label>Motivo<textarea [(ngModel)]="reason" maxlength="500" [disabled]="busy()" required></textarea></label>
 </div>
 <p>Kilometraje registrado para la intervención: {{mileage??'Pendiente'}} km.</p>
 @if(loading()){<p role="status">Consultando técnicos del centro…</p>}@else if(!technicians().length){<p role="status">{{isTechnician()?'Tu usuario no está habilitado como técnico en el centro de este vehículo.':'No hay técnicos disponibles para este vehículo.'}}</p>}
 @if(error()){<p role="alert">{{error()}} <button type="button" [disabled]="busy()||loading()" (click)="loadTechnicians()">Actualizar técnicos</button></p>}
 @if(message()){<p [attr.role]="success()?'status':'alert'">{{message()}}</p>}
 @if(!authorized()){<p role="alert">No tienes permiso para ejecutar esta intervención.</p>}
 <button type="button" class="primary" [disabled]="busy()||loading()||!authorized()" (click)="submit()">{{busy()?'Guardando…':'Revisar y registrar giro'}}</button>
</section>`,styles:[`:host{display:block;padding:16px}.fields{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,240px),1fr));gap:16px}label{display:flex;flex-direction:column;gap:8px}input,select,textarea,button{font:inherit;padding:10px;border:1px solid #ccd4dc;border-radius:8px;max-width:100%;box-sizing:border-box}button{cursor:pointer}.primary{background:#a80e27;color:white;margin-top:16px}button:disabled{opacity:.6;cursor:default}[role=alert]{color:#a80e27}app-vehicle-axle-diagram{display:block;max-width:600px;margin:auto}`]})
export class GiroRinForm implements OnChanges {
 @Input() vehicle:VehicleDetail|null=null;@Input() mileage:number|null=null;@Output() saved=new EventEmitter<void>();
 private http=inject(HttpClient);readonly auth=inject(AuthService);
 technicians=signal<{id:string;nombre:string}[]>([]);loading=signal(false);busy=signal(false);error=signal('');message=signal('');success=signal(false);
 positionId='';technicianId='';reason='';date='';private vehicleKey='';private version=0;
 isTechnician(){return this.auth.user()?.role==='TECNICO';}
 authorized(){return this.auth.has('operaciones.ejecutar')||this.auth.has('llantas.administrar');}
 ngOnChanges(){if(this.vehicle?.id!==this.vehicleKey){this.vehicleKey=this.vehicle?.id??'';this.positionId='';this.technicianId='';this.reason='';this.message.set('');const now=new Date();this.date=new Date(now.getTime()-now.getTimezoneOffset()*60000).toISOString().slice(0,16);void this.loadTechnicians();}}
 positions(){return this.vehicle?.ejes.flatMap(e=>e.posiciones).filter(p=>!!p.llantaId)??[];}
 axles():DiagramAxle[]{return this.vehicle?.ejes.map(e=>({id:e.id,name:e.nombre,type:e.tipoEje,positions:e.posiciones.map(p=>({id:p.id,code:p.codigo,side:p.lado+' '+p.ubicacion,tire:p.llantaCodigo??'LIBRE',state:p.llantaId?'normal':'empty'}))}))??[];}
 selectPosition(id:string){if(!this.busy()&&this.positions().some(p=>p.id===id))this.positionId=id;}
 async loadTechnicians(){const version=++this.version;this.technicians.set([]);this.error.set('');if(!this.vehicle||!this.authorized()){this.loading.set(false);return;}this.loading.set(true);try{const rows=await firstValueFrom(this.http.get<{id:string;nombre:string}[]>('/api/operaciones/giro-rin/tecnicos',{params:{vehiculoId:this.vehicle.id}}));if(version===this.version){this.technicians.set(rows);if(this.isTechnician())this.technicianId=rows[0]?.id??'';}}catch(e:any){if(version===this.version)this.error.set(e?.userMessage??e?.error?.detail??'No fue posible consultar los técnicos.');}finally{if(version===this.version)this.loading.set(false);}}
 async submit(){
  if(this.busy()||this.loading()||!this.authorized())return;
  this.success.set(false);this.message.set('');const p=this.positions().find(p=>p.id===this.positionId);const technician=this.technicians().find(t=>t.id===this.technicianId);const date=new Date(this.date);
  if(!this.vehicle||!p||!technician){this.message.set('Selecciona vehículo, llanta instalada y técnico.');return;}
  if(this.mileage===null||!Number.isFinite(this.mileage)||this.mileage<(this.vehicle.kilometraje??0)){this.message.set('Ingresa un kilometraje igual o superior al registrado en el vehículo.');return;}
  if(!this.date||!Number.isFinite(date.getTime())||date.getTime()>Date.now()){this.message.set('Ingresa una fecha válida que no sea futura.');return;}
  if(!this.reason.trim()||this.reason.trim().length>500){this.message.set('Ingresa un motivo de hasta 500 caracteres.');return;}
  if(!window.confirm('Confirmar giro en el mismo rin\nVehículo: '+this.vehicle.placa+'\nLlanta: '+(p.llantaCodigo??p.llantaId)+' · Posición: '+p.codigo+'\nKilometraje: '+this.mileage+' km\nTécnico: '+technician.nombre+'\nFecha: '+date.toLocaleString()+'\nMotivo: '+this.reason.trim()+'\nLa llanta conservará vehículo, rin y posición.'))return;
  this.busy.set(true);
  try{await firstValueFrom(this.http.post('/api/operaciones/giro-rin',{vehiculoId:this.vehicle.id,posicionId:p.id,llantaId:p.llantaId,kilometraje:this.mileage,tecnicoId:technician.id,fecha:date.toISOString(),motivo:this.reason.trim()}));this.success.set(true);this.message.set('Giro registrado en el historial de la llanta. La asignación se conserva.');this.positionId='';this.reason='';this.saved.emit();}
  catch(e:any){this.message.set(e?.userMessage??e?.error?.detail??e?.error?.message??'No fue posible registrar el giro.');}finally{this.busy.set(false);}
 }
}
