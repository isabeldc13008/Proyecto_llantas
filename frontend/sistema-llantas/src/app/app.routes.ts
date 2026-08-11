import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

const demo=(title:string,eyebrow:string,description:string,action:string,kind:string)=>({title,eyebrow,description,action,kind});
const protectedRoutes:Routes=[
 {path:'',loadComponent:()=>import('./features/dashboard/dashboard').then(m=>m.Dashboard)},
 {path:'llantas',loadChildren:()=>import('./features/llantas/llantas.routes').then(m=>m.LLANTAS_ROUTES)},
 {path:'vehiculos',data:demo('Vehículos y posiciones','Flota','Configuración visual de ejes, posiciones y llantas instaladas.','Nuevo vehículo','vehiculos'),loadComponent:page},
 {path:'inventario',data:demo('Inventario operativo','Existencias','Disponibilidad por centro, estado, marca, dimensión y ubicación.','Registrar traslado','inventario'),loadComponent:page},
 {path:'inspecciones',data:{kind:'inspecciones'},loadComponent:operations},
 {path:'alertas',data:demo('Alertas','Atención prioritaria','Hallazgos automáticos por desgaste, vencimiento y reglas del negocio.','Configurar reglas','alertas'),loadComponent:page},
 {path:'programacion',data:demo('Programación','Agenda operacional','Inspecciones, rotaciones y mantenimientos próximos o vencidos.','Programar actividad','programacion'),loadComponent:page},
 {path:'montajes',data:{kind:'montajes'},loadComponent:operations},
 {path:'movimientos',data:demo('Movimientos','Trazabilidad','Registro inmutable de cada cambio de estado, centro o posición.','Registrar movimiento','movimientos'),loadComponent:page},
 {path:'reparaciones',data:demo('Reparaciones','Taller','Diagnósticos, proveedores, costos, evidencias y resultados.','Enviar a reparación','reparaciones'),loadComponent:page},
 {path:'reencauches',data:demo('Reencauches','Renovación','Envíos, bandas utilizadas, recepción y vida extendida de la llanta.','Nuevo reencauche','reencauches'),loadComponent:page},
 {path:'disposicion-final',loadComponent:()=>import('./features/disposition/disposition').then(m=>m.Disposition)},
 {path:'historial',data:demo('Historial de llantas','Línea de tiempo','Consulta cronológica de inspecciones, movimientos y servicios.','Buscar llanta','historial'),loadComponent:page},
 {path:'carga-masiva',data:demo('Carga masiva','Importación','Validación y previsualización de archivos Excel antes de procesarlos.','Seleccionar Excel','carga'),loadComponent:page},
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
function operations(){return import('./features/operations/operations-lab').then(m=>m.OperationsLab);}
