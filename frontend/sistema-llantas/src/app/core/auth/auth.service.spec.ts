import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('Password authentication', () => {
 let auth: AuthService;
 let http: HttpTestingController;
 beforeEach(() => {
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});
  auth=TestBed.inject(AuthService); http=TestBed.inject(HttpTestingController);
 });
 afterEach(() => http.verify());
 it('initializes without requesting external configuration', async () => {
  await auth.initialize(); expect(auth.isLoggedIn()).toBeFalse();
 });
 it('loads the SQL profile after login and clears the token on logout', async () => {
  const result=auth.login('user','password');
  http.expectOne('/api/auth/login').flush({accessToken:'jwt',expiresAt:new Date(Date.now()+60000).toISOString()});
  await Promise.resolve();
  http.expectOne('/api/auth/me').flush({name:'User',username:'user',role:'TECNICO',permissions:[],centerIds:[]});
  expect(await result).toBeTrue(); expect(await auth.accessToken()).toBe('jwt');
  auth.logout(); expect(auth.isLoggedIn()).toBeFalse(); expect(await auth.accessToken()).toBe('');
 });
 it('rejects invalid credentials without keeping a session', async () => {
  const result=auth.login('user','wrong');
  http.expectOne('/api/auth/login').flush({message:'Usuario o contraseña incorrectos.'},{status:401,statusText:'Unauthorized'});
  expect(await result).toBeFalse(); expect(auth.isLoggedIn()).toBeFalse();
  expect(auth.loginError()).toBe('Usuario o contraseña incorrectos.');
 });
});
