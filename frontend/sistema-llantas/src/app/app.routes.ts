import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

const demo=(title:string,eyebrow:string,description:string,action:string,kind:string)=>({title,eyebrow,description,action,kind});
const protectedRoutes:Routes=[
 {path:'',loadComponent:()=>import('./features/dashboard/dashboard').then(m=>m.Dashboard)},
 {path:'mis-actividades',loadComponent:()=>import('./features/activities/activities-page').then(m=>m.ActivitiesPage)},
 {path:'llantas',loadChildren:()=>import('./features/llantas/llantas.routes').then(m=>m.LLANTAS_ROUTES)},
 {path:'vehiculos',loadComponent:()=>import('./features/vehicles/vehicles-page').then(m=>m.VehiclesPage)},
 {path:'inventario',loadComponent:()=>import('./features/inventory/inventory-page').then(m=>m.InventoryPage)},
 {path:'inspecciones',loadComponent:()=>import('./features/inspection/inspection-page').then(m=>m.InspectionPage)},
 {path:'alertas',loadComponent:()=>import('./features/alerts/alerts-page').then(m=>m.AlertsPage)},
 {path:'programacion',loadComponent:()=>import('./features/scheduling/scheduling-page').then(m=>m.SchedulingPage)},
 {path:'montajes',loadComponent:()=>import('./features/movements/movements-page').then(m=>m.MovementsPage)},
 {path:'movimientos',loadComponent:()=>import('./features/movements/movement-ledger-page').then(m=>m.MovementLedgerPage)},
 {path:'reparaciones',data:{serviceType:'Reparacion'},loadComponent:()=>import('./features/services/service-workflow-page').then(m=>m.ServiceWorkflowPage)},
 {path:'reencauches',data:{serviceType:'Reencauche'},loadComponent:()=>import('./features/services/service-workflow-page').then(m=>m.ServiceWorkflowPage)},
 {path:'disposicion-final',data:{serviceType:'DisposicionFinal'},loadComponent:()=>import('./features/services/service-workflow-page').then(m=>m.ServiceWorkflowPage)},
 {path:'historial',data:demo('Historial de llantas','Línea de tiempo','Consulta cronológica de inspecciones, movimientos y servicios.','Buscar llanta','historial'),loadComponent:page},
 {path:'carga-masiva',loadComponent:()=>import('./features/bulk-import/bulk-import-page').then(m=>m.BulkImportPage)},
 {path:'analitica',data:demo('Analítica','Indicadores','Rendimiento, costos, desgaste y tendencias para la toma de decisiones.','Exportar reporte','analitica'),loadComponent:page},
 {path:'administracion',loadComponent:()=>import('./features/admin/catalog-admin').then(m=>m.CatalogAdmin)},
 {path:'auditoria',data:demo('Auditoría','Gobierno de datos','Quién cambió qué, cuándo, desde dónde y con qué resultado.','Exportar auditoría','auditoria'),loadComponent:page},
];
export const routes:Routes=[
 {path:'acceso',loadComponent:()=>import('./features/auth/login').then(m=>m.Login)},
 ...protectedRoutes.map(route=>({...route,canActivate:[authGuard,roleGuard]})),
 {path:'**',redirectTo:''}
];
function page(){return import('./features/demo/demo-page').then(m=>m.DemoPage);}
