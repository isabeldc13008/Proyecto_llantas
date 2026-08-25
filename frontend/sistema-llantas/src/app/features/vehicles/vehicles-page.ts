import {DatePipe} from '@angular/common';
import {Component,OnInit,computed,inject,signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {firstValueFrom} from 'rxjs';
import {Router} from '@angular/router';
import {AuthService} from '../../core/auth/auth.service';
import {CatalogItem} from '../../core/models/api.models';
import {CatalogsApi} from '../../core/services/catalogs-api';
import {DiagramAxle,VehicleAxleDiagram} from '../../shared/vehicle-axle-diagram';
import {VehicleConfiguration,VehicleConfigurationInput,VehicleDetail,VehicleInput,VehiclePosition,VehicleSummary,VehiclesApi} from './vehicles-api';

@Component({selector:'app-vehicles-page',imports:[FormsModule,VehicleAxleDiagram,DatePipe],templateUrl:'./vehicles-page.html',styleUrls:['./vehicles-page.scss','./vehicle-operational.scss','./configuration-builder.scss','./configuration-catalog.scss']})
export class VehiclesPage implements OnInit{
 private readonly api=inject(VehiclesApi);private readonly catalogs=inject(CatalogsApi);private readonly router=inject(Router);readonly auth=inject(AuthService);
 readonly vehicles=signal<VehicleSummary[]>([]);readonly configurations=signal<VehicleConfiguration[]>([]);readonly centers=signal<CatalogItem[]>([]);readonly selected=signal<VehicleDetail|null>(null);readonly loading=signal(false);readonly message=signal('');
 search='';showForm=false;showConfiguration=false;editing=false;tab:'resumen'|'posiciones'|'inspecciones'|'movimientos'|'historial'='resumen';mode:'scheme'|'operational'='operational';selectedPosition=signal<VehiclePosition|null>(null);form:VehicleInput=this.blank();configurationForm:VehicleConfigurationInput=this.blankConfiguration();
 readonly diagram=computed<DiagramAxle[]>(()=>this.selected()?.ejes.map(e=>({id:e.id,name:e.nombre,type:e.tipoEje,positions:e.posiciones.map(p=>({id:p.id,code:p.codigo,side:[p.lado,p.ubicacion].filter(Boolean).join(' · '),tire:p.llantaCodigo??undefined,state:!p.llantaCodigo?'empty':this.mode==='scheme'?'normal':p.atencion==='Normal'?'normal':p.atencion.toLowerCase().includes('crit')?'alert':'inconsistency'}))}))??[]);
 readonly positions=computed(()=>this.selected()?.ejes.flatMap(e=>e.posiciones)??[]);readonly covered=computed(()=>this.positions().filter(x=>x.llantaId).length);readonly completion=computed(()=>this.positions().length?Math.round(this.covered()*1000/this.positions().length)/10:null);
 async ngOnInit(){await this.reload();const[cfg,centers]=await Promise.all([firstValueFrom(this.api.configurations()),firstValueFrom(this.catalogs.all('centros',true))]);this.configurations.set(cfg);this.centers.set(centers)}
 async reload(){this.loading.set(true);try{const p=await firstValueFrom(this.api.list(this.search));this.vehicles.set(p.items);if(!this.selected()&&p.items.length)await this.open(p.items[0].id)}catch(e:any){this.message.set(e?.error?.detail??'No fue posible cargar los vehículos.')}finally{this.loading.set(false)}}
 async open(id:string){try{this.selected.set(await firstValueFrom(this.api.get(id)));this.selectedPosition.set(null);this.tab='resumen'}catch{this.message.set('No fue posible cargar el detalle del vehículo.')}}
 selectDiagram(p:{id:string|number}){this.selectedPosition.set(this.positions().find(x=>x.id===p.id)??null)}openTire(id:string){void this.router.navigate(['/llantas'],{queryParams:{llantaId:id}})}
 newVehicle(){this.editing=false;this.form=this.blank();this.showForm=true}
 edit(){const v=this.selected();if(!v)return;this.editing=true;this.form={numeroInterno:v.numeroInterno,placa:v.placa,tipo:v.tipo,centroId:v.centroId,configuracionVehiculoId:v.configuracionVehiculoId,kilometraje:v.kilometraje,estado:v.estado,rowVersion:v.rowVersion};this.showForm=true}
 async save(){try{const v=this.editing&&this.selected()?await firstValueFrom(this.api.update(this.selected()!.id,this.form)):await firstValueFrom(this.api.create(this.form));this.showForm=false;await this.reload();await this.open(v.id);this.message.set('Vehículo guardado en SQL Server.')}catch(e:any){this.message.set(e?.error?.detail??e?.error?.title??'No se pudo guardar el vehículo.')}}
 newConfiguration(){this.configurationForm=this.blankConfiguration();this.showConfiguration=true}
 addAxle(){const order=this.configurationForm.ejes.length+1;this.configurationForm.ejes.push({orden:order,nombre:`Eje ${order}`,tipoEje:order===1?'Direccional':'Tracción',posiciones:[]});this.addPosition(this.configurationForm.ejes.length-1)}
 removeAxle(index:number){this.configurationForm.ejes.splice(index,1);this.renumberConfiguration()}
 addPosition(axleIndex:number){const axle=this.configurationForm.ejes[axleIndex];const order=axle.posiciones.length+1;axle.posiciones.push({codigo:`P${this.configurationForm.ejes.flatMap(x=>x.posiciones).length+1}`,lado:order%2?'Izquierda':'Derecha',ubicacion:'Externa',orden:order})}
 removePosition(axleIndex:number,index:number){this.configurationForm.ejes[axleIndex].posiciones.splice(index,1);this.renumberConfiguration()}
 async saveConfiguration(){try{const created=await firstValueFrom(this.api.createConfiguration(this.configurationForm));this.configurations.set(await firstValueFrom(this.api.configurations()));this.form.configuracionVehiculoId=created.id;this.showConfiguration=false;this.message.set('Configuración homologada guardada en SQL Server.')}catch(e:any){this.message.set(e?.error?.detail??e?.error?.title??'No se pudo guardar la configuración.')}}
 preview():DiagramAxle[]{const cfg=this.configurations().find(x=>x.id===this.form.configuracionVehiculoId);return cfg?.ejes.map((e,ei)=>({id:ei,name:e.nombre,type:e.tipoEje,positions:e.posiciones.map((p,pi)=>({id:`${ei}-${pi}`,code:p.codigo,side:[p.lado,p.ubicacion].join(' · '),state:'empty'}))}))??[]}
 configurationPositionCount(configuration:VehicleConfiguration){return configuration.ejes.reduce((total,eje)=>total+eje.posiciones.length,0)}
 private blank():VehicleInput{return{numeroInterno:'',placa:'',tipo:'',centroId:'',configuracionVehiculoId:null,kilometraje:null,estado:'Activo'}}
 private blankConfiguration():VehicleConfigurationInput{return{codigo:'',nombre:'',tipoVehiculo:'',ejes:[]}}
 private renumberConfiguration(){this.configurationForm.ejes.forEach((e,ei)=>{e.orden=ei+1;e.posiciones.forEach((p,pi)=>p.orden=pi+1)})}
}
