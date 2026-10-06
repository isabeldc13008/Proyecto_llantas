import {Component,Input,inject} from '@angular/core';
import {ActivatedRoute,Router,RouterLink} from '@angular/router';
import {MaintenanceRow} from './maintenance-models';
import {safeAnalyticsReturn} from './maintenance-navigation';

export function mountMatches(context:{vehiculoId:string;ejes:{posiciones:{id:string;llanta:{id:string}|null}[]}[]},vehicleId:string,positionId:string,tireId:string){
 const positions=context.ejes.flatMap(e=>e.posiciones).filter(p=>p.llanta?.id===tireId);
 return context.vehiculoId===vehicleId&&positions.length===1&&positions[0].id===positionId;
}
export function validatedScheduleContext(row:MaintenanceRow,tireId:string,vehicleId:string,positionId:string){
 if(row.id!==tireId||row.ubicacion.estado!=='UNICA'||row.ubicacion.vehiculo?.id!==vehicleId||row.ubicacion.posicion?.id!==positionId)
  throw new Error('El montaje cambió o no es inequívoco. Regresa a Analítica y actualiza la evidencia.');
 return row;
}
@Component({selector:'app-maintenance-return',imports:[RouterLink],template:`@if(active){<aside><a [routerLink]="back">Volver a Analítica</a><span>{{message||'Contexto de mantenimiento. Ninguna actividad se crea automáticamente.'}}</span></aside>}`,styles:[`aside{display:flex;gap:1rem;align-items:center;flex-wrap:wrap;padding:1rem;background:#eff5ee;border-left:3px solid #0c5d0c;margin-bottom:1rem;font-size:.85rem}a{color:#0c5d0c}span{color:#48545d}`]})
export class MaintenanceReturn{
 @Input() message='';private route=inject(ActivatedRoute);private router=inject(Router);
 active=!!this.route.snapshot.queryParamMap.get('volver');back=this.router.parseUrl(safeAnalyticsReturn(this.route.snapshot.queryParamMap.get('volver')));
}
