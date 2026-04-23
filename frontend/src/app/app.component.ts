import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="layout">
      <aside class="sidebar">
        <h2>TireControl</h2>
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/vehicles" routerLinkActive="active">Vehículos</a>
        <a routerLink="/inventory" routerLinkActive="active">Inventario</a>
        <a routerLink="/inspection" routerLinkActive="active">Inspección</a>
        <a routerLink="/mounting" routerLinkActive="active">Montaje</a>
        <a routerLink="/movements" routerLinkActive="active">Movimientos</a>
        <a routerLink="/schedule" routerLinkActive="active">Programación</a>
        <a routerLink="/alerts" routerLinkActive="active">Alertas</a>
      </aside>
      <main class="main">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {}
