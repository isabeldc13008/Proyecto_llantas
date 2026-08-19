import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CatalogItem, Page, Tire, TireDetail, TireInput } from '../models/api.models';
import { CatalogsApi } from './catalogs-api';

@Injectable({providedIn:'root'})
export class TiresApi {
  private readonly http=inject(HttpClient);
  private readonly catalogsApi=inject(CatalogsApi);

  list(pageNumber=1,search='',sortBy='codigo',filters:Record<string,unknown>={}):Observable<Page<Tire>>{
    let params=new HttpParams().set('pageNumber',pageNumber).set('pageSize',20).set('search',search).set('sortBy',sortBy);
    params=this.withFilters(params,filters);
    return this.http.get<Page<Tire>>('/api/llantas',{params});
  }
  export(search:string,sortBy:string,format:'csv'|'xlsx',filters:Record<string,unknown>={}){let params=new HttpParams().set('search',search).set('sortBy',sortBy).set('formato',format);params=this.withFilters(params,filters);return this.http.get('/api/llantas/exportar',{params,responseType:'blob',observe:'response'});}
  private withFilters(params:HttpParams,filters:Record<string,unknown>){const centers=filters['centroIds'] as string[]|undefined;const states=filters['estados'] as string[]|undefined;if(centers?.length)params=params.set('centroIds',centers.join(','));if(states?.length)params=params.set('estados',states.join(','));if(filters['profundidadMin']!==''&&filters['profundidadMin']!=null)params=params.set('profundidadMin',String(filters['profundidadMin']));if(filters['profundidadMax']!==''&&filters['profundidadMax']!=null)params=params.set('profundidadMax',String(filters['profundidadMax']));return params}
  create(value:TireInput){return this.http.post<Tire>('/api/llantas',value);}
  update(id:string,value:TireInput){return this.http.put<Tire>(`/api/llantas/${id}`,value);}
  setActive(id:string,activo:boolean){return this.http.patch<void>(`/api/llantas/${id}/estado`,{activo});}
  history(id:string){return this.http.get<TireDetail>(`/api/llantas/${id}/historial`);}
  transfer(id:string,centroDestinoId:string,motivo:string,observaciones:string){return this.http.post<void>(`/api/llantas/${id}/traslados`,{centroDestinoId,motivo,observaciones:observaciones||null});}
  catalog(type:string):Observable<Page<CatalogItem>>{
    return this.catalogsApi.all(type,true).pipe(map(items=>({items,pageNumber:1,pageSize:items.length,totalItems:items.length,totalPages:1})));
  }
}
