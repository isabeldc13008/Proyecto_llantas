import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({selector:'app-change-password',imports:[FormsModule],template:`
 <main><h1>Cambiar contraseña</h1><p>Establece una contraseña nueva para continuar.</p>
 <form (ngSubmit)="save()">
 <label>Contraseña actual<input name="actual" type="password" autocomplete="current-password" [(ngModel)]="actual" required maxlength="256" [disabled]="busy"></label>
 <label>Nueva contraseña<input name="nueva" type="password" autocomplete="new-password" [(ngModel)]="nueva" required minlength="8" maxlength="256" [disabled]="busy"></label>
 <label>Confirmar nueva contraseña<input name="confirmacion" type="password" autocomplete="new-password" [(ngModel)]="confirmacion" required maxlength="256" [disabled]="busy"></label>
 <p>Mínimo 8 caracteres. Debe ser distinta de la actual.</p>
 @if(error){<p role="alert">{{error}}</p>}
 <button type="submit" [disabled]="busy">{{busy?'Guardando…':'Cambiar contraseña'}}</button>
 <button type="button" [disabled]="busy" (click)="logout()">Cerrar sesión</button>
 </form></main>`,styles:[`main{max-width:440px;margin:8vh auto;padding:2rem;background:white;border-radius:12px}label{display:block;margin:1rem 0}input{display:block;box-sizing:border-box;width:100%;padding:.7rem;margin-top:.4rem}button{padding:.7rem;margin:.4rem}p{font-size:.9rem}[role=alert]{color:#a02020}`]})
export class ChangePassword {
 readonly auth=inject(AuthService);private router=inject(Router);
 actual='';nueva='';confirmacion='';error='';busy=false;
 async save(){if(this.busy)return;this.error='';
  if(!this.actual||this.nueva.trim().length===0||this.nueva.length<8||this.nueva.length>256){this.error='Ingresa la contraseña actual y una nueva de 8 a 256 caracteres.';return}
  if(this.nueva===this.actual){this.error='La nueva contraseña debe ser diferente de la actual.';return}
  if(this.nueva!==this.confirmacion){this.error='La confirmación no coincide.';return}
  this.busy=true;try{await this.auth.cambiarClave(this.actual,this.nueva,this.confirmacion);this.actual='';this.nueva='';this.confirmacion='';await this.router.navigateByUrl('/');}
  catch(error:any){this.error=error?.error?.message??error?.userMessage??'No se pudo cambiar la contraseña.';}
  finally{this.actual='';this.nueva='';this.confirmacion='';this.busy=false}
 }
 logout(){this.actual='';this.nueva='';this.confirmacion='';this.auth.logout();void this.router.navigateByUrl('/acceso')}
}
