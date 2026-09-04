import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {ScheduleInput,SchedulingApi} from './scheduling-api';

describe('SchedulingApi',()=>{let api:SchedulingApi;let http:HttpTestingController;beforeEach(()=>{TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});api=TestBed.inject(SchedulingApi);http=TestBed.inject(HttpTestingController)});afterEach(()=>http.verify());
it('sends combined filters to the real scheduling endpoint',()=>{api.list({centroId:'c1',estado:'Pendiente',tipo:'Inspección'}).subscribe();const request=http.expectOne(r=>r.url==='/api/programacion');expect(request.request.params.get('centroId')).toBe('c1');expect(request.request.params.get('estado')).toBe('Pendiente');expect(request.request.params.get('tipo')).toBe('Inspección');request.flush([])});
it('sends bulk activities in one atomic request',()=>{const input:ScheduleInput={tipo:'Inspección',inicio:'2026-08-20T10:00:00Z',fin:'2026-08-20T11:00:00Z',centroId:'c1',vehiculoId:'v1',tecnicoUsuarioId:'u1',prioridad:'Alta',observaciones:null,origen:'MANUAL',origenEntidadId:null};api.bulk([input]).subscribe();const request=http.expectOne('/api/programacion/masiva');expect(request.request.method).toBe('POST');expect(request.request.body.actividades).toEqual([input]);request.flush([])});
});
