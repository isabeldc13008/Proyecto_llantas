import { Routes } from '@angular/router';
import { DashboardPageComponent } from './pages/dashboard-page.component';
import { VehiclesPageComponent } from './pages/vehicles-page.component';
import { InventoryPageComponent } from './pages/inventory-page.component';
import { GenericPageComponent } from './pages/generic-page.component';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'vehicles', component: VehiclesPageComponent },
  { path: 'inventory', component: InventoryPageComponent },
  { path: 'inspection', component: GenericPageComponent, data: { title: 'Inspección' } },
  { path: 'mounting', component: GenericPageComponent, data: { title: 'Montaje / Desmontaje' } },
  { path: 'movements', component: GenericPageComponent, data: { title: 'Movimientos' } },
  { path: 'schedule', component: GenericPageComponent, data: { title: 'Programación' } },
  { path: 'alerts', component: GenericPageComponent, data: { title: 'Alertas' } }
];
