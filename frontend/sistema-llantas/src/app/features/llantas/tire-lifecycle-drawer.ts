import {CommonModule} from '@angular/common';
import {Component,EventEmitter,Input,Output} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {CatalogItem,TireDetail} from '../../core/models/api.models';
@Component({selector:'app-tire-lifecycle-drawer',imports:[CommonModule,FormsModule],templateUrl:'./tire-lifecycle-drawer.html',styleUrl:'./tire-lifecycle.scss'})
export class TireLifecycleDrawer{@Input({required:true})detail!:TireDetail;@Input()centers:CatalogItem[]=[];@Input()canTransfer=false;@Output()closed=new EventEmitter<void>();@Output()transferred=new EventEmitter<{centerId:string;reason:string;notes:string}>();centerId='';reason='';notes='';submit(){if(this.centerId&&this.reason)this.transferred.emit({centerId:this.centerId,reason:this.reason,notes:this.notes})}}
