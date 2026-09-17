import {TestBed} from '@angular/core/testing';
import {provideHttpClient,withInterceptors} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {provideRouter} from '@angular/router';
import {AuthService} from './auth.service';
import {authInterceptor} from '../http/interceptors';

describe('Restored session interceptor',()=>{
 afterEach(()=>localStorage.removeItem('glld_session'));
 it('sends the restored bearer token to me during app initialization',async()=>{
  TestBed.configureTestingModule({providers:[provideHttpClient(withInterceptors([authInterceptor])),provideHttpClientTesting(),provideRouter([])]});
  localStorage.setItem('glld_session',JSON.stringify({accessToken:'persisted-jwt',expiresAt:new Date(Date.now()+60000).toISOString()}));
  const auth=TestBed.inject(AuthService);const http=TestBed.inject(HttpTestingController);
  const initialized=auth.initialize();await Promise.resolve();
  const request=http.expectOne('/api/auth/me');expect(request.request.headers.get('Authorization')).toBe('Bearer persisted-jwt');
  request.flush({username:'user',permissions:[],centerIds:[]});await initialized;
  expect(auth.isLoggedIn()).toBeTrue();http.verify();
 });
});
