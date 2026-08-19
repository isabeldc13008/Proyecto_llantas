import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

const modules:Record<string,string>={'':'resumen','mis-actividades':'actividades','llantas':'llantas','vehiculos':'vehiculos','inventario':'inventario','inspecciones':'inspecciones','alertas':'alertas','programacion':'programacion','montajes':'montajes','movimientos':'movimientos','reparaciones':'reparaciones','reencauches':'reencauches','disposicion-final':'disposicion','historial':'historial','carga-masiva':'carga_masiva','analitica':'analitica','administracion':'administracion','auditoria':'auditoria'};

export const roleGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const path = route.routeConfig?.path ?? '';
  const module=modules[path];
  if(module&&auth.canModule(module)) return true;
  return router.createUrlTree(['/']);
};
