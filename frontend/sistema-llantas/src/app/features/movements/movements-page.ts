import {MountWorkList} from './mount-work-list';
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
@Component({host:{class:'edinsa-workflow'},selector:'app-movements-page',imports:[CommonModule,FormsModule,VehicleAxleDiagram,MountAssignmentEditor,MountWorkList],templateUrl:'./movements-page.html',styleUrls:['../../shared/operational-workflow.scss','./movements-page.scss']})
export class MovementsPage implements OnInit {
 private http=inject(HttpClient);private catalogs=inject(CatalogsApi);private route=inject(ActivatedRoute);
 readonly auth=inject(AuthService);
 vehicles=signal<VehicleSummary[]>([]);detail=signal<VehicleDetail|null>(null);available=signal<any[]>([]);
 centers=signal<any[]>([]);requests=signal<RequestRow[]>([]);
 busy=signal(false);messageKind=signal('info');message=signal('');refreshWarning=signal('');
 stage:'vehicle'|'positions'|'confirm'='vehicle';
 removalDestination='';vehicleLoading=signal(false);vehicleSearchError=signal('');private searchVersion=0;
 vehicleSearch='';tireSearch='';vehicleId='';type='Montaje';tireId='';destinationPositionId='';
 destinationCenterId='';displacedDestination='';reason='';notes='';mileage:number|null=null;
 selectedPosition=signal<DiagramPosition|null>(null);
 activityId='';setModeActive=true;editingPlan=false;planDraft:MountAssignment[]=[];planSelected:Record<string,boolean>={};assignments:MountAssignment[]=[];planned=signal<{tecnico?:string;fechaProgramada?:string;tipo:string;vehiculoId:string;motivo:string;observaciones:string;asignaciones:MountAssignment[]}|null>(null);

 flow:'planned'|'unplanned'|'rotation'|'other'='unplanned';
 schedules=signal<{id:string;vehiculo:string;tipo:string;estado:string;fecha:string}[]>([]);scheduleLoading=signal(false);scheduleError=signal('');schedulesLoaded=false;
 async chooseFlow(flow:typeof this.flow){if(this.busy())return;this.stage='vehicle';this.flow=flow;this.activityId='';this.planned.set(null);this.editingPlan=false;this.planSelected={};this.setMode(flow==='unplanned');this.resetSelection();if(flow==='rotation')this.type='Rotación';if(flow==='other')this.type='Desmontaje';if(flow==='planned')await this.loadSchedules();}
 async loadSchedules(){if(this.scheduleLoading())return;this.scheduleLoading.set(true);this.scheduleError.set('');try{
  const manage=this.auth.has('programacion.consultar')||this.auth.has('programacion.administrar');
  const rows=await firstValueFrom(this.http.get<any[]>(manage?'/api/programacion':'/api/mis-actividades'));
  const groups=new Set<string>();this.schedules.set(rows.filter(x=>['Montaje','Reemplazar llanta','Cambio de juego'].includes(x.tipo)&&!['Cumplida','Finalizada','Cancelada'].includes(x.estado)).filter(x=>{const key=x.grupoProgramacionId??x.id;if(groups.has(key))return false;groups.add(key);return true;}).map(x=>({id:x.id,vehiculo:x.vehiculo,tipo:x.tipo,estado:x.estado,fecha:x.inicio??x.fecha})));this.schedulesLoaded=true;
 }catch(e:any){this.scheduleError.set(e?.userMessage??'No fue posible consultar las programaciones.');}finally{this.scheduleLoading.set(false);}}
 async openSchedule(id:string){if(this.busy())return;this.stage='vehicle';this.flow='planned';this.activityId=id;this.planned.set(null);this.detail.set(null);this.editingPlan=false;this.planSelected={};this.message.set('');this.busy.set(true);try{const plan=await firstValueFrom(this.http.get<NonNullable<ReturnType<typeof this.planned>>>('/api/actividades/'+id+'/montaje'));this.vehicleId=plan.vehiculoId;const vehicle=await firstValueFrom(this.http.get<VehicleDetail>('/api/operaciones/vehiculos/'+plan.vehiculoId));this.acceptDetail(vehicle);this.planned.set(plan);}catch(e:any){this.messageKind.set('error');this.message.set(e?.userMessage??'No se pudo cargar la programación.');}finally{this.busy.set(false);}}
 async showWorkVehicle(id:string){if(this.busy())return;this.vehicleId=id;await this.loadVehicle();}
 selectPositionId(id:string){const p=this.axles().flatMap(a=>a.positions).find(p=>p.id===id);if(p)this.select(p);}
 positionLabel(id:string){return this.positions().find(p=>p.id===id)?.codigo??id;}
 mileageReady(){return this.detail()!==null&&this.mileage!==null&&Number.isFinite(this.mileage)&&this.mileage>=(this.detail()?.kilometraje??0);}
 canExecutePlan(){return !this.busy()&&!this.editingPlan&&this.mileageReady()&&!!this.planned()?.asignaciones.some(p=>p.estado==='APROBADO'&&this.planSelected[p.solicitudId!]);}
 canSubmitSet(){return !this.busy()&&this.mileageReady()&&!!this.reason.trim()&&this.assignments.length>0&&this.assignments.every(p=>!p.llantaActualId||!!p.destinoRetirada&&(p.destinoRetirada!=='Posicion'||!!p.posicionRetiradaId)&&(p.destinoRetirada!=='Traslado'||!!p.centroRetiradaId));}
 canSubmitOperation(){return !this.busy()&&this.mileageReady()&&!!this.reason.trim()&&!!this.selectedPosition()&&!!this.tireId&&(this.type!=='Rotación'||!!this.destinationPositionId&&this.destinationPositionId!==this.selectedPosition()?.id)&&(!['Desmontaje','Reemplazar llanta'].includes(this.type)||!!this.removalDestination)&&(!(this.type==='Traslado'||this.removalDestination==='Traslado')||!!this.destinationCenterId);}

