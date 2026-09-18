import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {MountAssignmentEditor,AvailableTire} from './mount-assignment';
import {VehiclePosition,VehicleDetail} from '../vehicles/vehicles-api';
describe('Asignación de montaje',()=>{
 let c:MountAssignmentEditor;let http:HttpTestingController;
 const tire={id:'t1',codigo:'LL-101',serial:'S1',marca:'Marca',referencia:'Ref',dimension:'22',estado:'Disponible',centro:'Centro'} as AvailableTire;
 const p=(id:string)=>({id,codigo:id,llantaId:'old-'+id,llantaCodigo:'LL-001',llantaSerial:'OLD',marcaReferencia:'Marca Ref',estadoLlanta:'Montada',kilometrajeLlanta:100,lado:'Izquierda',ubicacion:'Externa'} as VehiclePosition);
 beforeEach(()=>{TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});c=TestBed.createComponent(MountAssignmentEditor).componentInstance;http=TestBed.inject(HttpTestingController);});
 afterEach(()=>http.verify());
 it('muestra posiciones reales y sus llantas',()=>{c.vehicle={id:'v',tipo:'Camión',ejes:[{id:'e',nombre:'Eje',tipoEje:'D',posiciones:[p('P1'),p('P2')]}]} as VehicleDetail;expect(c.axles()[0].positions[0].tire).toBe('LL-001');expect(c.positions().length).toBe(2);});
 it('asigna varias y permite mantener una posición',()=>{c.choose(p('P1'),tire);c.choose(p('P2'),{...tire,id:'t2'});expect(c.removed()).toBe(2);c.keep('P1');expect(c.value.length).toBe(1);expect(c.value[0].llantaActualId).toBe('old-P2');});
 it('no duplica llantas y las oculta en otras posiciones',()=>{c.available.set([tire]);c.choose(p('P1'),tire);c.choose(p('P2'),tire);expect(c.value.length).toBe(1);expect(c.options()).toEqual([]);});
 it('individual mantiene solamente una asignación',()=>{c.individual=true;c.choose(p('P1'),tire);c.choose(p('P2'),{...tire,id:'t2'});expect(c.value.length).toBe(1);expect(c.value[0].posicionId).toBe('P2');});
 it('el técnico no puede reasignar ni quitar',()=>{c.choose(p('P1'),tire);c.readonly=true;c.keep('P1');c.choose(p('P2'),{...tire,id:'t2'});expect(c.value[0].llantaId).toBe('t1');expect(c.value.length).toBe(1);});
 it('consulta disponibilidad con vehículo y búsqueda',async()=>{c.vehicle={id:'v'} as VehicleDetail;c.search='Marca';const pending=c.find();const req=http.expectOne(r=>r.url==='/api/operaciones/llantas-disponibles');expect(req.request.params.get('vehiculoId')).toBe('v');expect(req.request.params.get('buscar')).toBe('Marca');req.flush([tire]);await pending;expect(c.options()[0].id).toBe('t1');});
 it('error elimina resultados anteriores',async()=>{c.vehicle={id:'v'} as VehicleDetail;c.available.set([tire]);const pending=c.find();http.expectOne(r=>r.url==='/api/operaciones/llantas-disponibles').flush({}, {status:503,statusText:'Unavailable'});await pending;expect(c.options()).toEqual([]);expect(c.error()).toBeTruthy();});
 for(const width of [1366,390])it('layout accesible sin overflow a '+width+'px',()=>{
  const fixture=TestBed.createComponent(MountAssignmentEditor);fixture.componentInstance.readonly=true;fixture.componentInstance.vehicle={id:'v',tipo:'Tractocamión',ejes:[{id:'e',nombre:'Eje 1',tipoEje:'Direccional',posiciones:[p('P1'),p('P2')]}]} as VehicleDetail;fixture.componentInstance.selectedId='P1';fixture.detectChanges();
  expect(fixture.nativeElement.querySelector('[aria-label="Asignación de llantas por posición"]')).not.toBeNull();
  const frame=document.createElement('iframe');frame.style.width=width+'px';frame.style.height='900px';document.body.appendChild(frame);
  try{const doc=frame.contentDocument!;const styles=Array.from(document.querySelectorAll('style')).map(x=>x.textContent).join('\n');doc.open();doc.write('<style>body{margin:0}*{box-sizing:border-box}'+styles+'</style><div style="margin-left:'+(width>900?245:0)+'px;padding:16px">'+fixture.nativeElement.outerHTML+'</div>');doc.close();expect(doc.documentElement.scrollWidth).toBeLessThanOrEqual(width+1);expect(doc.body.querySelectorAll('.position-list button').length).toBe(2);}finally{frame.remove();fixture.destroy();}
 });

});
