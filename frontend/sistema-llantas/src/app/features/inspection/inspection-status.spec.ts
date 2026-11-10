import {inspectionStatusText,inspectionVisualStatus,PositionStatus,validInspectionReading} from './inspection-status';
describe('Estados persistidos de Inspecciones',()=>{
 const base=():PositionStatus=>({valid:true,novelty:false,alerts:[],inconsistencies:[]});
 it('prioriza alerta sobre inconsistencia y conserva ambos textos',()=>{
  const s={...base(),alerts:['Profundidad mínima'],inconsistencies:['PendienteAutorizacion: LL-2']};
  expect(inspectionVisualStatus(s,true)).toBe('active-alert');expect(inspectionStatusText(s,true)).toContain('LL-2');expect(inspectionStatusText(s,true)).toContain('Profundidad mínima');
 });
 it('distingue pendiente, completa, inconsistencia y corrección regularizada',()=>{
  expect(inspectionVisualStatus(undefined,false)).toBe('pending');expect(inspectionVisualStatus(base(),true)).toBe('complete');
  expect(inspectionVisualStatus({...base(),inconsistencies:['Autorizada: LL-2']},true)).toBe('inconsistency');
  expect(inspectionVisualStatus({...base(),inconsistencies:['Regularizada: LL-2']},true)).toBe('complete');
  expect(inspectionVisualStatus(base(),false)).toBe('pending');expect(inspectionVisualStatus({...base(),novelty:true},true)).toBe('pending');
 });
 it('exige llanta, tres lecturas válidas, condición y recomendación; cero es válido',()=>{
  const d={posicionId:'p',llantaId:'t',exterior:0,centro:0,interior:0,condicionId:'c',recomendacionId:'r'};
  expect(validInspectionReading(d)).toBeTrue();
  for(const patch of [{llantaId:null},{exterior:null},{centro:NaN},{interior:16},{condicionId:null},{recomendacionId:null}])expect(validInspectionReading({...d,...patch})).toBeFalse();
 });
});
