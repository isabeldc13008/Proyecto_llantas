import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');
  return next(token ? req.clone({ setHeaders:{ Authorization:`Bearer ${token}` } }) : req);
};

export const apiErrorInterceptor: HttpInterceptorFn = (req, next) => next(req).pipe(catchError((error:HttpErrorResponse) => {
  const message = error.error?.message ?? (error.status === 0 ? 'No fue posible conectar con la API.' : 'No fue posible completar la operación.');
  return throwError(() => ({ ...error, userMessage:message }));
}));
