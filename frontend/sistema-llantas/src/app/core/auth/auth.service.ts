import { Injectable, signal } from '@angular/core';

export type UserRole='Administrador'|'Supervisor'|'Técnico';
export interface DemoUser {name:string;username:string;role:UserRole;initials:string}
const users:Record<string,{password:string;user:DemoUser}>={
 administrador:{password:'admin123',user:{name:'Isabel Martínez',username:'administrador',role:'Administrador',initials:'IM'}},
 supervisor:{password:'super123',user:{name:'Carlos Mendoza',username:'supervisor',role:'Supervisor',initials:'CM'}},
 tecnico:{password:'tec123',user:{name:'Laura Ruiz',username:'tecnico',role:'Técnico',initials:'LR'}}
};

@Injectable({providedIn:'root'})
export class AuthService{
 readonly user=signal<DemoUser|null>(this.restore());
 login(username:string,password:string){const account=users[username.toLowerCase().trim()];if(!account||account.password!==password)return false;localStorage.setItem('glld_session',JSON.stringify(account.user));this.user.set(account.user);return true;}
 logout(){localStorage.removeItem('glld_session');this.user.set(null)}
 isLoggedIn(){return this.user()!==null}
 isAdmin(){return this.user()?.role==='Administrador'}
 canSupervise(){return this.user()?.role==='Administrador'||this.user()?.role==='Supervisor'}
 private restore():DemoUser|null{try{return JSON.parse(localStorage.getItem('glld_session')??'null')}catch{return null}}
}
