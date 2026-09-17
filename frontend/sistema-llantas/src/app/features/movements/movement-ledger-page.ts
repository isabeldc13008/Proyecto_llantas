import {CommonModule} from '@angular/common';
import {HttpClient,HttpParams} from '@angular/common/http';
import {Component,ElementRef,HostListener,OnInit,computed,inject,signal} from '@angular/core';
import {RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {AuthService} from '../../core/auth/auth.service';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {DataTableToolbar,TableColumn,TableFilter} from '../../shared/data-table-toolbar';
interface Move{id:string;numero:string;fecha:string;tipo:string;llantaId:string;llanta:string;serial:string;origen:string;destino:string;vehiculoPosicion:string;centro:string;kilometrajeVehiculo:number|null;kilometrosTramo:number|null;usuario:string;actividadProgramadaId:string|null;motivo:string;observaciones:string|null;estado:string;vehiculoInterno:string|null;vehiculoPlaca:string|null;posicionOrigen:string|null;posicionDestino:string|null;centroOrigen:string|null;centroDestino:string|null;solicitudId:string|null}
interface Page<T>{items:T[];pageNumber:number;pageSize:number;totalItems:number;totalPages:number}
interface Request{id:string;tipo:string;estado:string;centroId:string;centro:string;llantaId:string;llanta:string;posicionOrigenId:string|null;posicionDestinoId:string|null;tipoDestino:string;centroDestinoId:string|null;motivo:string;observaciones:string|null;solicitante:string;aprobador:string|null;motivoRechazo:string|null;fecha:string;fechaRecepcionDestino:string|null}
@Component({selector:'app-movement-ledger',imports:[CommonModule,RouterLink,DataTableToolbar],templateUrl:'./movement-ledger-page.html',styleUrl:'./movement-ledger-page.scss'})
export class MovementLedgerPage implements OnInit{
 private http=inject(HttpClient);private catalogs=inject(CatalogsApi);readonly auth=inject(AuthService);moves=signal<Move[]>([]);requests=signal<Request[]>([]);centers=signal<{id:string;nombre:string}[]>([]);loading=signal(true);message=signal('');tab=signal<'moves'|'requests'|'transfers'|'approvals'>('moves');selected=signal<Move|null>(null);page=signal(1);total=signal(0);search='';filterValues:Record<string,unknown>={};sortBy='fecha';
 readonly columns:TableColumn[]=[{key:'fecha',label:'Fecha',required:true},{key:'numero',label:'Movimiento'},{key:'tipo',label:'Tipo',required:true},{key:'motivo',label:'Motivo'},{key:'llanta',label:'Llanta',required:true},{key:'vehiculo',label:'Vehículo / posición'},{key:'ruta',label:'Origen / destino',required:true},{key:'centro',label:'Centro'},{key:'usuario',label:'Usuario'},{key:'estado',label:'Estado'},{key:'acciones',label:'Acciones',required:true}];
 visibleColumns=this.columns.filter(x=>!['motivo','usuario'].includes(x.key)).map(x=>x.key); success=signal('');actionBusy=signal(false);private refreshVersion=0;
 readonly filters=computed<TableFilter[]>(()=>[{key:'fecha',label:'Rango de fechas',type:'date-range'},{key:'numero',label:'Número de movimiento',type:'text'},{key:'tipo',label:'Tipo de movimiento',type:'select',options:[...new Set(this.moves().map(x=>x.tipo))].map(x=>({value:x,label:x}))},{key:'centroId',label:'Centro',type:'select',options:this.centers().map(x=>({value:x.id,label:x.nombre}))},{key:'usuario',label:'Usuario / técnico',type:'text'},{key:'origen',label:'Origen',type:'text'},{key:'destino',label:'Destino',type:'text'}]);
 readonly visibleRequests=computed(()=>this.requests().filter(x=>this.tab()==='transfers'?!!x.centroDestinoId:this.tab()==='approvals'?x.estado==='PENDIENTE_APROBACION':true));
 async ngOnInit(){await this.refresh()}
 async refresh(page=1){
  const version=++this.refreshVersion;this.loading.set(true);this.message.set('');
  const current=()=>version===this.refreshVersion;
  const tasks:{name:string;run:()=>Promise<void>}[]=[
   {name:'movimientos',run:async()=>{const data=await firstValueFrom(this.http.get<Page<Move>>('/api/operaciones/movimientos',{params:this.params(page)}));if(current()){this.moves.set(data.items);this.page.set(data.pageNumber);this.total.set(data.totalItems);}}},
   {name:'solicitudes',run:async()=>{const rows=await firstValueFrom(this.http.get<Request[]>('/api/operaciones/solicitudes'));if(current())this.requests.set(rows);}}
  ];
  if(!this.centers().length)tasks.push({name:'centros',run:async()=>{const rows=await firstValueFrom(this.catalogs.all('centros',true));if(current())this.centers.set(rows);}});
  const results=await Promise.allSettled(tasks.map(task=>task.run()));
  if(!current())return;
  const failed=tasks.filter((_,i)=>results[i].status==='rejected').map(task=>task.name);
  this.message.set(failed.length?'No se pudo actualizar: '+failed.join(', ')+'. Los datos de esas secciones pueden estar desactualizados.':'');
  this.loading.set(false);
 }
 shortNumber(numero:string){return numero.replace(/^MOV-(\d{8})(\d{6})\d*(?:-.*)?$/,'MOV-$1-$2');}
 stateLabel(estado:string){return estado.replaceAll('_',' ');}
 centerName(id:string|null){return this.centers().find(x=>x.id===id)?.nombre??'Centro destino';}
 private element=inject<ElementRef<HTMLElement>>(ElementRef);private detailTrigger:HTMLElement|null=null;
 openDetail(move:Move){this.detailTrigger=document.activeElement as HTMLElement;this.selected.set(move);setTimeout(()=>this.element.nativeElement.querySelector<HTMLButtonElement>('[aria-label="Cerrar detalle"]')?.focus());}
 closeDetail(){this.selected.set(null);this.detailTrigger?.focus();}
 @HostListener('keydown',['$event']) onDetailKey(event:KeyboardEvent){
  if(!this.selected())return;
  if(event.key==='Escape'){event.preventDefault();this.closeDetail();}
  if(event.key==='Tab'){
   const controls=this.element.nativeElement.querySelectorAll<HTMLElement>('[role="dialog"] button,[role="dialog"] a');
   const first=controls[0],last=controls[controls.length-1];
   if(event.shiftKey&&document.activeElement===first){event.preventDefault();last?.focus();}
   else if(!event.shiftKey&&document.activeElement===last){event.preventDefault();first?.focus();}
  }
 }
 private params(page:number){let p=new HttpParams().set('pagina',page).set('tamano',20);if(this.search.trim())p=p.set('buscar',this.search.trim());const map:Record<string,string>={numero:'numero',tipo:'tipo',centroId:'centroId',usuario:'usuario',origen:'origen',destino:'destino',fechaMin:'desde',fechaMax:'hasta'};for(const[k,v]of Object.entries(map)){const value=this.filterValues[k];if(value!==''&&value!=null)p=p.set(v,String(value))}return p}
 show(key:string){return this.visibleColumns.includes(key)} clear(){this.search='';this.filterValues={};void this.refresh(1)} apply(){void this.refresh(1)}
 async resolve(x:Request,approve:boolean){
  const motivo=approve?'':prompt('Motivo obligatorio del rechazo:')?.trim();if(!approve&&!motivo)return;
  await this.perform('/api/operaciones/solicitudes/'+x.id+'/resolver',{aprobar:approve,motivo},'Solicitud procesada.');
 }
 async receive(x:Request){await this.perform('/api/operaciones/solicitudes/'+x.id+'/recibir',{},'Traslado recibido.');}
 private async perform(url:string,body:unknown,success:string){
  if(this.actionBusy())return;this.actionBusy.set(true);this.success.set('');
  try{await firstValueFrom(this.http.post(url,body));this.success.set(success);await this.refresh(this.page());}
  catch(e:any){this.message.set(e?.userMessage??'No fue posible procesar la solicitud.');}
  finally{this.actionBusy.set(false);}
 }
}
