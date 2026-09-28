import {Component,input,output,inject,signal,OnInit} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {HttpClient} from '@angular/common/http';
import {firstValueFrom} from 'rxjs';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {CatalogItem} from '../../core/models/api.models';
interface OrdenEnvio{id:string;llanta:string;centroId:string;centro:string;estado:string;resultado?:string|null;loteDisposicionFinalId?:string|null}
interface ItemLote{ordenId:string;llanta:string;estado:string;concepto:string;tecnico:string;fechaEvaluacion:string;aprobador:string;fechaAprobacion:string;fechaRecepcion:string|null;fechaDisposicion:string|null;empresa:string|null;evidencias:number}
interface Lote{id:string;codigo:string;origen:string;destino:string;relevanciaDestino:string|null;estado:string;fechaSalida:string;items:ItemLote[]}
@Component({selector:'app-disposition-lots',imports:[FormsModule],templateUrl:'./disposition-lots.html',styleUrl:'./disposition-lots.scss'})
export class DispositionLots implements OnInit{
 orders=input<OrdenEnvio[]>([]);providers=input<{id:string;nombre:string}[]>([]);changed=output<void>();
 private http=inject(HttpClient);private catalogs=inject(CatalogsApi);
 centers=signal<CatalogItem[]>([]);lots=signal<Lote[]>([]);message=signal('');busy=signal(false);selected=signal<string[]>([]);modal=signal(false);activeLot=signal<Lote|null>(null);action='recibir';items=signal<string[]>([]);
 form={centroDestinoId:'',fechaSalida:'',remision:'',transportador:'',observaciones:''};key='';onlyR1=true;empresa='';concepto='';
 async ngOnInit(){try{this.centers.set(await firstValueFrom(this.catalogs.all('centros',true)));await this.refresh()}catch{this.message.set('No fue posible cargar centros y lotes.')}}
 async refresh(){this.lots.set(await firstValueFrom(this.http.get<Lote[]>('/api/servicios-llanta/disposicion/lotes')))}
 aprobadas(){return this.orders().filter(o=>o.estado==='APROBADA'&&o.resultado==='DISPOSICION'&&!o.loteDisposicionFinalId)}
 destinations(){return this.centers().filter(c=>!this.onlyR1||c.relevancia==='R1').sort((a,b)=>(a.relevancia==='R1'?-1:0)-(b.relevancia==='R1'?-1:0)||a.nombre.localeCompare(b.nombre))}
 toggle(id:string,checked:boolean){this.selected.update(ids=>checked?[...new Set([...ids,id])]:ids.filter(x=>x!==id))}
 toggleItem(id:string,checked:boolean){this.items.update(ids=>checked?[...new Set([...ids,id])]:ids.filter(x=>x!==id))}
 elegidas(){return this.aprobadas().filter(o=>this.selected().includes(o.id))}
 open(){const rows=this.elegidas();if(!rows.length)return;if(new Set(rows.map(o=>o.centroId)).size!==1){this.message.set('Las llantas seleccionadas pertenecen a diferentes centros. Cree un lote por centro de origen.');return;}this.form={centroDestinoId:'',fechaSalida:new Date(Date.now()-new Date().getTimezoneOffset()*60000).toISOString().slice(0,16),remision:'',transportador:'',observaciones:''};this.key=crypto.randomUUID();this.modal.set(true)}
 async create(){if(this.busy()||!this.form.centroDestinoId||!this.form.fechaSalida)return;this.busy.set(true);try{const rows=this.elegidas();await firstValueFrom(this.http.post('/api/servicios-llanta/disposicion/lotes',{ordenIds:rows.map(o=>o.id),centroOrigenId:rows[0]?.centroId,...this.form,fechaSalida:new Date(this.form.fechaSalida).toISOString(),idempotencyKey:this.key}));this.modal.set(false);this.selected.set([]);this.changed.emit();await this.refresh()}catch(e:any){this.message.set(e?.userMessage??e?.error?.message??'No fue posible crear el lote.')}finally{this.busy.set(false)}}
 openAction(lot:Lote,action:string){this.activeLot.set(lot);this.action=action;this.items.set([]);this.empresa='';this.concepto=''}
 actionRows(){return this.activeLot()?.items.filter(i=>this.action==='recibir'?i.estado==='EN_TRANSITO_DISPOSICION':i.estado==='PENDIENTE_DISPOSICION')??[]}
 async confirm(){const lot=this.activeLot();if(!lot||this.busy()||!this.items().length)return;if(this.action==='cerrar'&&(!this.empresa||!this.concepto.trim())){this.message.set('Empresa receptora y concepto de cierre son obligatorios.');return;}this.busy.set(true);try{await firstValueFrom(this.http.post(`/api/servicios-llanta/disposicion/lotes/${lot.id}/${this.action}`,{ordenIds:this.items(),...(this.action==='cerrar'?{proveedorId:this.empresa,observaciones:this.concepto}:{})}));this.activeLot.set(null);this.changed.emit();await this.refresh()}catch(e:any){this.message.set(e?.userMessage??e?.error?.message??'No fue posible procesar la selección.')}finally{this.busy.set(false)}}
}
