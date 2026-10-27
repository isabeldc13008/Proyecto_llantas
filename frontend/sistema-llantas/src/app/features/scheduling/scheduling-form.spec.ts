import {ComponentFixture,TestBed} from '@angular/core/testing';
import {provideRouter} from '@angular/router';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {By} from '@angular/platform-browser';
import {SchedulingPage} from './scheduling-page';
import {SingleSelectFilter} from '../../shared/single-select-filter';
import {AuthService} from '../../core/auth/auth.service';
import {VehicleSummary} from '../vehicles/vehicles-api';

describe('Formulario de Programación',()=>{
 let fixture:ComponentFixture<SchedulingPage>,c:SchedulingPage,http:HttpTestingController;
 beforeEach(()=>{
  TestBed.configureTestingModule({providers:[provideRouter([]),provideHttpClient(),provideHttpClientTesting(),{provide:AuthService,useValue:{has:()=>true}}]});
  spyOn(SchedulingPage.prototype,'ngOnInit').and.resolveTo();
  fixture=TestBed.createComponent(SchedulingPage);c=fixture.componentInstance;http=TestBed.inject(HttpTestingController);
  c.technicians.set([{id:'t1',username:'one',nombre:'Uno',centroIds:['c1']},{id:'t2',username:'two',nombre:'Dos',centroIds:['c2']}]);
  c.vehicles.set([{id:'v1',centroId:'c1',placa:'ABC',numeroInterno:'1',centro:'Centro 1'} as VehicleSummary]);
  c.openNew();
 });
 afterEach(()=>http.verify());
 function valid(){Object.assign(c.form,{vehicleId:'v1',centroId:'c1',tecnicoUsuarioId:'t1',inicio:'2026-11-01T10:00',fin:'2026-11-01T11:00'});}
 it('actualiza los técnicos cuando cambia el centro',()=>{c.form.centroId='c1';expect(c.technicianOptions().map(x=>x.value)).toEqual(['t1']);c.form.centroId='c2';expect(c.technicianOptions().map(x=>x.value)).toEqual(['t2']);});
 it('asigna el centro al seleccionar vehículo',()=>{fixture.detectChanges();const select=fixture.debugElement.queryAll(By.directive(SingleSelectFilter)).find(x=>x.componentInstance.label==='Vehículo')!;select.componentInstance.valueChange.emit('v1');expect(c.form.centroId).toBe('c1');});
 it('muestra campos faltantes dentro del formulario',async()=>{await c.save();fixture.detectChanges();const alert=fixture.nativeElement.querySelector('form [role="alert"]');expect(alert).not.toBeNull();expect(alert?.textContent).toContain('vehículo');expect(alert?.textContent).toContain('técnico');http.expectNone(r=>r.method==='POST');});
 it('rechaza fechas invertidas antes de enviar',async()=>{valid();c.form.fin='2026-11-01T09:00';const pending=c.save();const requests=http.match(r=>r.method==='POST');expect(requests.length).toBe(0);requests.forEach(r=>r.flush({}, {status:400,statusText:'Bad Request'}));await pending;});
 it('guarda una sola vez y cierra antes de mostrar éxito',async()=>{valid();const p=c.save();void c.save();const requests=http.match(r=>r.method==='POST');expect(requests.length).toBe(1);requests.forEach(r=>r.flush({id:'ok'}));await Promise.resolve();await Promise.resolve();http.match(r=>r.method==='GET').forEach(r=>r.flush([]));await p;expect(c.modal()).toBeFalse();expect(c.message()).toContain('guardada');});
 for(const status of [403,409])it('conserva información HTTP '+status+' dentro del modal',async()=>{valid();const p=c.save();http.expectOne(r=>r.method==='POST').flush({message:'Condición incumplida',errors:{tecnico:['Revisa el centro']},traceId:'trace-test'},{status,statusText:'Error'});await p;fixture.detectChanges();const alert=fixture.nativeElement.querySelector('form [role="alert"]');expect(alert?.textContent).toContain('Condición incumplida');expect(alert?.textContent).toContain('Revisa el centro');expect(c.modal()).toBeTrue();});
 it('informa cuando el centro no tiene técnicos',()=>{c.form.centroId='sin-tecnicos';fixture.detectChanges();expect(fixture.nativeElement.querySelector('form').textContent).toContain('No hay técnicos activos');});

 it('permite actividad administrativa sin vehículo',async()=>{valid();c.form.tipo='Administrativa';c.form.vehicleId='';const p=c.save();const req=http.expectOne(r=>r.method==='POST');expect(req.request.body.vehiculoId).toBeNull();req.flush({id:'ok'});await Promise.resolve();await Promise.resolve();http.expectOne('/api/programacion').flush([]);await p;expect(c.modal()).toBeFalse();});
 it('no envía un técnico ajeno al centro',async()=>{valid();c.form.tecnicoUsuarioId='t2';await c.save();expect(c.formError()).toContain('no pertenece');http.expectNone(r=>r.method==='POST');});
 it('no pierde éxito si falla el refresco',async()=>{valid();const p=c.save();http.expectOne(r=>r.method==='POST').flush({id:'ok'});await Promise.resolve();await Promise.resolve();http.expectOne('/api/programacion').flush({}, {status:500,statusText:'Error'});await p;expect(c.modal()).toBeFalse();expect(c.message()).toContain('guardada');expect(c.message()).toContain('actualizar');});
 it('conserva la clave al reintentar una respuesta perdida',async()=>{valid();const p=c.save();const req=http.expectOne(r=>r.method==='POST');const key=req.request.body.idempotencyKey;req.flush({}, {status:503,statusText:'Error'});await p;const retry=c.save();const second=http.expectOne(r=>r.method==='POST');expect(second.request.body.idempotencyKey).toBe(key);second.flush({}, {status:503,statusText:'Error'});await retry;});
 for(const tipo of ['Montaje','Reemplazar llanta','Cambio de juego'])it('envía '+tipo+' con asignaciones y evita doble envío',async()=>{
  c.modal.set(false);c.openMount();valid();c.form.tipo=tipo;c.mountReason='Renovación';c.mountRows=[{posicionId:'p1',llantaId:'n1',llantaActualId:'old1'}];if(tipo==='Cambio de juego')c.mountRows.push({posicionId:'p2',llantaId:'n2',llantaActualId:'old2'});
  const p=c.saveMount();await c.saveMount();const req=http.expectOne(r=>r.method==='POST');expect(req.request.body.tipo).toBe(tipo);expect(req.request.body.asignaciones.length).toBe(tipo==='Cambio de juego'?2:1);req.flush({id:'ok'});await Promise.resolve();await Promise.resolve();http.expectOne('/api/programacion').flush([]);await p;expect(c.mountModal()).toBeFalse();
 });
 it('muestra el conflicto de montaje exclusivamente dentro de su modal',async()=>{c.modal.set(false);c.openMount();valid();c.mountReason='Renovar';c.mountRows=[{posicionId:'p',llantaId:'new',llantaActualId:'old'}];const p=c.saveMount();http.expectOne(r=>r.method==='POST').flush({message:'Llanta comprometida'},{status:409,statusText:'Conflict'});await p;fixture.detectChanges();expect(fixture.nativeElement.querySelector('.mount-wizard [role="alert"]').textContent).toContain('Llanta comprometida');expect(fixture.nativeElement.querySelector('.page-message')).toBeNull();});
 it('limpiar vehículo invalida una respuesta pendiente',async()=>{c.openMount();const p=c.pickMountVehicle('v1');await c.pickMountVehicle('');http.expectOne('/api/operaciones/vehiculos/v1').flush({id:'v1',centroId:'c1',ejes:[]});await p;expect(c.mountLoading()).toBeFalse();expect(c.mountVehicle()).toBeNull();expect(c.form.centroId).toBe('');});
 it('el lote conserva fallidos y elimina únicamente los creados',async()=>{
  c.modal.set(false);c.openWizard();c.needs.set(['a','b'].map(id=>({id,origen:'ALERTA',tipo:'Inspección',motivo:'Revisar',centroId:'c1',centro:'Centro',vehiculoId:'v1',vehiculo:'Mismo vehículo',posicionId:null,posicion:null,llantaId:null,llanta:null,prioridad:'Media',fechaOrigen:'2026-11-01',selected:true,technicianId:'t1',start:'2026-11-01T10:00',priority:'Media',error:''})));
  const p=c.confirm();await c.confirm();http.expectOne(r=>r.method==='POST').flush({id:'ok'});await Promise.resolve();await Promise.resolve();http.expectOne(r=>r.method==='POST').flush({message:'Conflicto de agenda'},{status:409,statusText:'Conflict'});await Promise.resolve();await Promise.resolve();http.expectOne('/api/programacion').flush([]);await p;expect(c.needs().map(x=>x.id)).toEqual(['b']);expect(c.result()?.created).toBe(1);expect(c.result()?.errors[0]).toContain('Conflicto de agenda');fixture.detectChanges();expect(fixture.nativeElement.querySelector('[role="dialog"]').textContent).toContain('Conflicto de agenda');c.closeWizard();expect(c.message()).toContain('1 programaciones');
 });
});
