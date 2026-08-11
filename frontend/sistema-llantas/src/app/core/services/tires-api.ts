import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { CatalogItem, Page, Tire, TireInput } from '../models/api.models';

const c=(id:string,codigo:string,nombre:string):CatalogItem=>({id,codigo,nombre,activo:true});
const catalogos:Record<string,CatalogItem[]>={
  marcas:[c('michelin','MIC','Michelin'),c('goodyear','GDY','Goodyear'),c('bridgestone','BRG','Bridgestone')],
  referencias:[c('xmulti','XMU','X Multi D'),c('kmax','KMX','KMAX S'),c('r268','R268','R268 Ecopia')],
  dimensiones:[c('295','295','295/80 R22.5'),c('315','315','315/80 R22.5'),c('12r','12R','12 R22.5')],
  'tipos-llanta':[c('radial','RAD','Radial'),c('direccional','DIR','Direccional'),c('traccion','TRA','Tracción')],
  'estados-llanta':[c('disponible','DIS','Disponible'),c('montada','MON','Montada'),c('reparacion','REP','En reparación')],
  centros:[c('bogota','BOG','Bogotá'),c('medellin','MED','Medellín'),c('cali','CAL','Cali')]
};
const inicial:Tire[]=[
  {id:'1',codigo:'LL-000184',serial:'MX932841',marca:'Michelin',referencia:'X Multi D',dimension:'295/80 R22.5',tipo:'Radial',estado:'Montada',centro:'Bogotá',ubicacionActual:'VHC-042 · Posición 3',profundidadInicial:14.2,activo:true,rowVersion:''},
  {id:'2',codigo:'LL-000327',serial:'GY837201',marca:'Goodyear',referencia:'KMAX S',dimension:'315/80 R22.5',tipo:'Direccional',estado:'Disponible',centro:'Medellín',ubicacionActual:'Bodega A · Estante 12',profundidadInicial:17.5,activo:true,rowVersion:''},
  {id:'3',codigo:'LL-000401',serial:'BR109283',marca:'Bridgestone',referencia:'R268 Ecopia',dimension:'295/80 R22.5',tipo:'Radial',estado:'En reparación',centro:'Cali',ubicacionActual:'Taller principal',profundidadInicial:8.1,activo:true,rowVersion:''},
  {id:'4',codigo:'LL-000588',serial:'MX771024',marca:'Michelin',referencia:'X Multi D',dimension:'12 R22.5',tipo:'Tracción',estado:'Disponible',centro:'Bogotá',ubicacionActual:'Bodega B · Estante 04',profundidadInicial:15.8,activo:true,rowVersion:''}
];

@Injectable({providedIn:'root'})
export class TiresApi {
  private readonly key='flota360_demo_llantas';
  private data(){const value=localStorage.getItem(this.key);return value?JSON.parse(value) as Tire[]:inicial;}
  private save(items:Tire[]){localStorage.setItem(this.key,JSON.stringify(items));}
  list(pageNumber=1,search=''):Observable<Page<Tire>>{const term=search.toLowerCase();const all=this.data().filter(x=>x.activo&&(!term||x.codigo.toLowerCase().includes(term)||x.serial.toLowerCase().includes(term)));return of({items:all.slice((pageNumber-1)*20,pageNumber*20),pageNumber,pageSize:20,totalItems:all.length,totalPages:Math.ceil(all.length/20)});}
  create(value:TireInput){const item=this.map(value,crypto.randomUUID());const all=this.data();all.unshift(item);this.save(all);return of(item);}
  update(id:string,value:TireInput){const all=this.data();const index=all.findIndex(x=>x.id===id);const item=this.map(value,id);all[index]=item;this.save(all);return of(item);}
  setActive(id:string,activo:boolean){const all=this.data();const item=all.find(x=>x.id===id);if(item)item.activo=activo;this.save(all);return of(void 0);}
  catalog(type:string):Observable<Page<CatalogItem>>{const items=this.catalogItems(type);return of({items,pageNumber:1,pageSize:100,totalItems:items.length,totalPages:1});}
  private catalogItems(type:string){let custom:Record<string,CatalogItem[]>={};try{custom=JSON.parse(localStorage.getItem('glld_catalogs')??'{}')}catch{}return[...(catalogos[type]??[]),...(custom[type]??[])];}
  private map(v:TireInput,id:string):Tire{const name=(type:string,key:string)=>this.catalogItems(type).find(x=>x.id===key)?.nombre??'—';return{id,codigo:v.codigo.toUpperCase(),serial:v.serial.toUpperCase(),marca:name('marcas',v.marcaId),referencia:name('referencias',v.referenciaId),dimension:name('dimensiones',v.dimensionId),tipo:name('tipos-llanta',v.tipoLlantaId),estado:name('estados-llanta',v.estadoLlantaId),centro:name('centros',v.centroId),ubicacionActual:v.ubicacionActual,profundidadInicial:v.profundidadInicial,activo:true,rowVersion:''};}
}
