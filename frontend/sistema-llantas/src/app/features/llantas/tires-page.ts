import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { CatalogItem, Tire, TireDetail } from '../../core/models/api.models';
import { TiresApi } from '../../core/services/tires-api';
import { DataTableToolbar, TableColumn, TableFilter } from '../../shared/data-table-toolbar';
import { AuthService } from '../../core/auth/auth.service';
import { TireLifecycleDrawer } from './tire-lifecycle-drawer';

@Component({ selector:'app-tires-page', imports:[CommonModule,FormsModule,ReactiveFormsModule,DataTableToolbar,TireLifecycleDrawer], templateUrl:'./tires-page.html', styleUrl:'./tires-page.scss' })
export class TiresPage implements OnInit {
  private readonly api=inject(TiresApi); private readonly fb=inject(FormBuilder);readonly auth=inject(AuthService);
  tires=signal<Tire[]>([]); total=signal(0); page=signal(1); loading=signal(true); error=signal(''); editing=signal<Tire|null>(null); showForm=signal(false);
  catalogs=signal<Record<string,CatalogItem[]>>({}); search=this.fb.control('',{nonNullable:true});
  detail=signal<TireDetail|null>(null);detailLoading=signal(false);transferCenter='';transferReason='';transferNotes='';
  sortBy='codigo';readonly sortOptions=[{value:'codigo',label:'Código'},{value:'serial',label:'Serial'},{value:'centro',label:'Centro'},{value:'estado',label:'Estado'}];
  filterValues:Record<string,unknown>={};visibleColumns=['llanta','especificacion','estado','centro','montaje','kilometraje','inspeccion','acciones'];
  readonly columns:TableColumn[]=[{key:'llanta',label:'Llanta',required:true},{key:'especificacion',label:'Especificación'},{key:'estado',label:'Estado'},{key:'centro',label:'Centro y ubicación'},{key:'montaje',label:'Montaje actual'},{key:'kilometraje',label:'Kilometraje'},{key:'inspeccion',label:'Última inspección'},{key:'profundidad',label:'Profundidad'},{key:'acciones',label:'Acciones',required:true}];
  readonly tableFilters=computed<TableFilter[]>(()=>[{key:'centroIds',label:'Centros',type:'multi',options:this.options('centros').map(x=>({value:x.id,label:x.nombre}))},{key:'estados',label:'Estados',type:'multi',options:this.options('estados-llanta').map(x=>({value:x.nombre,label:x.nombre}))},{key:'profundidad',label:'Profundidad (mm)',type:'range'}]);
  readonly visibleCenters=computed(()=>new Set(this.tires().map(x=>x.centro)).size);
  readonly visibleStates=computed(()=>new Set(this.tires().map(x=>x.estado)).size);
  form=this.fb.group({ codigo:['',[Validators.required,Validators.maxLength(50)]], serial:['',[Validators.required,Validators.maxLength(100)]], marcaId:['',Validators.required], referenciaId:['',Validators.required], dimensionId:['',Validators.required], tipoLlantaId:['',Validators.required], estadoLlantaId:['',Validators.required], centroId:['',Validators.required], ubicacionActual:['',Validators.required], fechaCompra:[''], costo:[null as number|null,[Validators.min(0)]], profundidadInicial:[0,[Validators.required,Validators.min(0),Validators.max(100)]], fechaIngreso:[''], observaciones:[''] });
  readonly types=['marcas','referencias','dimensiones','tipos-llanta','estados-llanta','centros'];
  ngOnInit(){ this.loadCatalogs(); this.load(); }
  load(page=1){ this.loading.set(true);this.error.set('');this.api.list(page,this.search.value,this.sortBy,this.filterValues).subscribe({next:r=>{this.tires.set(r.items);this.total.set(r.totalItems);this.page.set(r.pageNumber);this.loading.set(false)},error:e=>{this.error.set(e.userMessage);this.loading.set(false)}}); }
  export(format:'csv'|'xlsx'){this.api.export(this.search.value,this.sortBy,format,this.filterValues).subscribe({next:r=>{const url=URL.createObjectURL(r.body!);const a=document.createElement('a');a.href=url;a.download=`llantas.${format}`;a.click();URL.revokeObjectURL(url)},error:e=>this.error.set(e.userMessage)})}
  clearFilters(){this.search.setValue('');this.filterValues={};this.sortBy='codigo';this.load(1)}
  showColumn(key:string){return this.visibleColumns.includes(key)}
  loadCatalogs(){ forkJoin(Object.fromEntries(this.types.map(t=>[t,this.api.catalog(t)]))).subscribe({next:r=>this.catalogs.set(Object.fromEntries(Object.entries(r).map(([k,v])=>[k,v.items]))),error:e=>this.error.set(e.userMessage)}); }
  open(tire:Tire|null=null){this.editing.set(tire);this.showForm.set(true);this.form.reset({profundidadInicial:tire?.profundidadInicial??0,codigo:tire?.codigo??'',serial:tire?.serial??'',ubicacionActual:tire?.ubicacionActual??'',marcaId:this.catalogId('marcas',tire?.marca),referenciaId:this.catalogId('referencias',tire?.referencia),dimensionId:this.catalogId('dimensiones',tire?.dimension),tipoLlantaId:this.catalogId('tipos-llanta',tire?.tipo),estadoLlantaId:this.catalogId('estados-llanta',tire?.estado),centroId:this.catalogId('centros',tire?.centro)});}
  close(){this.showForm.set(false);this.editing.set(null);this.form.reset();}
  save(){if(this.form.invalid){this.form.markAllAsTouched();return}this.loading.set(true);const raw=this.form.getRawValue();const dto={...raw,fechaCompra:raw.fechaCompra||null,fechaIngreso:raw.fechaIngreso||null,observaciones:raw.observaciones||null,rowVersion:this.editing()?.rowVersion};const request=this.editing()?this.api.update(this.editing()!.id,dto as never):this.api.create(dto as never);request.subscribe({next:()=>{this.close();this.load(this.page())},error:e=>{this.error.set(e.userMessage);this.loading.set(false)}});}
  toggle(t:Tire){if(confirm(`¿Desea ${t.activo?'inactivar':'activar'} la llanta ${t.codigo}?`))this.api.setActive(t.id,!t.activo).subscribe({next:()=>this.load(this.page()),error:e=>this.error.set(e.userMessage)});}
  showHistory(t:Tire){this.detailLoading.set(true);this.api.history(t.id).subscribe({next:x=>{this.detail.set(x);this.detailLoading.set(false)},error:e=>{this.error.set(e.userMessage);this.detailLoading.set(false)}})}
  closeHistory(){this.detail.set(null);this.transferCenter='';this.transferReason='';this.transferNotes=''}
  transfer(){const current=this.detail();if(!current||!this.transferCenter||!this.transferReason)return;this.api.transfer(current.llanta.id,this.transferCenter,this.transferReason,this.transferNotes).subscribe({next:()=>{this.closeHistory();this.load(this.page())},error:e=>this.error.set(e.userMessage)})}
  transferFromDrawer(event:{centerId:string;reason:string;notes:string}){this.transferCenter=event.centerId;this.transferReason=event.reason;this.transferNotes=event.notes;this.transfer()}
  options(type:string){return this.catalogs()[type]??[];}
  private catalogId(type:string,name?:string|null){return this.options(type).find(x=>x.nombre===name)?.id??''}
  statusTone(status:string){const value=status.toLocaleLowerCase('es');if(value.includes('disponible')||value.includes('montada'))return'positive';if(value.includes('repar')||value.includes('traslado')||value.includes('pendiente'))return'warning';if(value.includes('bloque')||value.includes('disposición')||value.includes('inactiva'))return'danger';return'neutral'}
}
