import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PublicClientApplication, InteractionRequiredAuthError } from '@azure/msal-browser';
import { AuthConfig } from './auth.config';

export type UserRole='ADMINISTRADOR'|'SUPERVISOR_ADMINISTRADOR'|'SUPERVISOR'|'TECNICO';
export interface AuthUser{name:string;username:string;role:UserRole;roleName:string;initials:string;permissions:string[];centerIds:string[];canViewAllCenters:boolean}
interface LoginResponse extends AuthUser{accessToken:string;expiresAt:string}
@Injectable({providedIn:'root'})
export class AuthService {
 private readonly http=inject(HttpClient);
 readonly user=signal<AuthUser|null>(null); readonly loginError=signal(''); readonly localMode=signal(false);
 private config:AuthConfig={mode:'Entra'}; private msal?:PublicClientApplication;
 private localToken=''; private expiresAt=0; private ready?:Promise<void>;
 initialize(){return this.ready??=this.initializeSession()}
 private async initializeSession(){
  localStorage.removeItem('access_token'); localStorage.removeItem('glld_session');
  try {
   const response=await fetch('/auth-config.json',{cache:'no-store'});
   if(!response.ok)throw new Error('Configuración de autenticación no disponible.');
   this.config=await response.json();
   if(this.config.mode==='Local'){
    if(!['localhost','127.0.0.1','[::1]'].includes(location.hostname))throw new Error('Login local solo disponible en localhost.');
    this.localMode.set(true); return;
   }
   if(this.config.mode!=='Entra'||!this.config.tenantId||!this.config.clientId||!this.config.apiScope)throw new Error('Falta configurar Microsoft Entra ID.');
   this.msal=new PublicClientApplication({auth:{clientId:this.config.clientId,authority:`https://login.microsoftonline.com/${this.config.tenantId}`,redirectUri:location.origin+'/acceso'},cache:{cacheLocation:'sessionStorage'}});
   await this.msal.initialize();
   const result=await this.msal.handleRedirectPromise();
   if(result)this.msal.setActiveAccount(result.account);
   if(!this.msal.getActiveAccount())this.msal.setActiveAccount(this.msal.getAllAccounts()[0]??null);
   if(this.msal.getActiveAccount())await this.loadProfile();
  }catch(error:any){this.clearSession();this.loginError.set(error?.message??'No fue posible recuperar la sesión.');}
 }
 async login(username:string,password:string){
  this.loginError.set('');
  try{
   if(!this.localMode()){
    if(!this.msal)throw new Error('Falta configurar Microsoft Entra ID.');
    await this.msal.loginRedirect({scopes:[this.config.apiScope!]});return false;
   }
   const response=await firstValueFrom(this.http.post<LoginResponse>('/api/auth/login',{username,password}));
   this.localToken=response.accessToken;this.expiresAt=Date.parse(response.expiresAt);
   await this.loadProfile();return true;
  }catch(error:any){this.clearSession();this.loginError.set(error?.userMessage??error?.message??'No fue posible iniciar sesión.');return false;}
 }
 private async loadProfile(){this.user.set(await firstValueFrom(this.http.get<AuthUser>('/api/auth/me')))}
 async accessToken():Promise<string>{
  if(this.localMode())return Date.now()<this.expiresAt?this.localToken:'';
  const account=this.msal?.getActiveAccount();if(!account)return '';
  try{return (await this.msal!.acquireTokenSilent({account,scopes:[this.config.apiScope!]})).accessToken;}
  catch(error){if(error instanceof InteractionRequiredAuthError)this.clearSession();throw error;}
 }
 clearSession(){this.localToken='';this.expiresAt=0;this.user.set(null)}
 logout(){this.clearSession();if(this.msal)void this.msal.logoutRedirect({postLogoutRedirectUri:location.origin+'/acceso'});}
 isLoggedIn(){return this.user()!==null&&(!this.localMode()||Date.now()<this.expiresAt)}
 isAdmin(){return this.user()?.role==='ADMINISTRADOR'}
 canSupervise(){return ['ADMINISTRADOR','SUPERVISOR_ADMINISTRADOR','SUPERVISOR'].includes(this.user()?.role??'')}
 has(permission:string){return this.user()?.permissions.includes(permission)??false}
 canModule(module:string){return this.has(`modulos.${module}.consultar`)}
 hasOperationalScope(){const u=this.user();return !!u&&(u.canViewAllCenters||(u.centerIds?.length??0)>0)}
}
