import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// === Public Analytics DTOs ===

export interface PublicStatsDto {
  totalProfitLoss: number;
  strikeRate: number;
  roi: number;
  totalTips: number;
  won: number;
  lost: number;
  void: number;
  push: number;
  last30Days: Last30DaysSummaryDto;
  profitOverTime: ProfitOverTimeDto[];
  winRateTrends: WinRateTrendDto[];
  categoryComparisons: CategoryComparisonDto[];
  monthlyPnL: MonthlyPnLDto[];
}

export interface Last30DaysSummaryDto {
  profitLoss: number;
  strikeRate: number;
  roi: number;
  totalTips: number;
  won: number;
  lost: number;
}

export interface ProfitOverTimeDto {
  date: string;
  cumulativeProfit: number;
  dailyProfit: number;
}

export interface WinRateTrendDto {
  period: string;
  winRate: number;
  tipCount: number;
}

export interface CategoryComparisonDto {
  categoryId: string;
  categoryName: string;
  profitLoss: number;
  strikeRate: number;
  roi: number;
  tipCount: number;
}

export interface MonthlyPnLDto {
  year: number;
  month: number;
  profitLoss: number;
  tipCount: number;
  strikeRate: number;
}

// === Subscriber Performance DTOs ===

export interface SubscriberPerformanceDto {
  totalProfitLoss: number;
  strikeRate: number;
  totalTips: number;
  won: number;
  lost: number;
  currentStreak: number;
  streakType: string;
  longestWinStreak: number;
  longestLoseStreak: number;
  categoryBreakdown: CategoryComparisonDto[];
  monthlySummaries: MonthlyPerformanceSummaryDto[];
}

export interface MonthlyPerformanceSummaryDto {
  year: number;
  month: number;
  profitLoss: number;
  tipCount: number;
  won: number;
  lost: number;
  strikeRate: number;
  roi: number;
}

// === Revenue Analytics DTOs ===

export interface RevenueAnalyticsDto {
  mrr: number;
  arr: number;
  churnRate: number;
  totalActiveSubscribers: number;
  newSubscribersThisMonth: number;
  cancelledThisMonth: number;
  averageLTV: number;
  revenueByPlan: RevenueByPlanDto[];
  revenueTrends: RevenueTrendDto[];
  subscriberGrowth: SubscriberGrowthDto[];
  forecast: SubscriberForecastDto;
}

export interface RevenueByPlanDto {
  planId: string;
  planName: string;
  revenue: number;
  subscriberCount: number;
  averageLTV: number;
}

export interface RevenueTrendDto {
  date: string;
  revenue: number;
  provider: string;
  granularity: string;
}

export interface SubscriberGrowthDto {
  date: string;
  totalSubscribers: number;
  newSubscribers: number;
  churned: number;
  netGrowth: number;
}

export interface SubscriberForecastDto {
  projectedSubscribers30Days: number;
  projectedSubscribers90Days: number;
  projectedMRR30Days: number;
  projectedMRR90Days: number;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/analytics`;

  // === Public Performance ===

  getPublicStats(): Observable<PublicStatsDto> {
    return this.http.get<PublicStatsDto>(`${this.apiUrl}/public/stats`);
  }

  getLast30DaysSummary(): Observable<Last30DaysSummaryDto> {
    return this.http.get<Last30DaysSummaryDto>(`${this.apiUrl}/public/last-30-days`);
  }

  exportCsv(startDate?: string, endDate?: string, categoryId?: string): Observable<Blob> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get(`${this.apiUrl}/public/export/csv`, { params, responseType: 'blob' });
  }

  exportPdf(startDate?: string, endDate?: string, categoryId?: string): Observable<Blob> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get(`${this.apiUrl}/public/export/pdf`, { params, responseType: 'blob' });
  }

  // === Subscriber Performance ===

  getSubscriberPerformance(categoryId?: string, startDate?: string, endDate?: string): Observable<SubscriberPerformanceDto> {
    let params = new HttpParams();
    if (categoryId) params = params.set('categoryId', categoryId);
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    return this.http.get<SubscriberPerformanceDto>(`${this.apiUrl}/subscriber/performance`, { params });
  }

  getMonthlySummaries(months: number = 12): Observable<MonthlyPerformanceSummaryDto[]> {
    return this.http.get<MonthlyPerformanceSummaryDto[]>(`${this.apiUrl}/subscriber/monthly`, {
      params: new HttpParams().set('months', months.toString()),
    });
  }

  sendMonthlyDigest(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/subscriber/digest`, {});
  }

  // === Admin Revenue ===

  getRevenueAnalytics(filter?: { startDate?: string; endDate?: string; granularity?: string; provider?: string }): Observable<RevenueAnalyticsDto> {
    let params = new HttpParams();
    if (filter?.startDate) params = params.set('startDate', filter.startDate);
    if (filter?.endDate) params = params.set('endDate', filter.endDate);
    if (filter?.granularity) params = params.set('granularity', filter.granularity);
    if (filter?.provider) params = params.set('provider', filter.provider);
    return this.http.get<RevenueAnalyticsDto>(`${this.apiUrl}/admin/revenue`, { params });
  }

  getRevenueTrends(filter: { startDate?: string; endDate?: string; granularity?: string; provider?: string }): Observable<RevenueTrendDto[]> {
    let params = new HttpParams();
    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);
    if (filter.granularity) params = params.set('granularity', filter.granularity);
    if (filter.provider) params = params.set('provider', filter.provider);
    return this.http.get<RevenueTrendDto[]>(`${this.apiUrl}/admin/revenue/trends`, { params });
  }

  getSubscriberForecast(): Observable<SubscriberForecastDto> {
    return this.http.get<SubscriberForecastDto>(`${this.apiUrl}/admin/forecast`);
  }
}
