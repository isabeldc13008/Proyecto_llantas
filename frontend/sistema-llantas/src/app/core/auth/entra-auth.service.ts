import {Location} from '@angular/common';
import {HttpBackend,HttpClient} from '@angular/common/http';
import {inject,Injectable,Injector,signal} from '@angular/core';
import {MsalService} from '@azure/msal-angular';
import {AccountInfo,BrowserCacheLocation,InteractionRequiredAuthError,PublicClientApplication} from '@azure/msal-browser';
import {firstValueFrom} from 'rxjs';

let runtimeInstance: PublicClientApplication | null = null;

/** Resolves the one PCA created after the API configuration is loaded. */
export function getRuntimeMsalInstance(): PublicClientApplication {
 if(!runtimeInstance)throw new Error('MSAL aún no ha cargado la configuración Entra.');
 return runtimeInstance;
}

interface EntraConfig{enabled:boolean;clientId:string;tenantId:string;scope:string;redirectUri:string;postLogoutRedirectUri:string}
@Injectable({providedIn:'root'})
export class EntraAuthService{
 private http=new HttpClient(inject(HttpBackend));private location=inject(Location);private injector=inject(Injector);
 readonly enabled=signal(false);readonly error=signal('');readonly busy=signal(false);
 private config?:EntraConfig;private msal?:MsalService;private account=signal<AccountInfo|null>(null);private expires=signal(0);
 identity(){const claims=this.account()?.idTokenClaims;return claims?.tid&&claims?.oid?`${claims.tid}:${claims.oid}`:'';}
 authenticated(){return !!this.identity()&&Date.now()<this.expires();}
 async ensureAuthenticated():Promise<boolean>{
  if(!this.enabled())return true;
  if(!this.msal||!this.config){this.error.set('Microsoft Entra todavía no termina de inicializarse.');return false;}
  const account=this.msal.instance.getActiveAccount();
  if(!account){this.error.set('No hay una sesión Microsoft Entra activa.');return false;}
  try{await this.accessToken();return true;}
  catch{if(!this.error())this.error.set('No se pudo obtener el token Microsoft para el scope autorizado.');return false;}
 }
 async initialize(){
  try{
   const config=await firstValueFrom(this.http.get<EntraConfig>('/api/auth/local-config'));this.config=config;this.enabled.set(config.enabled);
   if(!config.enabled)return;
   if(!config.clientId||!config.tenantId||!config.redirectUri||!config.postLogoutRedirectUri||!config.scope)throw new Error('Configuración Entra incompleta. Contacta a TI.');
   const instance=new PublicClientApplication({auth:{clientId:config.clientId,authority:`https://login.microsoftonline.com/${config.tenantId}`,redirectUri:config.redirectUri,postLogoutRedirectUri:config.postLogoutRedirectUri,navigateToLoginRequestUrl:false},cache:{cacheLocation:BrowserCacheLocation.SessionStorage}});
   runtimeInstance=instance;
   this.msal=new MsalService(instance,this.location,this.injector);
   await firstValueFrom(this.msal.initialize());
   const result=await firstValueFrom(this.msal.handleRedirectObservable());
   if(result?.account)instance.setActiveAccount(result.account);
   const account=instance.getActiveAccount(); // Do not silently choose among corporate identities.
   if(account){this.account.set(account);await this.accessToken();}
  }catch(e:any){this.enabled.set(true);this.account.set(null);this.expires.set(0);this.error.set(e?.message??'No se pudo verificar la autenticación corporativa.');}
 }
 async signIn(){
  if(!this.msal||!this.config||this.busy())return;this.busy.set(true);this.error.set('');
  try{await firstValueFrom(this.msal.loginRedirect({scopes:[this.config.scope],prompt:'select_account'}));}
  catch{this.error.set('No fue posible iniciar sesión con Microsoft. Intenta nuevamente.');}finally{this.busy.set(false);}
 }
 async accessToken():Promise<string>{
  if(!this.enabled())return '';
  const account=this.msal?.instance.getActiveAccount();
  if(!this.msal||!this.config||!account){this.expires.set(0);throw new Error('Inicia sesión con Microsoft Entra ID.');}
  try{const result=await firstValueFrom(this.msal.acquireTokenSilent({account,scopes:[this.config.scope]}));
   if(!result.accessToken||!result.expiresOn||result.expiresOn.getTime()<=Date.now())throw new Error('Token corporativo vencido.');
   this.account.set(result.account);this.expires.set(result.expiresOn.getTime());return result.accessToken;
  }catch(e){this.expires.set(0);this.error.set(e instanceof InteractionRequiredAuthError?'Inicia sesión nuevamente con Microsoft.':'No se pudo renovar la sesión Microsoft.');throw e;}
 }
 async logout(){this.expires.set(0);this.account.set(null);if(this.msal&&this.enabled())await firstValueFrom(this.msal.logoutRedirect());}
}
