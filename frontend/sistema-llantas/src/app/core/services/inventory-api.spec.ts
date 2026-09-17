import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {InventoryApi} from './inventory-api';

describe('InventoryApi',()=>{
 let api:InventoryApi;
 let http:HttpTestingController;

 beforeEach(()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});
  api=TestBed.inject(InventoryApi);
  http=TestBed.inject(HttpTestingController);
 });
 afterEach(()=>http.verify());

 it('consulta las métricas y reservas del inventario',()=>{
  api.metrics().subscribe();
  expect(http.expectOne('/api/inventario/metricas').request.method).toBe('GET');
  api.reservations().subscribe();
  expect(http.expectOne('/api/inventario/reservas').request.method).toBe('GET');
 });

 it('envía reserva, liberación y ubicación como comandos separados',()=>{
  api.reserve('t1','Mantenimiento programado').subscribe();
  const reserve=http.expectOne('/api/inventario/t1/reservar');
  expect(reserve.request.method).toBe('POST');
  expect(reserve.request.body).toEqual({motivo:'Mantenimiento programado'});
  reserve.flush(null);

  api.release('t1').subscribe();
  const release=http.expectOne('/api/inventario/t1/liberar-reserva');
  expect(release.request.method).toBe('POST');
  expect(release.request.body).toEqual({});
  release.flush(null);

  api.location('t1','Zona A','Rack 3').subscribe();
  const location=http.expectOne('/api/inventario/t1/ubicacion');
  expect(location.request.method).toBe('PATCH');
  expect(location.request.body).toEqual({zonaBodega:'Zona A',rack:'Rack 3'});
  location.flush(null);
 });
});
