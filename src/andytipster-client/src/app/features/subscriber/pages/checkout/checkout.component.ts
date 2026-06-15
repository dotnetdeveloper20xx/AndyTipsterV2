import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CheckoutService, CheckoutSummary, PromoCodeValidation } from '../../../../core/services/checkout.service';
import { CurrencyDisplayPipe } from '../../../../shared/pipes/currency-display.pipe';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyDisplayPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-2xl mx-auto p-6">
      <h1 class="text-3xl font-bold mb-6">Checkout</h1>

      @if (confirmed()) {
        <!-- Confirmation Page -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body text-center">
            <div class="text-5xl mb-4">🎉</div>
            <h2 class="card-title justify-center text-2xl">Subscription Confirmed!</h2>
            <p class="text-base-content/70 mt-2">You're now subscribed to <strong>{{ summary()?.planName }}</strong></p>
            @if (summary()?.firstBillingDate) {
              <p class="text-sm">Next billing date: {{ summary()?.firstBillingDate | date:'dd MMM yyyy' }}</p>
            }
            <div class="card-actions justify-center mt-4">
              <button class="btn btn-primary" (click)="goToDashboard()">Go to Dashboard</button>
            </div>
          </div>
        </div>
      } @else {
        <!-- Checkout Form -->
        @if (loading()) {
          <div class="flex justify-center"><span class="loading loading-spinner loading-lg"></span></div>
        } @else if (summary()) {
          <div class="card bg-base-100 shadow-xl">
            <div class="card-body">
              <!-- Order Summary -->
              <h2 class="card-title">Order Summary</h2>
              <div class="divider"></div>
              <div class="flex justify-between">
                <span>{{ summary()!.planName }} ({{ summary()!.billingCycle }})</span>
                <span>{{ summary()!.originalPrice | currencyDisplay:summary()!.currency }}</span>
              </div>
              @if (summary()!.discountAmount) {
                <div class="flex justify-between text-success">
                  <span>Discount ({{ summary()!.promoCodeApplied }})</span>
                  <span>-{{ summary()!.discountAmount | currencyDisplay:summary()!.currency }}</span>
                </div>
              }
              @if (summary()!.trialDays > 0) {
                <div class="alert alert-info mt-2">
                  <span>🎁 {{ summary()!.trialDays }}-day free trial — billing starts {{ summary()!.trialEndDate | date:'dd MMM yyyy' }}</span>
                </div>
              }
              <div class="divider"></div>
              <div class="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span>{{ summary()!.finalPrice | currencyDisplay:summary()!.currency }}</span>
              </div>

              <!-- Promo Code -->
              <div class="form-control mt-4">
                <label class="label"><span class="label-text">Promo Code</span></label>
                <div class="join">
                  <input class="input input-bordered join-item w-full" [(ngModel)]="promoCode"
                    placeholder="Enter promo code" [disabled]="promoApplied()" />
                  <button class="btn btn-secondary join-item" (click)="applyPromoCode()"
                    [disabled]="!promoCode || promoApplied()">Apply</button>
                </div>
                @if (promoError()) {
                  <label class="label"><span class="label-text-alt text-error">{{ promoError() }}</span></label>
                }
              </div>

              <!-- Payment Method -->
              <div class="form-control mt-6">
                <label class="label"><span class="label-text font-semibold">Payment Method</span></label>
                <div class="flex gap-4">
                  <label class="cursor-pointer flex items-center gap-2 p-3 border rounded-lg"
                    [class.border-primary]="selectedProvider() === 'PayPal'">
                    <input type="radio" name="provider" class="radio radio-primary"
                      [checked]="selectedProvider() === 'PayPal'" (change)="selectProvider('PayPal')" />
                    <span class="font-medium">PayPal</span>
                  </label>
                  <label class="cursor-pointer flex items-center gap-2 p-3 border rounded-lg"
                    [class.border-primary]="selectedProvider() === 'Stripe'">
                    <input type="radio" name="provider" class="radio radio-primary"
                      [checked]="selectedProvider() === 'Stripe'" (change)="selectProvider('Stripe')" />
                    <span class="font-medium">Card (Stripe)</span>
                  </label>
                </div>
              </div>

              <!-- Pay Button -->
              <div class="card-actions justify-end mt-6">
                @if (error()) {
                  <div class="alert alert-error w-full mb-2">
                    <span>{{ error() }}</span>
                    <button class="btn btn-sm btn-ghost" (click)="error.set('')">Retry</button>
                  </div>
                }
                <button class="btn btn-primary btn-lg w-full" (click)="processPayment()"
                  [disabled]="processing()">
                  @if (processing()) {
                    <span class="loading loading-spinner"></span>
                  }
                  Subscribe Now
                </button>
              </div>
            </div>
          </div>
        }
      }
    </section>
  `,
})
export class CheckoutComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly checkoutService = inject(CheckoutService);

  summary = signal<CheckoutSummary | null>(null);
  loading = signal(true);
  processing = signal(false);
  confirmed = signal(false);
  error = signal('');
  promoCode = '';
  promoError = signal('');
  promoApplied = signal(false);
  selectedProvider = signal<'PayPal' | 'Stripe'>('PayPal');

  ngOnInit() {
    const planId = this.route.snapshot.queryParams['planId'];
    if (planId) {
      this.loadSummary(planId);
    } else {
      this.loading.set(false);
    }
  }

  private loadSummary(planId: string, promoCode?: string) {
    this.loading.set(true);
    this.checkoutService.getCheckoutSummary(planId, promoCode).subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load plan details.');
        this.loading.set(false);
      },
    });
  }

  selectProvider(provider: 'PayPal' | 'Stripe') {
    this.selectedProvider.set(provider);
  }

  applyPromoCode() {
    if (!this.promoCode || !this.summary()) return;
    this.promoError.set('');
    this.checkoutService.validatePromoCode(this.promoCode, this.summary()!.planId).subscribe({
      next: (result: PromoCodeValidation) => {
        if (result.isValid) {
          this.promoApplied.set(true);
          this.loadSummary(this.summary()!.planId, this.promoCode);
        } else {
          this.promoError.set(result.errorMessage || 'Invalid promo code.');
        }
      },
      error: () => this.promoError.set('Failed to validate promo code.'),
    });
  }

  processPayment() {
    if (!this.summary()) return;
    this.processing.set(true);
    this.error.set('');

    this.checkoutService.initiateCheckout({
      planId: this.summary()!.planId,
      provider: this.selectedProvider(),
      promoCode: this.promoApplied() ? this.promoCode : undefined,
      returnUrl: `${window.location.origin}/subscriber/checkout?confirmed=true`,
      cancelUrl: window.location.href,
    }).subscribe({
      next: (session) => {
        if (session.requiresRedirect && session.approvalUrl) {
          window.location.href = session.approvalUrl;
        } else {
          this.confirmed.set(true);
        }
        this.processing.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.error || 'Payment failed. Please try again.');
        this.processing.set(false);
      },
    });
  }

  goToDashboard() {
    this.router.navigate(['/subscriber/billing']);
  }
}
