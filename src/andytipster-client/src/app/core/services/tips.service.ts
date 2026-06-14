import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Tip } from '../../store/tips/tips.state';

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class TipsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/tips`;

  getTips(page?: number, pageSize?: number, category?: string): Observable<PaginatedResponse<Tip>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (pageSize) params = params.set('pageSize', pageSize.toString());
    if (category) params = params.set('category', category);
    return this.http.get<PaginatedResponse<Tip>>(this.apiUrl, { params });
  }

  getTip(tipId: string): Observable<Tip> {
    return this.http.get<Tip>(`${this.apiUrl}/${tipId}`);
  }

  createTip(tip: Partial<Tip>): Observable<Tip> {
    return this.http.post<Tip>(this.apiUrl, tip);
  }

  updateTip(tipId: string, changes: Partial<Tip>): Observable<Tip> {
    return this.http.patch<Tip>(`${this.apiUrl}/${tipId}`, changes);
  }

  publishTip(tipId: string): Observable<Tip> {
    return this.http.post<Tip>(`${this.apiUrl}/${tipId}/publish`, {});
  }

  recordResult(tipId: string, result: string): Observable<Tip> {
    return this.http.post<Tip>(`${this.apiUrl}/${tipId}/result`, { result });
  }
}
