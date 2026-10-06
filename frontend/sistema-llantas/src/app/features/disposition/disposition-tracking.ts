import {Component,input} from '@angular/core';
import {DatePipe} from '@angular/common';
import {Event} from './disposition-models';
@Component({selector:'app-disposition-tracking',imports:[DatePipe],templateUrl:'./disposition-tracking.html',styleUrl:'./disposition-tracking.scss'})
export class DispositionTracking{events=input.required<Event[]>()}
