import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {ActivatedRoute,convertToParamMap} from '@angular/router';
import {MovementsPage} from './movements-page';
import {VehicleDetail} from '../vehicles/vehicles-api';
describe('Odómetro común en montajes',()=>{
 let page:MovementsPage;let http:HttpTestingController;let route:{snapshot:{queryParamMap:ReturnType<typeof convertToParamMap>}};
 const vehicle={id:'v1',kilometraje:100000,ejes:[],historial:[],movimientos:[],inspecciones:[],eventos:[]} as unknown as VehicleDetail;
 beforeEach(()=>{route={snapshot:{queryParamMap:convertToParamMap({})}};TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),{provide:ActivatedRoute,useValue:route}]});http=TestBed.inject(HttpTestingController);page=TestBed.runInInjectionContext(()=>new MovementsPage());page.vehicleId='v1';page.centers.set([{id:'c1'}]);});
 afterEach(()=>http.verify());
 it('precarga 100000 al seleccionar vehículo',async()=>{const pending=page.loadVehicle();http.expectOne(r=>r.url==='/api/operaciones/vehiculos').flush({items:[vehicle]});http.expectOne('/api/operaciones/vehiculos/v1').flush(vehicle);http.expectOne('/api/operaciones/solicitudes').flush([]);http.expectOne(r=>r.url==='/api/operaciones/llantas-disponibles').flush([]);await pending;expect(page.mileage).toBe(100000);});
 for(const tipo of ['Montaje','Cambio de juego'])it('precarga al entrar desde programación '+tipo,async()=>{route.snapshot.queryParamMap=convertToParamMap({actividadId:'a1'});const pending=page.ngOnInit();http.expectOne('/api/actividades/a1/montaje').flush({tipo,vehiculoId:'v1',asignaciones:[]});await Promise.resolve();http.expectOne('/api/operaciones/vehiculos/v1').flush(vehicle);await pending;expect(page.mileage).toBe(100000);});
 it('conserva un valor mayor durante refresh y eleva el mínimo si el vehículo avanzó',()=>{page.acceptDetail(vehicle);page.mileage=105000;page.acceptDetail(vehicle);expect(page.mileage).toBe(105000);page.acceptDetail({...vehicle,kilometraje:110000});expect(page.mileage).toBe(110000);});
 it('no arrastra el odómetro de otro vehículo',()=>{page.acceptDetail(vehicle);page.mileage=150000;page.acceptDetail({...vehicle,id:'v2',kilometraje:2000});expect(page.mileage).toBe(2000);});
 for(const flow of ['individual','juego','individual-programado','juego-programado'])it('bloquea 99999 frente a 100000 en '+flow,async()=>{page.acceptDetail(vehicle);page.mileage=99999;page.tireId='new';page.reason='Montar';page.selectedPosition.set({id:'p1',code:'P1',side:''});page.assignments=[{posicionId:'p1',llantaId:'new',llantaActualId:null}];if(flow.includes('programado')){page.activityId='a1';page.planned.set({tipo:flow.startsWith('juego')?'Cambio de juego':'Montaje',vehiculoId:'v1',motivo:'Montar',observaciones:'',asignaciones:page.assignments});await page.executePlanned();}else if(flow==='juego')await page.submitSet();else await page.submit();http.expectNone(r=>r.method==='POST');expect(page.message()).toContain('100.000');});
 for(const mode of ['individual','juego','programado'])it('expone mínimo dinámico y odómetro registrado: '+mode,async()=>{
  route.snapshot.queryParamMap=convertToParamMap(mode==='programado'?{actividadId:'a1'}:{vehiculoId:'v1'});
  const fixture=TestBed.createComponent(MovementsPage);const c=fixture.componentInstance;c.centers.set([{id:'c1'}]);c.setModeActive=mode==='juego';fixture.detectChanges();
  if(mode==='programado'){http.expectOne('/api/actividades/a1/montaje').flush({tipo:'Montaje',vehiculoId:'v1',motivo:'Montar',observaciones:'',asignaciones:[]});await Promise.resolve();http.expectOne('/api/operaciones/vehiculos/v1').flush(vehicle);}
  else{http.expectOne(r=>r.url==='/api/operaciones/vehiculos').flush({items:[vehicle]});http.expectOne('/api/operaciones/vehiculos/v1').flush(vehicle);http.expectOne('/api/operaciones/solicitudes').flush([]);http.expectOne(r=>r.url==='/api/operaciones/llantas-disponibles').flush([]);}
  await Promise.resolve();fixture.detectChanges();for(const req of http.match(r=>r.url==='/api/operaciones/llantas-disponibles'))req.flush([]);await fixture.whenStable();fixture.detectChanges();
  const input=fixture.nativeElement.querySelector('input[type=number]') as HTMLInputElement;expect(input.min).toBe('100000');expect(input.value).toBe('100000');expect(fixture.nativeElement.textContent).toContain('Odómetro registrado');fixture.destroy();
 });
});
