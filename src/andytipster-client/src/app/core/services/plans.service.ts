import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Plan } from '../../store/plans/plans.state';
import { CacheService } from './cache.service';

export interface PromoCode {
  id: string;
  code: string;
  discountType: string;
  discountValue: number;
  maxUses: number;
  currentUses: number;
  expiresAt?: string;
  isActive: boolean;
  applicablePlanIds: string[];
  createdAt: string;
}

export interface CreatePromoCodeRequest {
  code: string;
  discountType: string;
  discountValue: number;
  maxUses: number;
  expiresAt?: string;
  applicablePlanIds: string[];
}

const PLANS_CACHE_KEY = 'plans:all';
const PLANS_CACHE_TTL = 5 * 60 * 1000; // 5 minutes

@Injectable({ providedIn: 'root' })
export class PlansService {
  private readonly http = inject(HttpClient);
  private readonly cache = inject(CacheService);
  private readonly apiUrl = `${environment.apiUrl}/api/plans`;

  getPlans(): Observable<Plan[]> {
    const cached = this.cache.get<Plan[]>(PLANS_CACHE_KEY);
    if (cached) return of(cached);

    return this.http.get<Plan[]>(this.apiUrl).pipe(
      tap((plans) => this.cache.set(PLANS_CACHE_KEY, plans, PLANS_CACHE_TTL)),
    );
  }

  getPlan(planId: string): Observable<Plan> {
    return this.http.get<Plan>(`${this.apiUrl}/${planId}`);
  }

  createPlan(plan: Partial<Plan>): Observable<Plan> {
    return this.http.post<Plan>(this.apiUrl, plan).pipe(
      tap(() => this.cache.invalidate(PLANS_CACHE_KEY)),
    );
  }

  updatePlan(planId: string, changes: Partial<Plan>): Observable<Plan> {
    return this.http.patch<Plan>(`${this.apiUrl}/${planId}`, changes).pipe(
      tap(() => this.cache.invalidate(PLANS_CACHE_KEY)),
    );
  }

  archivePlan(planId: string): Observable<Plan> {
    return this.http.post<Plan>(`${this.apiUrl}/${planId}/archive`, {}).pipe(
      tap(() => this.cache.invalidate(PLANS_CACHE_KEY)),
    );
  }

  syncToPayPal(planId: string): Observable<Plan> {
    return this.http.post<Plan>(`${this.apiUrl}/${planId}/sync-paypal`, {});
  }

  retrySyncToPayPal(planId: string): Observable<Plan> {
    return this.http.post<Plan>(`${this.apiUrl}/${planId}/retry-sync`, {});
  }

  configureTransitions(planId: string, upgradePlanIds: string[], downgradePlanIds: string[]): Observable<Plan> {
    return this.http.put<Plan>(`${this.apiUrl}/${planId}/transitions`, { upgradePlanIds, downgradePlanIds });
  }

  // Promo codes
  getPromoCodes(): Observable<PromoCode[]> {
    return this.http.get<PromoCode[]>(`${this.apiUrl}/promo-codes`);
  }

  createPromoCode(dto: CreatePromoCodeRequest): Observable<PromoCode> {
    return this.http.post<PromoCode>(`${this.apiUrl}/promo-codes`, dto);
  }

  updatePromoCode(id: string, changes: Partial<PromoCode>): Observable<PromoCode> {
    return this.http.patch<PromoCode>(`${this.apiUrl}/promo-codes/${id}`, changes);
  }

  deletePromoCode(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/promo-codes/${id}`);
  }
}
