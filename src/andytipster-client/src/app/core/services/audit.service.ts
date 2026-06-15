import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuditLogRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  actionTypeFilter?: string;
  actorFilter?: string;
  targetEntityFilter?: string;
  dateFrom?: string;
  dateTo?: string;
  sortBy?: string;
  sortDirection?: string;
}

export interface AuditLogEntry {
  id: number;
  actorUserId: string;
  actorName: string;
  actionType: string;
  targetEntity: string;
  targetEntityId: string | null;
  beforeJson: string | null;
  afterJson: string | null;
  timestamp: string;
  ipAddress: string | null;
}

export interface AuditLogResponse {
  entries: AuditLogEntry[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/audit`;

  getAuditLogs(request: AuditLogRequest): Observable<AuditLogResponse> {
    let params = new HttpParams();
    if (request.page) params = params.set('page', request.page);
    if (request.pageSize) params = params.set('pageSize', request.pageSize);
    if (request.search) params = params.set('search', request.search);
    if (request.actionTypeFilter) params = params.set('actionTypeFilter', request.actionTypeFilter);
    if (request.actorFilter) params = params.set('actorFilter', request.actorFilter);
    if (request.targetEntityFilter) params = params.set('targetEntityFilter', request.targetEntityFilter);
    if (request.dateFrom) params = params.set('dateFrom', request.dateFrom);
    if (request.dateTo) params = params.set('dateTo', request.dateTo);
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http.get<AuditLogResponse>(this.apiUrl, { params });
  }
}
