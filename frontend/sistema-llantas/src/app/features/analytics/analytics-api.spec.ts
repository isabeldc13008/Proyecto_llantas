import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {AnalyticsApi,AnalyticsFilters} from './analytics-api';

describe('Analytics API',()=>{
 it('encodes every filter and keeps pagination on specific endpoints',()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});
  const api=TestBed.inject(AnalyticsApi),http=TestBed.inject(HttpTestingController);
  const filter:AnalyticsFilters={centroId:'center',marcaId:'brand',referenciaId:'reference',dimensionId:'dimension',estadoId:'state',tipoVehiculo:'Tractocamión',ingresoDesde:'2026-01-01',ingresoHasta:'2026-09-17',minimoMuestra:8};
  api.groups('marcas-referencias',filter,2,'dimension').subscribe();
  const request=http.expectOne(r=>r.url==='/api/analitica/marcas-referencias');
  for(const [key,value] of Object.entries(filter))expect(request.request.params.get(key)).toBe(String(value));
  expect(request.request.params.get('pagina')).toBe('2');expect(request.request.params.get('agrupar')).toBe('dimension');
  request.flush({items:[],pageNumber:2,totalItems:0,totalPages:0});http.verify();
 });
});
