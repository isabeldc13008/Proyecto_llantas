import {safeDispositionReturn} from './disposition-models';
import {Component,inject,signal,OnInit} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {Dispatch,stageLabel,failure} from './disposition-models';
import {DispositionActaFiles} from './disposition-acta-files';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-dispatch-detail',imports:[DatePipe,FormsModule,RouterLink,DispositionActaFiles],templateUrl:'./disposition-dispatch-detail.html',styleUrl:'./disposition-shared.scss'})
export class DispositionDispatchDetail implements OnInit{
 private api=inject(DispositionApi);private route=inject(ActivatedRoute);auth=inject(AuthService);id=this.route.snapshot.paramMap.get('id')!;backLink=inject(Router).parseUrl(safeDispositionReturn(this.route.snapshot.queryParamMap.get('volver'),'/disposicion-final/despachos'));data=signal<Dispatch|null>(null);loading=signal(true);busy=signal(false);error=signal('');label=stageLabel;notes='';confirming=false;private version=0;
 ngOnInit(){void this.load()}
 async load(){const v=++this.version;this.loading.set(true);try{const d=await firstValueFrom(this.api.dispatch(this.id));if(v===this.version)this.data.set(d)}catch(e){this.error.set(failure(e))}finally{if(v===this.version)this.loading.set(false)}}
 async close(){const d=this.data();if(!d?.puedeCerrar||this.busy()||this.loading()||!this.notes.trim())return;this.busy.set(true);this.error.set('');try{this.data.set(await firstValueFrom(this.api.close(this.id,d.rowVersion,this.notes)));this.confirming=false}catch(e){this.error.set(failure(e));await this.load()}finally{this.busy.set(false)}}
}
