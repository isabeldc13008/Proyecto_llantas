import {TestBed} from '@angular/core/testing';
import {provideRouter,Router,ActivatedRouteSnapshot} from '@angular/router';
import {roleGuard} from '../../core/auth/role.guard';
import {AuthService} from '../../core/auth/auth.service';
import {mountMatches,validatedScheduleContext} from './maintenance-context';

describe('Maintenance operational context',()=>{
 it('allows analytical detail with the analytics module permission',()=>{
  TestBed.configureTestingModule({providers:[provideRouter([]),{provide:AuthService,useValue:{canModule:(m:string)=>m==='analitica'}}]});
  const result=TestBed.runInInjectionContext(()=>roleGuard({routeConfig:{path:'analitica/llantas/:id'}} as ActivatedRouteSnapshot,{} as any));
  expect(result).toBeTrue();
 });
 it('rejects a changed tire or ambiguous mount',()=>{
  expect(mountMatches({vehiculoId:'v',ejes:[{posiciones:[{id:'p',llanta:{id:'other'}}]}]},'v','p','t')).toBeFalse();
  expect(mountMatches({vehiculoId:'v',ejes:[{posiciones:[{id:'p',llanta:{id:'t'}}]}]},'v','p','t')).toBeTrue();
 });
 it('does not accept an alert whose tire context changed',()=>{
  expect(()=>validatedScheduleContext({id:'t',ubicacion:{estado:'UNICA',vehiculo:{id:'new'},posicion:{id:'p'}}} as any,'t','old','p')).toThrowError();
 });
});
