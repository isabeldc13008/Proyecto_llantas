import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CENTERS } from '../../core/data/centers';
import { DiagramAxle, DiagramPosition, VehicleAxleDiagram } from '../../shared/vehicle-axle-diagram';

interface Inspection {id:string;vehicle:string;tire:string;position:string;outer:number;center:number;inner:number;pressure:number;result:string;date:string}
interface Operation {id:string;type:string;vehicle:string;tire:string;position:string;destination:string;date:string}
interface InspectionRow {position:number;location:string;tire:string;outer:number;center:number;inner:number;pressure:number;condition:string;decision:string;cause:string}

@Component({selector:'app-operations-lab',imports:[CommonModule,FormsModule,ReactiveFormsModule,VehicleAxleDiagram],templateUrl:'./operations-lab.html',styleUrls:['./operations-lab.scss','./inspection-batch.scss','./vehicle-map.scss']})
export class OperationsLab{
 private fb=inject(FormBuilder); private route=inject(ActivatedRoute); readonly kind=this.route.snapshot.data['kind'] as string;
 readonly vehicles=['TJK-482 · Kenworth T680','WNP-193 · Chevrolet FVR','LST-809 · Mercedes-Benz O500'];
 readonly centers=CENTERS;
 readonly tires=['LL-000327 · Goodyear KMAX','LL-000588 · Michelin X Multi','LL-000612 · Bridgestone R268'];
 readonly installed=['LL-000184 · Posición 1','LL-000229 · Posición 2','LL-000401 · Posición 4'];
 readonly depths=Array.from({length:41},(_,i)=>i/2); readonly pressures=Array.from({length:31},(_,i)=>80+i);
 inspectionRows=signal<InspectionRow[]>([
  {position:1,location:'Direccional izquierda',tire:'LL-000184 · Michelin X Multi D',outer:12.5,center:13,inner:12,pressure:105,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:2,location:'Direccional derecha',tire:'LL-000229 · Michelin X Multi D',outer:11.5,center:12,inner:11.5,pressure:104,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:3,location:'Tracción 1 · Izq. externa',tire:'LL-000327 · Goodyear KMAX',outer:8,center:9,inner:8.5,pressure:106,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:4,location:'Tracción 1 · Izq. interna',tire:'LL-000401 · Bridgestone R268',outer:6,center:7.5,inner:6.5,pressure:102,condition:'Atención',decision:'Evaluar posible reencauche',cause:'Desgaste irregular'},
  {position:5,location:'Tracción 1 · Der. interna',tire:'LL-000588 · Michelin X Multi',outer:2.5,center:3,inner:2,pressure:98,condition:'Crítica',decision:'Disposición final',cause:'Profundidad mínima'},
  {position:6,location:'Tracción 1 · Der. externa',tire:'LL-000612 · Bridgestone R268',outer:10,center:10.5,inner:10,pressure:105,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:7,location:'Tracción 2 · Izq. externa',tire:'LL-000733 · Goodyear KMAX',outer:9.5,center:10,inner:9.5,pressure:104,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:8,location:'Tracción 2 · Izq. interna',tire:'LL-000741 · Goodyear KMAX',outer:9,center:9.5,inner:9,pressure:103,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:9,location:'Tracción 2 · Der. interna',tire:'LL-000752 · Michelin X Multi',outer:8.5,center:9,inner:8.5,pressure:104,condition:'Buena',decision:'Continúa en servicio',cause:''},
  {position:10,location:'Tracción 2 · Der. externa',tire:'LL-000768 · Michelin X Multi',outer:8,center:8.5,inner:8,pressure:105,condition:'Buena',decision:'Continúa en servicio',cause:''}
 ]);
 diagramAxles:DiagramAxle[]=[
  {id:1,name:'EJE 1',type:'Direccional',positions:[this.dp(1,'Izquierda'),this.dp(2,'Derecha')]},
  {id:2,name:'EJE 2',type:'Tracción',positions:[this.dp(3,'Izq. externa'),this.dp(4,'Izq. interna'),this.dp(5,'Der. interna','alert'),this.dp(6,'Der. externa')]},
  {id:3,name:'EJE 3',type:'Tracción',positions:[this.dp(7,'Izq. externa'),this.dp(8,'Izq. interna'),this.dp(9,'Der. interna'),this.dp(10,'Der. externa')]}
 ];
 selectedPosition=signal(3); mode=signal<'montaje'|'desmontaje'>('montaje'); message=signal(''); critical=signal(false);
 diagramOpen=signal(false);
 inspections=signal<Inspection[]>(this.read('glld_inspections',[])); operations=signal<Operation[]>(this.read('glld_operations',[]));
 inspectionForm=this.fb.group({site:['R1',Validators.required],center:[CENTERS.find(c=>c.code==='8002')?.label??CENTERS[0].label,Validators.required],vehicle:[this.vehicles[0],Validators.required],technician:['Laura Ruiz',Validators.required],mileage:[null as number|null,[Validators.required,Validators.min(0)]],notes:['Inspección general del vehículo.']});
 mountForm=this.fb.group({vehicle:[this.vehicles[0],Validators.required],tire:[this.tires[0],Validators.required],technician:['Carlos Mendoza',Validators.required],mileage:[38420,[Validators.required,Validators.min(0)]],destination:['Stock Bogotá'],reason:['Mantenimiento preventivo'],notes:['']});
 saveInspection(){if(this.inspectionForm.invalid){this.inspectionForm.markAllAsTouched();return}const v=this.inspectionForm.getRawValue();const date=new Date().toLocaleString('es-CO');let criticalCount=0,warningCount=0;const created=this.inspectionRows().map((row,index)=>{const min=Math.min(row.outer,row.center,row.inner),diff=Math.max(row.outer,row.center,row.inner)-min;const result=min<3?'Alerta crítica':diff>3?'Desgaste irregular':'Inspección aprobada';if(result==='Alerta crítica')criticalCount++;else if(result!=='Inspección aprobada')warningCount++;return{id:`INS-${String(843+this.inspections().length+index).padStart(5,'0')}`,vehicle:v.vehicle!,tire:row.tire,position:`Posición ${row.position}`,outer:row.outer,center:row.center,inner:row.inner,pressure:row.pressure,result,date};});const all=[...created,...this.inspections()];this.inspections.set(all);this.store('glld_inspections',all);const candidates=this.inspectionRows().filter(r=>r.decision!=='Continúa en servicio').map(r=>({id:r.tire.split(' · ')[0],brand:r.tire.split(' · ')[1]??'',reference:r.tire.split(' · ').slice(2).join(' · '),dot:'Pendiente',dimension:'Pendiente',retreadBand:'',outer:r.outer,center:r.center,inner:r.inner,cause:r.cause||r.condition,decision:r.decision,origin:v.site,destination:r.decision==='Disposición final'&&v.site!=='R1'?'R1':'',status:r.decision==='Disposición final'?(v.site==='R1'?'Lista para acta':'Pendiente envío a R1'):'Pendiente evaluación de carcasa',date}));this.store('glld_disposal_queue',[...candidates,...this.read('glld_disposal_queue',[])]);this.critical.set(criticalCount>0);this.message.set(`Inspección guardada. ${candidates.length} llantas fueron enviadas al flujo de reencauche o disposición final.`);}
 updateMeasure(position:number,field:'outer'|'center'|'inner'|'pressure'|'condition'|'decision'|'cause',value:string|number){this.inspectionRows.update(rows=>rows.map(row=>row.position===position?{...row,[field]:['condition','decision','cause'].includes(field)?String(value):Number(value)}:row));}
 saveOperation(){if(this.mountForm.invalid){this.mountForm.markAllAsTouched();return}const v=this.mountForm.getRawValue();const type=this.mode()==='montaje'?'Montaje':'Desmontaje';const tire=this.mode()==='montaje'?v.tire!:(this.installed[this.selectedPosition()-1]??'LL-000184');const item:Operation={id:`MOV-${String(19383+this.operations().length).padStart(6,'0')}`,type,vehicle:v.vehicle!,tire,position:`Posición ${this.selectedPosition()}`,destination:this.mode()==='montaje'?'Vehículo':v.destination!,date:new Date().toLocaleString('es-CO')};const all=[item,...this.operations()];this.operations.set(all);this.store('glld_operations',all);this.message.set(`${type} completado. Se generó el movimiento ${item.id} y se actualizó el historial.`);}
 choose(position:number){this.selectedPosition.set(position);this.message.set('');this.diagramOpen.set(false);setTimeout(()=>document.getElementById(`inspection-position-${position}`)?.scrollIntoView({behavior:'smooth',block:'center'}),50);}
 chooseDiagram(position:DiagramPosition){this.choose(Number(position.id));}
 vehicleType(){const vehicle=this.inspectionForm.value.vehicle??this.mountForm.value.vehicle??'';return vehicle.includes('Kenworth')?'Tractocamión':vehicle.includes('Chevrolet')?'Camión rígido':'Bus';}
 setMode(mode:'montaje'|'desmontaje'){this.mode.set(mode);this.message.set('');}
 chooseCenter(value:string){const center=this.centers.find(c=>c.label===value);if(center)this.inspectionForm.controls.site.setValue(center.relevance);}
 private read<T>(key:string,fallback:T):T{try{return JSON.parse(localStorage.getItem(key)??'') as T}catch{return fallback}}
 private store(key:string,value:unknown){localStorage.setItem(key,JSON.stringify(value));}
 private dp(id:number,side:string,state:DiagramPosition['state']='normal'):DiagramPosition{const row=this.inspectionRows().find(x=>x.position===id);return{id,code:`P${id}`,side,tire:row?.tire.split(' · ')[0]??row?.tire.split(' Â· ')[0],state}}
}
