import {CommonModule} from '@angular/common';
import {Component,OnInit,computed,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {forkJoin} from 'rxjs';
import {Router} from '@angular/router';
import {CatalogItem,InventoryMetrics,InventoryReservation,Tire} from '../../core/models/api.models';
import {InventoryApi} from '../../core/services/inventory-api';
import {TiresApi} from '../../core/services/tires-api';
import {DataTableToolbar,TableColumn,TableFilter} from '../../shared/data-table-toolbar';
import {AuthService} from '../../core/auth/auth.service';

type InventoryTab='disponibles'|'transito'|'servicio'|'bloqueadas'|'historial';
@Component({selector:'app-inventory-page',imports:[CommonModule,FormsModule,DataTableToolbar],templateUrl:'./inventory-page.html',styleUrl:'./inventory-page.scss'})
export class InventoryPage implements OnInit{
 private readonly tiresApi=inject(TiresApi);private readonly api=inject(InventoryApi);private readonly router=inject(Router);readonly auth=inject(AuthService);
 tires=signal<Tire[]>([]);reservations=signal<InventoryReservation[]>([]);metrics=signal<InventoryMetrics>({disponibles:0,enReparacion:0,enReencauche:0,enTraslado:0,bloqueadas:0,conAtencion:0});catalogs=signal<Record<string,CatalogItem[]>>({});loading=signal(true);error=signal('');total=signal(0);page=signal(1);
 tab=signal<InventoryTab>('disponibles');search='';sortBy='codigo';filterValues:Record<string,unknown>={};visibleColumns=['codigo','marca','estado','centro','profundidad','kilometraje','reencauches','atencion','acciones'];
 readonly columns:TableColumn[]=[{key:'codigo',label:'Código / serial',required:true},{key:'marca',label:'Marca / referencia'},{key:'estado',label:'Estado'},{key:'centro',label:'Centro / ubicación'},{key:'profundidad',label:'Profundidad'},{key:'kilometraje',label:'Km acumulados'},{key:'reencauches',label:'Reencauches'},{key:'atencion',label:'Atención'},{key:'acciones',label:'Acciones',required:true}];
 readonly filters=computed<TableFilter[]>(()=>[{key:'centroIds',label:'Centro',type:'multi',options:this.options('centros')},{key:'estados',label:'Estado',type:'multi',options:this.options('estados-llanta',true)},{key:'marcaIds',label:'Marca',type:'multi',options:this.options('marcas')},{key:'referenciaIds',label:'Referencia',type:'multi',options:this.options('referencias')},{key:'dimensionIds',label:'Dimensión',type:'multi',options:this.options('dimensiones')},{key:'tipoLlantaIds',label:'Tipo',type:'multi',options:this.options('tipos-llanta')},{key:'tieneReencauches',label:'Reencauchada',type:'multi',options:[{value:'true',label:'Sí'},{value:'false',label:'No'}]}]);
 ngOnInit(){forkJoin(Object.fromEntries(['centros','estados-llanta','marcas','referencias','dimensiones','tipos-llanta'].map(x=>[x,this.tiresApi.catalog(x)]))).subscribe(r=>this.catalogs.set(Object.fromEntries(Object.entries(r).map(([k,v])=>[k,v.items]))));this.load()}
 setTab(tab:InventoryTab){this.tab.set(tab);this.page.set(1);this.load()}
 load(page=1){this.loading.set(true);this.error.set('');if(this.tab()==='historial'){forkJoin({reservations:this.api.reservations(),metrics:this.api.metrics()}).subscribe({next:r=>{this.reservations.set(r.reservations);this.metrics.set(r.metrics);this.loading.set(false)},error:e=>{this.error.set(e.userMessage);this.loading.set(false)}});return}const filters={...this.filterValues};const states=this.tabStates();if(states.length)filters['estados']=states;forkJoin({list:this.tiresApi.list(page,this.search,this.sortBy,filters),metrics:this.api.metrics(),reservations:this.api.reservations()}).subscribe({next:r=>{this.tires.set(r.list.items);this.total.set(r.list.totalItems);this.page.set(r.list.pageNumber);this.metrics.set(r.metrics);this.reservations.set(r.reservations);this.loading.set(false)},error:e=>{this.error.set(e.userMessage);this.loading.set(false)}})}
 clear(){this.search='';this.filterValues={};this.load(1)}
 show(key:string){return this.visibleColumns.includes(key)}
 open(t:Tire){void this.router.navigate(['/llantas'],{queryParams:{llantaId:t.id}})}
 reserved(t:Tire){return this.reservations().some(x=>x.llantaId===t.id)}
 reserve(t:Tire){const reason=prompt(`Motivo o programación para reservar ${t.codigo}:`);if(reason===null)return;this.api.reserve(t.id,reason).subscribe({next:()=>this.load(this.page()),error:e=>this.error.set(e.userMessage)})}
 release(t:Tire){if(confirm(`¿Liberar la reserva de ${t.codigo}?`))this.api.release(t.id).subscribe({next:()=>this.load(this.page()),error:e=>this.error.set(e.userMessage)})}
 locate(t:Tire){const zone=prompt('Zona o bodega:',t.ubicacionActual==='Ubicación no definida'?'':t.ubicacionActual);if(zone===null)return;const rack=prompt('Ubicación o rack (opcional):','');if(rack===null)return;this.api.location(t.id,zone,rack).subscribe({next:()=>this.load(this.page()),error:e=>this.error.set(e.userMessage)})}
 statusTone(v:string){v=v.toLowerCase();return v.includes('dispon')?'positive':v.includes('traslado')||v.includes('repar')||v.includes('reencauch')?'warning':v.includes('bloque')?'danger':'neutral'}
 canOperate(){return this.auth.has('operaciones.solicitar')||this.auth.has('operaciones.ejecutar')||this.auth.has('operaciones.aprobar')}
 private options(key:string,useName=false){return(this.catalogs()[key]??[]).map(x=>({value:useName?x.nombre:x.id,label:x.nombre}))}
 private tabStates(){const states=(this.catalogs()['estados-llanta']??[]).map(x=>x.nombre);const has=(...parts:string[])=>states.filter(s=>parts.some(p=>s.toLowerCase().includes(p)));switch(this.tab()){case'disponibles':return has('dispon');case'transito':return has('traslado','tránsito','transito');case'servicio':return has('repar','reencauch','servicio');case'bloqueadas':return has('bloque');default:return[]}}
}
