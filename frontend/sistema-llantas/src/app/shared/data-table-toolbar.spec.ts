import { DataTableToolbar } from './data-table-toolbar';

describe('DataTableToolbar',()=>{
 beforeEach(()=>localStorage.removeItem('table:test'));
 it('persists and restores visible columns while retaining required columns',()=>{
  const first=new DataTableToolbar();first.persistenceKey='test';first.columns=[{key:'id',label:'ID',required:true},{key:'state',label:'Estado'}];first.ngOnInit();first.toggleColumn('state',false);
  expect(JSON.parse(localStorage.getItem('table:test')!)).toEqual(['id']);
  const restored=new DataTableToolbar();restored.persistenceKey='test';restored.columns=first.columns;restored.ngOnInit();expect(restored.visibleColumns).toEqual(['id']);
 });
 it('emits immutable typed filter values',()=>{const toolbar=new DataTableToolbar();const before=toolbar.values;toolbar.setValue('centroIds',['a','b']);expect(toolbar.values).not.toBe(before);expect(toolbar.values['centroIds']).toEqual(['a','b']);});
});
