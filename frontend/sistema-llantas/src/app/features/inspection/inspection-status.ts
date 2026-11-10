export interface PersistedReading {
  posicionId:string; llantaId?:string|null;
  exterior:number|null; centro:number|null; interior:number|null;
  condicionId:string|null; recomendacionId:string|null; causaId?:string|null; observaciones?:string|null;
}
export interface PositionStatus {
  valid:boolean; novelty:boolean; alerts:string[]; inconsistencies:string[];
}
export function validInspectionReading(d:PersistedReading):boolean {
  return !!d.llantaId&&!!d.condicionId&&!!d.recomendacionId&&
    [d.exterior,d.centro,d.interior].every(x=>x!==null&&x!==undefined&&Number.isFinite(x)&&x>=0&&x<=15);
}
export function inspectionVisualStatus(s:PositionStatus|undefined, saved:boolean){
  if(s?.alerts.length)return 'active-alert' as const;
  if(s?.inconsistencies.some(x=>!x.startsWith('Regularizada:')&&!x.startsWith('Rechazada:')))return 'inconsistency' as const;
  if(s?.valid&&saved&&!s.novelty)return 'complete' as const;
  return 'pending' as const;
}
export function inspectionStatusText(s:PositionStatus|undefined,saved:boolean):string {
  const parts=[s?.valid&&saved?'Lectura completa guardada':'Pendiente de lectura válida'];
  if(s?.novelty)parts.push('Novedades registradas');
  if(s?.alerts.length)parts.push('Alerta activa: '+s.alerts.join('; '));
  if(s?.inconsistencies.length)parts.push('Inconsistencias: '+s.inconsistencies.join('; '));
  return parts.join(' · ');
}
