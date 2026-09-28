import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {ActivatedRoute} from '@angular/router';
import {ServiceWorkflowPage} from './service-workflow-page';

describe('Disposición final: evaluación técnica',()=>{
 let page:ServiceWorkflowPage;let http:HttpTestingController;
 beforeEach(()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),{provide:ActivatedRoute,useValue:{snapshot:{data:{serviceType:'DisposicionFinal'}}}}]});
  page=TestBed.runInInjectionContext(()=>new ServiceWorkflowPage());http=TestBed.inject(HttpTestingController);
 });
 afterEach(()=>http.verify());
 const orden=()=>({id:'o1',tipo:'DisposicionFinal',estado:'PENDIENTE_EVALUACION_TECNICA',llantaId:'t1',llanta:'LL-1',centro:'Centro',centroId:'c1',proveedorId:null,proveedor:null,costo:null,motivo:'Evaluar',observaciones:null,elegible:true,criterio:null,fechaEnvio:null,fechaRecepcion:null,evidencias:0});
 for(const reutilizable of [true,false])it('envía concepto obligatorio y decisión '+reutilizable,async()=>{
  spyOn(window,'prompt').and.returnValue('  Concepto técnico  ');
  const pending=page.evaluate(orden(),reutilizable);
  const request=http.expectOne('/api/servicios-llanta/o1/evaluar-disposicion');expect(request.request.body).toEqual({reutilizable,observaciones:'Concepto técnico'});request.flush({});
  await Promise.resolve();http.expectOne(r=>r.url==='/api/servicios-llanta').flush([{...orden(),estado:reutilizable?'RETORNADA_INVENTARIO':'PENDIENTE_APROBACION'}]);await pending;
  expect(page.activeCount()).toBe(reutilizable?0:1);
 });
 it('no envía una evaluación sin concepto',async()=>{spyOn(window,'prompt').and.returnValue('   ');await page.evaluate(orden(),true);http.expectNone('/api/servicios-llanta/o1/evaluar-disposicion');expect(page.message()).toContain('obligatorio');});
 it('cuenta retornadas como cerradas',()=>{page.orders.set([{...orden(),estado:'RETORNADA_INVENTARIO'},orden()]);expect(page.activeCount()).toBe(1);expect(page.closedCount()).toBe(1);});
});
