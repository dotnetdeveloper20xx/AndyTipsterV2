import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Plan } from '../../store/plans/plans.state';

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

@Injectable({ providedIn: 'root' })
export class PlansService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/plans`;

  getPlans(): Observable<Plan[]> {
    return this.http.get<Plan[]>(this.apiUrl);
  }

  getPlan(planId: string): Observable<Plan> {
    return this.http.get<Plan>(`${this.apiUrl}/${planId}`);
  }

  createPlan(plan: Partial<Plan>): Observable<Plan> {
    return this.http.post<Plan>(this.apiUrl, plan);
  }

  updatePlan(planId: string, changes: Partial<Plan>): Observable<Plan> {
    return this.http.patch<Plan>(`${this.apiUrl}/${planId}`, changes);
  }

  archivePlan(planId: string): Observable<Plan> {
    return this.http.post<Plan>(`${this.apiUrl}/${planId}/archive`, {});
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
