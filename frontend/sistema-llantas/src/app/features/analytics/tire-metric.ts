import {CommonModule} from '@angular/common';
import {Component,Input} from '@angular/core';

@Component({
 selector:'app-tire-metric',imports:[CommonModule],
 template: '<div class="metric" role="img" [attr.aria-label]="description"><svg viewBox="0 0 220 220" aria-hidden="true"><circle class="tread" cx="110" cy="110" r="99"/><circle class="track" cx="110" cy="110" r="82"/><circle class="progress" cx="110" cy="110" r="82" pathLength="100" [attr.stroke-dasharray]="percent+\' 100\'"/></svg><div class="center"><small>RECORRIDO OBSERVADO</small><strong>{{value===null?\'Sin dato\':(value|number:\'1.0-0\')}}</strong><span>km promedio · tramos cerrados</span></div></div><p><b>{{percent|number:\'1.0-0\'}}%</b> de la cohorte con kilometraje verificable</p>',
 styles: [':host{display:block;text-align:center}.metric{position:relative;width:min(100%,300px);margin:auto;aspect-ratio:1}svg{width:100%;height:100%;transform:rotate(-90deg)}circle{fill:none}.tread{stroke:#153743;stroke-width:12;stroke-dasharray:3 5}.track{stroke:#e5edf0;stroke-width:13}.progress{stroke:#00a6cc;stroke-width:13;stroke-linecap:round}.center{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;flex-direction:column;padding:2.5rem}.center small{font-size:.6rem;letter-spacing:.08em;color:#617780}.center strong{font-size:2rem;color:#153743;margin:.45rem 0}.center span{font-size:.7rem;color:#617780;max-width:150px}p{color:#617780;font-size:.8rem}p b{color:#087a99}']
})
export class TireMetric {
 @Input() value:number|null=null;
 @Input() measured=0;
 @Input() total=0;
 get percent(){return this.total>0?Math.max(0,Math.min(100,this.measured/this.total*100)):0;}
 get description(){return 'Cobertura de medición: '+this.measured+' de '+this.total+' llantas. Recorrido promedio registrado: '+(this.value===null?'sin dato':this.value+' km')+'. No es una estimación de vida restante.';}
}
