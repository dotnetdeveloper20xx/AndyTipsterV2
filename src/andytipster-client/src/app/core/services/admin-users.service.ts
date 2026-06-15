import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserListRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  roleFilter?: string;
  planFilter?: string;
  statusFilter?: string;
  registeredFrom?: string;
  registeredTo?: string;
  lastLoginFrom?: string;
  lastLoginTo?: string;
  sortBy?: string;
  sortDirection?: string;
}

export interface UserSummary {
  id: string;
  displayName: string;
  email: string;
  roles: string[];
  status: string;
  plan: string | null;
  createdAt: string;
  lastLoginAt: string | null;
  avatarUrl: string | null;
}

export interface UserListResponse {
  users: UserSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserDetail {
  id: string;
  displayName: string;
  email: string;
  bio: string | null;
  avatarUrl: string | null;
  timeZone: string | null;
  roles: string[];
  status: string;
  plan: string | null;
  createdAt: string;
  lastLoginAt: string | null;
  isSuspended: boolean;
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
}

export interface BulkActionRequest {
  userIds: string[];
  action: string;
  roleName?: string;
}

export interface BulkActionResponse {
  totalRequested: number;
  succeeded: number;
  failed: number;
  failures: { userId: string; reason: string }[];
}

export interface ImpersonateResponse {
  impersonationToken: string;
  impersonatedUserId: string;
  impersonatedUserName: string;
  impersonatedUserEmail: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/users`;

  getUsers(request: UserListRequest): Observable<UserListResponse> {
    let params = new HttpParams();
    if (request.page) params = params.set('page', request.page);
    if (request.pageSize) params = params.set('pageSize', request.pageSize);
    if (request.search) params = params.set('search', request.search);
    if (request.roleFilter) params = params.set('roleFilter', request.roleFilter);
    if (request.planFilter) params = params.set('planFilter', request.planFilter);
    if (request.statusFilter) params = params.set('statusFilter', request.statusFilter);
    if (request.registeredFrom) params = params.set('registeredFrom', request.registeredFrom);
    if (request.registeredTo) params = params.set('registeredTo', request.registeredTo);
    if (request.lastLoginFrom) params = params.set('lastLoginFrom', request.lastLoginFrom);
    if (request.lastLoginTo) params = params.set('lastLoginTo', request.lastLoginTo);
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http.get<UserListResponse>(this.apiUrl, { params });
  }

  getUser(id: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.apiUrl}/${id}`);
  }

  impersonateUser(id: string): Observable<ImpersonateResponse> {
    return this.http.post<ImpersonateResponse>(`${this.apiUrl}/${id}/impersonate`, {});
  }

  suspendUser(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/suspend`, {});
  }

  bulkAction(request: BulkActionRequest): Observable<BulkActionResponse> {
    return this.http.post<BulkActionResponse>(`${this.apiUrl}/bulk-action`, request);
  }

  exportUsers(request: UserListRequest): Observable<Blob> {
    let params = new HttpParams();
    if (request.search) params = params.set('search', request.search);
    if (request.roleFilter) params = params.set('roleFilter', request.roleFilter);
    if (request.statusFilter) params = params.set('statusFilter', request.statusFilter);

    return this.http.get(`${this.apiUrl}/export`, { params, responseType: 'blob' });
  }
}
