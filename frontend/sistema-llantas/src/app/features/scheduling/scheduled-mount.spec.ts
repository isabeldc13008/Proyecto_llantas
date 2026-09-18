import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {SchedulingPage} from './scheduling-page';
import {AuthService} from '../../core/auth/auth.service';
import {VehicleDetail} from '../vehicles/vehicles-api';
describe('Programar montaje asignado',()=>{
 let c:SchedulingPage;let http:HttpTestingController;
 beforeEach(()=>{TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),{provide:AuthService,useValue:{has:()=>true}}]});c=TestBed.createComponent(SchedulingPage).componentInstance;http=TestBed.inject(HttpTestingController);c.openMount();});afterEach(()=>http.verify());
 it('no avanza sin llanta asignada',()=>{c.mountStep.set(3);c.mountNext();expect(c.mountStep()).toBe(3);expect(c.message()).toContain('Asigna');});
 it('conserva llanta, posición y ocupante esperado al guardar',async()=>{c.form.vehicleId='v';c.form.centroId='c';c.form.tecnicoUsuarioId='tech';c.form.inicio='2026-10-01T10:00';c.form.fin='2026-10-01T11:00';c.mountReason='Cambio';c.mountRows=[{posicionId:'p',llantaId:'new',llantaActualId:'old'}];const pending=c.saveMount();const req=http.expectOne('/api/programacion');expect(req.request.body.asignaciones).toEqual(c.mountRows);expect(req.request.body.motivo).toBe('Cambio');req.flush({id:'saved'});await Promise.resolve();await Promise.resolve();http.expectOne('/api/programacion').flush([]);await pending;expect(c.mountModal()).toBeFalse();});
 it('cambiar vehículo elimina asignaciones anteriores',async()=>{c.mountRows=[{posicionId:'p',llantaId:'new',llantaActualId:'old'}];const pending=c.pickMountVehicle('v');expect(c.mountRows).toEqual([]);http.expectOne('/api/operaciones/vehiculos/v').flush({id:'v',centroId:'c',ejes:[]} as unknown as VehicleDetail);await pending;expect(c.form.centroId).toBe('c');});
});
