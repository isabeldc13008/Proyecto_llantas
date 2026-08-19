import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { forkJoin, map, of, switchMap } from 'rxjs';
import { CatalogItem, Page } from '../models/api.models';

@Injectable({providedIn:'root'})
export class CatalogsApi {
  private readonly http=inject(HttpClient);
  all(type:string,active?:boolean){
    let params=new HttpParams().set('pageNumber',1).set('pageSize',100);
    if(active!==undefined)params=params.set('activo',active);
    return this.http.get<Page<CatalogItem>>(`/api/catalogos/${type}`,{params}).pipe(switchMap(first=>{
      if(first.totalPages<=1)return of(first.items);
      const requests=Array.from({length:first.totalPages-1},(_,index)=>this.http.get<Page<CatalogItem>>(`/api/catalogos/${type}`,{params:params.set('pageNumber',index+2)}));
      return forkJoin(requests).pipe(map(pages=>[...first.items,...pages.flatMap(page=>page.items)]));
    }));
  }
}
