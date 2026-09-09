import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type UserRole='ADMINISTRADOR'|'SUPERVISOR_ADMINISTRADOR'|'SUPERVISOR'|'TECNICO';
export interface AuthUser{name:string;username:string;role:UserRole;roleName:string;initials:string;permissions:string[];centerIds:string[];canViewAllCenters:boolean}
interface LoginResponse extends AuthUser{accessToken:string;expiresAt:string}
@Injectable({providedIn:'root'})
export class AuthService {
 private readonly http=inject(HttpClient);
 readonly user=signal<AuthUser|null>(null); readonly loginError=signal('');
 private token=''; private expiresAt=0;
 async initialize(){localStorage.removeItem('access_token');localStorage.removeItem('glld_session');}
 async login(username:string,password:string){
  this.loginError.set('');
  try{
   const response=await firstValueFrom(this.http.post<LoginResponse>('/api/auth/login',{username,password}));
   this.token=response.accessToken;this.expiresAt=Date.parse(response.expiresAt);
   this.user.set(await firstValueFrom(this.http.get<AuthUser>('/api/auth/me')));return true;
  }catch(error:any){this.clearSession();this.loginError.set(error?.userMessage??error?.error?.message??'No fue posible iniciar sesión.');return false;}
 }
 async accessToken():Promise<string>{return Date.now()<this.expiresAt?this.token:'';}
 clearSession(){this.token='';this.expiresAt=0;this.user.set(null)}
 logout(){this.clearSession();}
 isLoggedIn(){return this.user()!==null&&Date.now()<this.expiresAt}
 isAdmin(){return this.user()?.role==='ADMINISTRADOR'}
 canSupervise(){return ['ADMINISTRADOR','SUPERVISOR_ADMINISTRADOR','SUPERVISOR'].includes(this.user()?.role??'')}
 has(permission:string){return this.user()?.permissions.includes(permission)??false}
 canModule(module:string){return this.has(`modulos.${module}.consultar`)}
 hasOperationalScope(){const u=this.user();return !!u&&(u.canViewAllCenters||(u.centerIds?.length??0)>0)}
}
