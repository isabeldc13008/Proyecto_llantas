import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

const technicianModules = new Set(['', 'mis-actividades', 'llantas', 'inspecciones', 'montajes', 'movimientos', 'historial']);
const supervisorBlocked = new Set(['carga-masiva', 'administracion', 'auditoria']);

export const roleGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.user()?.role;
  const path = route.routeConfig?.path ?? '';

  if (role === 'Administrador') return true;
  if (role === 'Supervisor' && !supervisorBlocked.has(path)) return true;
  if (role === 'Técnico' && technicianModules.has(path)) return true;
  return router.createUrlTree(['/']);
};
