import {TestBed,ComponentFixture} from '@angular/core/testing';
import {provideRouter,Router} from '@angular/router';
import {of,Subject,throwError} from 'rxjs';
import {AnalyticsApi} from './analytics-api';
import {AnalyticsPageComponent} from './analytics-page';
import {MaintenanceQueueData} from './maintenance-models';
const response=():MaintenanceQueueData=>({generadoEn:'2026-10-05T10:00:00Z',alcance:{} as any,totalEnAlcance:2,totalEnCola:1,totalEvaluacionCompleta:{estado:'NO_EVALUABLE',valor:null,motivos:[]},conteos:[{clasificacion:'ATENCION_INMEDIATA',resultado:{estado:'NO_EVALUABLE',valor:null,motivos:[]}},{clasificacion:'CONDICION_POR_VERIFICAR',resultado:{estado:'PARCIAL',valor:0,motivos:[]}}],limitaciones:[],pagina:{items:[],pageNumber:1,pageSize:20,totalItems:0,totalPages:0},concentracion:[]});
describe('Maintenance decision center',()=>{
 let api:jasmine.SpyObj<AnalyticsApi>,fixture:ComponentFixture<AnalyticsPageComponent>;
 beforeEach(async()=>{api=jasmine.createSpyObj('AnalyticsApi',['options','maintenance','maintenanceVehicles']);api.options.and.returnValue(of({centros:[],marcas:[],referencias:[],dimensiones:[],estados:[],tiposVehiculo:[]}));api.maintenance.and.returnValue(of(response()));api.maintenanceVehicles.and.returnValue(of({items:[],pageNumber:1,pageSize:20,totalItems:0,totalPages:0}));await TestBed.configureTestingModule({imports:[AnalyticsPageComponent],providers:[provideRouter([]),{provide:AnalyticsApi,useValue:api}]}).compileComponents();fixture=TestBed.createComponent(AnalyticsPageComponent);fixture.detectChanges();await fixture.whenStable();fixture.detectChanges()});
 it('requests mounted tires and the review queue by default',()=>{expect(api.maintenance).toHaveBeenCalledWith(jasmine.objectContaining({montaje:'MONTADAS',vista:'COLA'}),1)});
 it('shows unavailable classification instead of zero and preserves an evaluated zero',()=>{const el:HTMLElement=fixture.nativeElement;expect(el.textContent).toContain('No evaluable');expect(el.querySelector('.count')?.textContent?.trim()).toBe('0')});
 it('ignores stale queue responses after changing filters',async()=>{const older=new Subject<MaintenanceQueueData>();api.maintenance.and.returnValue(older);const first=fixture.componentInstance.load();api.maintenance.and.returnValue(of({...response(),totalEnAlcance:9}));await fixture.componentInstance.load();older.next({...response(),totalEnAlcance:99});older.complete();await first;expect(fixture.componentInstance.data()?.totalEnAlcance).toBe(9)});
 it('shows a query error rather than a successful empty count',async()=>{api.maintenance.and.returnValue(throwError(()=>({userMessage:'Consulta fallida'})));await fixture.componentInstance.load();fixture.detectChanges();expect(fixture.componentInstance.data()).toBeNull();expect(fixture.nativeElement.textContent).toContain('Consulta fallida')});
});
