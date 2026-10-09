import {toSignal} from '@angular/core/rxjs-interop';
import {filter,map} from 'rxjs';
import { Component, HostListener, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  menuOpen = false;
  sidebarCollapsed = false;

  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly currentUrl=toSignal(this.router.events.pipe(filter(e=>e instanceof NavigationEnd),map(e=>(e as NavigationEnd).urlAfterRedirects)),{initialValue:this.router.url});
  showShell(){return !!this.auth.user()&&!this.auth.requiereCambioClave()}
  hideProtected(){return !this.auth.user()&&this.currentUrl().split('?')[0]!=='/acceso'}

  toggleSidebar() {
    if (window.matchMedia('(max-width: 900px)').matches) {
      this.menuOpen = !this.menuOpen;
      return;
    }

    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  closeMobileMenu() {
    if (window.matchMedia('(max-width: 900px)').matches) {
      this.menuOpen = false;
    }
  }

  @HostListener('window:resize') onResize(){if(!window.matchMedia('(max-width: 900px)').matches)this.menuOpen=false;}
  @HostListener('document:keydown.escape') escape(){this.menuOpen=false;}
  logout() {
    this.menuOpen = false;
    this.auth.logout();

  }
}

