import {CommonModule} from '@angular/common';
import {Component,Input,Output,EventEmitter} from '@angular/core';
import {AnalyticsPage} from './analytics-api';
import {MaintenanceRow} from './maintenance-models';
@Component({selector:'app-maintenance-queue',imports:[CommonModule],templateUrl:'./maintenance-queue.html',styleUrl:'./maintenance-queue.scss'})
export class MaintenanceQueue{
 @Input({required:true}) pagina!:AnalyticsPage<MaintenanceRow>;
 @Input() selectedId='';
 @Output() selected=new EventEmitter<string>();
 @Output() pageChanged=new EventEmitter<number>();
}
