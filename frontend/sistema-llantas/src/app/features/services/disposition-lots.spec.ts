import {TestBed} from '@angular/core/testing';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {DispositionLots} from './disposition-lots';
describe('Lotes de disposición',()=>{
 beforeEach(()=>TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]}));
 it('prioriza R1 y no selecciona un destino automáticamente',()=>{const page=TestBed.runInInjectionContext(()=>new DispositionLots());page.centers.set([{id:'a',codigo:'MED001',nombre:'Medellín',activo:true,relevancia:'R1'},{id:'b',codigo:'R1',nombre:'Otro',activo:true,relevancia:'R2'}]);expect(page.destinations().map(c=>c.id)).toEqual(['a']);expect(page.form.centroDestinoId).toBe('');page.onlyR1=false;expect(page.destinations().length).toBe(2);});
 it('recepción y cierre comienzan sin llantas marcadas',()=>{const page=TestBed.runInInjectionContext(()=>new DispositionLots());page.openAction({id:'l',codigo:'DSP',origen:'a',destino:'b',relevanciaDestino:'R1',estado:'RECEPCION_PARCIAL',fechaSalida:'',items:[]} as any,'recibir');expect(page.items()).toEqual([]);});
 it('envía una sola petición para recibir varias llantas',async()=>{const page=TestBed.runInInjectionContext(()=>new DispositionLots()),http=TestBed.inject(HttpTestingController);page.openAction({id:'l',items:[]} as any,'recibir');page.items.set(['a','b']);const pending=page.confirm();const request=http.expectOne('/api/servicios-llanta/disposicion/lotes/l/recibir');expect(request.request.body.ordenIds).toEqual(['a','b']);request.flush({});await Promise.resolve();http.expectOne('/api/servicios-llanta/disposicion/lotes').flush([]);await pending;http.verify();});
});
