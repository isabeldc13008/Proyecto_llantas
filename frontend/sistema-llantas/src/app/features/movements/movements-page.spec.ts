import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {ActivatedRoute,convertToParamMap} from '@angular/router';
import {MovementsPage} from './movements-page';
import {VehicleDetail} from '../vehicles/vehicles-api';

describe('Movements refresh',()=>{
 let page:MovementsPage;let http:HttpTestingController;
 const vehicle=(tires:(string|null)[]):VehicleDetail=>({id:'v1',kilometraje:1500,historial:[],movimientos:[{id:'m1'}],ejes:[{id:'axle',nombre:'Eje',tipoEje:'Simple',posiciones:tires.map((t,i)=>({id:'p'+i,codigo:'P'+i,llantaId:t,llantaCodigo:t,lado:'',ubicacion:''}))}]} as unknown as VehicleDetail);
 beforeEach(()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),{provide:ActivatedRoute,useValue:{snapshot:{queryParamMap:convertToParamMap({})}}}]});
  http=TestBed.inject(HttpTestingController);page=TestBed.runInInjectionContext(()=>new MovementsPage());
  page.vehicleId='v1';page.centers.set([{id:'c1'}]);page.detail.set(vehicle([null,'t2']));
  page.selectedPosition.set({id:'p0',code:'P0',side:'',tire:'LIBRE',state:'empty'});
  page.tireId='t1';page.reason='Cambio';page.notes='Observación';page.mileage=1500;
 });
 afterEach(()=>http.verify());
 function flushRefresh(tires:(string|null)[],fail?:'solicitudes'|'llantas-disponibles'){
  http.expectOne(r=>r.url==='/api/operaciones/vehiculos').flush({items:[{id:'v1'}]});
  http.expectOne('/api/operaciones/vehiculos/v1').flush(vehicle(tires));
  for(const resource of ['solicitudes','llantas-disponibles']){
   const request=http.expectOne(r=>r.url==='/api/operaciones/'+resource);
   if(resource===fail)request.flush({}, {status:503,statusText:'Unavailable'});else request.flush([]);
  }
 }
 async function execute(tires:(string|null)[],fail?:'solicitudes'|'llantas-disponibles'){
  const pending=page.submit();http.expectOne('/api/operaciones/solicitudes').flush({id:'s1',estado:'EJECUTADO'});
  await Promise.resolve();flushRefresh(tires,fail);await pending;
 }
 it('updates the mounting diagram, history and mileage and removes the mounted tire from available',async()=>{
  page.available.set([{id:'t1'}]);await execute(['t1','t2']);
  expect(page.axles()[0].positions[0].tire).toBe('t1');expect(page.available()).toEqual([]);
  expect(page.detail()?.kilometraje).toBe(1500);expect(page.detail()?.movimientos[0].id).toBe('m1');
 });
 it('leaves the origin empty after unmounting',async()=>{
  page.type='Desmontaje';page.detail.set(vehicle(['t1','t2']));await execute([null,'t2']);
  expect(page.axles()[0].positions[0].state).toBe('empty');
 });
 it('updates both positions after a rotation',async()=>{
  page.type='Rotación';page.detail.set(vehicle(['t1','t2']));page.destinationPositionId='p1';page.displacedDestination='Inventario';
  await execute(['t2','t1']);expect(page.positions().map(p=>p.llantaId)).toEqual(['t2','t1']);
 });
 it('recalculates the occupant whenever the plain destination selection changes',()=>{
  page.type='Rotación';page.destinationPositionId='p0';expect(page.destinationOccupant()).toBeNull();
  page.destinationPositionId='p1';expect(page.destinationOccupant()?.llantaId).toBe('t2');
 });
 it('clears selections and form after success',async()=>{
  page.destinationPositionId='p1';page.displacedDestination='Inventario';page.destinationCenterId='c2';
  await execute(['t1','t2']);expect(page.selectedPosition()).toBeNull();
  for(const value of [page.tireId,page.destinationPositionId,page.destinationCenterId,page.displacedDestination,page.reason,page.notes])expect(value).toBe('');
 });
 for(const failed of ['solicitudes','llantas-disponibles'] as const){
  it('preserves success and updates independent resources when '+failed+' fails',async()=>{
   await execute(['t1','t2'],failed);
   expect(page.message()).toBe('Operación ejecutada.');expect(page.messageKind()).toBe('success');
   expect(page.refreshWarning()).toContain(failed==='solicitudes'?'solicitudes':'llantas disponibles');
   expect(page.axles()[0].positions[0].tire).toBe('t1');
   const retry=page.refresh();flushRefresh(['t1','t2']);await retry;
   expect(page.refreshWarning()).toBe('');expect(page.message()).toBe('Operación ejecutada.');
   http.expectNone(r=>r.method==='POST');
  });
 }
 it('keeps input when the operation itself fails',async()=>{
  const pending=page.submit();http.expectOne('/api/operaciones/solicitudes').flush({}, {status:409,statusText:'Conflict'});
  await pending;expect(page.reason).toBe('Cambio');expect(page.tireId).toBe('t1');expect(page.messageKind()).toBe('error');
 });
 it('keeps a pending request distinct from an executed movement',async()=>{
  const pending=page.submit();http.expectOne('/api/operaciones/solicitudes').flush({id:'s1',estado:'PENDIENTE_APROBACION'});
  await Promise.resolve();flushRefresh([null,'t2']);await pending;
  expect(page.message()).toContain('Solicitud enviada para autorización');expect(page.axles()[0].positions[0].state).toBe('empty');
 });
 it('does not let a slow old vehicle response replace the newly selected vehicle',async()=>{
  const old=page.refresh();const oldRequests=http.match(()=>true);page.vehicleId='v2';
  const latest=page.refresh();http.expectOne(r=>r.url==='/api/operaciones/vehiculos').flush({items:[{id:'v2'}]});
  http.expectOne('/api/operaciones/solicitudes').flush([]);http.expectOne('/api/operaciones/vehiculos/v2').flush({...vehicle(['new',null]),id:'v2'});
  http.expectOne(r=>r.url==='/api/operaciones/llantas-disponibles').flush([{id:'new-available'}]);await latest;
  for(const request of oldRequests){if(request.request.url==='/api/operaciones/vehiculos')request.flush({items:[{id:'v1'}]});else if(request.request.url.endsWith('/v1'))request.flush(vehicle(['old',null]));else request.flush([]);}
  await old;expect(page.detail()?.id).toBe('v2');expect(page.available()[0].id).toBe('new-available');expect(page.vehicles()[0].id).toBe('v2');
 });

 it('rejects a new single-position set without sending a request',async()=>{page.assignments=[{posicionId:'p0',llantaId:'new',llantaActualId:'t1'}];page.reason='Reemplazar';await page.submitSet();http.expectNone(r=>r.method==='POST');expect(page.message()).toContain('al menos dos');});
 it('requests explicit replacement with the expected outgoing tire',async()=>{page.type='Reemplazar llanta';page.detail.set(vehicle(['t1','t2']));page.tireId='new';page.reason='Reemplazar';page.mileage=5000;page.selectedPosition.set({id:'p0',code:'P1',side:'',tire:'t1'});spyOn(page,'refresh').and.resolveTo();const pending=page.submit();const req=http.expectOne('/api/operaciones/solicitudes');expect(req.request.body.tipo).toBe('Reemplazar llanta');expect(req.request.body.asignaciones).toEqual([{posicionId:'p0',llantaId:'new',llantaActualId:'t1'}]);req.flush({id:'s',estado:'PENDIENTE_APROBACION'});await pending;expect(page.message()).toContain('autorización');});
});
