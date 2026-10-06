import {TestBed} from '@angular/core/testing';
import {TireDepthChart} from './tire-depth-chart';
import {AnalyticsMeasurement} from './analytics-api';
import {evaluationText} from './maintenance-models';
import {maintenanceDestination} from './maintenance-navigation';

describe('Maintenance V1 evidence',()=>{
 it('does not render unavailable numbers as zero',()=>{
  expect(evaluationText({estado:'NO_EVALUABLE',valor:null,motivos:[]})).toBe('No evaluable');
  expect(evaluationText({estado:'PENDIENTE_CONFIGURACION',valor:null,motivos:[]})).toBe('Pendiente de configuración');
  expect(evaluationText({estado:'PARCIAL',valor:0,motivos:[]})).toBe('0');
 });
 it('keeps tire context and return location when opening inspections',()=>{
  const target=maintenanceDestination({tipo:'INSPECCIONAR',llantaId:'t',vehiculoId:'v',posicionId:'p',alertaId:null},'/analitica?pagina=2');
  expect(target.path).toBe('/inspecciones');expect(target.query['llantaId']).toBe('t');expect(target.query['posicionId']).toBe('p');
  expect(target.query['volver']).toBe('/analitica?pagina=2');
 });
 it('does not connect readings across mounts or incomplete measurements',async()=>{
  await TestBed.configureTestingModule({imports:[TireDepthChart]}).compileComponents();
  const fixture=TestBed.createComponent(TireDepthChart);
  const m=(id:string,tramo:string|null,min:number|null):AnalyticsMeasurement=>({lectura:{id,inspeccionId:id,fecha:`2026-10-0${id}T10:00:00Z`,posicionId:'p',posicion:'DD',odometro:100,exterior:min,centro:min,interior:min},minima:min,tramoId:tramo,kmDesdeMontaje:100,calidad:'observado'});
  fixture.componentRef.setInput('mediciones',[m('1','a',5),m('2','b',4),m('3','b',null),m('4','b',3)]);fixture.detectChanges();
  expect(fixture.nativeElement.querySelectorAll('line.data-segment').length).toBe(0);
  expect(fixture.nativeElement.textContent).toContain('Fecha de registro');
 });
});
