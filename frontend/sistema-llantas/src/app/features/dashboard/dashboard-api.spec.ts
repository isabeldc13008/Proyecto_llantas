import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {DashboardApi} from './dashboard-api';

describe('DashboardApi',()=>{
 let api:DashboardApi;
 let http:HttpTestingController;

 beforeEach(()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});
  api=TestBed.inject(DashboardApi);
  http=TestBed.inject(HttpTestingController);
 });
 afterEach(()=>http.verify());

 it('consulta el resumen global sin forzar un centro',()=>{
  api.get().subscribe();
  const request=http.expectOne('/api/dashboard/resumen');
  expect(request.request.method).toBe('GET');
  expect(request.request.params.keys()).toEqual([]);
  request.flush({metrics:{},attention:[],today:[],fleet:{},tireDistribution:{},centers:[]});
 });

 it('envía el centro seleccionado para respetar el alcance contextual',()=>{
  api.get('center-1').subscribe();
  const request=http.expectOne(r=>r.url==='/api/dashboard/resumen'&&r.params.get('centroId')==='center-1');
  expect(request.request.method).toBe('GET');
  request.flush({metrics:{},attention:[],today:[],fleet:{},tireDistribution:{},centers:[]});
 });
});
