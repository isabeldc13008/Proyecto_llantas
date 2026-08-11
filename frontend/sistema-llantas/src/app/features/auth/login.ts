import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({selector:'app-login',imports:[ReactiveFormsModule],templateUrl:'./login.html',styleUrl:'./login.scss'})
export class Login{
 private fb=inject(FormBuilder);private auth=inject(AuthService);private router=inject(Router);error='';showPassword=false;
 form=this.fb.group({username:['administrador',Validators.required],password:['admin123',Validators.required]});
 submit(){const v=this.form.getRawValue();if(this.auth.login(v.username!,v.password!))this.router.navigateByUrl('/');else this.error='Usuario o contraseña incorrectos.';}
 use(username:string,password:string){this.form.setValue({username,password});this.error='';}
}
