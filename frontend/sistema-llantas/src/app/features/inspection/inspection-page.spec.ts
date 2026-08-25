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
    http.expectOne('/api/inspecciones/contexto/1').flush({ vehiculoId: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroNombre: 'Bogotá', ejes: [] });
    await fixture.whenStable();

    fixture.componentInstance.vehicleSearch.set('no existe');
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredVehicles()).toEqual([]);
    expect(fixture.componentInstance.selectedVehicleId()).toBe('1');
    expect((fixture.nativeElement as HTMLElement).querySelector('.search-status')?.textContent).toContain('No se encontraron');
  });
});
