import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {AnalyticsApi,AnalyticsFilters} from './analytics-api';

describe('Analytics API',()=>{
 it('sends the search and indicator to the authorized list and uses the tire wear endpoint',()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});
  const api=TestBed.inject(AnalyticsApi),http=TestBed.inject(HttpTestingController);
  const filter:AnalyticsFilters={buscar:'LL-1',centroId:'allowed',marcaId:'',referenciaId:'',dimensionId:'',estadoId:'',tipoVehiculo:'',ingresoDesde:'',ingresoHasta:'',minimoMuestra:5};
  api.tires(filter,2,'alertas').subscribe();const list=http.expectOne(r=>r.url==='/api/analitica/llantas');
  expect(list.request.params.get('indicador')).toBe('alertas');expect(list.request.params.get('buscar')).toBe('LL-1');expect(list.request.params.get('centroId')).toBe('allowed');expect(list.request.params.get('pagina')).toBe('2');list.flush({items:[]});
  api.wear('tire-id').subscribe();http.expectOne('/api/analitica/llantas/tire-id/desgaste').flush({});http.verify();
 });

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
