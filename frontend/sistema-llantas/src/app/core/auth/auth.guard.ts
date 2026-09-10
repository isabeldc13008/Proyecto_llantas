import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
export const authGuard:CanActivateFn=(route)=>{const auth=inject(AuthService);const router=inject(Router);if(!auth.isLoggedIn())return router.createUrlTree(['/acceso']);if(auth.requiereCambioClave()&&route.routeConfig?.path!=='cambiar-clave')return router.createUrlTree(['/cambiar-clave']);return true;};
export const pendingPasswordGuard:CanActivateFn=()=>inject(AuthService).requiereCambioClave()?inject(Router).createUrlTree(['/cambiar-clave']):true;
