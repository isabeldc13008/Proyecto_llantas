import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { from, switchMap, catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api/') || req.url === '/api/auth/login') return next(req);
  const auth=inject(AuthService); const router=inject(Router);
  return from(auth.accessToken()).pipe(
    switchMap(token=>next(token?req.clone({setHeaders:{Authorization:`Bearer ${token}`}}):req)),
    catchError(error=>{
      if(error.status===401){auth.clearSession();void router.navigateByUrl('/acceso');}
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
