import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('Password authentication', () => {
 let auth: AuthService;
 let http: HttpTestingController;
 beforeEach(() => {
  localStorage.removeItem("glld_session");
  TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),provideRouter([])]});
  auth=TestBed.inject(AuthService); http=TestBed.inject(HttpTestingController);
 });
 afterEach(() => {http.verify();localStorage.removeItem("glld_session");});
 it('initializes without requesting external configuration', async () => {
  await auth.initialize(); expect(auth.isLoggedIn()).toBeFalse();
 });
 it('loads the SQL profile after login and clears the token on logout', async () => {
  const result=auth.login('user','password');
  http.expectOne('/api/auth/login').flush({accessToken:'jwt',expiresAt:new Date(Date.now()+60000).toISOString()});
  await Promise.resolve();
  http.expectOne('/api/auth/me').flush({name:'User',username:'user',role:'TECNICO',permissions:[],centerIds:[]});
  expect(await result).toBeTrue(); expect(await auth.accessToken()).toBe('jwt');
  expect(JSON.parse(localStorage.getItem('glld_session')!)).toEqual({accessToken:'jwt',expiresAt:jasmine.any(String)});
  auth.logout(); expect(localStorage.getItem('glld_session')).toBeNull(); expect(auth.isLoggedIn()).toBeFalse(); expect(await auth.accessToken()).toBe('');
 });
 it('rejects invalid credentials without keeping a session', async () => {
  const result=auth.login('user','wrong');
  http.expectOne('/api/auth/login').flush({message:'Usuario o contraseña incorrectos.'},{status:401,statusText:'Unauthorized'});
  expect(await result).toBeFalse(); expect(auth.isLoggedIn()).toBeFalse();
  expect(auth.loginError()).toBe('Usuario o contraseña incorrectos.');
 });
 const saved=()=>({accessToken:'restored-token',expiresAt:new Date(Date.now()+60000).toISOString()});
 const profile={name:'User',username:'user',role:'TECNICO',permissions:[],centerIds:[]};
 it('allows template session checks while login is in flight',async()=>{
  const pending=auth.login('user','password');expect(auth.isLoggedIn()).toBeFalse();expect(await auth.accessToken()).toBe('');
  http.expectOne('/api/auth/login').flush(saved());await Promise.resolve();
  http.expectOne('/api/auth/me').flush(profile);expect(await pending).toBeTrue();
 });
 it('restores a valid session and lets the guard keep the route after F5',async()=>{
  localStorage.setItem('glld_session',JSON.stringify(saved()));
  const initialized=auth.initialize();
  expect(await auth.accessToken()).toBe('restored-token');
  http.expectOne('/api/auth/me').flush(profile);await initialized;
  expect(auth.isLoggedIn()).toBeTrue();
  expect(TestBed.runInInjectionContext(()=>authGuard({routeConfig:{path:'movimientos'}} as ActivatedRouteSnapshot,{} as RouterStateSnapshot))).toBeTrue();
 });
 it('clears an expired session without consulting me',async()=>{
  localStorage.setItem('glld_session',JSON.stringify({...saved(),expiresAt:new Date(0).toISOString()}));
  await auth.initialize();expect(auth.isLoggedIn()).toBeFalse();expect(localStorage.getItem('glld_session')).toBeNull();
  http.expectNone('/api/auth/me');
 });
 for(const value of ['invalid-json','{}',JSON.stringify({accessToken:12,expiresAt:'tomorrow'})]){
  it('rejects malformed stored session '+value,async()=>{
   localStorage.setItem('glld_session',value);await auth.initialize();
   expect(await auth.accessToken()).toBe('');expect(localStorage.getItem('glld_session')).toBeNull();
  });
 }
 it('clears session when me rejects the restored token',async()=>{
  localStorage.setItem('glld_session',JSON.stringify(saved()));
  const initialized=auth.initialize();http.expectOne('/api/auth/me').flush({}, {status:401,statusText:'Unauthorized'});
  await initialized;expect(auth.user()).toBeNull();expect(localStorage.getItem('glld_session')).toBeNull();
 });
 it('does not restore a profile after logout while me is pending',async()=>{
  localStorage.setItem('glld_session',JSON.stringify(saved()));
  const initialized=auth.initialize();const request=http.expectOne('/api/auth/me');auth.logout();request.flush(profile);
  await initialized;expect(auth.user()).toBeNull();expect(localStorage.getItem('glld_session')).toBeNull();
 });
 it('removes a session that expires while the app is open',async()=>{
  const session=saved();localStorage.setItem('glld_session',JSON.stringify(session));
  const initialized=auth.initialize();http.expectOne('/api/auth/me').flush(profile);await initialized;
  spyOn(Date,'now').and.returnValue(Date.parse(session.expiresAt)+1);
  expect(await auth.accessToken()).toBe('');expect(auth.user()).toBeNull();expect(localStorage.getItem('glld_session')).toBeNull();
 });
});
