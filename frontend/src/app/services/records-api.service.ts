import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface RecordItem {
  id: string;
  type: string;
  tireId: string;
  plate: string;
  brand: string;
  dimension: string;
  status: string;
  center: string;
  position: string;
  observation: string;
  alert: string;
  depthExt?: number;
  depthCenter?: number;
  depthInt?: number;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class RecordsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/records';

  getAll(type?: string): Observable<RecordItem[]> {
    const url = type ? `${this.baseUrl}?type=${type}` : this.baseUrl;
    return this.http.get<RecordItem[]>(url);
  }
}
