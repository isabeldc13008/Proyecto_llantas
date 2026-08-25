import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
export interface DashboardMetrics{totalLlantas:number;montadas:number;disponibles:number;atencionRequerida:number;inspeccionesVencidas:number;vehiculosIncompletos:number;enReparacion:number;enReencauche:number;disposicionFinal:number;programacionesPendientes:number}
export interface DashboardAttention{prioridad:'CRITICA'|'ALTA'|'MEDIA';tipo:string;llantaCodigo:string|null;placa:string|null;centro:string;descripcion:string;fecha:string;ruta:string}
export interface DashboardToday{id:string;tipo:string;fecha:string;centro:string;vehiculo:string|null;estado:string;ruta:string}
export interface DashboardFleet{vehiculosControlados:number;vehiculosCompletos:number;vehiculosIncompletos:number;vehiculosConAlerta:number;porcentajeCompletos:number|null}
export interface DashboardDistribution{montadas:number;disponibles:number;reparacion:number;reencauche:number;otros:number}
export interface DashboardCenter{id:string;nombre:string;llantas:number;vehiculos:number;alertasCriticas:number;inspeccionesVencidas:number;pendientes:number;estado:string}
export interface DashboardSummary{metrics:DashboardMetrics;attention:DashboardAttention[];today:DashboardToday[];fleet:DashboardFleet;tireDistribution:DashboardDistribution;centers:DashboardCenter[]}
@Injectable({providedIn:'root'}) export class DashboardApi{private readonly http=inject(HttpClient);get(centerId?:string){return this.http.get<DashboardSummary>('/api/dashboard/resumen',{params:centerId?new HttpParams().set('centroId',centerId):undefined})}}
