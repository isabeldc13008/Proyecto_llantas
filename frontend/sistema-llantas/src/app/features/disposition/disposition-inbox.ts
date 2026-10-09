import {SingleSelectFilter} from '../../shared/single-select-filter';
import {Component,inject,signal,OnInit,DestroyRef} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {firstValueFrom} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {Filter,Page,Order,Lot,Dispatch,Center,stageLabel,orderStates,failure} from './disposition-models';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-inbox',imports:[SingleSelectFilter,DatePipe,FormsModule,RouterLink],templateUrl:'./disposition-inbox.html',styleUrl:'./disposition-shared.scss'})
export class DispositionInbox implements OnInit{
 private api=inject(DispositionApi);private route=inject(ActivatedRoute);readonly router=inject(Router);private destroy=inject(DestroyRef);private catalogs=inject(CatalogsApi);auth=inject(AuthService);
 mode=this.route.snapshot.data['kind']??'ordenes';title=this.mode==='lotes'?'Lotes R1':this.mode==='despachos'?'Despachos a Sistema Verde':'Bandeja de llantas';
 rows=signal<Page<any>|null>(null);centers=signal<Center[]>([]);loading=signal(true);error=signal('');filter:Filter={};label=stageLabel;states=this.mode==='ordenes'?orderStates:this.mode==='lotes'?[['EN_TRANSITO','En tránsito'],['RECEPCION_PARCIAL','Recepción parcial'],['RECIBIDO','Recibido'],['CERRADO','Cerrado']]:[['ENVIADO','Enviado'],['CERRADO','Cerrado']];private version=0;
 ngOnInit(){this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroy)).subscribe(p=>{this.filter={buscar:p.get('buscar')??'',centroId:p.get('centroId')??'',estado:p.get('estado')??'',pageNumber:Math.max(1,Number(p.get('pagina'))||1)};void this.load()});void firstValueFrom(this.catalogs.all('centros',true)).then(c=>this.centers.set(c)).catch(()=>{});}
 change(page=1){void this.router.navigate([],{relativeTo:this.route,queryParams:{buscar:this.filter.buscar||null,centroId:this.filter.centroId||null,estado:this.filter.estado||null,pagina:page}})}
 async load(){const version=++this.version;this.loading.set(true);this.rows.set(null);this.error.set('');try{const q=this.mode==='lotes'?this.api.lots(this.filter):this.mode==='despachos'?this.api.dispatches(this.filter):this.api.orders(this.filter);const page=await firstValueFrom(q as any) as Page<Order|Lot|Dispatch>;if(version===this.version)this.rows.set(page)}catch(e){if(version===this.version)this.error.set(failure(e))}finally{if(version===this.version)this.loading.set(false)}}
 target(row:any){return ['/disposicion-final',this.mode,row.ordenId??row.id]}
 centerOptions(){return this.centers().map(c=>({value:c.id,label:c.nombre}))} stateOptions(){return this.states.map(s=>({value:s[0],label:s[1]}))}
}

