import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateSubscriptionRequest {
  planId: string;
  provider: 'PayPal' | 'Stripe';
  promoCode?: string;
  returnUrl?: string;
  cancelUrl?: string;
}

export interface CheckoutSession {
  sessionId: string;
  approvalUrl?: string;
  clientSecret?: string;
  paymentProvider: string;
  requiresRedirect: boolean;
}

export interface CheckoutConfirmation {
  success: boolean;
  subscriptionId?: string;
  planName?: string;
  nextBillingDate?: string;
  amountCharged?: number;
  errorMessage?: string;
}

export interface CheckoutSummary {
  planId: string;
  planName: string;
  originalPrice: number;
  finalPrice: number;
  discountAmount?: number;
  promoCodeApplied?: string;
  trialDays: number;
  trialEndDate?: string;
  firstBillingDate?: string;
  currency: string;
  billingCycle: string;
}

export interface PromoCodeValidation {
  isValid: boolean;
  errorMessage?: string;
  discountedPrice?: number;
  discountAmount?: number;
  discountType?: string;
  discountValue?: number;
}

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api`;

  initiateCheckout(request: CreateSubscriptionRequest): Observable<CheckoutSession> {
    return this.http.post<CheckoutSession>(`${this.apiUrl}/checkout`, request);
  }

  confirmCheckout(sessionId: string): Observable<CheckoutConfirmation> {
    return this.http.post<CheckoutConfirmation>(`${this.apiUrl}/checkout/confirm`, { sessionId });
  }

  getCheckoutSummary(planId: string, promoCode?: string): Observable<CheckoutSummary> {
    const params: Record<string, string> = {};
    if (promoCode) params['promoCode'] = promoCode;
    return this.http.get<CheckoutSummary>(`${this.apiUrl}/checkout/summary/${planId}`, { params });
  }

  validatePromoCode(code: string, planId: string): Observable<PromoCodeValidation> {
    return this.http.post<PromoCodeValidation>(`${this.apiUrl}/plans/promo-codes/validate`, { code, planId });
  }
}
