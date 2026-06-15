import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SubscriptionSelfService {
  subscriptionId: string;
  planName: string;
  price: number;
  currency: string;
  billingCycle: string;
  status: string;
  nextBillingDate?: string;
  paymentProvider: string;
  paymentMethodLast4?: string;
  paymentHistory: PaymentHistoryItem[];
}

export interface PaymentHistoryItem {
  id: string;
  amount: number;
  currency: string;
  status: string;
  paidAt: string;
  provider: string;
}

export interface UpgradeResult {
  success: boolean;
  newSubscriptionId?: string;
  proratedAmount?: number;
  errorMessage?: string;
}

export interface AdminDashboardSummary {
  totalSubscribers: number;
  mrr: number;
  tipsPublishedToday: number;
  recentSignups: number;
  paymentAlerts: number;
  revenueTrend: { date: string; revenue: number }[];
  subscriberTrend: { date: string; count: number }[];
  recentActivity: { action: string; description: string; userName?: string; timestamp: string }[];
}

export interface RevenueAnalytics {
  mrr: number;
  arr: number;
  churnRate: number;
  revenueByPlan: Record<string, number>;
  activeSubscribers: number;
  newSubscribersThisMonth: number;
  revenueTrend: { date: string; revenue: number }[];
  subscriberTrend: { date: string; count: number }[];
}

export interface TransactionSearchParams {
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
  status?: string;
  provider?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface TransactionListResult {
  items: PaymentHistoryItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api`;

  getMySubscription(): Observable<SubscriptionSelfService> {
    return this.http.get<SubscriptionSelfService>(`${this.apiUrl}/subscriptions/me`);
  }

  upgradePlan(targetPlanId: string): Observable<UpgradeResult> {
    return this.http.post<UpgradeResult>(`${this.apiUrl}/subscriptions/upgrade`, { targetPlanId });
  }

  cancelSubscription(reason?: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/subscriptions/cancel`, { reason });
  }

  getDashboardSummary(): Observable<AdminDashboardSummary> {
    return this.http.get<AdminDashboardSummary>(`${this.apiUrl}/admin/dashboard/summary`);
  }

  getRevenueAnalytics(): Observable<RevenueAnalytics> {
    return this.http.get<RevenueAnalytics>(`${this.apiUrl}/admin/dashboard/revenue`);
  }

  getTransactions(params: TransactionSearchParams): Observable<TransactionListResult> {
    return this.http.get<TransactionListResult>(`${this.apiUrl}/admin/dashboard/transactions`, { params: params as Record<string, string> });
  }

  processRefund(externalTransactionId: string, amount?: number, reason?: string): Observable<{ success: boolean; refundId?: string; errorMessage?: string }> {
    return this.http.post<{ success: boolean; refundId?: string; errorMessage?: string }>(`${this.apiUrl}/admin/dashboard/refund`, { externalTransactionId, amount, reason });
  }
}
