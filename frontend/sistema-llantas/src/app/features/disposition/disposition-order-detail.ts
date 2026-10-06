import {safeDispositionReturn} from './disposition-models';
import {Component,inject,signal,OnInit} from '@angular/core';
import {DatePipe,DecimalPipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {DispositionApi,downloadBlob} from './disposition-api';
import {Detail,Evidence,failure} from './disposition-models';
import {DispositionTracking} from './disposition-tracking';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-order-detail',imports:[DatePipe,DecimalPipe,FormsModule,RouterLink,DispositionTracking],templateUrl:'./disposition-order-detail.html',styleUrl:'./disposition-shared.scss'})
export class DispositionOrderDetail implements OnInit{
 private api=inject(DispositionApi);private route=inject(ActivatedRoute);auth=inject(AuthService);id=this.route.snapshot.paramMap.get('id')!;backLink=inject(Router).parseUrl(safeDispositionReturn(this.route.snapshot.queryParamMap.get('volver'),'/disposicion-final/bandeja'));
 data=signal<Detail|null>(null);loading=signal(true);busy=signal(false);error=signal('');action='';notes='';reusable=false;
 ngOnInit(){void this.load()}
 async load(){this.loading.set(true);this.error.set('');try{this.data.set(await firstValueFrom(this.api.detail(this.id)))}catch(e){this.error.set(failure(e))}finally{this.loading.set(false)}}
 async run(){if(this.busy()||!this.action)return;if(this.action!=='APROBAR'&&!this.notes.trim())return;this.busy.set(true);this.error.set('');try{const request=this.action==='EVALUAR'?this.api.evaluate(this.id,this.reusable,this.notes):this.action==='RECHAZAR'?this.api.reject(this.id,this.notes):this.api.approve(this.id);await firstValueFrom(request);this.action='';this.notes='';await this.load()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false)}}
 async upload(event:globalThis.Event){const input=event.target as HTMLInputElement,file=input.files?.[0];if(!file||this.busy())return;this.busy.set(true);try{await firstValueFrom(this.api.evidence(this.id,file));await this.load()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false);input.value=''}}
 async file(e:Evidence){try{downloadBlob(await firstValueFrom(this.api.file(e.id,true)),e.nombreArchivo)}catch(error){this.error.set(failure(error))}}
}
