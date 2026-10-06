import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { InspectionPage } from './inspection-page';
import { provideRouter } from '@angular/router';

describe('InspectionPage vehicle search', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InspectionPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('filters vehicles by internal number, plate, type, center and without accents', async () => {
    const fixture = TestBed.createComponent(InspectionPage);
    fixture.detectChanges();

    http.expectOne('/api/inspecciones/vehiculos').flush([
      { id: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroCodigo: 'BOG', centroNombre: 'Bogotá Norte' },
      { id: '2', numeroInterno: 'CAM-202', placa: 'XYZ987', tipo: 'Camión', centroCodigo: 'MED', centroNombre: 'Medellín' },
    ]);
    http.expectOne('/api/inspecciones/opciones').flush({ condiciones: [], causas: [], recomendaciones: [] });
    http.expectOne('/api/inspecciones/resumen').flush({ pendientesHoy: 0, realizadasHoy: 0, conNovedad: 0, conAlerta: 0 });
    http.expectOne('/api/inspecciones/historial').flush([]);
    await fixture.whenStable();
    expect(fixture.componentInstance.selectedVehicleId()).toBeFalsy();
    void fixture.componentInstance.chooseVehicle('1');
    http.expectOne('/api/inspecciones/contexto/1').flush({ vehiculoId: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroNombre: 'Bogotá Norte', ejes: [] });
    await fixture.whenStable();

    fixture.componentInstance.vehicleSearch.set('camion med');
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredVehicles().map(vehicle => vehicle.id)).toEqual(['2']);
  });

  it('keeps the active vehicle unchanged when a search has no results', async () => {
    const fixture = TestBed.createComponent(InspectionPage);
    fixture.detectChanges();

    http.expectOne('/api/inspecciones/vehiculos').flush([
      { id: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroCodigo: 'BOG', centroNombre: 'Bogotá' },
    ]);
    http.expectOne('/api/inspecciones/opciones').flush({ condiciones: [], causas: [], recomendaciones: [] });
    http.expectOne('/api/inspecciones/resumen').flush({ pendientesHoy: 0, realizadasHoy: 0, conNovedad: 0, conAlerta: 0 });
    http.expectOne('/api/inspecciones/historial').flush([]);
    await fixture.whenStable();
    expect(fixture.componentInstance.selectedVehicleId()).toBeFalsy();
    void fixture.componentInstance.chooseVehicle('1');
    http.expectOne('/api/inspecciones/contexto/1').flush({ vehiculoId: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroNombre: 'Bogotá', ejes: [] });
    await fixture.whenStable();

    fixture.componentInstance.vehicleSearch.set('no existe');
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredVehicles()).toEqual([]);
    expect(fixture.componentInstance.selectedVehicleId()).toBe('1');
    expect(fixture.componentInstance.stage()).toBe('summary');
  });

  it('blocks operational mounting without a physical discrepancy',async()=>{const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.assignmentTireId='new';page.assignmentReason='Instalar nueva';await page.assignTire();http.expectNone(r=>r.method==='POST');expect(page.assignmentMessage()).toContain('Programación');});
  it('sends physical identity and expected previous tire when correcting',async()=>{const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.mileage=1000;page.selectedVehicleId.set('v');page.physicalDiscrepancy=true;page.physicalIdentifier='SER-Y';page.assignmentTireId='Y';page.assignmentReason='Encontrada físicamente';page.context.set({vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:{id:'X',codigo:'X',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});page.positions.set([{id:'p',code:'P1',side:'I',tire:'X',brand:'M',reference:'R',dimension:'D',state:'normal',outer:null,center:null,inner:null,condition:'',cause:'',recommendation:'',notes:'',saved:false,identification:'confirmed'}]);page.selectedId.set('p');const pending=page.assignTire();http.expectOne('/api/inspecciones').flush({id:'i1'});await Promise.resolve();await Promise.resolve();const req=http.expectOne('/api/inspecciones/i1/posiciones/p/asignar');expect(req.request.body).toEqual({llantaId:'Y',motivo:'Encontrada físicamente',discrepanciaFisica:true,identificadorFisico:'SER-Y',llantaAnteriorId:'X'});req.flush({...page.context(),ejes:[]});await pending;});

  it('precarga odómetro y bloquea kilometraje inferior',async()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());const pending=page.chooseVehicle('v');
    http.expectOne('/api/inspecciones/contexto/v').flush({vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',kilometraje:100000,ejes:[]});await pending;
    expect(page.mileage).toBe(100000);expect(page.mileageValid()).toBeTrue();page.mileage=99999;expect(page.mileageValid()).toBeFalse();
  });
  it('permite cerrar mensajes, autocierra éxito y conserva errores',fakeAsync(()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.notify('Guardada','success');tick(4499);expect(page.message()).toBe('Guardada');tick(1);expect(page.message()).toBe('');
    page.notify('No guardada');tick(10000);expect(page.message()).toBe('No guardada');page.clearMessage();expect(page.message()).toBe('');
    page.notify('Anterior');page.changeStage('summary');expect(page.message()).toBe('');page.ngOnDestroy();
  }));
  it('el botón del aviso cierra sin bloquear las acciones',()=>{
    spyOn(InspectionPage.prototype,'ngOnInit').and.resolveTo();const f=TestBed.createComponent(InspectionPage);f.componentInstance.notify('Error');f.detectChanges();
    const button=f.nativeElement.querySelector('.toast button');expect(button.getAttribute('aria-label')).toBe('Cerrar mensaje');button.click();f.detectChanges();expect(f.nativeElement.querySelector('.toast')).toBeNull();
  });
  it('registra nueva por endpoint limitado y continúa leyendo la misma posición',async()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.mileage=1000;page.selectedVehicleId.set('v');
    const context={vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:null}]}]};
    page.context.set(context);page.positions.set([{id:'p',code:'P1',side:'I',tire:'Sin llanta',brand:'',reference:'',dimension:'',state:'empty',outer:null,center:null,inner:null,condition:'',cause:'',recommendation:'',notes:'',saved:false,identification:'vacant'}]);page.selectedId.set('p');
    page.openAssignment();page.assignmentMode.set('new');page.foundCatalogs.set({marcas:[],referencias:[],dimensiones:[],tipos:[]});page.newTire={codigo:'N',serial:'S',marcaId:'m',referenciaId:'r',dimensionId:'d',tipoLlantaId:'t',profundidadInicial:12};page.physicalDiscrepancy=true;page.assignmentReason='Observada';
    const pending=page.assignTire();http.expectOne('/api/inspecciones').flush({id:'i'});await Promise.resolve();await Promise.resolve();
    const req=http.expectOne('/api/inspecciones/i/llantas-encontradas');expect(req.request.body.centroId).toBe('c');expect(req.request.body.llantaAnteriorId).toBeNull();http.expectNone('/api/llantas');
    req.flush({...context,ejes:[{...context.ejes[0],posiciones:[{...context.ejes[0].posiciones[0],llanta:{id:'n',codigo:'N',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});await pending;
    expect(page.assignmentOpen()).toBeFalse();expect(page.selected().id).toBe('p');expect(page.selected().tire).toBe('N');expect(page.selected().state).toBe('normal');expect(page.selected().saved).toBeFalse();
    Object.assign(page.selected(),{outer:11,center:11,inner:11,condition:'cond',recommendation:'rec'});const save=page.savePosition();await Promise.resolve();await Promise.resolve();const put=http.expectOne('/api/inspecciones/i/posiciones/p');expect(put.request.body.profundidadCentro).toBe(11);put.flush({});await save;expect(page.selected().saved).toBeTrue();page.ngOnDestroy();
  });
  it('una corrección regularizada no vuelve a quedar pendiente al abrir borrador',async()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.vehicles.set([{id:'v',internal:'1',plate:'P',type:'Camión',centerId:'c',centerCode:'C',centerName:'Centro',regional:''}]);
    const pending=page.openInspection({id:'i',fecha:'2026-09-29',placa:'P',numeroInterno:'1',centro:'C',tecnico:'T',kilometraje:1000,inspeccionadas:0,novedades:0,alertas:0,estado:'Borrador'});
    http.expectOne('/api/inspecciones/contexto/v').flush({vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:{id:'n',codigo:'N',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});
    http.expectOne('/api/inspecciones/i').flush({id:'i',vehiculoId:'v',kilometraje:1000,estado:'Borrador',detalles:[],inconsistencias:[{id:'x',posicionId:'p',llantaEncontradaId:'n',identificadorEncontrado:'N',estado:'Regularizada'}]});await pending;
    expect(page.selected().state).toBe('normal');expect(page.selected().identification).toBe('confirmed');
  });
});


