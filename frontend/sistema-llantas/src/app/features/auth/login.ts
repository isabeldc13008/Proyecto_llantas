import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({selector:'app-login',imports:[ReactiveFormsModule],templateUrl:'./login.html',styleUrl:'./login.scss'})
export class Login{
 private fb=inject(FormBuilder);private auth=inject(AuthService);private router=inject(Router);error='';showPassword=false;
 form=this.fb.group({username:['administrador',Validators.required],password:['admin123',Validators.required]});
 async submit(){const v=this.form.getRawValue();if(await this.auth.login(v.username!,v.password!))await this.router.navigateByUrl('/');else this.error=this.auth.loginError();}
 use(username:string,password:string){this.form.setValue({username,password});this.error='';}
}
