import {SingleSelectFilter} from '../../shared/single-select-filter';
import {safeDispositionReturn} from './disposition-models';
import {Component,inject,signal,OnInit} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {Lot,Event,stageLabel,failure,remainingAfterReceipt} from './disposition-models';
import {DispositionTracking} from './disposition-tracking';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-lot-detail',imports:[SingleSelectFilter,DatePipe,FormsModule,RouterLink,DispositionTracking],templateUrl:'./disposition-lot-detail.html',styleUrl:'./disposition-shared.scss'})
export class DispositionLotDetail implements OnInit{
 private api=inject(DispositionApi);private route=inject(ActivatedRoute);auth=inject(AuthService);id=this.route.snapshot.paramMap.get('id')!;backLink=inject(Router).parseUrl(safeDispositionReturn(this.route.snapshot.queryParamMap.get('volver'),'/disposicion-final/lotes'));receive=this.route.snapshot.data['receive']===true;
 data=signal<Lot|null>(null);loading=signal(true);busy=signal(false);error=signal('');success=signal('');selected=signal<string[]>([]);notes='';order='';noveltyOpen=false;resolutionId='';resolution='';label=stageLabel;
 ngOnInit(){void this.load()}
 async load(){this.loading.set(true);try{this.data.set(await firstValueFrom(this.api.lot(this.id)))}catch(e){this.error.set(failure(e))}finally{this.loading.set(false)}}
 events():Event[]{const d=this.data()!;return [{tipo:'ENVIO',etiqueta:'Envío a R1',estadoVisual:'COMPLETADO',fecha:d.fechaSalida,responsable:null,observacion:d.transportador,referencia:d.remision},{tipo:'RECEPCION',etiqueta:d.pendientes?'Recepción pendiente':'Recepción registrada',estadoVisual:d.pendientes?'ACTUAL':'COMPLETADO',fecha:null,responsable:null,observacion:`${d.recibidas} recibidas; ${d.pendientes} pendientes`,referencia:null},{tipo:'CIERRE',etiqueta:'Disposición final',estadoVisual:d.estado==='CERRADO'?'COMPLETADO':'PENDIENTE',fecha:null,responsable:null,observacion:null,referencia:null}]}
 remaining(){const d=this.data();return d?remainingAfterReceipt(d.enviadas,d.recibidas,this.selected().length):0}
 toggle(id:string,checked:boolean){this.selected.update(rows=>checked?[...new Set([...rows,id])]:rows.filter(x=>x!==id))}
 async confirm(){if(this.busy()||!this.selected().length)return;this.busy.set(true);this.error.set('');try{await firstValueFrom(this.api.receive(this.id,this.selected()));this.selected.set([]);this.success.set('Recepción registrada. Las llantas no seleccionadas siguen pendientes.');await this.load()}catch(e){this.error.set(failure(e)+' Revisa la información actualizada y vuelve a seleccionar.');this.selected.set([]);await this.load()}finally{this.busy.set(false)}}
 async novelty(){if(!this.notes.trim()||this.busy())return;this.busy.set(true);try{await firstValueFrom(this.api.novelty(this.id,this.notes,this.order||null));this.notes='';this.noveltyOpen=false;await this.load()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false)}}
 async resolve(){if(!this.resolution.trim()||this.busy())return;this.busy.set(true);try{await firstValueFrom(this.api.resolve(this.resolutionId,this.resolution));this.resolutionId='';this.resolution='';await this.load()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false)}}
 orderOptions(){return (this.data()?.items??[]).map(o=>({value:o.ordenId,label:o.codigo+' · '+o.serial}))}
}

