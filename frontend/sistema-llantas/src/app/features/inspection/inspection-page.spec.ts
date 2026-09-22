import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
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
});
