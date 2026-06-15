import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SubscriptionService, SubscriptionSelfService } from '../../../../core/services/subscription.service';
import { PlansService } from '../../../../core/services/plans.service';

@Component({
  selector: 'app-billing',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-4xl mx-auto p-6">
      <h1 class="text-3xl font-bold mb-6">Billing & Subscription</h1>

      @if (loading()) {
        <div class="flex justify-center"><span class="loading loading-spinner loading-lg"></span></div>
      } @else if (!subscription()) {
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body text-center">
            <h2 class="text-xl">No Active Subscription</h2>
            <p class="text-base-content/70">Choose a plan to get started.</p>
            <button class="btn btn-primary mt-4" (click)="goToPlans()">View Plans</button>
          </div>
        </div>
      } @else {
        <!-- Current Plan Card -->
        <div class="card bg-base-100 shadow-xl mb-6">
          <div class="card-body">
            <h2 class="card-title">Current Plan</h2>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
              <div>
                <p class="text-sm text-base-content/60">Plan</p>
                <p class="font-semibold">{{ subscription()!.planName }}</p>
              </div>
              <div>
                <p class="text-sm text-base-content/60">Status</p>
                <span class="badge" [class.badge-success]="subscription()!.status === 'Active'"
                  [class.badge-warning]="subscription()!.status === 'PastDue'"
                  [class.badge-info]="subscription()!.status === 'Trialing'">
                  {{ subscription()!.status }}
                </span>
              </div>
              <div>
                <p class="text-sm text-base-content/60">Next Billing</p>
                <p class="font-semibold">{{ subscription()!.nextBillingDate | date }}</p>
              </div>
              <div>
                <p class="text-sm text-base-content/60">Amount</p>
                <p class="font-semibold">{{ subscription()!.currency }} {{ subscription()!.price | number:'1.2-2' }} / {{ subscription()!.billingCycle }}</p>
              </div>
            </div>
            <div class="card-actions justify-end mt-4">
              <button class="btn btn-outline btn-sm" (click)="showUpgrade.set(true)">Change Plan</button>
              <button class="btn btn-outline btn-error btn-sm" (click)="showCancel.set(true)">Cancel</button>
            </div>
          </div>
        </div>

        <!-- Payment History -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h2 class="card-title">Payment History</h2>
            @if (subscription()!.paymentHistory.length === 0) {
              <p class="text-base-content/60">No payments recorded yet.</p>
            } @else {
              <div class="overflow-x-auto">
                <table class="table table-sm">
                  <thead>
                    <tr>
                      <th>Date</th><th>Amount</th><th>Status</th><th>Provider</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (payment of subscription()!.paymentHistory; track payment.id) {
                      <tr>
                        <td>{{ payment.paidAt | date:'short' }}</td>
                        <td>{{ payment.currency }} {{ payment.amount | number:'1.2-2' }}</td>
                        <td><span class="badge badge-sm" [class.badge-success]="payment.status === 'completed' || payment.status === 'succeeded'">{{ payment.status }}</span></td>
                        <td>{{ payment.provider }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>

        <!-- Cancel Modal -->
        @if (showCancel()) {
          <div class="modal modal-open">
            <div class="modal-box">
              <h3 class="font-bold text-lg">Cancel Subscription</h3>
              <p class="py-4">Your access will continue until the end of the current billing period.</p>
              <div class="modal-action">
                <button class="btn" (click)="showCancel.set(false)">Keep Subscription</button>
                <button class="btn btn-error" (click)="cancelSubscription()">Confirm Cancel</button>
              </div>
            </div>
          </div>
        }
      }
    </section>
  `,
})
export class BillingComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly router = inject(Router);

  subscription = signal<SubscriptionSelfService | null>(null);
  loading = signal(true);
  showUpgrade = signal(false);
  showCancel = signal(false);

  ngOnInit() {
    this.subscriptionService.getMySubscription().subscribe({
      next: (sub) => { this.subscription.set(sub); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  cancelSubscription() {
    this.subscriptionService.cancelSubscription('User requested').subscribe({
      next: () => { this.showCancel.set(false); this.ngOnInit(); },
    });
  }

  goToPlans() {
    this.router.navigate(['/']);
  }
}
