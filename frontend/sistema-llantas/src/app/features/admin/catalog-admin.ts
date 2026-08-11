import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CatalogItem } from '../../core/models/api.models';
import { CENTERS } from '../../core/data/centers';

interface CatalogType {key:string;name:string;description:string}
const base:Record<string,CatalogItem[]>={
 marcas:[item('michelin','MIC','Michelin'),item('goodyear','GDY','Goodyear'),item('bridgestone','BRG','Bridgestone')],
 referencias:[item('xmulti','XMU','X Multi D'),item('kmax','KMX','KMAX S'),item('r268','R268','R268 Ecopia')],
 dimensiones:[item('295','295','295/80 R22.5'),item('315','315','315/80 R22.5'),item('12r','12R','12 R22.5')],
 'tipos-llanta':[item('radial','RAD','Radial'),item('direccional','DIR','Direccional'),item('traccion','TRA','Tracción')],
 'estados-llanta':[item('disponible','DIS','Disponible'),item('montada','MON','Montada'),item('reparacion','REP','En reparación')],
 centros:CENTERS.map(c=>item(`${c.relevance}-${c.code}`,c.code,`${c.name} · ${c.relevance}`)),
 talleres:[item('taller-bog','TB01','Taller principal Bogotá')],tecnicos:[item('laura','LR','Laura Ruiz'),item('carlos','CM','Carlos Mendoza')],
 motivos:[item('desgaste','DES','Desgaste crítico'),item('averia','AVE','Daño o avería')],
 tolerancias:[item('prof-critica','PC','Profundidad crítica: 3 mm'),item('dif-desgaste','DD','Diferencia máxima: 3 mm')]
};
function item(id:string,codigo:string,nombre:string):CatalogItem{return{id,codigo,nombre,activo:true}}

@Component({selector:'app-catalog-admin',imports:[FormsModule],templateUrl:'./catalog-admin.html',styleUrl:'./catalog-admin.scss'})
export class CatalogAdmin{
 readonly types:CatalogType[]=[{key:'marcas',name:'Marcas',description:'Fabricantes de llantas'},{key:'referencias',name:'Referencias',description:'Líneas y modelos'},{key:'dimensiones',name:'Dimensiones',description:'Medidas homologadas'},{key:'tipos-llanta',name:'Tipos de llanta',description:'Clasificación operativa'},{key:'estados-llanta',name:'Estados',description:'Estados del ciclo de vida'},{key:'centros',name:'Centros',description:'Centros operativos'},{key:'talleres',name:'Talleres',description:'Talleres propios y aliados'},{key:'tecnicos',name:'Técnicos',description:'Personal autorizado'},{key:'motivos',name:'Motivos',description:'Motivos de movimientos'},{key:'tolerancias',name:'Tolerancias',description:'Reglas y límites'}];
 selected=signal(this.types[0]); custom=signal<Record<string,CatalogItem[]>>(this.read());showForm=false;codigo='';nombre='';search='';message='';
 records(){const key=this.selected().key;return[...(base[key]??[]),...(this.custom()[key]??[])].filter(x=>!this.search||x.codigo.toLowerCase().includes(this.search.toLowerCase())||x.nombre.toLowerCase().includes(this.search.toLowerCase()))}
 choose(type:CatalogType){this.selected.set(type);this.search='';this.showForm=false}
 open(){this.codigo='';this.nombre='';this.showForm=true}
 save(){if(!this.codigo.trim()||!this.nombre.trim())return;const key=this.selected().key;const created:CatalogItem={id:crypto.randomUUID(),codigo:this.codigo.trim().toUpperCase(),nombre:this.nombre.trim(),activo:true};this.custom.update(all=>({...all,[key]:[...(all[key]??[]),created]}));localStorage.setItem('glld_catalogs',JSON.stringify(this.custom()));this.showForm=false;this.message=`${created.nombre} fue agregado a ${this.selected().name}.`;setTimeout(()=>this.message='',2500)}
 toggle(record:CatalogItem){record.activo=!record.activo;this.custom.update(all=>({...all}));}
 private read(){try{return JSON.parse(localStorage.getItem('glld_catalogs')??'{}')}catch{return{}}}
}
