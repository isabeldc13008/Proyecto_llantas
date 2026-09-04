import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({selector:'app-login',imports:[ReactiveFormsModule],templateUrl:'./login.html',styleUrl:'./login.scss'})
export class Login{
 private fb=inject(FormBuilder);readonly auth=inject(AuthService);private router=inject(Router);error='';showPassword=false;
 form=this.fb.group({username:['',Validators.required],password:['',Validators.required]});
 ngOnInit(){if(this.auth.isLoggedIn())void this.router.navigateByUrl('/');}
 async submit(){const v=this.form.getRawValue();if(await this.auth.login(v.username!,v.password!))await this.router.navigateByUrl('/');else this.error=this.auth.loginError();}
 use(username:string,password:string){this.form.setValue({username,password});this.error='';}
}
