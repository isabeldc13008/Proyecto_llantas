import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface DiagramPosition { id:number|string; code:string; side:string; tire?:string; state?:'normal'|'alert'|'critical'|'empty'|'inconsistency' }
export interface DiagramAxle { id:number|string; name:string; type:string; positions:DiagramPosition[] }

@Component({selector:'app-vehicle-axle-diagram',imports:[CommonModule],template:`
<section class="diagram" aria-label="Vista superior de la configuración de ejes y llantas">
  <header><span>PARTE DELANTERA</span><b>{{vehicleType}}</b></header>
  <div class="vehicle">
    <div class="cab" aria-hidden="true"><span class="windshield"></span><span class="steering-wheel"></span><span class="seat"></span></div>
    <div class="frame" aria-hidden="true"><i></i><i></i></div>
    <div class="axles">
      @for(axle of axles;track axle.id){
        <div class="axle">
          <div class="axle-label"><b>{{axle.name}}</b><small>{{axle.type}}</small></div>
          <div class="axle-layout">
            <div class="positions left">
              @for(p of leftPositions(axle);track p.id){<ng-container *ngTemplateOutlet="position;context:{$implicit:p}"></ng-container>}
            </div>
            <div class="crossmember" aria-hidden="true"></div>
            <div class="positions right">
              @for(p of rightPositions(axle);track p.id){<ng-container *ngTemplateOutlet="position;context:{$implicit:p}"></ng-container>}
            </div>
          </div>
        </div>
      }
    </div>
  </div>
  <footer><span><i></i>Normal</span><span><i class="s"></i>Seleccionada</span><span><i class="a"></i>Alerta</span><span><i class="n"></i>Sin llanta</span><span><i class="x"></i>Inconsistencia</span></footer>
</section>
<ng-template #position let-p><button type="button" [class.selected]="selectedId===p.id" [class.alert]="p.state==='alert'||p.state==='critical'" [class.empty]="p.state==='empty'" [class.inconsistency]="p.state==='inconsistency'" (click)="positionSelected.emit(p)" [attr.aria-pressed]="selectedId===p.id" [attr.aria-label]="p.code+', '+p.side+', '+(p.tire||'sin llanta')"><i class="tire"></i><span><b>{{p.code}}</b><small>{{p.tire||'Sin llanta'}}</small></span></button></ng-template>
`,styles:[`
.diagram{background:#f7fafb;border:1px solid #d7e3e8;border-radius:18px;padding:1rem;min-width:0;max-width:100%;overflow:hidden}.diagram header{text-align:center;display:grid;color:#183f57;margin-bottom:.75rem}.diagram header span{font-size:.6rem;letter-spacing:.18em;color:#6b8290}.vehicle{position:relative;width:min(100%,560px);margin:auto;padding:0 clamp(.25rem,3vw,1.5rem) 1rem}.cab{position:relative;width:42%;min-width:130px;height:82px;margin:0 auto -4px;border:3px solid #69808a;border-bottom-width:8px;border-radius:38% 38% 15px 15px;background:linear-gradient(180deg,#dce9ed,#f7fafb);z-index:2}.windshield{position:absolute;inset:16px 12% auto;height:25px;border:2px solid #8da3ac;border-radius:16px 16px 7px 7px;background:#bcd4de}.steering-wheel{position:absolute;top:32px;left:22%;width:23px;height:23px;border:4px solid #536970;border-radius:50%}.steering-wheel:before,.steering-wheel:after{content:'';position:absolute;left:50%;top:50%;width:3px;height:19px;background:#536970;transform:translate(-50%,-50%)}.steering-wheel:after{transform:translate(-50%,-50%) rotate(90deg)}.seat{position:absolute;right:18%;bottom:7px;width:31px;height:18px;border:2px solid #8da3ac;border-radius:7px;background:#cbdcdf}.frame{position:absolute;top:75px;bottom:10px;left:42%;right:42%;display:flex;justify-content:space-between;z-index:0}.frame i{width:7px;border-radius:5px;background:linear-gradient(90deg,#43575e,#81939a,#43575e)}.axles{position:relative;z-index:1;display:grid;gap:1rem;padding-top:.8rem}.axle-label{text-align:center;display:grid;font-size:.72rem;margin-bottom:.25rem}.axle-label small{color:#768c97}.axle-layout{display:grid;grid-template-columns:minmax(0,1fr) minmax(44px,24%) minmax(0,1fr);align-items:center}.crossmember{height:7px;background:#53666d;box-shadow:0 0 0 2px #f7fafb;border-radius:8px}.positions{display:flex;align-items:center;gap:3px;min-width:0}.positions.left{justify-content:flex-end}.positions.right{justify-content:flex-start}.positions button{z-index:1;border:0;background:transparent;padding:2px;min-width:0;color:#294b5c;display:flex;align-items:center;gap:.28rem;cursor:pointer;border-radius:9px}.positions.left button{flex-direction:row-reverse;text-align:right}.positions button>span{display:grid;min-width:0;max-width:64px}.positions b{font-size:.68rem}.positions small{font-size:.5rem;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.tire{display:block;flex:0 0 auto;width:21px;height:43px;border:3px solid #294047;border-radius:8px;background:repeating-linear-gradient(150deg,#294047 0 4px,#647980 4px 6px);box-shadow:inset 0 0 0 2px #1d3137}.positions button:hover,.positions button:focus-visible{background:#e6eef1;outline:2px solid #2c6e8e;outline-offset:1px}.positions button.selected{background:#eff8e6;outline:3px solid #8bc53f}.positions button.selected .tire{border-color:#5d8d28;background:repeating-linear-gradient(150deg,#76a937 0 4px,#a6cf72 4px 6px)}.positions button.alert .tire{border-color:#a83820;background:repeating-linear-gradient(150deg,#d84b2c 0 4px,#ef947d 4px 6px)}.positions button.empty .tire{background:white;border-color:#8ba0aa;box-shadow:inset 0 0 0 2px #dce5e8}.positions button.inconsistency .tire{border-color:#a97400;background:repeating-linear-gradient(150deg,#f0b429 0 4px,#f8d47a 4px 6px)}.diagram footer{display:flex;justify-content:center;flex-wrap:wrap;gap:.55rem;font-size:.55rem;color:#667d88;margin-top:.6rem}.diagram footer i{display:inline-block;width:8px;height:8px;background:#294047;border-radius:2px;margin-right:3px}.diagram footer .s{background:#76a937}.diagram footer .a{background:#e15b35}.diagram footer .n{background:white;border:1px solid #8ba0aa}.diagram footer .x{background:#f0b429}@media(max-width:600px){.diagram{padding:.65rem}.vehicle{padding-inline:0}.cab{width:48%;min-width:112px;height:72px}.axle-layout{grid-template-columns:minmax(0,1fr) 38px minmax(0,1fr)}.positions button>span{display:none}.tire{width:19px;height:39px}.axles{gap:.8rem}.diagram footer{justify-content:flex-start}}
`]})
export class VehicleAxleDiagram {
  @Input() axles:DiagramAxle[]=[]; @Input() selectedId:number|string|null=null; @Input() vehicleType='Vehículo'; @Output() positionSelected=new EventEmitter<DiagramPosition>();
  leftPositions(axle:DiagramAxle){return this.sidePositions(axle,true)}
  rightPositions(axle:DiagramAxle){return this.sidePositions(axle,false)}
  private sidePositions(axle:DiagramAxle,left:boolean){
    const classified=axle.positions.map(position=>({position,side:position.side.toLocaleLowerCase('es')}));
    const explicit=classified.filter(item=>left?(item.side.includes('izq')||item.side.includes('left')):(item.side.includes('der')||item.side.includes('right'))).map(item=>item.position);
    const unknown=classified.filter(item=>!item.side.includes('izq')&&!item.side.includes('left')&&!item.side.includes('der')&&!item.side.includes('right')).map(item=>item.position);
    const middle=Math.ceil(unknown.length/2);
    return explicit.concat(left?unknown.slice(0,middle):unknown.slice(middle));
  }
}