 private mileageVehicleId='';
 acceptDetail(detail:VehicleDetail){const minimum=detail.kilometraje??0;this.mileage=this.mileageVehicleId===detail.id&&this.mileage!==null&&Number.isFinite(this.mileage)?Math.max(this.mileage,minimum):minimum;this.mileageVehicleId=detail.id;this.detail.set(detail);}
 private mileageValid(){const minimum=this.detail()?.kilometraje??0;if(this.mileage===null||!Number.isFinite(this.mileage)||this.mileage<minimum){this.messageKind.set('error');this.message.set('El kilometraje actual debe ser igual o superior a '+minimum.toLocaleString('es-CO')+' km.');return false;}return true;}
 private refreshVersion=0;private availableVersion=0;
 positions=computed(()=>this.detail()?.ejes.flatMap(e=>e.posiciones)??[]);
 axles=computed<DiagramAxle[]>(()=>this.detail()?.ejes.map(e=>({id:e.id,name:e.nombre,type:e.tipoEje,positions:e.posiciones.map(p=>({id:p.id,code:p.codigo,side:p.lado+' '+p.ubicacion,tire:p.llantaCodigo??'LIBRE',state:p.llantaId?'normal':'empty'}))}))??[]);
 destinationOccupant(){return this.type==='Rotación'?this.positions().find(p=>p.id===this.destinationPositionId&&p.llantaId)??null:null;}
 async ngOnInit(){this.activityId=this.route.snapshot.queryParamMap.get('actividadId')??'';if(this.activityId){this.flow='planned';try{const plan=await firstValueFrom(this.http.get<NonNullable<ReturnType<typeof this.planned>>>('/api/actividades/'+this.activityId+'/montaje'));this.planned.set(plan);this.vehicleId=plan.vehiculoId;this.acceptDetail(await firstValueFrom(this.http.get<VehicleDetail>('/api/operaciones/vehiculos/'+this.vehicleId)));}catch(e:any){this.message.set(e?.userMessage??'No se pudo cargar la asignación. Solicita revisar la programación.');}return;}this.vehicleId=this.route.snapshot.queryParamMap.get('vehiculoId')??'';await this.refresh();}
 async searchVehicles(){
  const version=++this.searchVersion;this.vehicleLoading.set(true);this.vehicleSearchError.set('');
  try{const page=await firstValueFrom(this.http.get<{items:VehicleSummary[]}>('/api/operaciones/vehiculos',{params:{search:this.vehicleSearch.trim(),pageSize:100}}));if(version===this.searchVersion)this.vehicles.set(page.items);}
  catch(e:any){if(version===this.searchVersion){this.vehicles.set([]);this.vehicleSearchError.set(e?.userMessage??'No fue posible buscar vehículos.');}}finally{if(version===this.searchVersion)this.vehicleLoading.set(false);}
 }
 async loadVehicle(){this.stage='vehicle';this.assignments=[];this.resetSelection();this.detail.set(null);this.available.set([]);await this.refresh();}
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
  this.selectedPosition.set(p);if(this.type!=='Reemplazar llanta')this.tireId=raw?.llantaId??this.tireId;
 }
 resetSelection(){this.selectedPosition.set(null);this.tireId='';this.destinationPositionId='';this.destinationCenterId='';this.displacedDestination='';this.removalDestination='';}
 private resetForm(){this.resetSelection();this.reason='';this.notes='';this.mileage=null;}
 async submit(){
  if(this.busy())return;
  const source=this.selectedPosition();
  if(!this.tireId||this.type!=='Montaje'&&!source)return this.message.set('Selecciona la llanta o posición origen.');
  if(this.type==='Montaje'&&!source)return this.message.set('Selecciona la posición destino libre.');
  if(!this.mileageValid())return;
  if(!this.reason.trim())return this.message.set('El motivo es obligatorio.');
  if(['Desmontaje','Reemplazar llanta'].includes(this.type)&&!this.removalDestination)return this.message.set('Selecciona el destino de la llanta saliente.');
  if((this.type==='Traslado'||this.removalDestination==='Traslado')&&!this.destinationCenterId)return this.message.set('Selecciona el centro destino.');
  if(this.type==='Reemplazar llanta'){
   const current=this.positions().find(p=>p.id===source?.id)?.llantaId;
   if(!source||!current||current===this.tireId)return this.message.set('Selecciona una posición ocupada y una llanta nueva diferente.');
   this.assignments=[{posicionId:String(source.id),llantaId:this.tireId,llantaActualId:current,destinoRetirada:this.removalDestination,centroRetiradaId:this.destinationCenterId||null}];
   return this.submitSet('Reemplazar llanta');
  }
  if(this.type==='Rotación'&&(!this.destinationPositionId||this.destinationPositionId===source?.id))return this.message.set('Selecciona otra posición del mismo vehículo.');
  const destination=this.type==='Montaje'?source?.id:this.type==='Rotación'?this.destinationPositionId:null;
  const occupied=this.destinationOccupant();

  const body={tipo:this.type,llantaId:this.tireId,posicionOrigenId:this.type==='Montaje'?null:source?.id,posicionDestinoId:destination,tipoDestino:this.destinationType(),centroDestinoId:(this.type==='Traslado'||this.removalDestination==='Traslado')?this.destinationCenterId:null,llantaDesplazadaId:occupied?.llantaId??null,posicionDestinoDesplazadaId:this.type==='Rotación'&&occupied?source?.id:null,destinoDesplazada:this.type==='Rotación'&&occupied?'Posicion':null,motivo:this.reason,observaciones:this.notes||null,kilometrajeVehiculo:this.mileage,actividadProgramadaId:this.activityId||null};
  this.busy.set(true);
  try{
   const result=await firstValueFrom(this.http.post<RequestRow>('/api/operaciones/solicitudes',body));
   this.messageKind.set('success');
   this.message.set(result.estado==='EJECUTADO'?'Operación ejecutada.':'Solicitud enviada para autorización. No se ha modificado el inventario.');
   this.resetForm();await this.refresh();
  }catch(e:any){this.messageKind.set('error');this.message.set(e?.userMessage??'No fue posible registrar la solicitud.');}
  finally{this.busy.set(false);}
 }
 destinationType(){if(this.type==='Desmontaje')return this.removalDestination;return this.type==='Montaje'||this.type==='Rotación'?'Posicion':this.type==='Reparación'?'Reparacion':this.type;}
 async refresh(){
  const version=++this.refreshVersion;const vehicleId=this.vehicleId;const searchVersion=++this.searchVersion;this.vehicleLoading.set(false);
  const current=()=>version===this.refreshVersion&&vehicleId===this.vehicleId;
  const tasks:{name:string;run:()=>Promise<void>}[]=[
   {name:'lista de vehículos',run:async()=>{const page=await firstValueFrom(this.http.get<{items:VehicleSummary[]}>('/api/operaciones/vehiculos',{params:{search:this.vehicleSearch.trim(),pageSize:100}}));if(current()&&searchVersion===this.searchVersion)this.vehicles.set(page.items);}},
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

 }
 setMode(active:boolean){this.setModeActive=active;if(!active&&['Montaje','Reemplazar llanta'].includes(this.type))this.type='Desmontaje';this.assignments=[];this.message.set('');}
 async submitSet(tipo=''){if(this.busy())return;tipo=tipo||(this.assignments.length>1?'Cambio de juego':this.assignments[0]?.llantaActualId?'Reemplazar llanta':'Montaje');if(!this.assignments.length||!this.reason.trim())return this.message.set('Asigna llantas e ingresa el motivo.');if(!this.mileageValid())return;if(!this.canSubmitSet())return this.message.set('Selecciona el destino de cada llanta saliente y su posición o centro cuando corresponda.');this.busy.set(true);try{await firstValueFrom(this.http.post('/api/operaciones/solicitudes',{tipo,vehiculoId:this.vehicleId,asignaciones:this.assignments,motivo:this.reason,observaciones:this.notes,kilometrajeVehiculo:this.mileage}));this.message.set(tipo+' enviado para autorización.');this.assignments=[];await this.refresh();}catch(e:any){this.message.set(e?.userMessage??'No fue posible enviar el cambio de juego.');}finally{this.busy.set(false);}}
 async editPlan(){this.planDraft=structuredClone(this.planned()?.asignaciones??[]);this.editingPlan=true;try{this.centers.set(await firstValueFrom(this.catalogs.all('centros',true)))}catch{this.message.set('No se pudo cargar el catálogo de centros.')}}
 async savePlan(){if(this.busy()||!this.planned())return;const old=this.planned()!.asignaciones;if(this.planDraft.length!==old.length||this.planDraft.some(f=>!old.some(x=>x.posicionId===f.posicionId))){this.message.set('Conserva las posiciones programadas; modifica las llantas o destinos necesarios.');return}const changes=this.planDraft.map(f=>({...f,rowVersion:old.find(x=>x.posicionId===f.posicionId)?.rowVersion}));this.busy.set(true);try{await firstValueFrom(this.http.post('/api/actividades/'+this.activityId+'/montaje/cambios',changes));this.editingPlan=false;this.planned.set(await firstValueFrom(this.http.get<NonNullable<ReturnType<typeof this.planned>>>('/api/actividades/'+this.activityId+'/montaje')));this.planSelected={};this.message.set('Solo las posiciones modificadas requieren nueva revisión.');}catch(e:any){this.message.set(e?.userMessage??'No se pudieron enviar los cambios.')}finally{this.busy.set(false)}}
 async executePlanned(){if(this.busy()||!this.planned())return;if(!this.mileageValid())return;this.busy.set(true);try{await firstValueFrom(this.http.post('/api/actividades/'+this.activityId+'/montaje',{kilometraje:this.mileage,posiciones:this.planned()!.asignaciones.filter(p=>p.estado==='APROBADO'&&this.planSelected[p.solicitudId!]).map(p=>({id:p.solicitudId,rowVersion:p.rowVersion}))}));this.planned.set(null);this.message.set('Posiciones seleccionadas ejecutadas. Actualiza la programación para continuar con las restantes.');}catch(e:any){this.message.set(e?.userMessage??'No se ejecutó el trabajo. Actualiza la programación.');}finally{this.busy.set(false);}}
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

