import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { provideRouter } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { provideHttpClient } from '@angular/common/http';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]),provideHttpClient()],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the product brand', () => {
    localStorage.setItem('access_token','test-token');
    TestBed.inject(AuthService).user.set({name:'Isabel Martínez',username:'administrador',role:'ADMINISTRADOR',roleName:'Administrador',initials:'IM',permissions:['centros.ver_todos','modulos.resumen.consultar'],centerIds:[],canViewAllCenters:true});
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand')?.textContent).toContain('GLLD');
    localStorage.removeItem('access_token');
  });

  it('recognizes explicit module and global-center permissions',()=>{
    const auth=TestBed.inject(AuthService);
    auth.user.set({name:'Admin',username:'admin',role:'ADMINISTRADOR',roleName:'Administrador',initials:'AD',permissions:['centros.ver_todos','modulos.llantas.consultar'],centerIds:[],canViewAllCenters:true});
    expect(auth.canModule('llantas')).toBeTrue();
    expect(auth.canModule('administracion')).toBeFalse();
    expect(auth.hasOperationalScope()).toBeTrue();
  });

  it('marks a user without centers or global permission as having no operational scope',()=>{
    const auth=TestBed.inject(AuthService);
    auth.user.set({name:'Técnico',username:'tecnico',role:'TECNICO',roleName:'Técnico',initials:'TE',permissions:['modulos.inspecciones.consultar'],centerIds:[],canViewAllCenters:false});
    expect(auth.hasOperationalScope()).toBeFalse();
  });
});
