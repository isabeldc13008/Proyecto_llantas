import {TestBed,ComponentFixture} from '@angular/core/testing';
import {HttpClient} from '@angular/common/http';
import {provideRouter} from '@angular/router';
import {of} from 'rxjs';
import {CatalogAdmin} from './admin/catalog-admin';
import {AuthorizationsPage} from './authorizations/authorizations-page';
import {RepairsPage} from './services/repairs-page';
import {MovementLedgerPage} from './movements/movement-ledger-page';
import {MovementsPage} from './movements/movements-page';
import {AuthService} from '../core/auth/auth.service';
import {CatalogsApi} from '../core/services/catalogs-api';
import {TiresApi} from '../core/services/tires-api';

const roles=[{id:'r1',codigo:'OPERADOR',nombre:'Operador',permisos:['modulos.vehiculos','modulos.movimientos','operaciones.montar']},{id:'r2',codigo:'LECTOR',nombre:'Lector',permisos:['modulos.inventario','modulos.analitica']}];
const long='ReferenciaSinEspacios'.repeat(12);
const request:any={id:'s1',fecha:'2026-09-18',tipo:'Cambio de juego',solicitante:long,centro:long,vehiculo:'DEMO-1542',llanta:long,serial:long,posicionDestino:'P1',estado:'PENDIENTE_APROBACION',grupoOperacionId:'grupo-1',motivo:long};
const move:any={...request,id:'m1',numero:'MOV-20260918-123456',llantaId:'t1',origen:long,destino:long,vehiculoPosicion:'DEMO-1542 / P1',usuario:long,estado:'EJECUTADO'};

// Real iframe viewport: Chrome's top-level window has a minimum width above 390px.
function viewport(fixture:ComponentFixture<any>,width:number,check?:(doc:Document)=>void){
 const frame=document.createElement('iframe');frame.style.cssText=`width:${width}px;height:1000px;border:0`;document.body.append(frame);
 try{const doc=frame.contentDocument!;const styles=Array.from(document.styleSheets).map(sheet=>{try{return Array.from(sheet.cssRules).map(rule=>rule.cssText).join('\n')}catch{return ''}}).join('\n');
 doc.open();doc.write(`<style>body{margin:0}*{box-sizing:border-box}${styles}</style><div style="display:grid;grid-template-columns:${width>900?'245px ':''}minmax(0,1fr)">${width>900?'<aside>Menú</aside>':''}<div style="min-width:0">${fixture.nativeElement.outerHTML}</div></div>`);doc.close();
 expect(doc.documentElement.scrollWidth).withContext('viewport '+width).toBeLessThanOrEqual(width);
 check?.(doc);
 }finally{frame.remove();}
}

