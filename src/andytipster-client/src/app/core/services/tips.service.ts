import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
}

export interface TipDto {
  id: string;
  eventDate: string;
  raceName: string;
  selection: string;
  odds: number;
  stake: number;
  categoryId: string;
  categoryName: string;
  commentary?: string;
  status: string;
  result?: string;
  profitLoss?: number;
  createdAt: string;
  publishedAt?: string;
  scheduledPublishAt?: string;
  createdByUserId: string;
}

export interface CreateTipDto {
  eventDate: string;
  raceName: string;
  selection: string;
  odds: number;
  stake: number;
  categoryId: string;
  commentary?: string;
}

export interface UpdateTipDto {
  eventDate?: string;
  raceName?: string;
  selection?: string;
  odds?: number;
  stake?: number;
  categoryId?: string;
  commentary?: string;
}

export interface TipFilterDto {
  startDate?: string;
  endDate?: string;
  categoryId?: string;
  result?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface PnLSummaryDto {
  totalProfitLoss: number;
  totalTips: number;
  won: number;
  lost: number;
  void: number;
  push: number;
  strikeRate: number;
  periods: PnLPeriodDto[];
  categories: PnLCategoryDto[];
}

export interface PnLPeriodDto {
  period: string;
  profitLoss: number;
  tipCount: number;
}

export interface PnLCategoryDto {
  categoryId: string;
  categoryName: string;
  profitLoss: number;
  tipCount: number;
}

export interface BulkImportResultDto {
  totalRows: number;
  successCount: number;
  errorCount: number;
  errors: BulkImportErrorDto[];
}

export interface BulkImportErrorDto {
  rowNumber: number;
  field: string;
  error: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface AccessCheckResult {
  hasAccess: boolean;
  denialReason?: string;
  isTipOfTheDay: boolean;
  showPaywall: boolean;
}

@Injectable({ providedIn: 'root' })
export class TipsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/tips`;
  private readonly categoriesUrl = `${environment.apiUrl}/api/categories`;

  // === Tips CRUD ===

  getTips(filter?: TipFilterDto): Observable<PaginatedResponse<TipDto>> {
    let params = new HttpParams();
    if (filter?.page) params = params.set('page', filter.page.toString());
    if (filter?.pageSize) params = params.set('pageSize', filter.pageSize.toString());
    if (filter?.categoryId) params = params.set('categoryId', filter.categoryId);
    if (filter?.startDate) params = params.set('startDate', filter.startDate);
    if (filter?.endDate) params = params.set('endDate', filter.endDate);
    if (filter?.result) params = params.set('result', filter.result);
    if (filter?.status) params = params.set('status', filter.status);
    return this.http.get<PaginatedResponse<TipDto>>(this.apiUrl, { params });
  }

  getTipsFeed(filter?: TipFilterDto): Observable<PaginatedResponse<TipDto>> {
    let params = new HttpParams();
    if (filter?.page) params = params.set('page', filter.page.toString());
    if (filter?.pageSize) params = params.set('pageSize', filter.pageSize.toString());
    if (filter?.categoryId) params = params.set('categoryId', filter.categoryId);
    if (filter?.startDate) params = params.set('startDate', filter.startDate);
    if (filter?.endDate) params = params.set('endDate', filter.endDate);
    if (filter?.result) params = params.set('result', filter.result);
    return this.http.get<PaginatedResponse<TipDto>>(`${this.apiUrl}/feed`, { params });
  }

  getTipOfTheDay(): Observable<TipDto> {
    return this.http.get<TipDto>(`${this.apiUrl}/tip-of-the-day`);
  }

  getTip(tipId: string): Observable<TipDto> {
    return this.http.get<TipDto>(`${this.apiUrl}/${tipId}`);
  }

  createTip(dto: CreateTipDto): Observable<TipDto> {
    return this.http.post<TipDto>(this.apiUrl, dto);
  }

  updateTip(tipId: string, dto: UpdateTipDto): Observable<TipDto> {
    return this.http.patch<TipDto>(`${this.apiUrl}/${tipId}`, dto);
  }

  deleteTip(tipId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${tipId}`);
  }

  publishTip(tipId: string, scheduledPublishAt?: string): Observable<TipDto> {
    return this.http.post<TipDto>(`${this.apiUrl}/${tipId}/publish`, { scheduledPublishAt });
  }

  archiveTip(tipId: string): Observable<TipDto> {
    return this.http.post<TipDto>(`${this.apiUrl}/${tipId}/archive`, {});
  }

  recordResult(tipId: string, result: string): Observable<TipDto> {
    return this.http.post<TipDto>(`${this.apiUrl}/${tipId}/result`, { result });
  }

  checkAccess(tipId: string): Observable<AccessCheckResult> {
    return this.http.get<AccessCheckResult>(`${this.apiUrl}/${tipId}/access`);
  }

  // === P&L ===

  getPnLSummary(startDate?: string, endDate?: string, categoryId?: string, groupBy?: string): Observable<PnLSummaryDto> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (categoryId) params = params.set('categoryId', categoryId);
    if (groupBy) params = params.set('groupBy', groupBy);
    return this.http.get<PnLSummaryDto>(`${this.apiUrl}/pnl`, { params });
  }

  // === Bulk Import ===

  bulkImport(file: File): Observable<BulkImportResultDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<BulkImportResultDto>(`${this.apiUrl}/import`, formData);
  }

  // === Categories ===

  getCategories(includeInactive = false): Observable<CategoryDto[]> {
    let params = new HttpParams();
    if (includeInactive) params = params.set('includeInactive', 'true');
    return this.http.get<CategoryDto[]>(this.categoriesUrl, { params });
  }

  getCategory(categoryId: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`${this.categoriesUrl}/${categoryId}`);
  }

  createCategory(dto: { name: string; description?: string }): Observable<CategoryDto> {
    return this.http.post<CategoryDto>(this.categoriesUrl, dto);
  }

  updateCategory(categoryId: string, dto: { name?: string; description?: string; isActive?: boolean }): Observable<CategoryDto> {
    return this.http.patch<CategoryDto>(`${this.categoriesUrl}/${categoryId}`, dto);
  }

  deleteCategory(categoryId: string): Observable<void> {
    return this.http.delete<void>(`${this.categoriesUrl}/${categoryId}`);
  }

  seedDefaultCategories(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.categoriesUrl}/seed`, {});
  }
}
