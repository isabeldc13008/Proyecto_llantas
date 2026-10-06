import {Component,inject,signal,OnInit} from '@angular/core';
import {DatePipe} from '@angular/common';
import {Router,RouterLink} from '@angular/router';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {Summary,Center,Ref,Page,Detail,Evidence,failure} from './disposition-models';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-home',imports:[DatePipe,RouterLink,FormsModule],templateUrl:'./disposition-home.html',styleUrls:['./disposition-shared.scss','./disposition-home.scss']})
export class DispositionHome implements OnInit{
 private api=inject(DispositionApi);private catalogs=inject(CatalogsApi);private router=inject(Router);auth=inject(AuthService);
 data=signal<Summary|null>(null);centers=signal<Center[]>([]);loading=signal(true);error=signal('');center='';newOrder=false;search='';tires=signal<Page<Ref>|null>(null);tire='';reason='';busy=signal(false);formError=signal('');private version=0;
 async ngOnInit(){await this.load();try{this.centers.set(await firstValueFrom(this.catalogs.all('centros',true)))}catch{}}
 async load(){const version=++this.version;this.loading.set(true);this.error.set('');this.data.set(null);try{const d=await firstValueFrom(this.api.summary({centroId:this.center}));if(version===this.version){this.data.set(d);this.details.set({});for(const row of d.atencion)void this.loadRow(row,version)}}catch(e){if(version===this.version)this.error.set(failure(e))}finally{if(version===this.version)this.loading.set(false)}}
 async lookup(page=1){this.formError.set('');try{this.tires.set(await firstValueFrom(this.api.tires(this.search,page)))}catch(e){this.formError.set(failure(e))}}
 async create(){if(this.busy()||!this.tire||!this.reason.trim())return;this.busy.set(true);try{const o=await firstValueFrom(this.api.createOrder(this.tire,this.reason));await this.router.navigate(['/disposicion-final/ordenes',o.id])}catch(e){this.formError.set(failure(e))}finally{this.busy.set(false)}}
 scopeLink(route:string){const tree=this.router.parseUrl(route);if(this.center)tree.queryParams['centroId']=this.center;return tree}
 details=signal<Record<string,Detail>>({});rowErrors=signal<Record<string,string>>({});working=signal<string[]>([]);expanded=signal<string|null>(null);notice=signal('');
 rowId(row:Summary['atencion'][number]){return row.ruta.split('/').pop()!}
 async loadRow(row:Summary['atencion'][number],version=this.version){try{const d=await firstValueFrom(this.api.detail(this.rowId(row)));if(version===this.version)this.details.update(x=>({...x,[row.ruta]:d}))}catch(e){if(version===this.version)this.rowErrors.update(x=>({...x,[row.ruta]:failure(e)}))}}
 async approveRow(row:Summary['atencion'][number]){if(this.working().includes(row.ruta)||!this.auth.has('servicios_llanta.gestionar')||!this.details()[row.ruta]?.accionesPermitidas.includes('APROBAR'))return;this.working.update(x=>[...x,row.ruta]);this.rowErrors.update(x=>({...x,[row.ruta]:''}));try{await firstValueFrom(this.api.approve(this.rowId(row)));this.notice.set(`${row.codigo}: aprobación registrada.`);await this.load()}catch(e){this.rowErrors.update(x=>({...x,[row.ruta]:failure(e)}));await this.loadRow(row)}finally{this.working.update(x=>x.filter(r=>r!==row.ruta))}}
 async uploadRow(row:Summary['atencion'][number],event:globalThis.Event){const input=event.target as HTMLInputElement,file=input.files?.[0];if(!file||this.working().includes(row.ruta))return;this.working.update(x=>[...x,row.ruta]);this.rowErrors.update(x=>({...x,[row.ruta]:''}));try{await firstValueFrom(this.api.evidence(this.rowId(row),file));await this.loadRow(row);this.expanded.set(row.ruta);this.notice.set(`${row.codigo}: evidencia cargada.`)}catch(e){this.rowErrors.update(x=>({...x,[row.ruta]:failure(e)}))}finally{input.value='';this.working.update(x=>x.filter(r=>r!==row.ruta))}}
 async viewEvidence(row:Summary['atencion'][number],e:Evidence){const tab=window.open('about:blank','_blank');if(!tab){this.rowErrors.update(x=>({...x,[row.ruta]:'Permite abrir una pestaña para ver la evidencia.'}));return}tab.opener=null;try{const blob=await firstValueFrom(this.api.file(e.id,true));const url=URL.createObjectURL(blob);tab.location.href=url;setTimeout(()=>URL.revokeObjectURL(url),60000)}catch(error){tab.close();this.rowErrors.update(x=>({...x,[row.ruta]:failure(error)}))}}}

