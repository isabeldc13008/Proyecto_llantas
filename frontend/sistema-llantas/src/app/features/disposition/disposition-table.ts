import {Component,DestroyRef,EventEmitter,OnInit,Output,inject,signal} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router} from '@angular/router';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {firstValueFrom} from 'rxjs';
import {MultiSelectFilter,MultiSelectOption} from '../../shared/multi-select-filter';
import {DispositionApi} from './disposition-api';
import {Filter,Order,Page,failure} from './disposition-models';
import {DispositionReview} from './disposition-review';
@Component({selector:'app-disposition-table',imports:[DatePipe,FormsModule,MultiSelectFilter,DispositionReview],templateUrl:'./disposition-table.html',styleUrls:['./disposition-shared.scss','./disposition-table.scss']})
export class DispositionTable implements OnInit{
 private api=inject(DispositionApi);private route=inject(ActivatedRoute);private router=inject(Router);private destroy=inject(DestroyRef);@Output()changed=new EventEmitter<void>();
 filter:Filter={soloPendientes:true};search='';rows=signal<Page<Order>|null>(null);loading=signal(false);error=signal('');selected=signal<string|null>(null);version=0;
 columns=[{key:'llanta',label:'Llanta',field:'llantaIds'},{key:'centro',label:'Centro',field:'centroIds'},{key:'estado',label:'Estado',field:'estados'}];facets:Record<string,{text:string,page:number,options:MultiSelectOption[],more:boolean,busy:boolean,error:string,version:number}>={};labels:Record<string,string>={};evidenceOptions=[{value:'true',label:'Con evidencia'},{value:'false',label:'Sin evidencia'}];timers:Record<string,ReturnType<typeof setTimeout>>={};
 ngOnInit(){this.destroy.onDestroy(()=>Object.values(this.timers).forEach(clearTimeout));this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroy)).subscribe(p=>{this.filter={llantaTexto:p.has('llantaTexto')?p.get('llantaTexto')!:undefined,centroTexto:p.has('centroTexto')?p.get('centroTexto')!:undefined,estadoTexto:p.has('estadoTexto')?p.get('estadoTexto')!:undefined,buscar:p.get('buscar')||'',llantaIds:p.getAll('llantaIds'),centroIds:p.getAll('centroIds'),estados:p.getAll('estados'),centroId:p.get('centroId')||'',estado:p.get('estado')||'',desde:p.get('desde')||'',hasta:p.get('hasta')||'',conEvidencia:p.has('conEvidencia')?p.get('conEvidencia')==='true':undefined,ordenarPor:p.get('ordenarPor')||'fecha',descendente:p.get('descendente')==='true',soloPendientes:p.get('soloPendientes')!=='false',pageNumber:Number(p.get('pagina'))||1};this.search=this.filter.buscar||'';void this.load();for(const c of this.columns)void this.loadFacet(c.key)});}
 async load(){const v=++this.version;this.loading.set(true);this.error.set('');try{const p=await firstValueFrom(this.api.orders(this.filter));if(v===this.version)this.rows.set(p)}catch(e){if(v===this.version){this.rows.set(null);this.error.set(failure(e))}}finally{if(v===this.version)this.loading.set(false)}}
 apply(page=1){void this.router.navigate([],{relativeTo:this.route,queryParams:{...this.filter,buscar:this.search||null,pageNumber:null,pagina:page},replaceUrl:true})}
 clear(){this.filter={soloPendientes:true};this.search='';this.apply()}
 values(field:string){return (this.filter as any)[field]??[]}
 set(field:string,values:string[]){(this.filter as any)[field]=values;const c=this.columns.find(c=>c.field===field);if(c)delete (this.filter as any)[c.key+"Texto"];this.apply()}
 allSelected(key:string){return (this.filter as any)[key+"Texto"]!==undefined}
 selectMatches(key:string,text:string){const c=this.columns.find(c=>c.key===key)!;(this.filter as any)[c.field]=[];(this.filter as any)[key+"Texto"]=text;this.apply()}
 sort(column:string){this.filter.descendente=this.filter.ordenarPor===column?!this.filter.descendente:false;this.filter.ordenarPor=column;this.apply()}
 facet(key:string){return this.facets[key]??={text:'',page:1,options:[],more:false,busy:false,error:'',version:0}}
 searchFacet(key:string,text:string){this.facet(key).text=text;clearTimeout(this.timers[key]);this.timers[key]=setTimeout(()=>void this.loadFacet(key),250)}
 async loadFacet(key:string,more=false){const f=this.facet(key),v=++f.version;f.busy=true;f.error='';const page=more?f.page+1:1;try{const p=await firstValueFrom(this.api.facets(key,f.text,page,this.filter));if(v!==f.version)return;const opts=p.items.map(x=>({value:x.valor,label:x.etiqueta}));for(const o of opts)this.labels[o.value]=o.label;f.options=more?[...f.options,...opts]:opts;f.page=page;f.more=p.pageNumber<p.totalPages}catch(e){if(v===f.version)f.error=failure(e)}finally{if(v===f.version)f.busy=false}}
 evidence(values:string[]){this.filter.conEvidencia=values.length===1?values[0]==='true':undefined;this.apply()}
 removeChip(field:string,value:string){this.set(field,this.values(field).filter((v:string)=>v!==value))}
 chips(){return this.columns.flatMap(c=>this.values(c.field).map((v:string)=>({field:c.field,value:v,label:`${c.label}: ${this.labels[v]??v}`})))}
 refreshed(){void this.load();this.changed.emit()}
}


