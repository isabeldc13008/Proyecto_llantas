import {Component,inject,signal,OnInit} from '@angular/core';

import {ActivatedRoute,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {Summary,failure} from './disposition-models';
import {AuthService} from '../../core/auth/auth.service';
import {DispositionTable} from './disposition-table';
import {DispositionProposal} from './disposition-proposal';
@Component({selector:'app-disposition-home',imports:[RouterLink,DispositionTable,DispositionProposal],templateUrl:'./disposition-home.html',styleUrls:['./disposition-shared.scss','./disposition-home.scss']})
export class DispositionHome implements OnInit{
 private route=inject(ActivatedRoute);mountTire=this.route.snapshot.queryParamMap.get('proponerLlanta')||'';mountPosition=this.route.snapshot.queryParamMap.get('posicionId')||'';mountCode=this.route.snapshot.queryParamMap.get('codigo')||'';private api=inject(DispositionApi);auth=inject(AuthService);data=signal<Summary|null>(null);loading=signal(true);error=signal('');newOrder=false;revision=signal(0);
 ngOnInit(){if(this.mountTire&&this.mountPosition)this.newOrder=true;void this.load()}
 async load(){this.loading.set(true);this.error.set('');this.data.set(null);try{this.data.set(await firstValueFrom(this.api.summary({})))}catch(e){this.error.set(failure(e))}finally{this.loading.set(false)}}
 created(){this.revision.update(v=>v+1);void this.load()}
}

