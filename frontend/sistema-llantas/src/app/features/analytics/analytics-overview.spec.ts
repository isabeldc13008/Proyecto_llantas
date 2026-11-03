import {TestBed} from '@angular/core/testing';
import {provideRouter, Router} from '@angular/router';
import {of, Subject, throwError} from 'rxjs';
import {AnalyticsApi, QualityData} from './analytics-api';
import {AnalyticsOverview} from './analytics-overview';

const data=():QualityData=>({generadoEn:'2026-11-03T12:00:00Z',procedencia:'Datos del entorno actual · Procedencia no verificada',
 totales:{total:3,montadas:2,disponibles:1,conAlertas:0,conInspeccion:2,ultimaCompleta:1,sinInspeccion:1,ultimaIncompleta:1,coberturaInspeccion:66.67},
 estados:[{id:'e',nombre:'Disponible',total:3,conInspeccion:2}],centros:[{id:'c',nombre:'Centro',total:3,conInspeccion:2}],
 llantas:{items:[{id:'t',codigo:'Llanta sin lectura',centro:'Centro',estado:'Disponible',inspeccionId:null,fechaRegistro:null,ultimaCompleta:false,alertasActivas:0}],pageNumber:1,pageSize:20,totalItems:1,totalPages:1}});
describe('Resumen y calidad de Analítica',()=>{
 let api:jasmine.SpyObj<AnalyticsApi>;
 beforeEach(async()=>{
  api=jasmine.createSpyObj('AnalyticsApi',['options','quality','maintenanceVehicles','maintenance']);
  api.options.and.returnValue(of({centros:[],marcas:[],referencias:[],dimensiones:[],estados:[],tiposVehiculo:[]}));
  api.quality.and.returnValue(of(data()));api.maintenanceVehicles.and.returnValue(of({items:[],pageNumber:1,pageSize:20,totalItems:0,totalPages:0}));
  await TestBed.configureTestingModule({imports:[AnalyticsOverview],providers:[provideRouter([{path:'analitica',component:AnalyticsOverview}]),{provide:AnalyticsApi,useValue:api}]}).compileComponents();
 });
 async function create(){const f=TestBed.createComponent(AnalyticsOverview);f.detectChanges();await f.whenStable();f.detectChanges();return f;}
 it('abre resumen con todos los estados y advierte la procedencia sin enviar faltantes a mantenimiento',async()=>{
  const f=await create();expect(api.quality).toHaveBeenCalledWith(jasmine.objectContaining({montaje:'TODAS'}),1,'todas');
  expect(api.maintenance).not.toHaveBeenCalled();expect(f.nativeElement.textContent).toContain('Resumen ejecutivo');
  expect(f.nativeElement.textContent).toContain('Procedencia no verificada');expect(f.nativeElement.textContent).toContain('66.67');
 });
 it('conserva un cero real y no inventa cobertura en población vacía',async()=>{
  const d=data();d.totales={total:0,montadas:0,disponibles:0,conAlertas:0,conInspeccion:0,ultimaCompleta:0,sinInspeccion:0,ultimaIncompleta:0,coberturaInspeccion:null};d.estados=[];d.centros=[];
  api.quality.and.returnValue(of(d));const f=await create();expect(f.nativeElement.textContent).toContain('Sin datos suficientes');
  expect(f.componentInstance.display(0)).toBe('0');expect(f.componentInstance.display(null)).toBe('Sin datos suficientes');
 });
 it('conserva filtros al navegar a calidad y muestra ausencia sin calificarla como crítica',async()=>{
  const f=await create();f.componentInstance.filters.centroId='c';f.componentInstance.filters.vehiculoId='v';f.componentInstance.filters.estadoId='e';
  await f.componentInstance.navigate('calidad');await f.whenStable();f.detectChanges();
  expect(TestBed.inject(Router).url).toContain('centroId=c');expect(TestBed.inject(Router).url).toContain('vehiculoId=v');expect(TestBed.inject(Router).url).toContain('estadoId=e');
  expect(f.nativeElement.textContent).toContain('Sin inspección registrada');expect(f.nativeElement.textContent).not.toContain('Atención inmediata');
 });
 it('ignora respuesta anterior después de cambiar filtros',async()=>{
  const f=await create();const old=new Subject<QualityData>();api.quality.and.returnValue(old);const pending=f.componentInstance.load();
  api.quality.and.returnValue(of({...data(),totales:{...data().totales,total:8}}));await f.componentInstance.load();old.next(data());old.complete();await pending;
  expect(f.componentInstance.data()?.totales.total).toBe(8);
 });
 it('muestra el error sin conservar cifras anteriores ni convertirlo a cero',async()=>{
  const f=await create();api.quality.and.returnValue(throwError(()=>({status:403,userMessage:'Sin permiso'})));await f.componentInstance.load();f.detectChanges();
  expect(f.componentInstance.data()).toBeNull();expect(f.nativeElement.textContent).toContain('Sin permiso');
 });
});
