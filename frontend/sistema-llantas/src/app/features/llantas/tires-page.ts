import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { CatalogItem, Tire } from '../../core/models/api.models';
import { TiresApi } from '../../core/services/tires-api';

@Component({ selector:'app-tires-page', imports:[CommonModule,ReactiveFormsModule], templateUrl:'./tires-page.html', styleUrl:'./tires-page.scss' })
export class TiresPage implements OnInit {
  private readonly api=inject(TiresApi); private readonly fb=inject(FormBuilder);
  tires=signal<Tire[]>([]); total=signal(0); page=signal(1); loading=signal(true); error=signal(''); editing=signal<Tire|null>(null); showForm=signal(false);
  catalogs=signal<Record<string,CatalogItem[]>>({}); search=this.fb.control('',{nonNullable:true});
  form=this.fb.group({ codigo:['',[Validators.required,Validators.maxLength(50)]], serial:['',[Validators.required,Validators.maxLength(100)]], marcaId:['',Validators.required], referenciaId:['',Validators.required], dimensionId:['',Validators.required], tipoLlantaId:['',Validators.required], estadoLlantaId:['',Validators.required], centroId:['',Validators.required], ubicacionActual:['',Validators.required], fechaCompra:[''], costo:[null as number|null,[Validators.min(0)]], profundidadInicial:[0,[Validators.required,Validators.min(0),Validators.max(100)]], fechaIngreso:[''], observaciones:[''] });
  readonly types=['marcas','referencias','dimensiones','tipos-llanta','estados-llanta','centros'];
  ngOnInit(){ this.loadCatalogs(); this.load(); }
  load(page=1){ this.loading.set(true);this.error.set('');this.api.list(page,this.search.value).subscribe({next:r=>{this.tires.set(r.items);this.total.set(r.totalItems);this.page.set(r.pageNumber);this.loading.set(false)},error:e=>{this.error.set(e.userMessage);this.loading.set(false)}}); }
  loadCatalogs(){ forkJoin(Object.fromEntries(this.types.map(t=>[t,this.api.catalog(t)]))).subscribe({next:r=>this.catalogs.set(Object.fromEntries(Object.entries(r).map(([k,v])=>[k,v.items]))),error:e=>this.error.set(e.userMessage)}); }
  open(tire:Tire|null=null){this.editing.set(tire);this.showForm.set(true);this.form.reset({profundidadInicial:tire?.profundidadInicial??0,codigo:tire?.codigo??'',serial:tire?.serial??'',ubicacionActual:tire?.ubicacionActual??''});}
  close(){this.showForm.set(false);this.editing.set(null);this.form.reset();}
  save(){if(this.form.invalid){this.form.markAllAsTouched();return}this.loading.set(true);const raw=this.form.getRawValue();const dto={...raw,fechaCompra:raw.fechaCompra||null,fechaIngreso:raw.fechaIngreso||null,observaciones:raw.observaciones||null,rowVersion:this.editing()?.rowVersion};const request=this.editing()?this.api.update(this.editing()!.id,dto as never):this.api.create(dto as never);request.subscribe({next:()=>{this.close();this.load(this.page())},error:e=>{this.error.set(e.userMessage);this.loading.set(false)}});}
  toggle(t:Tire){if(confirm(`¿Desea ${t.activo?'inactivar':'activar'} la llanta ${t.codigo}?`))this.api.setActive(t.id,!t.activo).subscribe({next:()=>this.load(this.page()),error:e=>this.error.set(e.userMessage)});}
  options(type:string){return this.catalogs()[type]??[];}
}
