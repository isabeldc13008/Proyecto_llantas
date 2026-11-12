import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BulkImportPage } from './bulk-import-page';
import { apiErrorInterceptor } from '../../core/http/interceptors';

describe('Bulk import confirmation', () => {
  let page: BulkImportPage;
  let backend: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BulkImportPage],
      providers: [provideHttpClient(withInterceptors([apiErrorInterceptor])), provideHttpClientTesting()]
    });
    page = TestBed.createComponent(BulkImportPage).componentInstance;
    backend = TestBed.inject(HttpTestingController);
    page.type='vehiculos';
    page.preview.set({ id: 'load', total: 12, validas: 12, conError: 0, errores: [] });
  });
  afterEach(() => backend.verify());

  it('rejects the whole tire batch when a row has errors', async () => {
    page.type='llantas';
    page.preview.set({id:'x',token:'signed',total:2,validas:1,conError:1,errores:[{fila:3,campo:'Posicion',error:'Ocupada'}]});
    await page.confirm();
    backend.expectNone('/api/carga-masiva/llantas/confirmar');
    expect(page.preview()?.conError).toBe(1);
    expect(page.loading()).toBeFalse();
  });

  it('resends the previewed file and protected ticket on tire confirmation', async () => {
    page.type='llantas';
    const file=new File(['IdentificadorOriginal\n0001'], 'llantas.csv');page.file.set(file);
    page.preview.set({id:'x',token:'signed',total:1,validas:1,conError:0,errores:[]});
    const pending=page.confirm();
    const request=backend.expectOne('/api/carga-masiva/llantas/confirmar');
    expect(request.request.body.get('token')).toBe('signed');
    const uploaded=request.request.body.get('archivo') as File;
    expect(uploaded.name).toBe(file.name);
    expect(await uploaded.text()).toBe(await file.text());
    request.flush({procesadas:1,omitidas:0});await pending;
    expect(page.preview()).toBeNull();
  });

  it('previews mounting without using the legacy persistence endpoint', async () => {
    page.type='llantas';page.mode='montaje';page.file.set(new File(['test'],'llantas.csv'));
    const pending=page.previewFile();
    const request=backend.expectOne('/api/carga-masiva/llantas/previsualizar');
    expect(request.request.body.get('modalidad')).toBe('montaje');
    request.flush({id:'x',token:'signed',total:1,validas:1,conError:0,montajes:1,errores:[],filas:[{Posicion:'1'}]});await pending;
    expect(page.message()).toContain('sin guardar');expect(page.columns()).toEqual(['Posicion']);
  });

  it('invalidates preview when the mode changes', () => {page.reset();expect(page.preview()).toBeNull()});

  it('shows the backend validation message with the original row', async () => {
    const pending = page.confirm();
    const message = "Fila 4: Referencia 'X Multi D': No pertenece a la marca 'Michelin'.";
    backend.expectOne('/api/carga-masiva/load/confirmar').flush(
      { code: 'VALIDATION_ERROR', message }, { status: 400, statusText: 'Bad Request' }
    );
    await pending;
    expect(page.message()).toBe(message);
    expect(page.loading()).toBeFalse();
  });

  it('reports all twelve processed rows', async () => {
    const pending = page.confirm();
    backend.expectOne('/api/carga-masiva/load/confirmar').flush({ procesadas: 12, omitidas: 0 });
    await pending;
    expect(page.message()).toBe('12 filas procesadas en una transacción.');
    expect(page.loading()).toBeFalse();
  });
});
