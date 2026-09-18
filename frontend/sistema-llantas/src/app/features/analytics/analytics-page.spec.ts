import {ComponentFixture,TestBed} from '@angular/core/testing';
import {of,Subject,throwError} from 'rxjs';
import {provideRouter,ActivatedRouteSnapshot,Router} from '@angular/router';
import {AnalyticsApi,AnalyticsGroup,AnalyticsPage,AnalyticsSummary,KmStats} from './analytics-api';
import {AnalyticsPageComponent} from './analytics-page';
import {TireMetric} from './tire-metric';
import {AuthService} from '../../core/auth/auth.service';
import {roleGuard} from '../../core/auth/role.guard';

describe('Analytics phase one',()=>{
 let fixture:ComponentFixture<AnalyticsPageComponent>,page:AnalyticsPageComponent,api:jasmine.SpyObj<AnalyticsApi>;
 const stats=(n=0,value:number|null=null):KmStats=>({muestra:n,promedio:value,mediana:value,desviacion:n?0:null,minimo:value,maximo:value});
 const summary=(total=2):AnalyticsSummary=>({total,montadas:1,disponibles:1,enReparacion:0,enReencauche:0,disposicionFinal:0,conAlertas:0,sinKm:1,conTramosIncompletos:0,profundidadPromedio:null,muestraProfundidad:0,movimientosPromedio:0,enOperacion:stats(1,1000),finalizadas:stats(),estados:[{nombre:'Disponible',cantidad:1}]});
 const group=(n=1):AnalyticsGroup=>({id:'brand',nombre:'Marca real',total:10,enOperacionTotal:9,finalizadasTotal:1,enOperacion:stats(n,1000),finalizadas:stats(1,2000),reparacionesPromedio:0,reencauchesPromedio:0,alertas:0,movimientos:2,muestraSuficiente:false});
 const paged=<T>(items:T[],page=1):AnalyticsPage<T>=>({items,pageNumber:page,pageSize:20,totalItems:41,totalPages:3});
 beforeEach(async()=>{
  api=jasmine.createSpyObj('AnalyticsApi',['options','summary','groups','positions','movements']);
  api.options.and.returnValue(of({centros:[],marcas:[],referencias:[],dimensiones:[],estados:[],tiposVehiculo:[]}));
  api.summary.and.returnValue(of(summary()));api.groups.and.returnValue(of(paged([group()])));
  api.positions.and.returnValue(of(paged([])));api.movements.and.returnValue(of({ranking:paged([]),tipos:[]}));
  await TestBed.configureTestingModule({imports:[AnalyticsPageComponent],providers:[{provide:AnalyticsApi,useValue:api}]}).compileComponents();
  fixture=TestBed.createComponent(AnalyticsPageComponent);page=fixture.componentInstance;
  fixture.detectChanges();await fixture.whenStable();fixture.detectChanges();
 });
 it('loads only the active view and options',()=>{
  expect(api.summary).toHaveBeenCalledTimes(1);expect(api.options).toHaveBeenCalledTimes(1);
  expect(api.groups).not.toHaveBeenCalled();expect(api.movements).not.toHaveBeenCalled();
 });
 it('renders real zero, missing data and measurement coverage distinctly',()=>{
  const text=fixture.nativeElement.textContent;
  expect(text).toContain('Sin dato');expect(text).toContain('Movimientos por llanta');
  expect(text).toContain('cobertura');expect(text).not.toContain('km restantes');
  expect(page.summary()?.movimientosPromedio).toBe(0);
  const ring=fixture.nativeElement.querySelector('app-tire-metric [role="img"]');
  expect(ring.getAttribute('aria-label')).toContain('1 de 2');expect(ring.getAttribute('aria-label')).toContain('No es una estimación');
 });
 it('keeps summary available if filter options fail',async()=>{
  api.options.and.returnValue(throwError(()=>new Error('Unavailable')));await page.ngOnInit();
  expect(page.summary()?.total).toBe(2);expect(page.optionsError()).not.toBe('');
 });
 it('shows a genuine empty state',async()=>{
  api.summary.and.returnValue(of(summary(0)));await page.load();fixture.detectChanges();
  expect(fixture.nativeElement.textContent).toContain('No hay llantas en esta cohorte');expect(fixture.nativeElement.querySelector('.kpis')).toBeNull();
 });
 it('passes all applied filters to the next page and switches views without stale results',async()=>{
  page.filters={centroId:'c',marcaId:'m',referenciaId:'r',dimensionId:'d',estadoId:'e',tipoVehiculo:'Camión',ingresoDesde:'2026-01-01',ingresoHasta:'2026-09-17',minimoMuestra:3};
  await page.apply();await page.selectView('posiciones');await page.load(2);
  expect(api.positions).toHaveBeenCalledWith(jasmine.objectContaining(page.filters),2);expect(page.summary()).toBeNull();
  page.filters.centroId='not-applied';await page.load(3);
  expect(api.positions.calls.mostRecent().args[0].centroId).toBe('c');
 });
 it('rejects reversed dates and fractional sample minimum before querying',async()=>{
  const count=api.summary.calls.count();page.filters.ingresoDesde='2026-09-17';page.filters.ingresoHasta='2026-01-01';await page.apply();
  expect(api.summary.calls.count()).toBe(count);expect(page.error()).toContain('fecha');
  page.filters.ingresoHasta='';page.filters.minimoMuestra=1.5;await page.apply();expect(page.error()).toContain('entero');
 });
 it('separates finalized and ongoing cohorts and suppresses low-sample bars',async()=>{
  await page.selectView('vida-util');fixture.detectChanges();
  expect(fixture.nativeElement.textContent).toContain('Muestra insuficiente');expect(page.bar(group())).toBe(0);
  page.cohort='finalizadas';expect(page.stats(group()).promedio).toBe(2000);
  page.applied.minimoMuestra=1;expect(page.sufficient(group())).toBeTrue();
 });
 it('keeps a real zero in the comparison statistics',async()=>{
  const zero={...group(5),enOperacion:stats(5,0)};api.groups.and.returnValue(of(paged([zero])));
  await page.selectView('marcas-referencias');fixture.detectChanges();
  expect(fixture.nativeElement.textContent).toContain('0 km');expect(fixture.nativeElement.textContent).not.toContain('Muestra insuficiente');
 });
 it('does not present configuration heat as wear or combine unknown P1 positions',async()=>{
  api.positions.and.returnValue(of(paged([{configuracion:'Configuración A',tipoVehiculo:'Camión',eje:1,tipoEje:'Direccional',posicion:'P1 · Izquierda',llantas:1,muestraKm:1,tramos:2,tramosValidos:2,kmPromedio:100,diasPromedio:5,muestraSuficiente:false}])));
  await page.selectView('posiciones');fixture.detectChanges();
  const text=fixture.nativeElement.textContent;expect(text).toContain('Configuración A');expect(text).toContain('no significa mayor desgaste');
  expect(page.heat(page.positions()!.items[0])).toBe(0);
 });
 it('retains real backend movement types and distinct-count explanations',async()=>{
  api.movements.and.returnValue(of({ranking:paged([{id:'t',codigo:'LL-1',serial:'SER',marca:'M',referencia:'R',estado:'Disponible',total:4,vehiculos:2,centros:1,posiciones:3,tipos:[{nombre:'ROTACION',cantidad:4}]}]),tipos:[{nombre:'ROTACION',cantidad:4}]}));
  await page.selectView('movimientos');fixture.detectChanges();expect(fixture.nativeElement.textContent).toContain('ROTACION');
  expect(fixture.nativeElement.textContent).toContain('una vez por llanta');
 });
 it('ignores a late response from an old tab',async()=>{
  const late=new Subject<AnalyticsSummary>();api.summary.and.returnValue(late);
  const old=page.load();await page.selectView('centros');late.next(summary(100));late.complete();await old;
  expect(page.view()).toBe('centros');expect(page.summary()).toBeNull();expect(page.groups()).not.toBeNull();
 });
 it('clears stale data and preserves the error message on failure',async()=>{
  api.summary.and.returnValue(throwError(()=>({userMessage:'Cohorte demasiado grande; acote filtros.'})));
  await page.load();fixture.detectChanges();expect(page.summary()).toBeNull();expect(fixture.nativeElement.textContent).toContain('acote filtros');
 });
 it('renders six distinct navigation choices and a responsive grid',()=>{
  expect(fixture.nativeElement.querySelectorAll('nav button').length).toBe(6);
  const columns=getComputedStyle(fixture.nativeElement.querySelector('.kpis')).gridTemplateColumns.split(' ').length;
  expect(columns).toBeLessThanOrEqual(window.innerWidth<=650?2:6);
  expect(fixture.nativeElement.querySelector('nav').getAttribute('aria-label')).toContain('Analítica');
 });
});

describe('Analytics access and tire visualization',()=>{
 it('uses the existing module guard without granting dashboard users analytics',()=>{
  const auth={canModule:jasmine.createSpy().and.returnValue(false),has:()=>false,user:()=>null};
  TestBed.configureTestingModule({providers:[provideRouter([]),{provide:AuthService,useValue:auth}]});
  const route={routeConfig:{path:'analitica'}} as ActivatedRouteSnapshot;
  const result=TestBed.runInInjectionContext(()=>roleGuard(route,{} as any));
  expect(TestBed.inject(Router).serializeUrl(result as any)).toBe('/sin-acceso');expect(auth.canModule).toHaveBeenCalledWith('analitica');
  auth.canModule.and.returnValue(true);expect(TestBed.runInInjectionContext(()=>roleGuard(route,{} as any))).toBeTrue();
 });
 it('does not turn an absent kilometre value into zero or remaining life',()=>{
  const ring=new TireMetric();expect(ring.value).toBeNull();expect(ring.percent).toBe(0);
  expect(ring.description).toContain('sin dato');ring.total=10;ring.measured=4;expect(ring.percent).toBe(40);
 });
});
