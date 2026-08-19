import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type UserRole='ADMINISTRADOR'|'SUPERVISOR_ADMINISTRADOR'|'SUPERVISOR'|'TECNICO';
export interface AuthUser{name:string;username:string;role:UserRole;roleName:string;initials:string;permissions:string[];centerIds:string[];canViewAllCenters:boolean}
interface LoginResponse extends AuthUser{accessToken:string;expiresAt:string}
@Injectable({providedIn:'root'})
export class AuthService{
 private readonly http=inject(HttpClient);readonly user=signal<AuthUser|null>(this.restore());readonly loginError=signal('');
 async login(username:string,password:string){this.loginError.set('');try{const response=await firstValueFrom(this.http.post<LoginResponse>('/api/auth/login',{username,password}));localStorage.setItem('access_token',response.accessToken);localStorage.setItem('glld_session',JSON.stringify({name:response.name,username:response.username,role:response.role,roleName:response.roleName,initials:response.initials,permissions:response.permissions,centerIds:response.centerIds,canViewAllCenters:response.canViewAllCenters}));this.user.set(response);return true}catch(error:any){this.loginError.set(error?.status===401?'Usuario o contraseña incorrectos.':'No fue posible conectar con el servidor local. Verifica que la API esté activa.');return false}}
 logout(){localStorage.removeItem('access_token');localStorage.removeItem('glld_session');this.user.set(null)}
 isLoggedIn(){return this.user()!==null&&!!localStorage.getItem('access_token')}
 isAdmin(){return this.user()?.role==='ADMINISTRADOR'}
 canSupervise(){return ['ADMINISTRADOR','SUPERVISOR_ADMINISTRADOR','SUPERVISOR'].includes(this.user()?.role??'')}
 has(permission:string){return this.user()?.permissions.includes(permission)??false}
 canModule(module:string){return this.has(`modulos.${module}.consultar`)}
 hasOperationalScope(){const u=this.user();return !!u&&(u.canViewAllCenters||(u.centerIds?.length??0)>0)}
 private restore():AuthUser|null{try{return JSON.parse(localStorage.getItem('glld_session')??'null')}catch{return null}}
}
