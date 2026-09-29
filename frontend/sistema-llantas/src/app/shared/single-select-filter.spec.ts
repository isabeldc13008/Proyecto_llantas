import {TestBed} from '@angular/core/testing';
import {SingleSelectFilter} from './single-select-filter';

describe('SingleSelectFilter móvil',()=>{
 it('muestra opciones sin búsqueda ni enfoque automático; buscar es explícito',async()=>{
  const f=TestBed.createComponent(SingleSelectFilter);f.componentInstance.label='Centro';f.componentInstance.options=[{value:'c',label:'Centro Norte'}];f.detectChanges();
  const focus=spyOn(HTMLInputElement.prototype,'focus');
  f.nativeElement.querySelector('.trigger').click();f.detectChanges();await f.whenStable();
  expect(f.nativeElement.querySelector('input[type=search]')).toBeNull();expect(f.nativeElement.querySelector('.options').textContent).toContain('Centro Norte');expect(focus).not.toHaveBeenCalled();
  f.nativeElement.querySelector('.commands button').click();f.detectChanges();
  expect(f.nativeElement.querySelector('input[type=search]')).not.toBeNull();expect(focus).not.toHaveBeenCalled();
  f.nativeElement.querySelector('.select-backdrop').click();f.detectChanges();expect(f.componentInstance.open()).toBeFalse();
 });
 it('actualiza opciones recibidas después de abrir y emite selección',()=>{
  const f=TestBed.createComponent(SingleSelectFilter);const p=f.componentInstance;p.toggle();f.detectChanges();
  p.options=[{value:'new',label:'Disponible'}];f.detectChanges();const emit=spyOn(p.valueChange,'emit');
  f.nativeElement.querySelector('input[type=radio]').click();f.detectChanges();expect(emit).toHaveBeenCalledWith('new');expect(p.open()).toBeFalse();
 });
});
