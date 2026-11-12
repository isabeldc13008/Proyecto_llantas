import {TestBed} from '@angular/core/testing';
import {TireDepthChart} from './tire-depth-chart';
import {AnalyticsMeasurement} from './analytics-api';
describe('Orientación del desgaste',()=>{
 const measurement=(id:string,inverted:boolean):AnalyticsMeasurement=>({lectura:{id,inspeccionId:id,fecha:'2026-10-0'+id+'T10:00:00Z',posicionId:'p',posicion:'P1',odometro:100,exterior:8,centro:7,interior:6},minima:6,tramoId:'a',kmDesdeMontaje:10,calidad:'Completa',normalizada:{exterior:inverted?6:8,centro:7,interior:inverted?8:6,invertida:inverted,girosAplicados:[]},orientacion:'Referencia'});
 it('compara hombros normalizados conservando la captura original',()=>{const c=new TireDepthChart();c.axis='exterior';expect(c.value(measurement('1',false))).toBe(8);expect(c.value(measurement('2',true))).toBe(6);c.axis='interior';expect(c.value(measurement('2',true))).toBe(8);c.axis='centro';expect(c.value(measurement('2',true))).toBe(7);expect(measurement('2',true).lectura.exterior).toBe(8);});
 it('no conecta puntos con orientación desconocida',()=>{const c=new TireDepthChart();const unknown={...measurement('2',false),normalizada:null,orientacion:'No verificable'};c.mediciones=[measurement('1',false),unknown,measurement('3',true)];expect(c.segments()).toEqual([]);c.axis='exterior';expect(c.value(unknown)).toBeNull();});
 it('presenta No verificable y conserva los valores originales como evidencia',()=>{const f=TestBed.createComponent(TireDepthChart);f.componentRef.setInput('mediciones',[{...measurement('1',false),normalizada:null,orientacion:'No verificable'}]);f.detectChanges();expect(f.nativeElement.textContent).toContain('No verificable');expect(f.nativeElement.textContent).toContain('Captura original E/C/I: 8 / 7 / 6');f.destroy();});
});
