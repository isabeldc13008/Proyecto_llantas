import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MultiSelectFilter } from './multi-select-filter';
export interface TableFilter{key:string;label:string;type:'multi'|'range'|'select';options?:{value:string;label:string}[]}
export interface TableColumn{key:string;label:string;required?:boolean}
@Component({selector:'app-data-table-toolbar',imports:[FormsModule,MultiSelectFilter],templateUrl:'./data-table-toolbar.html',styleUrl:'./data-table-toolbar.scss'})
export class DataTableToolbar implements OnInit{
 @Input() search='';@Output() searchChange=new EventEmitter<string>();@Input() sortBy='codigo';@Output() sortByChange=new EventEmitter<string>();@Input() placeholder='Buscar…';@Input() sortOptions:{value:string;label:string}[]=[];
 @Input() filters:TableFilter[]=[];@Input() columns:TableColumn[]=[];@Input() persistenceKey='';@Input() values:Record<string,unknown>={};@Output() valuesChange=new EventEmitter<Record<string,unknown>>();@Input() visibleColumns:string[]=[];@Output() visibleColumnsChange=new EventEmitter<string[]>();
 @Output() apply=new EventEmitter<void>();@Output() clear=new EventEmitter<void>();@Output() exportCsv=new EventEmitter<void>();@Output() exportExcel=new EventEmitter<void>();
 @Input() collapsibleFilters=false;filtersOpen=false;
 ngOnInit(){if(!this.visibleColumns.length)this.visibleColumns=this.columns.map(x=>x.key);if(this.persistenceKey){try{const saved=JSON.parse(localStorage.getItem('table:'+this.persistenceKey)??'null');if(Array.isArray(saved))this.visibleColumns=[...saved,...this.columns.filter(c=>c.required&&!saved.includes(c.key)).map(c=>c.key)];}catch{}this.visibleColumnsChange.emit(this.visibleColumns)}}
 setValue(key:string,value:unknown){this.values={...this.values,[key]:value};this.valuesChange.emit(this.values)}
 toggleColumn(key:string,checked:boolean){this.visibleColumns=checked?[...new Set([...this.visibleColumns,key])]:this.visibleColumns.filter(x=>x!==key);if(this.persistenceKey)localStorage.setItem('table:'+this.persistenceKey,JSON.stringify(this.visibleColumns));this.visibleColumnsChange.emit(this.visibleColumns)}
 activeCount(){return this.filters.reduce((n,f)=>n+(f.type==='range'?Number(this.has(f.key+'Min'))+Number(this.has(f.key+'Max')):Array.isArray(this.values[f.key])?(this.values[f.key] as unknown[]).length:Number(this.has(f.key))),0)}
 chips(){const result:{filter:TableFilter,key:string,value:unknown,label:string}[]=[];for(const f of this.filters){if(f.type==='range'){for(const suffix of ['Min','Max']){const key=f.key+suffix;if(this.has(key))result.push({filter:f,key,value:this.values[key],label:`${f.label} ${suffix==='Min'?'mín.':'máx.'}: ${this.values[key]}`})}}else{const values=Array.isArray(this.values[f.key])?this.values[f.key] as unknown[]:[this.values[f.key]].filter(v=>v!==''&&v!=null);for(const value of values)result.push({filter:f,key:f.key,value,label:`${f.label}: ${f.options?.find(o=>o.value===String(value))?.label??value}`})}}return result}
 removeChip(chip:{filter:TableFilter;key:string;value:unknown}){if(chip.filter.type==='multi'){this.setValue(chip.key,((this.values[chip.key] as unknown[])??[]).filter(x=>x!==chip.value))}else this.setValue(chip.key,'');this.apply.emit()}
 applyAndClose(){this.filtersOpen=false;this.apply.emit()} clearAndClose(){this.values={};this.valuesChange.emit(this.values);this.filtersOpen=false;this.clear.emit()}
 private has(key:string){return this.values[key]!==''&&this.values[key]!=null}
}
