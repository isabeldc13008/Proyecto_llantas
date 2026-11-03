import {TestBed} from '@angular/core/testing';
import {provideRouter} from '@angular/router';
import {of,Subject,throwError} from 'rxjs';
import {AnalyticsApi,AnalyticsWear,FleetWear} from './analytics-api';
import {AnalyticsWearPage} from './analytics-wear-page';
import {maintenanceDefaults} from './maintenance-models';

const data=():FleetWear=>({generadoEn:'2026-11-03T12:00:00Z',procedencia:'Datos del entorno actual · Procedencia no verificada',
 total:{id:'total',nombre:'Total',total:2,muestraProfundidad:1,profundidadMedia:0,muestraKm:0,kmMedio:null},
 grupos:{items:[{id:'g',nombre:'Marca',total:2,muestraProfundidad:1,profundidadMedia:0,muestraKm:0,kmMedio:null}],pageNumber:1,pageSize:20,totalItems:1,totalPages:1},
 llantas:{items:[{id:'t',codigo:'LL-1',profundidad:0,km:null}],pageNumber:1,pageSize:20,totalItems:1,totalPages:1}});
describe('Desgaste y vida útil',()=>{
 let api:jasmine.SpyObj<AnalyticsApi>;
 beforeEach(async()=>{
  api=jasmine.createSpyObj('AnalyticsApi',['fleetWear','wear','interventions']);api.fleetWear.and.returnValue(of(data()));
  api.interventions.and.returnValue(of({items:[],pageNumber:1,pageSize:20,totalItems:0,totalPages:0}));
  await TestBed.configureTestingModule({imports:[AnalyticsWearPage],providers:[provideRouter([]),{provide:AnalyticsApi,useValue:api}]}).compileComponents();
 });
 async function create(){const f=TestBed.createComponent(AnalyticsWearPage);f.componentRef.setInput('filters',{...maintenanceDefaults(),montaje:'TODAS',centroId:'c',vehiculoId:'v',dimensionId:'d'});f.detectChanges();await f.whenStable();f.detectChanges();return f;}
 it('reutiliza filtros y distingue cero, ausencia, muestra y procedencia',async()=>{
  const f=await create();expect(api.fleetWear).toHaveBeenCalledWith(jasmine.objectContaining({centroId:'c',vehiculoId:'v',dimensionId:'d'}),'marca',1,1);
  expect(f.componentInstance.display(0)).toBe('0');expect(f.componentInstance.display(null)).toBe('Sin datos suficientes');
  const text=f.nativeElement.textContent;expect(text).toContain('Muestra 1 / 2');expect(text).toContain('Procedencia no verificada');expect(text).toContain('Vida útil proyectada');
  expect(f.nativeElement.querySelector('a').getAttribute('href')).toContain('/analitica/llantas/t');
 });
 it('cambia agrupación, pagina y descarta respuestas obsoletas',async()=>{
  const f=await create();const c=f.componentInstance;const old=new Subject<FleetWear>();api.fleetWear.and.returnValue(old);const pending=c.load();
  c.group='posicion';api.fleetWear.and.returnValue(of({...data(),total:{...data().total,total:9}}));await c.regroup();old.next(data());old.complete();await pending;
  expect(c.data()?.total.total).toBe(9);expect(api.fleetWear).toHaveBeenCalledWith(c.filters,'posicion',1,1);
  await c.changePage(2);expect(api.fleetWear).toHaveBeenCalledWith(c.filters,'posicion',2,1);
 });
 it('errores de permisos no se convierten en indicadores cero',async()=>{
  const f=await create();api.fleetWear.and.returnValue(throwError(()=>({status:403})));await f.componentInstance.load();f.detectChanges();
  expect(f.componentInstance.data()).toBeNull();expect(f.nativeElement.textContent).toContain('Sin permiso');
 });
 it('filtrar cancela evidencia individual anterior',async()=>{
  const f=await create();const c=f.componentInstance;const old=new Subject<AnalyticsWear>();api.wear.and.returnValue(old);const pending=c.select('t');
  f.componentRef.setInput('filters',{...maintenanceDefaults(),centroId:'otro'});f.detectChanges();old.next({id:'t',codigo:'LL-1',mediciones:[],totalLecturas:0,lecturasCompletas:0,lecturasConTramo:0,reglas:[],pronostico:'Datos insuficientes',limitaciones:[]});old.complete();await pending;
  expect(c.selected()).toBe('');expect(c.wear()).toBeNull();expect(c.history()).toBeNull();
 });
});
