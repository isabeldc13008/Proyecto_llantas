import {CommonModule} from '@angular/common';
import {Component,Input} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {AnalyticsMeasurement} from './analytics-api';
type ReadingAxis='minima'|'exterior'|'centro'|'interior';
@Component({selector:'app-tire-depth-chart',imports:[CommonModule,FormsModule],templateUrl:'./tire-depth-chart.html',styleUrl:'./tire-depth-chart.scss'})
export class TireDepthChart{
 @Input() mediciones:AnalyticsMeasurement[]=[];
 axis:ReadingAxis='minima';
 value(m:AnalyticsMeasurement){const value=this.axis==='minima'?m.minima:m.lectura[this.axis];return value!==null&&value>=0?value:null}
 points(){
  const sorted=[...this.mediciones].sort((a,b)=>Date.parse(a.lectura.fecha)-Date.parse(b.lectura.fecha)||a.lectura.id.localeCompare(b.lectura.id));
  const times=sorted.map(m=>Date.parse(m.lectura.fecha));const min=Math.min(...times),max=Math.max(...times);const upper=Math.max(1,...sorted.map(m=>this.value(m)??0));
  return sorted.map(m=>({m,value:this.value(m),x:max===min?310:45+530*(Date.parse(m.lectura.fecha)-min)/(max-min),y:170-140*(this.value(m)??0)/upper}));
 }
 segments(){const p=this.points();return p.slice(1).flatMap((b,i)=>{const a=p[i];return a.value!==null&&b.value!==null&&a.m.minima!==null&&b.m.minima!==null&&a.m.tramoId!==null&&a.m.tramoId===b.m.tramoId?[{a,b}]:[]})}
 maximum(){return Math.max(1,...this.mediciones.map(m=>this.value(m)??0))}
}
