import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
interface Preview { id:string; token?:string; total:number; validas:number; conError:number; montajes?:number; errores:{fila:number;campo:string;error:string}[]; filas?:Record<string,string>[] }
@Component({selector:'app-bulk-import',standalone:true,imports:[CommonModule,FormsModule],templateUrl:'./bulk-import-page.html',styleUrl:'./bulk-import-page.scss'})
export class BulkImportPage {
 private http=inject(HttpClient);type='llantas';mode='inventario';file=signal<File|null>(null);preview=signal<Preview|null>(null);loading=signal(false);message=signal('');
 reset(){this.preview.set(null);this.message.set('')}
 choose(e:Event){this.file.set((e.target as HTMLInputElement).files?.[0]??null);this.reset()}
 columns(){return Object.keys(this.preview()?.filas?.[0]??{})}
 async previewFile(){const f=this.file();if(!f)return;this.loading.set(true);this.preview.set(null);const b=new FormData();b.append('archivo',f,f.name);b.append(this.type==='llantas'?'modalidad':'tipo',this.type==='llantas'?this.mode:this.type);
 try{this.preview.set(await firstValueFrom(this.http.post<Preview>(this.type==='llantas'?'/api/carga-masiva/llantas/previsualizar':'/api/carga-masiva/previsualizar',b)));this.message.set(this.type==='llantas'?'Vista previa sin guardar datos.':'Vista previa de vehículos registrada.')}catch(e:any){this.message.set(e?.userMessage??'No se pudo validar el archivo.')}finally{this.loading.set(false)}}
 async confirm(){const p=this.preview();if(!p||p.conError>0||!p.validas||this.loading())return;let url=`/api/carga-masiva/${p.id}/confirmar`;let b:FormData|object={};
 if(this.type==='llantas'){const f=this.file();if(!f||!p.token){this.message.set('Vuelve a previsualizar el archivo.');return}url='/api/carga-masiva/llantas/confirmar';const form=new FormData();form.append('archivo',f,f.name);form.append('token',p.token);b=form}
 this.loading.set(true);try{const r:any=await firstValueFrom(this.http.post(url,b));this.message.set(`${r.procesadas} filas procesadas en una transacción.`);this.preview.set(null)}catch(e:any){this.message.set(e?.userMessage??'No fue posible procesar la carga.')}finally{this.loading.set(false)}}
 download(format:string){const t=this.type==='llantas'&&this.mode==='montaje'?'llantas-montaje':this.type;this.blob(`/api/carga-masiva/plantillas/${t}?formato=${format}`,`plantilla-${t}.${format}`)}
 errors(){const p=this.preview();if(!p)return;if(this.type==='vehiculos'){this.blob(`/api/carga-masiva/${p.id}/errores`,'errores.xlsx');return}
 const cell=(x:unknown)=>'"'+String(x).replace(/"/g,'""').replace(/^[=+@-]/,"'$&")+'"';
 this.save(new Blob(['\uFEFFFila,Columna,Error\r\n'+p.errores.map(e=>[e.fila,e.campo,e.error].map(cell).join(',')).join('\r\n')],{type:'text/csv;charset=utf-8'}),'validaciones.csv')}
 private async blob(url:string,name:string){try{this.save(await firstValueFrom(this.http.get(url,{responseType:'blob'})),name)}catch{this.message.set('No se pudo descargar el archivo.')}}
 private save(b:Blob,name:string){const url=URL.createObjectURL(b),a=document.createElement('a');a.href=url;a.download=name;a.click();URL.revokeObjectURL(url)}
}
