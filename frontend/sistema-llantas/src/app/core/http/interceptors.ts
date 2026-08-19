import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');
  if(token)return next(req.clone({setHeaders:{Authorization:`Bearer ${token}`}}));
  let username='';
  try{username=JSON.parse(localStorage.getItem('glld_session')??'null')?.username??''}catch{}
  return next(username?req.clone({setHeaders:{'X-Development-User':username}}):req);
};

export const apiErrorInterceptor: HttpInterceptorFn = (req, next) => next(req).pipe(catchError((error:HttpErrorResponse) => {
  const validationErrors=error.error?.errors&&typeof error.error.errors==='object'
    ? Object.values(error.error.errors).flat().filter((value):value is string=>typeof value==='string') : [];
  const unavailable=error.status===0||[502,503,504].includes(error.status);
  const message = error.error?.message
    ?? validationErrors[0]
    ?? (unavailable ? 'La API local no está disponible. Inicia el backend en el puerto 5262 e inténtalo de nuevo.'
      : error.status===401 ? 'La sesión no es válida o expiró. Inicia sesión nuevamente.'
      : error.status===403 ? 'Tu usuario no tiene permiso para realizar esta operación.'
      : error.status===404 ? 'La operación solicitada no existe en la API local.'
      : `No fue posible completar la operación (HTTP ${error.status}).`);
  return throwError(() => ({ ...error, userMessage:message }));
}));
