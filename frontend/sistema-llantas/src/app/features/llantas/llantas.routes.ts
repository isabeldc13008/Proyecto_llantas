import { Routes } from '@angular/router';
export const LLANTAS_ROUTES:Routes=[{path:'',loadComponent:()=>import('./tires-page').then(m=>m.TiresPage)}];
