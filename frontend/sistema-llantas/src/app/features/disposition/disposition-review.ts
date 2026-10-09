import {AfterViewInit,Component,ElementRef,EventEmitter,Input,OnDestroy,Output,ViewChild,inject,signal} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {DispositionApi,downloadBlob} from './disposition-api';
import {Detail,Evidence,failure} from './disposition-models';
import {AuthService} from '../../core/auth/auth.service';
@Component({selector:'app-disposition-review',imports:[DatePipe,FormsModule,RouterLink],templateUrl:'./disposition-review.html',styleUrls:['./disposition-shared.scss','./disposition-dialog.scss']})
export class DispositionReview implements AfterViewInit,OnDestroy{
 @Input()id='';@Output()closed=new EventEmitter<void>();@Output()changed=new EventEmitter<void>();@ViewChild('dialog')dialog!:ElementRef<HTMLDialogElement>;
 api=inject(DispositionApi);auth=inject(AuthService);data=signal<Detail|null>(null);error=signal('');loading=signal(true);busy=signal(false);images=signal<{evidence:Evidence;url:string}[]>([]);index=signal(0);reviewed=false;rejecting=false;notes='';reusable=false;private generation=0;
 ngAfterViewInit(){this.dialog.nativeElement.showModal();void this.load()}
 async load(){const generation=++this.generation;this.loading.set(true);this.clearImages();this.reviewed=false;try{const d=await firstValueFrom(this.api.detail(this.id));if(generation!==this.generation)return;this.data.set(d);for(const e of d.evidencias.filter(e=>e.mimeType.startsWith('image/'))){try{const blob=await firstValueFrom(this.api.file(e.id,true));if(generation!==this.generation)return;this.images.update(x=>[...x,{evidence:e,url:URL.createObjectURL(blob)}])}catch{this.error.set('No se pudo cargar alguna imagen. Puedes reintentar abriendo el archivo.')}}}catch(e){this.error.set(failure(e))}finally{if(generation===this.generation)this.loading.set(false)}}
 async run(action:string){if(this.busy()||this.loading()||!this.data()?.accionesPermitidas.includes(action)||action==='APROBAR'&&!this.reviewed||action!=='APROBAR'&&!this.notes.trim())return;this.busy.set(true);this.error.set('');try{await firstValueFrom(action==='APROBAR'?this.api.approve(this.id):action==='RECHAZAR'?this.api.reject(this.id,this.notes):this.api.evaluate(this.id,this.reusable,this.notes));this.changed.emit();this.rejecting=false;this.notes='';await this.load()}catch(e){this.error.set(failure(e));await this.load()}finally{this.busy.set(false)}}
 async upload(event:Event){const input=event.target as HTMLInputElement,file=input.files?.[0];if(!file||this.busy())return;this.busy.set(true);try{await firstValueFrom(this.api.evidence(this.id,file));this.changed.emit();await this.load()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false);input.value=''}}
 async open(e:Evidence){const tab=window.open('about:blank','_blank');if(!tab){this.error.set('Permite abrir pestañas para ver el archivo.');return}tab.opener=null;try{const b=await firstValueFrom(this.api.file(e.id,true));const url=URL.createObjectURL(b);tab.location.href=url;setTimeout(()=>URL.revokeObjectURL(url),60000)}catch(error){tab.close();this.error.set(failure(error))}}
 async download(e:Evidence){try{downloadBlob(await firstValueFrom(this.api.file(e.id,true)),e.nombreArchivo)}catch(error){this.error.set(failure(error))}}
 close(){if(!this.busy())this.closed.emit()}
 private clearImages(){for(const i of this.images())URL.revokeObjectURL(i.url);this.images.set([]);this.index.set(0)}
 ngOnDestroy(){this.generation++;this.clearImages();this.dialog?.nativeElement.close()}
}
