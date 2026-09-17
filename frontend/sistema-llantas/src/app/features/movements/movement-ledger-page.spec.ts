import {ComponentFixture,TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {provideRouter} from '@angular/router';
import {MovementLedgerPage} from './movement-ledger-page';

describe('Movement ledger',()=>{
 let page:MovementLedgerPage;let fixture:ComponentFixture<MovementLedgerPage>;let http:HttpTestingController;
 const move={id:'m1',numero:'MOV-20260917215345706-265e76',fecha:'2026-09-17T21:53:45Z',tipo:'Montaje',llantaId:'t1',llanta:'DEMO-LL-122',serial:'123477',origen:'Inventario',destino:'LKO235 / P1',vehiculoPosicion:'DEMO-1543 · LKO235 / P1',vehiculoInterno:'DEMO-1543',vehiculoPlaca:'LKO235',posicionOrigen:null,posicionDestino:'P1',centro:'Medellín',centroOrigen:'Medellín',centroDestino:'Medellín',kilometrajeVehiculo:1000,kilometrosTramo:null,usuario:'Técnico Uno',actividadProgramadaId:null,motivo:'Cambio preventivo',observaciones:'Revisión completa',estado:'EJECUTADO',solicitudId:'solicitud-123'};
 beforeEach(async()=>{
  localStorage.removeItem('glld.columns.movimientos');
  await TestBed.configureTestingModule({imports:[MovementLedgerPage],providers:[provideHttpClient(),provideHttpClientTesting(),provideRouter([])]}).compileComponents();
  http=TestBed.inject(HttpTestingController);fixture=TestBed.createComponent(MovementLedgerPage);page=fixture.componentInstance;
  const refresh=spyOn(page,'refresh').and.resolveTo();page.loading.set(false);page.centers.set([{id:'c1',nombre:'Medellín'}]);page.moves.set([{...move}]);fixture.detectChanges();refresh.and.callThrough();
 });
 afterEach(()=>{http.verify();localStorage.removeItem('glld.columns.movimientos');});
 it('renders route, structured vehicle, short number and the full identifier tooltip without damaged text',()=>{
  const table=fixture.nativeElement.querySelector('table') as HTMLElement;
  expect(table.textContent).toContain('Inventario');expect(table.textContent).toContain('→');expect(table.textContent).toContain('LKO235 / P1');
  expect(table.textContent).toContain('DEMO-1543');expect(table.textContent).toContain('MOV-20260917-215345');
  expect(table.querySelector('.movement-number')?.getAttribute('title')).toBe(move.numero);
  expect(table.textContent).not.toMatch(/â€|Ã|Â/);expect(move.numero).toBe('MOV-20260917215345706-265e76');
 });
 it('keeps secondary data in the detail and configurable columns',()=>{
  expect(page.show('usuario')).toBeFalse();expect(page.show('motivo')).toBeFalse();
  expect(page.columns.some(x=>x.key==='usuario')).toBeTrue();
  (fixture.nativeElement.querySelector('tbody button') as HTMLButtonElement).click();fixture.detectChanges();
  const dialog=fixture.nativeElement.querySelector('[role="dialog"]') as HTMLElement;
  for(const text of [move.numero,move.usuario,move.motivo,move.observaciones,move.solicitudId,'Posición origen','Posición destino','Centro origen','Centro destino','Kilometraje'])expect(dialog.textContent).toContain(text);
 });
 it('provides mobile cards and a keyboard-accessible horizontal table viewport',()=>{
  expect(fixture.nativeElement.querySelector('.mobile-list article')).not.toBeNull();
  expect(fixture.nativeElement.querySelector('.table-shell').getAttribute('tabindex')).toBe('0');
  expect(fixture.nativeElement.querySelector('.mobile-list button').textContent).toContain('Ver detalle');
  const mobile=window.matchMedia('(max-width:700px)').matches;
  expect(getComputedStyle(fixture.nativeElement.querySelector('.table-shell')).display==='none').toBe(mobile);
  expect(getComputedStyle(fixture.nativeElement.querySelector('.mobile-list')).display==='none').toBe(!mobile);
 });
 it('closes details with Escape and returns focus to the trigger',()=>{
  const trigger=fixture.nativeElement.querySelector(window.matchMedia('(max-width:700px)').matches?'.mobile-list button':'tbody button') as HTMLButtonElement;
  trigger.focus();page.openDetail({...move});fixture.detectChanges();
  const close=fixture.nativeElement.querySelector('[aria-label="Cerrar detalle"]') as HTMLButtonElement;close.focus();
  close.dispatchEvent(new KeyboardEvent('keydown',{key:'Escape',bubbles:true}));fixture.detectChanges();
  expect(page.selected()).toBeNull();expect(document.activeElement).toBe(trigger);
 });
 it('preserves search, filters and page parameters and refreshes requests even with centers loaded',async()=>{
  page.search=' LKO235 ';page.filterValues={fechaMin:'2026-09-01',fechaMax:'2026-09-17',centroId:'c1',origen:'P1',destino:'P2',usuario:'Técnico',tipo:'Rotación'};
  const pending=page.refresh(2);const request=http.expectOne(r=>r.url==='/api/operaciones/movimientos');
  expect(request.request.params.get('pagina')).toBe('2');expect(request.request.params.get('buscar')).toBe('LKO235');
  expect(request.request.params.get('desde')).toBe('2026-09-01');expect(request.request.params.get('hasta')).toBe('2026-09-17');
  for(const key of ['centroId','origen','destino','usuario','tipo'])expect(request.request.params.get(key)).toBe(String(page.filterValues[key]));
  request.flush({items:[move],pageNumber:2,totalItems:45});http.expectOne('/api/operaciones/solicitudes').flush([{id:'s2',estado:'EJECUTADO'}]);await pending;
  expect(page.page()).toBe(2);expect(page.total()).toBe(45);expect(page.requests()[0].id).toBe('s2');
 });
 it('keeps successful movement data when requests refresh fails',async()=>{
  page.success.set('Solicitud procesada.');const pending=page.refresh();
  http.expectOne(r=>r.url==='/api/operaciones/movimientos').flush({items:[move],pageNumber:1,totalItems:1});
  http.expectOne('/api/operaciones/solicitudes').flush({}, {status:503,statusText:'Unavailable'});await pending;
  expect(page.moves().length).toBe(1);expect(page.success()).toBe('Solicitud procesada.');expect(page.message()).toContain('solicitudes');
 });
 it('keeps transfer and approval filtering based on actual backend fields',()=>{
  page.requests.set([{id:'a',estado:'PENDIENTE_APROBACION',centroDestinoId:null},{id:'b',estado:'EJECUTADO',centroDestinoId:'c1'}] as any);
  page.tab.set('approvals');expect(page.visibleRequests().map(x=>x.id)).toEqual(['a']);
  page.tab.set('transfers');expect(page.visibleRequests().map(x=>x.id)).toEqual(['b']);
 });
});
