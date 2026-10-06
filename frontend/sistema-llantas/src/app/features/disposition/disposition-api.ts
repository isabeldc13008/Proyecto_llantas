import {inject,Injectable} from '@angular/core';
import {HttpClient,HttpParams} from '@angular/common/http';
import {Detail,Dispatch,Filter,Lot,Order,Page,Provider,Summary,Ref} from './disposition-models';
@Injectable({providedIn:'root'})
export class DispositionApi{
 private http=inject(HttpClient);private base='/api/disposicion';
 private params(f:Filter){let p=new HttpParams();Object.entries(f).forEach(([k,v])=>{if(v!==undefined&&v!=='')p=p.set(k,v)});return p}
 summary(f:Filter){return this.http.get<Summary>(this.base+'/resumen',{params:this.params(f)})}
 orders(f:Filter){return this.http.get<Page<Order>>(this.base+'/ordenes',{params:this.params(f)})}
 detail(id:string){return this.http.get<Detail>(this.base+'/ordenes/'+id)}
 lots(f:Filter){return this.http.get<Page<Lot>>(this.base+'/lotes',{params:this.params(f)})}
 lot(id:string){return this.http.get<Lot>(this.base+'/lotes/'+id)}
 available(f:Filter,r1:string){return this.http.get<Page<Order>>(this.base+'/disponibles',{params:this.params(f).set('centroR1Id',r1)})}
 dispatches(f:Filter){return this.http.get<Page<Dispatch>>(this.base+'/despachos',{params:this.params(f)})}
 dispatch(id:string){return this.http.get<Dispatch>(this.base+'/despachos/'+id)}
 providers(){return this.http.get<Provider[]>('/api/servicios-llanta/proveedores')}
 tires(buscar:string,pageNumber=1){return this.http.get<Page<Ref>>(this.base+'/llantas',{params:{buscar,pageNumber}})}
 createLot(body:unknown){return this.http.post<{id:string}>('/api/servicios-llanta/disposicion/lotes',body)}
 receive(id:string,ordenIds:string[]){return this.http.post('/api/servicios-llanta/disposicion/lotes/'+id+'/recibir',{ordenIds})}
 createDispatch(body:unknown){return this.http.post<Dispatch>(this.base+'/despachos',body)}
 close(id:string,rowVersion:string,observaciones:string){return this.http.post<Dispatch>(this.base+'/despachos/'+id+'/cerrar',{rowVersion,observaciones})}
 support(id:string,file:File){const b=new FormData();b.append('archivo',file);return this.http.post(this.base+'/actas/'+id+'/soportes',b)}
 deleteSupport(id:string){return this.http.delete(this.base+'/soportes/'+id)}
 file(id:string,technical=false){return this.http.get(technical?'/api/servicios-llanta/evidencias/'+id+'/descargar':this.base+'/soportes/'+id+'/archivo',{responseType:'blob'})}
 evidence(id:string,file:File){const b=new FormData();b.append('archivo',file);return this.http.post('/api/servicios-llanta/'+id+'/evidencias',b)}
 evaluate(id:string,reutilizable:boolean,observaciones:string){return this.http.post('/api/servicios-llanta/'+id+'/evaluar-disposicion',{reutilizable,observaciones})}
 approve(id:string){return this.http.post('/api/servicios-llanta/'+id+'/aprobar',{})}
 reject(id:string,motivo:string){return this.http.post('/api/servicios-llanta/'+id+'/rechazar',{motivo})}
 novelty(id:string,observacion:string,ordenId:string|null){return this.http.post(this.base+'/lotes/'+id+'/novedades',{observacion,ordenId})}
 resolve(id:string,observacion:string){return this.http.post(this.base+'/novedades/'+id+'/resolver',{observacion})}
 createOrder(llantaId:string,motivo:string){return this.http.post<{id:string}>('/api/servicios-llanta',{tipo:'DisposicionFinal',llantaId,motivo,proveedorId:null,costo:null,observaciones:null})}
}
export function downloadBlob(blob:Blob,name:string){const url=URL.createObjectURL(blob),a=document.createElement('a');a.href=url;a.download=name;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000)}
