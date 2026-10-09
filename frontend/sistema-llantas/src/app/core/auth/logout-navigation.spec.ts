import {Component} from '@angular/core';
import {TestBed} from '@angular/core/testing';
import {provideRouter,Router} from '@angular/router';
import {provideHttpClient} from '@angular/common/http';
import {provideHttpClientTesting,HttpTestingController} from '@angular/common/http/testing';
import {App} from '../../app';
import {AuthService} from './auth.service';
import {authGuard} from './auth.guard';
@Component({selector:'test-protected',template:'Contenido protegido'}) class Protected{}
@Component({selector:'test-access',template:'Formulario de acceso'}) class Access{}
describe('Salida del shell autenticado',()=>{
 afterEach(()=>localStorage.removeItem('glld_session'));
 it('oculta la pantalla protegida, muestra acceso sin recargar y bloquea volver',async()=>{
 TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting(),provideRouter([{path:'privado',component:Protected,canActivate:[authGuard]},{path:'acceso',component:Access}])]});
 const auth=TestBed.inject(AuthService),http=TestBed.inject(HttpTestingController),router=TestBed.inject(Router);
 const login=auth.login('user','password');http.expectOne('/api/auth/login').flush({accessToken:'jwt',expiresAt:new Date(Date.now()+60000).toISOString()});await Promise.resolve();http.expectOne('/api/auth/me').flush({name:'User',username:'user',permissions:[],centerIds:[],canViewAllCenters:true});await login;
 const f=TestBed.createComponent(App);f.detectChanges();await router.navigateByUrl('/privado');f.detectChanges();expect(f.nativeElement.textContent).toContain('Contenido protegido');
 auth.logout();f.detectChanges();expect(f.nativeElement.querySelector('main').hidden).toBeTrue();await f.whenStable();f.detectChanges();expect(router.url).toBe('/acceso');expect(f.nativeElement.textContent).toContain('Formulario de acceso');expect(f.nativeElement.textContent).not.toContain('Contenido protegido');expect(f.nativeElement.querySelectorAll('router-outlet').length).toBe(1);
 await router.navigateByUrl('/privado');f.detectChanges();expect(router.url).toBe('/acceso');http.verify();
 });
});

