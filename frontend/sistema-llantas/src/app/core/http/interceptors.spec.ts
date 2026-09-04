import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { authInterceptor, apiErrorInterceptor } from './interceptors';

describe('API session boundary',()=>{
 let http:HttpClient;let backend:HttpTestingController;let auth:jasmine.SpyObj<AuthService>;
 beforeEach(()=>{
  auth=jasmine.createSpyObj('AuthService',['accessToken','clearSession']);auth.accessToken.and.resolveTo('test-token');
  TestBed.configureTestingModule({providers:[provideRouter([]),provideHttpClient(withInterceptors([authInterceptor,apiErrorInterceptor])),provideHttpClientTesting(),{provide:AuthService,useValue:auth}]});
  http=TestBed.inject(HttpClient);backend=TestBed.inject(HttpTestingController);
 });
 afterEach(()=>backend.verify());
 it('never sends tokens to external URLs or static assets',()=>{
  for(const url of ['https://example.org/api/data','//example.org/api/data','/auth-config.json']){
   http.get(url).subscribe();const req=backend.expectOne(url);expect(req.request.headers.has('Authorization')).toBeFalse();req.flush({});
  }
  expect(auth.accessToken).not.toHaveBeenCalled();
 });
 it('clears an expired session on 401',async()=>{
  const navigation=spyOn(TestBed.inject(Router),'navigateByUrl').and.resolveTo(true);
  http.get('/api/llantas').subscribe({error:()=>{}});await Promise.resolve();
  const req=backend.expectOne('/api/llantas');expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
  req.flush({}, {status:401,statusText:'Unauthorized'});expect(auth.clearSession).toHaveBeenCalled();expect(navigation).toHaveBeenCalledWith('/acceso');
 });
 it('keeps the session and explains a 403',async()=>{
  let message='';http.get('/api/llantas').subscribe({error:e=>message=e.userMessage});await Promise.resolve();
  backend.expectOne('/api/llantas').flush({}, {status:403,statusText:'Forbidden'});
  expect(auth.clearSession).not.toHaveBeenCalled();expect(message).toContain('permiso');
 });
});
