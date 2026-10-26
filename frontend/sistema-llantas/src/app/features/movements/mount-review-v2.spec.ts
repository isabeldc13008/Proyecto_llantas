import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {provideRouter} from '@angular/router';
import {AuthorizationsPage} from '../authorizations/authorizations-page';
import {MountWorkList} from './mount-work-list';
import {MountAssignmentEditor} from './mount-assignment';

describe('Mount position decisions',()=>{
 let http:HttpTestingController;
 beforeEach(()=>{TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),provideRouter([])]});http=TestBed.inject(HttpTestingController)});
 afterEach(()=>http.verify());
 const row:any={id:'s1',tipo:'Cambio de juego',rowVersion:'AQI=',estado:'PENDIENTE_APROBACION',llantaDesplazadaId:'old',destinoDesplazada:'Inventario'};
 it('approves only the selected position, never executes',async()=>{const page=TestBed.runInInjectionContext(()=>new AuthorizationsPage());spyOn(page,'load').and.resolveTo();const pending=page.decide(row,'APROBAR');const req=http.expectOne('/api/operaciones/solicitudes/s1/decision-posicion');expect(req.request.body).toEqual({accion:'APROBAR',rowVersion:'AQI=',motivo:null,destino:null});req.flush({});await pending;http.expectNone(r=>r.url.includes('/ejecutar'));expect(page.message()).toContain('No se ejecutaron');});
 it('requires a justification to return a position',async()=>{const page=TestBed.runInInjectionContext(()=>new AuthorizationsPage());await page.decide(row,'DEVOLVER');http.expectNone(r=>r.method==='POST');});
 it('sends destination and justification separately from execution',async()=>{const page=TestBed.runInInjectionContext(()=>new AuthorizationsPage());spyOn(page,'load').and.resolveTo();page.decision(row).motivo='Revisión técnica';page.decision(row).tipo='Reparacion';const pending=page.decide(row,'MODIFICAR_DESTINO');const req=http.expectOne('/api/operaciones/solicitudes/s1/decision-posicion');expect(req.request.body.destino.tipo).toBe('Reparacion');req.flush({});await pending;});
 it('executes only approved selected positions with their versions',async()=>{const page=TestBed.runInInjectionContext(()=>new MountWorkList());spyOn(page,'load').and.resolveTo();const work:any={id:'group',posiciones:[{...row,estado:'APROBADO'},{...row,id:'s2',estado:'DEVUELTO'}]};page.selected={s1:true,s2:true};page.mileages['group']=100;const pending=page.execute(work);const req=http.expectOne('/api/operaciones/trabajos-montaje/group/ejecutar');expect(req.request.body.posiciones).toEqual([{id:'s1',rowVersion:'AQI='}]);req.flush({});await pending;});
 it('requires destination for an occupied position in the new editor',()=>{const editor=TestBed.runInInjectionContext(()=>new MountAssignmentEditor());editor.destinations=true;editor.choose({id:'p',llantaId:'old'} as any,{id:'new',codigo:'LL',marca:'M',referencia:'R'} as any);expect(editor.value[0].destinoRetirada).toBe('');editor.destination('p','Reparacion');expect(editor.value[0].destinoRetirada).toBe('Reparacion');});
});
