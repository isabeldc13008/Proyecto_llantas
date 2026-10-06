import {Component,input,output,inject,signal} from '@angular/core';
import {DatePipe,DecimalPipe} from '@angular/common';
import {firstValueFrom} from 'rxjs';
import {DispositionApi,downloadBlob} from './disposition-api';
import {Acta,Evidence,failure} from './disposition-models';
import {downloadActa} from './disposition-acta-pdf';
@Component({selector:'app-disposition-acta-files',imports:[DatePipe,DecimalPipe],templateUrl:'./disposition-acta-files.html',styleUrl:'./disposition-shared.scss'})
export class DispositionActaFiles{
 acta=input.required<Acta>();editable=input(false);changed=output<void>();private api=inject(DispositionApi);busy=signal(false);error=signal('');confirmDelete=signal<string|null>(null);
 async pdf(){try{await downloadActa(this.acta().snapshot)}catch(e){this.error.set(failure(e))}}
 async select(event:Event){const input=event.target as HTMLInputElement,file=input.files?.[0];if(file)await this.upload(file);input.value=''}
 async drop(event:DragEvent){event.preventDefault();const file=event.dataTransfer?.files[0];if(file)await this.upload(file)}
 async upload(file:File){if(!this.editable()||this.busy())return;this.error.set('');if(!/\.(pdf|jpg|jpeg|png)$/i.test(file.name)||file.size<=0||file.size>10_000_000){this.error.set('Selecciona un archivo PDF, JPG, JPEG o PNG de máximo 10 MB.');return}this.busy.set(true);try{await firstValueFrom(this.api.support(this.acta().id,file));this.changed.emit()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false)}}
 async download(e:Evidence){try{downloadBlob(await firstValueFrom(this.api.file(e.id)),e.nombreArchivo)}catch(error){this.error.set(failure(error))}}
 async remove(id:string){if(!this.editable()||this.busy())return;this.busy.set(true);try{await firstValueFrom(this.api.deleteSupport(id));this.confirmDelete.set(null);this.changed.emit()}catch(e){this.error.set(failure(e))}finally{this.busy.set(false)}}
}
