import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
@Component({selector:'app-root',imports:[RouterOutlet,RouterLink,RouterLinkActive],templateUrl:'./app.html',styleUrl:'./app.scss'})
export class App {
  menuOpen=false;
  readonly auth=inject(AuthService);
  private readonly router=inject(Router);

  logout(){
    this.menuOpen=false;
    this.auth.logout();
    void this.router.navigateByUrl('/acceso');
  }
}
