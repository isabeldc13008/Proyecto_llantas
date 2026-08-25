import {HttpClient} from '@angular/common/http';
import {inject,Injectable} from '@angular/core';
import {InventoryMetrics,InventoryReservation} from '../models/api.models';
@Injectable({providedIn:'root'})
export class InventoryApi{
 private readonly http=inject(HttpClient);
 metrics(){return this.http.get<InventoryMetrics>('/api/inventario/metricas')}
 reservations(){return this.http.get<InventoryReservation[]>('/api/inventario/reservas')}
 reserve(id:string,motivo:string){return this.http.post<void>(`/api/inventario/${id}/reservar`,{motivo})}
 release(id:string){return this.http.post<void>(`/api/inventario/${id}/liberar-reserva`,{})}
 location(id:string,zonaBodega:string,rack:string){return this.http.patch<void>(`/api/inventario/${id}/ubicacion`,{zonaBodega,rack})}
}
