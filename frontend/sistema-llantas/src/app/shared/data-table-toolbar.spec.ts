import { DataTableToolbar } from './data-table-toolbar';

describe('DataTableToolbar',()=>{
 beforeEach(()=>localStorage.clear());
 it('persists and restores visible columns while retaining required columns',()=>{
  const first=new DataTableToolbar();first.persistenceKey='test';first.columns=[{key:'id',label:'ID',required:true},{key:'state',label:'Estado'}];first.ngOnInit();first.toggleColumn('state',false);
  expect(JSON.parse(localStorage.getItem('glld.columns.test')!)).toEqual(['id']);
  const restored=new DataTableToolbar();restored.persistenceKey='test';restored.columns=first.columns;restored.ngOnInit();expect(restored.visibleColumns).toEqual(['id']);
 });
 it('keeps configurations independent and resets the current table',()=>{const a=new DataTableToolbar();a.persistenceKey='llantas';a.columns=[{key:'id',label:'ID',required:true},{key:'marca',label:'Marca'}];a.ngOnInit();a.toggleColumn('marca',false);const b=new DataTableToolbar();b.persistenceKey='vehiculos';b.columns=[{key:'id',label:'ID',required:true},{key:'placa',label:'Placa'}];b.ngOnInit();expect(b.visibleColumns).toEqual(['id','placa']);b.resetColumns();expect(JSON.parse(localStorage.getItem('glld.columns.llantas')!)).toEqual(['id']);expect(JSON.parse(localStorage.getItem('glld.columns.vehiculos')!)).toEqual(['id','placa']);});
 it('removes duplicate and foreign persisted columns',()=>{localStorage.setItem('glld.columns.test',JSON.stringify(['state','state','foreign']));const toolbar=new DataTableToolbar();toolbar.persistenceKey='test';toolbar.columns=[{key:'id',label:'ID',required:true},{key:'state',label:'Estado'}];toolbar.ngOnInit();expect(toolbar.visibleColumns).toEqual(['state','id']);toolbar.toggleColumn('state',false);toolbar.toggleColumn('id',false);expect(toolbar.visibleColumns).toEqual(['id']);});
 it('emits immutable typed filter values',()=>{const toolbar=new DataTableToolbar();const before=toolbar.values;toolbar.setValue('centroIds',['a','b']);expect(toolbar.values).not.toBe(before);expect(toolbar.values['centroIds']).toEqual(['a','b']);});
});
