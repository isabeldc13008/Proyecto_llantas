import {Injectable,inject} from '@angular/core';
import {Router} from '@angular/router';
import {AuthService} from '../../core/auth/auth.service';
import {MaintenanceAction} from './maintenance-models';

export function safeAnalyticsReturn(value:string|null):string{return value&&/^\/analitica(?:\?|\/llantas\/[a-zA-Z0-9-]+(?:\?)?|$)/.test(value)&&!value.includes('://')?value:'/analitica'}
export function maintenanceDestination(action:MaintenanceAction,returnUrl:string){
 const query:Record<string,string>={llantaId:action.llantaId,volver:safeAnalyticsReturn(returnUrl)};
 if(action.vehiculoId)query['vehiculoId']=action.vehiculoId;
 if(action.posicionId)query['posicionId']=action.posicionId;
 if(action.alertaId)query['alertaId']=action.alertaId;
 let path='/llantas';
 if(action.tipo==='REVISAR_ALERTA')path='/alertas';
 if(action.tipo==='INSPECCIONAR')path='/inspecciones';
 if(action.tipo==='PROGRAMAR_EVALUACION'){path='/programacion';query['origen']=action.alertaId?'ALERTA':'MANUAL';if(action.alertaId)query['origenEntidadId']=action.alertaId;}
 return{path,query};
}
@Injectable({providedIn:'root'})
export class MaintenanceNavigation{
 private auth=inject(AuthService);private router=inject(Router);
 label(action:MaintenanceAction){return {REVISAR_ALERTA:'Revisar alerta',INSPECCIONAR:'Inspeccionar',PROGRAMAR_EVALUACION:'Programar evaluación',VER_HISTORIAL:'Historial operativo'}[action.tipo]}
 allowed(action:MaintenanceAction){
  const rules:Record<MaintenanceAction['tipo'],[string,string[]]>={REVISAR_ALERTA:['alertas',['alertas.consultar','alertas.gestionar']],INSPECCIONAR:['inspecciones',['inspecciones.crear']],PROGRAMAR_EVALUACION:['programacion',['programacion.administrar']],VER_HISTORIAL:['llantas',['llantas.consultar','llantas.administrar']]};
  const[module,permissions]=rules[action.tipo];return this.auth.canModule(module)&&permissions.some(p=>this.auth.has(p));
 }
 open(action:MaintenanceAction){if(!this.allowed(action))return;const target=maintenanceDestination(action,this.router.url);void this.router.navigate([target.path],{queryParams:target.query});}
}
