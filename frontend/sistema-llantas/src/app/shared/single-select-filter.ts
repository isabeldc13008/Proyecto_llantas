import {Component,ElementRef,EventEmitter,HostListener,Input,Output,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {MultiSelectOption} from './multi-select-filter';

@Component({selector:'app-single-select-filter',imports:[FormsModule],template:`
<div class="multi single" [class.open]="open()"><b>{{label}}</b>
 <button type="button" class="trigger" [disabled]="disabled" (click)="toggle($event)" [attr.aria-expanded]="open()" aria-haspopup="dialog"><span>{{summary()}}</span><i>{{open()?'▲':'▼'}}</i></button>
 @if(open()){
  <button class="select-backdrop" type="button" aria-label="Cerrar selector" (click)="close()"></button>
  <section class="popover" role="dialog" [attr.aria-label]="label" (pointerdown)="$event.stopPropagation()">
   <header><strong>{{label}}</strong><button type="button" aria-label="Cerrar selector" (click)="close()">×</button></header>
   <div class="commands"><button type="button" (click)="searchVisible.set(!searchVisible())" [attr.aria-expanded]="searchVisible()">⌕ Buscar</button>@if(clearable){<button type="button" (click)="clear()">Limpiar</button>}</div>
   @if(searchVisible()){<label class="search"><span>Buscar</span><input type="search" [ngModel]="search()" (ngModelChange)="search.set($event)" placeholder="Escribe para filtrar"></label>}
   <div class="options">@for(option of filtered();track option.value){<label [class.selected]="value===option.value"><input type="radio" [name]="label" [checked]="value===option.value" (change)="choose(option.value)"><span>{{option.label}}</span></label>}@empty{<p>{{emptyMessage()}}</p>}</div>
  </section>
 }
</div>`,styleUrl:'./multi-select-filter.scss'})
export class SingleSelectFilter{
 @Input()label='';@Input()options:MultiSelectOption[]=[];@Input()value='';@Input()loading=false;@Input()error='';@Input()disabled=false;@Input()placeholder='Seleccionar';@Input()clearable=true;
 @Output()valueChange=new EventEmitter<string>();open=signal(false);search=signal('');searchVisible=signal(false);
 constructor(private host:ElementRef<HTMLElement>){}
 filtered(){const term=this.searchVisible()?this.search().trim().toLocaleLowerCase('es'):'';return term?this.options.filter(x=>x.label.toLocaleLowerCase('es').includes(term)):this.options;}
 toggle(e?:Event){e?.stopPropagation();if(this.disabled)return;if(this.open())this.close();else{this.searchVisible.set(false);this.open.set(true);}}
 choose(value:string){this.value=value;this.valueChange.emit(value);this.close();}
 clear(){if(!this.clearable)return;this.value='';this.valueChange.emit('');this.close();}
 summary(){return this.options.find(x=>x.value===this.value)?.label??this.placeholder;}
 emptyMessage(){if(this.loading)return'Cargando…';if(this.error)return'Error al cargar';if(!this.options.length)return'Sin opciones';return`Sin resultados para “${this.search()}”`;}
 close(){this.open.set(false);this.search.set('');this.searchVisible.set(false);}
 @HostListener('document:pointerdown',['$event'])outside(e:PointerEvent){if(this.open()&&!this.host.nativeElement.contains(e.target as Node))this.close();}
 @HostListener('keydown.escape')escape(){this.close();}
}
