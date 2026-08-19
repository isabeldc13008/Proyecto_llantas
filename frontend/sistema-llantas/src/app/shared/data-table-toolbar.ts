import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
export interface TableFilter{key:string;label:string;type:'multi'|'range';options?:{value:string;label:string}[]}
export interface TableColumn{key:string;label:string;required?:boolean}
@Component({selector:'app-data-table-toolbar',imports:[FormsModule],templateUrl:'./data-table-toolbar.html',styleUrl:'./data-table-toolbar.scss'})
export class DataTableToolbar implements OnInit{
 @Input() search='';@Output() searchChange=new EventEmitter<string>();@Input() sortBy='codigo';@Output() sortByChange=new EventEmitter<string>();@Input() placeholder='Buscar…';@Input() sortOptions:{value:string;label:string}[]=[];
 @Input() filters:TableFilter[]=[];@Input() columns:TableColumn[]=[];@Input() persistenceKey='';@Input() values:Record<string,unknown>={};@Output() valuesChange=new EventEmitter<Record<string,unknown>>();@Input() visibleColumns:string[]=[];@Output() visibleColumnsChange=new EventEmitter<string[]>();
 @Output() apply=new EventEmitter<void>();@Output() clear=new EventEmitter<void>();@Output() exportCsv=new EventEmitter<void>();@Output() exportExcel=new EventEmitter<void>();
 ngOnInit(){if(!this.visibleColumns.length)this.visibleColumns=this.columns.map(x=>x.key);if(this.persistenceKey){try{const saved=JSON.parse(localStorage.getItem('table:'+this.persistenceKey)??'null');if(Array.isArray(saved))this.visibleColumns=[...saved,...this.columns.filter(c=>c.required&&!saved.includes(c.key)).map(c=>c.key)];}catch{}this.visibleColumnsChange.emit(this.visibleColumns)}}
 setValue(key:string,value:unknown){this.values={...this.values,[key]:value};this.valuesChange.emit(this.values)}
 toggleColumn(key:string,checked:boolean){this.visibleColumns=checked?[...new Set([...this.visibleColumns,key])]:this.visibleColumns.filter(x=>x!==key);if(this.persistenceKey)localStorage.setItem('table:'+this.persistenceKey,JSON.stringify(this.visibleColumns));this.visibleColumnsChange.emit(this.visibleColumns)}
}
