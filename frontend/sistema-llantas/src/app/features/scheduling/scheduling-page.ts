import {ActivatedRoute} from '@angular/router';
import {MaintenanceReturn,validatedScheduleContext} from '../analytics/maintenance-context';
import {AnalyticsApi} from '../analytics/analytics-api';
import {of} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {MountAssignment,MountAssignmentEditor} from '../movements/mount-assignment';
import {CommonModule} from '@angular/common';
import {Component,OnInit,computed,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';
import {AuthService} from '../../core/auth/auth.service';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {DataTableToolbar,TableFilter} from '../../shared/data-table-toolbar';
import {SingleSelectFilter} from '../../shared/single-select-filter';
import {VehicleSummary,VehicleDetail} from '../vehicles/vehicles-api';
import {Schedule,ScheduleInput,ScheduleNeed,SchedulingApi,Technician} from './scheduling-api';
interface Option{id:string;nombre:string} interface PlanRow extends ScheduleNeed{selected:boolean;technicianId:string;start:string;priority:string;error:string}
@Component({selector:'app-scheduling-page',imports:[CommonModule,FormsModule,DataTableToolbar,SingleSelectFilter,MountAssignmentEditor,MaintenanceReturn],templateUrl:'./scheduling-page.html',styleUrl:'./scheduling-page.scss'})
export class SchedulingPage implements OnInit{
 private http=inject(HttpClient);private route=inject(ActivatedRoute);private analytics=inject(AnalyticsApi);maintenanceMessage='';
 private api=inject(SchedulingApi);private catalogs=inject(CatalogsApi);readonly auth=inject(AuthService);
 formError=signal('');mountError=signal('');wizardError=signal('');private createKey='';
 rows=signal<Schedule[]>([]);centers=signal<Option[]>([]);vehicles=signal<VehicleSummary[]>([]);technicians=signal<Technician[]>([]);needs=signal<PlanRow[]>([]);message=signal('');loading=signal(true);view=signal<'list'|'calendar'>('list');tab=signal('Programada');modal=signal(false);wizard=signal(false);step=signal(1);saving=signal(false);result=signal<{created:number;errors:string[]}|null>(null);search='';filterValues:Record<string,unknown>={estados:['Programada']};sortBy='inicio';batch={technicianId:'',start:'',priority:'Media'};form=this.emptyForm();
 mountModal=signal(false);mountStep=signal(1);mountVehicle=signal<VehicleDetail|null>(null);mountRows:MountAssignment[]=[];mountReason='';mountSearch='';mountLoading=signal(false);private mountVersion=0;
 readonly priorities=['Baja','Media','Alta','Crítica'];readonly types=['Inspección','Cambio de juego','Reemplazar llanta','Montaje','Desmontaje','Movimiento','Administrativa'];readonly tabs=[['Pendiente','Pendientes'],['Programada','Programadas'],['EnEjecucion','En ejecución'],['Finalizada','Finalizadas'],['Cancelada','Canceladas']];
 readonly typeOptions=this.types.map(x=>({value:x,label:x}));readonly priorityOptions=this.priorities.map(x=>({value:x,label:x}));readonly originOptions=['MANUAL','PREVENTIVO','INSPECCION','ALERTA','INCONSISTENCIA','SOLICITUD'].map(x=>({value:x,label:x}));
 readonly vehicleOptions=computed(()=>this.vehicles().map(v=>({value:v.id,label:`${v.placa} · ${v.numeroInterno} · ${v.centro}`})));readonly centerOptions=computed(()=>this.centers().map(c=>({value:c.id,label:c.nombre})));technicianOptions=()=>this.techs(this.form.centroId).map(t=>({value:t.id,label:t.nombre}));
 readonly filters=computed<TableFilter[]>(()=>[{key:'centros',label:'Centro',type:'multi',options:this.centers().map(x=>({value:x.id,label:x.nombre}))},{key:'estados',label:'Estado',type:'multi',options:this.tabs.map(x=>({value:x[0],label:x[1]}))},{key:'tipos',label:'Tipo',type:'multi',options:this.types.map(x=>({value:x,label:x}))},{key:'prioridades',label:'Prioridad',type:'multi',options:this.priorities.map(x=>({value:x,label:x}))},{key:'tecnicos',label:'Técnico',type:'multi',options:this.technicians().map(x=>({value:x.id,label:x.nombre}))},{key:'origenes',label:'Origen',type:'multi',options:this.originOptions},{key:'fecha',label:'Fecha',type:'range'}]);
 readonly visibleRows=computed(()=>this.rows().filter(x=>{const v=this.filterValues,text=this.search.toLowerCase();const match=(key:string,value:string)=>!(v[key] as string[]|undefined)?.length||(v[key] as string[]).includes(value);return(!text||x.vehiculo.toLowerCase().includes(text))&&match('centros',x.centroId)&&match('estados',x.estado)&&match('tipos',x.tipo)&&match('prioridades',x.prioridad)&&match('tecnicos',x.tecnicoUsuarioId??'')&&match('origenes',x.origen??'')}));
 readonly metrics=computed(()=>{const now=new Date(),today=now.toISOString().slice(0,10),r=this.rows();return{pending:this.needs().length,today:r.filter(x=>x.inicio.slice(0,10)===today).length,overdue:r.filter(x=>x.estado==='Vencida'||(x.estado==='Pendiente'&&new Date(x.inicio)<now)).length,high:r.filter(x=>['Alta','Crítica'].includes(x.prioridad)).length,unassigned:r.filter(x=>!x.tecnicoUsuarioId).length}});
 readonly workload=computed(()=>this.technicians().map(t=>({name:t.nombre,count:this.rows().filter(x=>x.tecnicoUsuarioId===t.id&&!['Cumplida','Cancelada'].includes(x.estado)).length,overdue:this.rows().filter(x=>x.tecnicoUsuarioId===t.id&&x.estado==='Vencida').length})).sort((a,b)=>b.count-a.count));readonly selected=computed(()=>this.needs().filter(x=>x.selected));readonly ready=computed(()=>this.selected().filter(x=>!x.error&&x.technicianId&&x.start));
 async ngOnInit(){try{const[c,v,t,n]=await Promise.all([firstValueFrom(this.catalogs.all('centros',true)),firstValueFrom(this.http.get<{items:VehicleSummary[]}>('/api/operaciones/vehiculos',{params:{search:'',pageSize:100}})),firstValueFrom(this.api.technicians()),firstValueFrom(this.canManage()?this.api.needs():of([]))]);this.centers.set(c.map(x=>({id:x.id,nombre:`${x.codigo} · ${x.nombre}`})));this.vehicles.set(v.items);this.technicians.set(t);this.needs.set(n.map(x=>({...x,selected:false,technicianId:'',start:'',priority:x.prioridad,error:''})));await this.load();await this.applyMaintenanceContext()}catch(e:any){this.message.set(e?.userMessage??'No fue posible cargar Programación.')}finally{this.loading.set(false)}}
  async applyMaintenanceContext(){
  const p=this.route.snapshot.queryParamMap,tire=p.get('llantaId');if(!p.get('volver')||!tire)return;
  if(!this.canManage()){this.maintenanceMessage='Sin permiso para programar. No se creó ninguna actividad.';return;}
  try{
   const detail=await firstValueFrom(this.analytics.maintenanceDetail(tire));
   const row=validatedScheduleContext(detail.llanta,tire,p.get('vehiculoId')??'',p.get('posicionId')??'');
   const alertId=p.get('origenEntidadId');
   if(alertId){
    const existing=this.rows().find(x=>x.origen==='ALERTA'&&x.origenEntidadId===alertId&&x.estado!=='Cancelada');
    if(existing){this.filterValues={estados:[]};this.search=existing.vehiculo;this.maintenanceMessage='Ya existe una actividad vinculada: '+existing.estado+'. Su estado no prueba resolución del motivo.';return;}
    const need=this.needs().find(x=>x.id===alertId&&x.origen==='ALERTA'&&x.llantaId===tire&&x.vehiculoId===row.ubicacion.vehiculo?.id&&x.posicionId===row.ubicacion.posicion?.id);
    if(!need)throw new Error('La alerta no está disponible para esta llanta y ubicación. Actualiza Analítica.');
   }
   this.openNew();this.form.tipo='Inspección';this.form.centroId=row.centro.id;this.form.vehicleId=row.ubicacion.vehiculo!.id;
   this.form.llantaId=row.id;this.form.posicionVehiculoId=row.ubicacion.posicion!.id;
   this.form.origen=alertId?'ALERTA':'MANUAL';this.form.origenEntidadId=alertId??'';
   this.form.observaciones='Llanta '+row.codigo+' · Posición '+row.ubicacion.posicion!.nombre+'. '+(row.motivoPrincipal?.descripcion??'Evaluación solicitada');
   this.maintenanceMessage='Contexto verificado. Selecciona técnico, fecha y prioridad operativa antes de guardar. No es severidad analítica.';
  }catch(e:any){this.modal.set(false);this.maintenanceMessage=e?.message??'No fue posible validar el contexto de mantenimiento.';}
 }
 async load(){const rows=await firstValueFrom(this.api.list({}));const groups=new Set<string>();this.rows.set(rows.filter(x=>{if(x.tipo!=='Cambio de juego'||!x.grupoProgramacionId)return true;if(groups.has(x.grupoProgramacionId))return false;groups.add(x.grupoProgramacionId);return true;}))} setTab(v:string){this.tab.set(v);this.filterValues={...this.filterValues,estados:[v]}} clear(){this.search='';this.filterValues={estados:[this.tab()]}} canManage(){return this.auth.has('programacion.administrar')}
 applyFilters(){this.filterValues={...this.filterValues}} openNew(){this.form=this.emptyForm();this.createKey=crypto.randomUUID();this.formError.set('');this.message.set('');this.modal.set(true)} edit(x:Schedule){this.formError.set('');this.message.set('');if(x.tipo==='Montaje'||x.tipo==='Reemplazar llanta'||x.tipo==='Cambio de juego'){this.message.set('Para modificar asignaciones, cancela el trabajo completo y vuelve a programarlo.');return;}this.form={id:x.id,tipo:x.tipo,inicio:this.local(x.inicio),fin:this.local(x.fin??x.inicio),centroId:x.centroId,vehicleId:x.vehiculoId??'',tecnicoUsuarioId:x.tecnicoUsuarioId??'',prioridad:x.prioridad,observaciones:x.observaciones??'',origen:x.origen??'MANUAL',origenEntidadId:x.origenEntidadId??'',llantaId:x.llantaId??'',posicionVehiculoId:x.posicionVehiculoId??'',rowVersion:x.rowVersion};this.modal.set(true)}
 async save(){
  if(this.saving())return;
  const error=this.validateForm();this.formError.set(error);if(error)return;
  this.saving.set(true);
  try{const input=this.input(this.form);if(this.form.id)await firstValueFrom(this.api.update(this.form.id,input));else await firstValueFrom(this.api.create({...input,idempotencyKey:this.createKey||(this.createKey=crypto.randomUUID())}));}
  catch(e:unknown){this.formError.set(this.errorText(e,'No fue posible guardar.'));return;}
  finally{this.saving.set(false);}
  this.modal.set(false);await this.saved('Programación guardada.');
 }
 pickVehicle(id:string){this.form.vehicleId=id;const vehicle=this.vehicles().find(v=>v.id===id);this.changeCenter(vehicle?.centroId??'');this.form.llantaId='';this.form.posicionVehiculoId='';}
 changeCenter(id:string){this.form.centroId=id;this.form.tecnicoUsuarioId='';}
 private validateForm():string{
  const f=this.form,errors:string[]=[];
  if(!this.canManage())errors.push('No tienes permiso para administrar Programación.');
  if(!this.types.includes(f.tipo))errors.push('Selecciona un tipo de actividad válido.');
  if(!f.vehicleId&&f.tipo!=='Administrativa')errors.push('Selecciona un vehículo.');
  if(!f.centroId)errors.push('Selecciona un centro.');
  const vehicle=this.vehicles().find(v=>v.id===f.vehicleId);
  if(vehicle&&vehicle.centroId!==f.centroId)errors.push('El vehículo no pertenece al centro seleccionado.');
  if(!f.tecnicoUsuarioId)errors.push('Selecciona un técnico.');
  else if(!this.techs(f.centroId).some(t=>t.id===f.tecnicoUsuarioId))errors.push('El técnico no está activo o no pertenece al centro.');
  const inicio=new Date(f.inicio).getTime(),fin=new Date(f.fin).getTime();
  if(!Number.isFinite(inicio))errors.push('Indica una fecha de inicio válida.');
  if(!Number.isFinite(fin))errors.push('Indica una fecha de fin válida.');
  if(Number.isFinite(inicio)&&Number.isFinite(fin)&&fin<=inicio)errors.push('La fecha final debe ser posterior a la inicial.');
  if(!this.priorities.includes(f.prioridad))errors.push('Selecciona una prioridad válida.');
  if(f.observaciones.length>1000)errors.push('La observación admite hasta 1000 caracteres.');
  return errors.join('\n');
 }
 private errorText(error:unknown,fallback:string):string{
  const e=error as {error?:{message?:string;detail?:string;title?:string;errors?:Record<string,unknown>;traceId?:string};userMessage?:string;status?:number};
  const body=e?.error;const details=Object.values(body?.errors??{}).flat().filter(v=>typeof v==='string') as string[];
  const main=body?.message||body?.detail||e?.userMessage||body?.title||(e?.status===403?'No tienes permiso para realizar esta operación.':e?.status===401?'Tu sesión expiró. Inicia sesión nuevamente.':fallback);
  return [...new Set([main,...details,...(body?.traceId?['Referencia: '+body.traceId]:[])])].join('\n');
 }
 private async saved(text:string){this.message.set(text);try{await this.load();}catch(e:unknown){this.message.set(text+'\nNo fue posible actualizar la lista. '+this.errorText(e,'Actualiza la página.'));}}

 openWizard(){this.wizardError.set('');this.message.set('');this.step.set(1);this.result.set(null);this.wizard.set(true)} toggle(n:PlanRow){n.selected=!n.selected;this.needs.set([...this.needs()])} selectAll(){const all=this.needs().every(x=>x.selected);this.needs.update(rows=>rows.map(x=>({...x,selected:!all})))} applyBatch(){this.needs.update(rows=>rows.map(x=>x.selected?{...x,technicianId:this.batch.technicianId||x.technicianId,start:this.batch.start||x.start,priority:this.batch.priority||x.priority}:x));this.validateRows()}
 validateRows(){const seen=new Set<string>();this.needs.update(rows=>rows.map(x=>{if(!x.selected)return x;let error='';if(x.tipo==='Montaje'||x.tipo==='Reemplazar llanta'||x.tipo==='Cambio de juego')error='Usa Programar montaje para asignar llantas antes de guardar';else if(!x.tipo.trim())error='Falta tipo de actividad';else if(!x.centroId)error='Falta centro';else if(!x.vehiculoId&&!x.tipo.toLowerCase().includes('administr'))error='Falta vehículo';else if(!x.technicianId||!x.start)error='Falta técnico o fecha';else if(!Number.isFinite(new Date(x.start).getTime()))error='Fecha de inicio inválida';else if(!this.technicians().some(t=>t.id===x.technicianId&&t.centroIds.includes(x.centroId)))error='Técnico fuera del centro';const key=`${x.id}|${x.technicianId}|${x.start}`;if(seen.has(key))error='Duplicada en el lote';seen.add(key);return{...x,error}}))} next(){if(this.step()===2)this.validateRows();this.step.update(x=>Math.min(4,x+1))} back(){this.step.update(x=>Math.max(1,x-1))}
 async confirm(){
  if(this.saving()||this.result())return;
  this.wizardError.set('');if(!this.canManage()){this.wizardError.set('No tienes permiso para administrar Programación.');return;}
  this.validateRows();const ready=this.ready();if(!ready.length){this.wizardError.set('Corrige los campos de las actividades antes de continuar.');return;}
  this.saving.set(true);const errors:string[]=[],createdIds=new Set<string>();
  try{for(const x of ready){try{await firstValueFrom(this.api.create({tipo:x.tipo,inicio:new Date(x.start).toISOString(),fin:new Date(new Date(x.start).getTime()+3600000).toISOString(),centroId:x.centroId,vehiculoId:x.vehiculoId,tecnicoUsuarioId:x.technicianId,prioridad:x.priority,observaciones:x.motivo,origen:x.origen,origenEntidadId:x.id,idempotencyKey:'plan-'+x.origen+'-'+x.id}));createdIds.add(x.id);}catch(e:unknown){errors.push(x.vehiculo+': '+this.errorText(e,'No fue posible programar.'));}}
   this.result.set({created:createdIds.size,errors});this.step.set(4);this.needs.update(rows=>rows.filter(x=>!createdIds.has(x.id)));
   try{await this.load();}catch(e:unknown){this.wizardError.set('El lote se procesó, pero no se pudo actualizar la lista. '+this.errorText(e,'Actualiza la página.'));}
  }finally{this.saving.set(false);}
 }
 closeWizard(){if(this.saving())return;const count=this.result()?.created??0;this.wizard.set(false);if(count)this.message.set(count+' programaciones creadas.');}

 async cancel(x:Schedule){const motivo=prompt('Motivo obligatorio de cancelación:')?.trim();if(motivo){await firstValueFrom(this.api.cancel(x.id,motivo));await this.load()}} techs(center:string){return this.technicians().filter(x=>x.centroIds.includes(center))} input(f:any):ScheduleInput{return{tipo:f.tipo,inicio:new Date(f.inicio).toISOString(),fin:new Date(f.fin).toISOString(),centroId:f.centroId,vehiculoId:f.vehicleId||null,tecnicoUsuarioId:f.tecnicoUsuarioId,prioridad:f.prioridad,observaciones:f.observaciones||null,origen:f.origen||'MANUAL',origenEntidadId:f.origenEntidadId||null,rowVersion:f.rowVersion,llantaId:f.llantaId||null,posicionVehiculoId:f.posicionVehiculoId||null}} emptyForm(){return{id:'',tipo:'Inspección',inicio:'',fin:'',centroId:'',vehicleId:'',tecnicoUsuarioId:'',prioridad:'Media',observaciones:'',origen:'MANUAL',origenEntidadId:'',llantaId:'',posicionVehiculoId:'',rowVersion:undefined as string|undefined}} local(v:string){const d=new Date(v);return new Date(d.getTime()-d.getTimezoneOffset()*60000).toISOString().slice(0,16)} date(v:string){return new Intl.DateTimeFormat('es-CO',{day:'2-digit',month:'short'}).format(new Date(v))} time(v:string){return new Intl.DateTimeFormat('es-CO',{hour:'2-digit',minute:'2-digit',hour12:false}).format(new Date(v))}
 openMount(){++this.mountVersion;this.mountLoading.set(false);this.mountError.set('');this.createKey=crypto.randomUUID();this.form=this.emptyForm();this.form.tipo='Montaje';this.mountStep.set(1);this.mountRows=[];this.mountReason='';this.mountVehicle.set(null);this.mountModal.set(true);this.message.set('');}
 changeType(value:string){this.form.tipo=value;if(value==='Montaje'||value==='Reemplazar llanta'||value==='Cambio de juego'){this.modal.set(false);this.openMount();this.form.tipo=value;}}
 async searchMountVehicles(){try{const page=await firstValueFrom(this.http.get<{items:VehicleSummary[]}>('/api/operaciones/vehiculos',{params:{search:this.mountSearch,pageSize:100}}));this.vehicles.set(page.items);}catch(e:any){this.mountError.set(this.errorText(e,'No se pudieron buscar vehículos.'));}}
 async pickMountVehicle(id:string){const version=++this.mountVersion;this.form.vehicleId=id;this.mountVehicle.set(null);this.mountRows=[];this.form.centroId='';this.form.tecnicoUsuarioId='';this.mountError.set('');this.mountLoading.set(false);if(!id)return;this.mountLoading.set(true);try{const vehicle=await firstValueFrom(this.http.get<VehicleDetail>('/api/operaciones/vehiculos/'+id));if(version!==this.mountVersion)return;this.mountVehicle.set(vehicle);this.form.centroId=vehicle.centroId;this.form.tecnicoUsuarioId='';}catch(e:any){if(version===this.mountVersion)this.mountError.set(this.errorText(e,'No se pudo cargar el vehículo.'));}finally{if(version===this.mountVersion)this.mountLoading.set(false);}}
 private mountAssignmentsError(){
  const rows=this.mountRows;
  if(!rows.length)return 'Asigna las llantas a montar.';
  if(this.form.tipo==='Cambio de juego'&&rows.length<2)return 'Cambio de juego requiere al menos dos posiciones.';
  if(this.form.tipo!=='Cambio de juego'&&rows.length!==1)return 'El montaje individual requiere exactamente una posición.';
  if(this.form.tipo==='Reemplazar llanta'&&rows.some(x=>!x.llantaActualId))return 'Reemplazar llanta requiere una posición ocupada.';
  if(rows.some(x=>!x.posicionId||!x.llantaId||x.llantaId===x.llantaActualId))return 'Cada posición requiere una llanta nueva diferente a la actual.';
  if(new Set(rows.map(x=>x.posicionId)).size!==rows.length||new Set(rows.map(x=>x.llantaId)).size!==rows.length)return 'No repitas posiciones ni llantas en el trabajo.';
  return '';
 }
 private mountScheduleError(){return [this.validateForm(),!this.mountReason.trim()?'Indica el motivo del montaje.':'',this.mountReason.length>500?'El motivo admite hasta 500 caracteres.':''].filter(Boolean).join('\n');}
 mountNext(){if(this.saving())return;const step=this.mountStep();let error='';if(step===1&&!this.mountVehicle())error='Selecciona un vehículo.';if(step===3)error=this.mountAssignmentsError();if(step===4)error=this.mountScheduleError();this.mountError.set(error);if(!error)this.mountStep.update(x=>Math.min(5,x+1));}
 async saveMount(){
  if(this.saving())return;
  const error=this.mountAssignmentsError()||this.mountScheduleError();this.mountError.set(error);if(error)return;
  this.saving.set(true);
  try{await firstValueFrom(this.api.create({...this.input(this.form),asignaciones:this.mountRows,motivo:this.mountReason.trim(),idempotencyKey:this.mountKey()}));}
  catch(e:unknown){this.mountError.set(this.errorText(e,'No fue posible programar.'));return;}
  finally{this.saving.set(false);}
  this.mountModal.set(false);await this.saved('Montaje programado con llantas asignadas y comprometidas.');
 }

 private mountKey(){return this.createKey||(this.createKey=crypto.randomUUID());}

}