describe('Organización visual de módulos operativos',()=>{
 beforeEach(()=>{TestBed.configureTestingModule({providers:[provideRouter([]),{provide:AuthService,useValue:{has:()=>true,isAdmin:()=>true,user:()=>({username:'otro'})}},{provide:CatalogsApi,useValue:{all:()=>of([])}},{provide:TiresApi,useValue:{list:()=>of({items:[]})}},{provide:HttpClient,useValue:{get:(url:string)=>of(url==='/api/usuarios/roles'?roles:url.includes('autorizaciones')||url.includes('/movimientos')||url.includes('/vehiculos')?{items:[],totalItems:0,pageNumber:1,pageSize:20,totalPages:0}:[])}}]});});
 async function create<T>(type:any):Promise<ComponentFixture<T>>{try{const f=TestBed.createComponent<T>(type);f.detectChanges();await f.whenStable();f.detectChanges();return f;}catch(e){throw new Error('Fixture '+type.name+': '+String(e));}}
 it('muestra módulos amigables y actualiza los chips al cambiar de rol',async()=>{
  const f=await create<CatalogAdmin>(CatalogAdmin),p=f.componentInstance;p.open();f.detectChanges();await f.whenStable();
  expect(f.nativeElement.querySelector('.role-access').textContent).toContain('Selecciona un rol para ver sus accesos.');
  const select=f.nativeElement.querySelector('.user-section select') as HTMLSelectElement;
  select.value='r1';select.dispatchEvent(new Event('change'));f.detectChanges();await f.whenStable();f.detectChanges();
  let text=f.nativeElement.querySelector('.role-access').textContent;expect(text).toContain('Vehículos');expect(text).toContain('Movimientos');expect(text).not.toContain('operaciones.');expect(text).not.toContain('modulos.');
  select.value='r2';select.dispatchEvent(new Event('change'));f.detectChanges();await f.whenStable();f.detectChanges();
  text=f.nativeElement.querySelector('.role-access').textContent;expect(text).toContain('Inventario');expect(text).toContain('Analítica');expect(text).not.toContain('Vehículos');
 });
 for(const width of [1440,1366,1024,768,390]){
  it('Administración: tabla y drawer contenidos a '+width,async()=>{
   const f=await create<CatalogAdmin>(CatalogAdmin),p=f.componentInstance;
   p.users.set([{id:'u1',username:long,nombre:long,activo:true,rolId:'r1',rol:'Operador',rolCodigo:'OPERADOR',centroIds:[],centros:[long],accesoGlobal:false}]);p.open();p.roleId='r1';
   p.centers.set(Array.from({length:30},(_,i)=>({id:''+i,codigo:''+i,nombre:long,activo:true})));f.detectChanges();
   viewport(f,width,doc=>{const drawer=doc.querySelector('.drawer')!;expect(drawer.getBoundingClientRect().right).toBeLessThanOrEqual(width);expect(drawer.getBoundingClientRect().left).toBeGreaterThanOrEqual(0);const table=doc.querySelector('.table')!;expect(doc.defaultView!.getComputedStyle(table).overflowX).toBe('auto');if(width===390)expect(table.scrollWidth).toBeGreaterThan(table.clientWidth);const centers=doc.querySelector('.center-list')!;expect(centers.scrollHeight).toBeGreaterThan(centers.clientHeight);});
  });
  it('Autorizaciones: tabla y grupo en modal contenidos a '+width,async()=>{
   const f=await create<AuthorizationsPage>(AuthorizationsPage),p=f.componentInstance;p.rows.set([request]);p.selected.set(request);p.groupRows.set([request,{...request,id:'s2',posicionDestino:'P2'}]);f.detectChanges();
   viewport(f,width,doc=>{expect(doc.querySelectorAll('.position-change').length).toBe(2);const modal=doc.querySelector('.detail')!;expect(modal.getBoundingClientRect().right).toBeLessThanOrEqual(width);expect(modal.scrollWidth).toBeLessThanOrEqual(modal.clientWidth);const table=doc.querySelector('.table')!;expect(doc.defaultView!.getComputedStyle(table).overflowX).toBe('auto');});
  });
  it('Reparaciones: tarjetas y recepción contenidas a '+width,async()=>{
   const f=await create<RepairsPage>(RepairsPage),p=f.componentInstance;p.orders.set([{...request,id:'r1',estado:'OPCIONADA',kilometrajeAcumulado:100000,fechaOpcionada:'2026-09-18',marca:long,dimension:long,origen:long,evidencias:0}]);p.modal.set('receive');p.receiveRows.set([{orderId:'o1',llanta:long,selected:true,result:'REPARADA',cost:100,notes:long}]);f.detectChanges();
   viewport(f,width,doc=>{const modal=doc.querySelector('.receive')!;expect(modal.scrollWidth).toBeLessThanOrEqual(modal.clientWidth);expect(modal.getBoundingClientRect().right).toBeLessThanOrEqual(width);});
  });
  it('Movimientos: tabla, pestañas y detalle contenidos a '+width,async()=>{
   const f=TestBed.createComponent(MovementLedgerPage),p=f.componentInstance;spyOn(p,'refresh').and.resolveTo();p.loading.set(false);p.moves.set([move]);p.selected.set(move);f.detectChanges();
   viewport(f,width,doc=>{const dialog=doc.querySelector('.detail')!;expect(dialog.scrollWidth).toBeLessThanOrEqual(dialog.clientWidth);expect(dialog.getBoundingClientRect().right).toBeLessThanOrEqual(width);expect(doc.defaultView!.getComputedStyle(doc.querySelector('.table-shell')!).overflowX).toBe('auto');});
  });
  it('Montajes: formulario contenido a '+width,async()=>{const f=await create<MovementsPage>(MovementsPage);viewport(f,width);});
 }
});
