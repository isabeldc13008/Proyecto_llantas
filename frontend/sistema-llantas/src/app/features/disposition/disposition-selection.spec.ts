import {visibleSelection,actaCount,remainingAfterReceipt,stageLabel,safeDispositionReturn} from './disposition-models';

describe('Disposición: selección física y actas',()=>{
 it('seleccionar todas afecta solo a las filas visibles',()=>{expect(visibleSelection(['old'],['a','b'],true)).toEqual(['old','a','b']);expect(visibleSelection(['old','a'],['a','b'],false)).toEqual(['old']);});
 it('cuenta actas por origen distinto, no por llanta ni lote',()=>{expect(actaCount([{centroOrigen:{id:'b'}},{centroOrigen:{id:'b'}},{centroOrigen:{id:'i'}},{centroOrigen:{id:'g'}}])).toBe(3);});
 it('recepción parcial conserva cantidades y considera recepciones previas',()=>{expect(remainingAfterReceipt(18,0,15)).toBe(3);expect(remainingAfterReceipt(18,15,3)).toBe(0);});
 it('presenta estados legibles sin inventar etapas para estados desconocidos',()=>{expect(stageLabel('PENDIENTE_EVALUACION_TECNICA')).toBe('Pendiente de evaluación');expect(stageLabel('NUEVO_ESTADO')).toBe('NUEVO_ESTADO');});
 it('restaura centro y página solo en bandejas internas',()=>{expect(safeDispositionReturn('/disposicion-final/bandeja?centroId=a&pagina=3')).toBe('/disposicion-final/bandeja?centroId=a&pagina=3');expect(safeDispositionReturn('//example.org')).toBe('/disposicion-final/bandeja');expect(safeDispositionReturn('/administracion')).toBe('/disposicion-final/bandeja');});
});
