import { EntraAuthService } from '../auth/entra-auth.service';
import { inject } from '@angular/core';

import { AuthService } from '../auth/auth.service';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { from, switchMap, catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api/') || req.url === '/api/auth/local-config') return next(req);
  const auth=inject(AuthService), entra=inject(EntraAuthService);
  const login=req.url==='/api/auth/login';
  if(!entra.enabled()){
    if(login)return next(req);
    return from(auth.accessToken()).pipe(
      switchMap(token=>next(token?req.clone({setHeaders:{Authorization:`Bearer ${token}`}}):req)),
      catchError(error=>{if(error.status===401)auth.logout(false);return throwError(()=>error);})
    );
  }
  const headers=async()=>{
    const result:Record<string,string>={};
    if(entra.enabled()){
      const entraToken=`Bearer ${await entra.accessToken()}`;
      result['X-Entra-Authorization']=entraToken;
      // The internal-login endpoint is the only endpoint without a local JWT
      // yet; keep the standard bearer header available through local proxies.
      if(login)result['Authorization']=entraToken;
    }
    if(!login){const token=await auth.accessToken();if(token)result['Authorization']=`Bearer ${token}`;}
    return result;
  };
  return from(headers()).pipe(
    switchMap(setHeaders=>next(req.clone({setHeaders}))),
    catchError(error=>{
      if(!login&&(error.status===401||entra.enabled()&&!entra.authenticated()))auth.logout(false);
      return throwError(()=>error);
    })
  );
};
export const apiErrorInterceptor: HttpInterceptorFn = (req, next) => next(req).pipe(catchError((error:HttpErrorResponse) => {
  const validationErrors=error.error?.errors&&typeof error.error.errors==='object'
    ? Object.values(error.error.errors).flat().filter((value):value is string=>typeof value==='string') : [];
  const unavailable=error.status===0||[502,503,504].includes(error.status);
  const message = error.error?.message
    ?? validationErrors[0]
    ?? (unavailable ? 'El servicio no está disponible. Inténtalo de nuevo más tarde.'
      : error.status===401 ? 'La sesión no es válida o expiró. Inicia sesión nuevamente.'
      : error.status===403 ? 'Tu usuario no tiene permiso para realizar esta operación.'
      : error.status===404 ? 'La operación solicitada no existe en la API.'
      : `No fue posible completar la operación (HTTP ${error.status}).`);
  return throwError(() => ({ ...error, userMessage:message }));
}));

