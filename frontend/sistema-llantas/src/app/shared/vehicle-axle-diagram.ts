import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DiagramPosition { id:number|string; code:string; side:string; tire?:string; state?:'normal'|'alert'|'critical'|'empty'|'inconsistency' }
export interface DiagramAxle { id:number|string; name:string; type:string; positions:DiagramPosition[] }

@Component({selector:'app-vehicle-axle-diagram',imports:[CommonModule],template:`
<section class="diagram" aria-label="Configuración de ejes y llantas">
 <header><span>PARTE DELANTERA</span><b>{{vehicleType}}</b></header>
 @for(axle of axles;track axle.id){
  <div class="axle"><div class="axle-title"><b>{{axle.name}}</b><small>{{axle.type}}</small></div>
   <div class="bar"></div><div class="positions">
    @for(p of axle.positions;track p.id){<button type="button" [class.selected]="selectedId===p.id" [class.alert]="p.state==='alert'" [class.empty]="p.state==='empty'" [class.inconsistency]="p.state==='inconsistency'" (click)="positionSelected.emit(p)" [attr.aria-pressed]="selectedId===p.id"><i></i><b>{{p.code}}</b><small>{{p.tire||'Sin llanta'}}</small><em>{{p.side}}</em></button>}
   </div>
  </div>
 }
 <footer><span><i></i>Normal</span><span><i class="s"></i>Seleccionada</span><span><i class="a"></i>Alerta</span><span><i class="n"></i>Sin llanta</span><span><i class="x"></i>Inconsistencia</span></footer>
</section>`,styles:[`
.diagram{background:#f7fafb;border:1px solid #d7e3e8;border-radius:18px;padding:1rem;min-width:250px}.diagram header{text-align:center;display:grid;color:#183f57;margin-bottom:1rem}.diagram header span{font-size:.6rem;letter-spacing:.18em;color:#6b8290}.axle{position:relative;margin:1rem 0}.axle-title{text-align:center;display:grid;font-size:.72rem}.axle-title small{color:#768c97}.bar{height:5px;background:#53666d;border-radius:8px;margin:8px 13% -18px}.positions{display:flex;justify-content:space-between;gap:.35rem}.positions button{z-index:1;border:1px solid #c8d7dd;background:white;border-radius:10px;padding:.35rem;min-width:58px;color:#294b5c;display:grid;place-items:center;cursor:pointer}.positions i{width:30px;height:18px;background:#294047;border-radius:5px;border:3px dotted #647980}.positions b{font-size:.72rem}.positions small{font-size:.52rem;max-width:62px;overflow:hidden;text-overflow:ellipsis}.positions em{font-size:.48rem;color:#7a8e97;font-style:normal}.positions button.selected{outline:3px solid #8bc53f;background:#eff8e6}.positions button.selected i{background:#76a937}.positions button.alert i{background:#e15b35}.positions button.empty i{background:#fff;border-color:#8ba0aa}.positions button.inconsistency i{background:#f0b429}.diagram footer{display:flex;flex-wrap:wrap;gap:.5rem;font-size:.52rem;color:#667d88;margin-top:.8rem}.diagram footer i{display:inline-block;width:8px;height:8px;background:#294047;border-radius:2px;margin-right:3px}.diagram footer .s{background:#76a937}.diagram footer .a{background:#e15b35}.diagram footer .n{background:white;border:1px solid #8ba0aa}.diagram footer .x{background:#f0b429}@media(max-width:600px){.diagram{padding:.7rem}.positions button{min-width:48px}.positions small{display:none}}
`]})
export class VehicleAxleDiagram { @Input() axles:DiagramAxle[]=[]; @Input() selectedId:number|string|null=null; @Input() vehicleType='Vehículo'; @Output() positionSelected=new EventEmitter<DiagramPosition>(); }
