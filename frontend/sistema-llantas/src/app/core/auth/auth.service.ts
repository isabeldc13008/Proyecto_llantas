import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type UserRole=string;
export interface AuthUser{name:string;username:string;role:UserRole;roleName:string;initials:string;permissions:string[];centerIds:string[];canViewAllCenters:boolean;requiereCambioClave?:boolean}
interface LoginResponse extends AuthUser{accessToken:string;expiresAt:string}
@Injectable({providedIn:'root'})
export class AuthService {
 private readonly http=inject(HttpClient);
 readonly user=signal<AuthUser|null>(null); readonly loginError=signal('');
 private token=''; private expiresAt=0;
 private readonly sessionKey='glld_session';
 private generation=0;
 async initialize(){
  const generation=++this.generation;
  try{
   localStorage.removeItem('access_token');
   const session=JSON.parse(localStorage.getItem(this.sessionKey)??'null');
   if(!session||typeof session.accessToken!=='string'||!session.accessToken.trim()||typeof session.expiresAt!=='string'||!(Date.parse(session.expiresAt)>Date.now())){this.clearSession();return;}
   this.token=session.accessToken;this.expiresAt=Date.parse(session.expiresAt);
   const user=await firstValueFrom(this.http.get<AuthUser>('/api/auth/me'));
   if(generation===this.generation&&this.validToken())this.user.set(user);
  }catch{if(generation===this.generation)this.clearSession();}
 }
 async login(username:string,password:string){
  this.clearSession();this.loginError.set('');const generation=this.generation;
  try{
   const response=await firstValueFrom(this.http.post<LoginResponse>('/api/auth/login',{username,password}));
   if(generation!==this.generation)return false;
   this.token=response.accessToken;this.expiresAt=Date.parse(response.expiresAt);
   if(!this.token||!(this.expiresAt>Date.now()))throw new Error('Sesión inválida.');
   const user=await firstValueFrom(this.http.get<AuthUser>('/api/auth/me'));
   if(generation!==this.generation||!this.validToken())return false;
   localStorage.setItem(this.sessionKey,JSON.stringify({accessToken:this.token,expiresAt:response.expiresAt}));
   this.user.set(user);return true;
  }catch(error:any){if(generation===this.generation){this.clearSession();this.loginError.set(error?.userMessage??error?.error?.message??'No fue posible iniciar sesión.');}return false;}
 }
 private validToken(){if(this.token&&Date.now()<this.expiresAt)return true;if(this.token||this.expiresAt||this.user())this.clearSession();return false;}
 async accessToken():Promise<string>{return this.validToken()?this.token:'';}
 clearSession(){this.generation++;this.token='';this.expiresAt=0;this.user.set(null);try{localStorage.removeItem(this.sessionKey);localStorage.removeItem('access_token');}catch{/* Storage can be unavailable; the in-memory session is still cleared. */}}
 logout(){this.clearSession();}
 isLoggedIn(){return this.validToken()&&this.user()!==null}
 requiereCambioClave(){return this.user()?.requiereCambioClave===true}
 async cambiarClave(actual:string,nueva:string,confirmacion:string){await firstValueFrom(this.http.post('/api/auth/cambiar-clave',{actual,nueva,confirmacion}));this.user.set(await firstValueFrom(this.http.get<AuthUser>('/api/auth/me')))}
 isAdmin(){return this.user()?.role==='ADMINISTRADOR'}
 canSupervise(){return ['ADMINISTRADOR','SUPERVISOR_ADMINISTRADOR','SUPERVISOR'].includes(this.user()?.role??'')}
 has(permission:string){return this.user()?.permissions.includes(permission)??false}
 canModule(module:string){return this.has(`modulos.${module}.consultar`)}
 hasOperationalScope(){const u=this.user();return !!u&&(u.canViewAllCenters||(u.centerIds?.length??0)>0)}
}
