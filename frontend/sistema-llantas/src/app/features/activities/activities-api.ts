import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export interface Activity {
  id:string; tipo:string; fecha:string; centro:string; vehiculoId:string|null;
  vehiculo:string; prioridad:string; estado:string; rutaInicio:string; fechaCumplimiento:string|null;
}

@Injectable({providedIn:'root'})
export class ActivitiesApi {
  private readonly http=inject(HttpClient);
  list(){return this.http.get<Activity[]>('/api/mis-actividades')}
  start(id:string){return this.http.post<Activity>(`/api/actividades/${id}/iniciar`,{})}
  complete(id:string){return this.http.post<Activity>(`/api/actividades/${id}/completar`,{})}
}
