import {TestBed} from '@angular/core/testing';
import {provideRouter,ActivatedRoute,convertToParamMap} from '@angular/router';
import {of,throwError} from 'rxjs';
import {DispositionApi} from './disposition-api';
import {DispositionHome} from './disposition-home';
import {DispositionLotDetail} from './disposition-lot-detail';
import {DispositionDispatchDetail} from './disposition-dispatch-detail';
import {DispositionTracking} from './disposition-tracking';
import {AuthService} from '../../core/auth/auth.service';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {Lot,Dispatch} from './disposition-models';

describe('Disposición: operaciones y errores visibles',()=>{
 let api:any;
 beforeEach(()=>{api={summary:()=>throwError(()=>new Error('Consulta no disponible')),lot:()=>of(lot),receive:jasmine.createSpy().and.returnValue(of({})),dispatch:()=>of(dispatch),close:jasmine.createSpy().and.returnValue(of({...dispatch,estado:'CERRADO'}))};TestBed.configureTestingModule({providers:[provideRouter([]),{provide:ActivatedRoute,useValue:{snapshot:{paramMap:convertToParamMap({id:'lot'}),queryParamMap:convertToParamMap({}),data:{receive:true}}}},{provide:DispositionApi,useValue:api},{provide:AuthService,useValue:{has:()=>true}},{provide:CatalogsApi,useValue:{all:()=>of([])}}]})});
 it('aprobar desde tabla exige acción autorizada por servidor',async()=>{api.approve=jasmine.createSpy().and.returnValue(of({}));const c=TestBed.createComponent(DispositionHome).componentInstance;const row={ruta:'/disposicion-final/ordenes/order'} as any;await c.approveRow(row);expect(api.approve).not.toHaveBeenCalled();c.details.set({[row.ruta]:{accionesPermitidas:['APROBAR'],evidencias:[]} as any});await c.approveRow(row);expect(api.approve).toHaveBeenCalledOnceWith('order');});
 const lot={id:'lot',codigo:'DSP-1',origen:{id:'a',nombre:'Bello'},destino:{id:'r',nombre:'R1'},estado:'RECEPCION_PARCIAL',fechaSalida:'2026-10-01',enviadas:18,recibidas:0,pendientes:18,items:[],novedades:[],puedeRecibir:true} as unknown as Lot;
 const dispatch={id:'d',codigo:'SV-1',estado:'ENVIADO',puedeCerrar:false,rowVersion:'one',items:[],actas:[],motivosBloqueo:['Falta soporte']} as unknown as Dispatch;
 it('un error de resumen no muestra indicadores cero',async()=>{const f=TestBed.createComponent(DispositionHome);f.detectChanges();await f.whenStable();f.detectChanges();expect(f.nativeElement.textContent).toContain('Consulta no disponible');expect(f.nativeElement.querySelector('.summary-grid')).toBeNull();});
 it('recepción inicia vacía y envía exclusivamente la selección',async()=>{const c=TestBed.createComponent(DispositionLotDetail).componentInstance;await c.load();expect(c.selected()).toEqual([]);await c.confirm();expect(api.receive).not.toHaveBeenCalled();c.selected.set(['first','second']);await c.confirm();expect(api.receive).toHaveBeenCalledOnceWith('lot',['first','second']);expect(c.selected()).toEqual([]);});
 it('cierre no envía petición si falta soporte o hay una actualización pendiente',async()=>{const c=TestBed.createComponent(DispositionDispatchDetail).componentInstance;await c.load();c.notes='Concepto';await c.close();expect(api.close).not.toHaveBeenCalled();c.data.set({...dispatch,puedeCerrar:true});c.loading.set(true);await c.close();expect(api.close).not.toHaveBeenCalled();c.loading.set(false);await c.close();expect(api.close).toHaveBeenCalledOnceWith('lot','one','Concepto');});
 it('una etapa sin evidencia sigue pendiente y muestra ausencia de registro',()=>{const f=TestBed.createComponent(DispositionTracking);f.componentRef.setInput('events',[{tipo:'EVALUACION',etiqueta:'Evaluación',estadoVisual:'PENDIENTE',fecha:null,responsable:null,observacion:null,referencia:null}]);f.detectChanges();expect(f.nativeElement.querySelector('.complete')).toBeNull();expect(f.nativeElement.textContent).toContain('Sin registro histórico');});
});

