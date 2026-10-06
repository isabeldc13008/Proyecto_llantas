import {Component,inject,signal,OnInit,computed} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {Center,Order,Page,Provider,actaCount,failure,localDate} from './disposition-models';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-lot-editor',imports:[DatePipe,FormsModule,RouterLink],templateUrl:'./disposition-lot-editor.html',styleUrl:'./disposition-shared.scss'})
export class DispositionLotEditor implements OnInit{
 private api=inject(DispositionApi);private catalogs=inject(CatalogsApi);private route=inject(ActivatedRoute);private router=inject(Router);auth=inject(AuthService);
 dispatch=this.route.snapshot.data['kind']==='despachos';center=this.route.snapshot.queryParamMap.get('centroId')??'';search='';centers=signal<Center[]>([]);providers=signal<Provider[]>([]);page=signal<Page<Order>|null>(null);selected=signal<Order[]>([]);loading=signal(false);busy=signal(false);error=signal('');key=crypto.randomUUID();private version=0;
 form={destination:'',provider:'',date:localDate(),carrier:'',plate:'',remission:'',notes:''};
 groups=computed(()=>{const groups=new Map<string,{name:string;rows:Order[]}>();for(const o of this.page()?.items??[]){if(!groups.has(o.centroOrigen.id))groups.set(o.centroOrigen.id,{name:o.centroOrigen.nombre,rows:[]});groups.get(o.centroOrigen.id)!.rows.push(o)}return [...groups.values()]});
 selectedGroups=computed(()=>{const m=new Map<string,{name:string;count:number}>();for(const o of this.selected()){if(!m.has(o.centroOrigen.id))m.set(o.centroOrigen.id,{name:o.centroOrigen.nombre,count:0});m.get(o.centroOrigen.id)!.count++}return [...m.values()]});actas=computed(()=>actaCount(this.selected()));
 async ngOnInit(){try{const[c,p]=await Promise.all([firstValueFrom(this.catalogs.all('centros',true)),firstValueFrom(this.api.providers())]);this.centers.set(c);this.providers.set(p.filter(x=>!x.tipo||x.tipo==='DisposicionFinal'));if(this.center)await this.load()}catch(e){this.error.set(failure(e))}}
 r1s(){return this.centers().filter(c=>c.relevancia==='R1')}
 centerName(){return this.centers().find(c=>c.id===this.center)?.nombre??''}
 changeCenter(){this.selected.set([]);this.key=crypto.randomUUID();void this.load()}
 async load(number=1){const v=++this.version;this.page.set(null);if(!this.center)return;this.loading.set(true);this.error.set('');try{const f={buscar:this.search,centroId:this.center,pageNumber:number,estado:this.dispatch?'':'LISTAS_R1'};const p=await firstValueFrom(this.dispatch?this.api.available(f,this.center):this.api.orders(f));if(v===this.version)this.page.set(p)}catch(e){if(v===this.version)this.error.set(failure(e))}finally{if(v===this.version)this.loading.set(false)}}
 has(id:string){return this.selected().some(o=>o.ordenId===id)}
 toggle(o:Order,checked:boolean){this.selected.update(rows=>checked?(rows.some(r=>r.ordenId===o.ordenId)?rows:[...rows,o]):rows.filter(r=>r.ordenId!==o.ordenId))}
 selectVisible(checked:boolean){for(const o of this.page()?.items??[])this.toggle(o,checked)}
 allVisible(){return !!this.page()?.items.length&&this.page()!.items.every(o=>this.has(o.ordenId))}
 outside(){return this.selected().filter(o=>!this.page()?.items.some(r=>r.ordenId===o.ordenId)).length}
 async create(){if(this.busy()||!this.selected().length||!this.form.date)return;this.busy.set(true);this.error.set('');try{const common={ordenIds:this.selected().map(o=>o.ordenId),fechaSalida:new Date(this.form.date).toISOString(),transportador:this.form.carrier,placa:this.form.plate,remision:this.form.remission||null,observaciones:this.form.notes||null,idempotencyKey:this.key};const request=this.dispatch?this.api.createDispatch({...common,centroR1Id:this.center,proveedorId:this.form.provider}):this.api.createLot({...common,centroOrigenId:this.center,centroDestinoId:this.form.destination});const result=await firstValueFrom(request);await this.router.navigate(['/disposicion-final',this.dispatch?'despachos':'lotes',result.id])}catch(e){this.error.set(failure(e))}finally{this.busy.set(false)}}
}
