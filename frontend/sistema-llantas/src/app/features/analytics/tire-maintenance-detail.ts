import {CommonModule} from '@angular/common';
import {Component,OnInit,inject,signal} from '@angular/core';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {AuthService} from '../../core/auth/auth.service';
import {TiresApi} from '../../core/services/tires-api';
import {TireDetail} from '../../core/models/api.models';
import {TireEvidencePanel} from './tire-evidence-panel';
import {safeAnalyticsReturn} from './maintenance-navigation';
@Component({selector:'app-tire-maintenance-detail',imports:[CommonModule,RouterLink,TireEvidencePanel],templateUrl:'./tire-maintenance-detail.html',styleUrl:'./tire-maintenance-detail.scss'})
export class TireMaintenanceDetail implements OnInit{
 private route=inject(ActivatedRoute);private api=inject(TiresApi);private auth=inject(AuthService);
 id=this.route.snapshot.paramMap.get('id')??'';back=inject(Router).parseUrl(safeAnalyticsReturn(this.route.snapshot.queryParamMap.get('volver')));
 history=signal<TireDetail|null>(null);error=signal('');loading=signal(false);
 canHistory=this.auth.canModule('llantas')&&(this.auth.has('llantas.consultar')||this.auth.has('llantas.administrar'));
 ngOnInit(){void this.loadHistory()}
 async loadHistory(){if(!this.canHistory)return;this.loading.set(true);this.error.set('');try{this.history.set(await firstValueFrom(this.api.history(this.id)))}catch(e:any){this.error.set(e?.status===403?'Sin permiso para consultar el historial operativo':e?.userMessage??'No fue posible consultar el historial operativo')}finally{this.loading.set(false)}}
}
