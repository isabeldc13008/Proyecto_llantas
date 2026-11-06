import {Login} from '../../features/auth/login';
import {TestBed} from '@angular/core/testing';
import {provideHttpClient,HttpClient,withInterceptors} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {provideRouter} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {AuthService} from './auth.service';
import {EntraAuthService} from './entra-auth.service';
import {authInterceptor} from '../http/interceptors';

describe('AutenticaciÃ³n consecutiva Entra e interna',()=>{
 let http:HttpTestingController;
 beforeEach(()=>{localStorage.removeItem('glld_session');TestBed.configureTestingModule({imports:[Login],providers:[provideHttpClient(withInterceptors([authInterceptor])),provideHttpClientTesting(),provideRouter([])]});http=TestBed.inject(HttpTestingController);});
 afterEach(()=>{http.verify();localStorage.removeItem('glld_session');});
 it('muestra el formulario interno solo después del paso Microsoft',()=>{
  const entra=TestBed.inject(EntraAuthService);entra.enabled.set(true);const authenticated=spyOn(entra,'authenticated').and.returnValue(false);
  const fixture=TestBed.createComponent(Login);fixture.detectChanges();
  expect(fixture.nativeElement.querySelector('form')).toBeNull();expect(fixture.nativeElement.textContent).toContain('Iniciar sesión con Microsoft');
  authenticated.and.returnValue(true);fixture.detectChanges();expect(fixture.nativeElement.querySelector('form')).not.toBeNull();
 });
 it('conserva modo local cuando la integraciÃ³n estÃ¡ desactivada',async()=>{
  const entra=TestBed.inject(EntraAuthService);const pending=entra.initialize();
  http.expectOne('/api/auth/local-config').flush({enabled:false});await pending;expect(entra.enabled()).toBeFalse();
 });
 it('no permite login interno sin Microsoft cuando estÃ¡ activada',async()=>{
  TestBed.inject(EntraAuthService).enabled.set(true);
  expect(await TestBed.inject(AuthService).login('user','password')).toBeFalse();http.expectNone('/api/auth/login');
 });
 it('descarta una sesiÃ³n interna de otra identidad',async()=>{
  const entra=TestBed.inject(EntraAuthService);entra.enabled.set(true);spyOn(entra,'authenticated').and.returnValue(true);spyOn(entra,'identity').and.returnValue('tenant:other');
  localStorage.setItem('glld_session',JSON.stringify({accessToken:'old',expiresAt:new Date(Date.now()+60000).toISOString(),entraIdentity:'tenant:first'}));
  const auth=TestBed.inject(AuthService);await auth.initialize();expect(auth.isLoggedIn()).toBeFalse();http.expectNone('/api/auth/me');
 });
 it('envÃ­a Microsoft al login interno sin enviar un JWT interno',async()=>{
  const entra=TestBed.inject(EntraAuthService);entra.enabled.set(true);spyOn(entra,'accessToken').and.resolveTo('microsoft');
  const pending=firstValueFrom(TestBed.inject(HttpClient).post('/api/auth/login',{}));
  await new Promise(resolve=>setTimeout(resolve,0));const request=http.expectOne('/api/auth/login');
  expect(request.request.headers.get('X-Entra-Authorization')).toBe('Bearer microsoft');expect(request.request.headers.get('Authorization')).toBe('Bearer microsoft');request.flush({});await pending;
 });
});
