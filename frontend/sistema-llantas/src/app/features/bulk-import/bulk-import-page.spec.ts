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
    page.preview.set({ id: 'load', total: 12, validas: 12, conError: 0, errores: [] });
  });
  afterEach(() => backend.verify());

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
