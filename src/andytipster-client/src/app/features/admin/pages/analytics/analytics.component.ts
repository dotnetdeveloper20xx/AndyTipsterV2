import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService, RevenueAnalyticsDto } from '../../../../core/services/analytics.service';
import { ScrollRevealDirective } from '../../../../shared/directives/scroll-reveal.directive';
import { CounterAnimationDirective } from '../../../../shared/directives/counter-animation.directive';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule, ScrollRevealDirective, CounterAnimationDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-7xl mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold text-base-content mb-2">Revenue Analytics</h1>
      <p class="text-base-content/70 mb-6">Unified PayPal + Stripe revenue view</p>

      <!-- Filters -->
      <div class="flex flex-wrap gap-3 mb-6">
        <select class="select select-bordered select-sm" [(ngModel)]="granularity"
                (change)="loadAnalytics()" aria-label="Granularity">
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
          <option value="monthly">Monthly</option>
        </select>
        <select class="select select-bordered select-sm" [(ngModel)]="provider"
                (change)="loadAnalytics()" aria-label="Payment provider">
          <option value="">All Providers</option>
          <option value="PayPal">PayPal</option>
          <option value="Stripe">Stripe</option>
        </select>
        <input type="date" class="input input-bordered input-sm" [(ngModel)]="startDate"
               (change)="loadAnalytics()" aria-label="Start date" />
        <input type="date" class="input input-bordered input-sm" [(ngModel)]="endDate"
               (change)="loadAnalytics()" aria-label="End date" />
      </div>

      @if (loading()) {
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          @for (i of [1,2,3,4]; track i) {
            <div class="card bg-base-200 animate-pulse h-24"></div>
          }
        </div>
      }

      @if (analytics()) {
        <!-- Key Metrics -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8" appScrollReveal>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">MRR</div>
            <div class="text-2xl font-bold text-primary">
              <span appCounterAnimation [counterTarget]="analytics()!.mrr" [counterDecimals]="2"
                    counterPrefix="£"></span>
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">ARR</div>
            <div class="text-2xl font-bold">
              £{{ analytics()!.arr | number:'1.2-2' }}
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Churn Rate</div>
            <div class="text-2xl font-bold" [class.text-warning]="analytics()!.churnRate > 5"
                 [class.text-success]="analytics()!.churnRate <= 5">
              {{ analytics()!.churnRate | number:'1.1-1' }}%
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Avg LTV</div>
            <div class="text-2xl font-bold">£{{ analytics()!.averageLTV | number:'1.2-2' }}</div>
          </div>
        </div>

        <!-- Subscriber Summary -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8" appScrollReveal [revealDelay]="100">
          <div class="card bg-success/10 p-4">
            <div class="text-sm">Active Subscribers</div>
            <div class="text-3xl font-bold">{{ analytics()!.totalActiveSubscribers }}</div>
          </div>
          <div class="card bg-primary/10 p-4">
            <div class="text-sm">New This Month</div>
            <div class="text-3xl font-bold text-primary">+{{ analytics()!.newSubscribersThisMonth }}</div>
          </div>
          <div class="card bg-error/10 p-4">
            <div class="text-sm">Cancelled This Month</div>
            <div class="text-3xl font-bold text-error">-{{ analytics()!.cancelledThisMonth }}</div>
          </div>
        </div>

        <!-- Revenue by Plan -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="200">
          <h2 class="text-xl font-semibold mb-4">Revenue by Plan</h2>
          <div class="overflow-x-auto">
            <table class="table table-sm">
              <thead>
                <tr>
                  <th>Plan</th>
                  <th>Revenue</th>
                  <th>Subscribers</th>
                  <th>Avg LTV</th>
                </tr>
              </thead>
              <tbody>
                @for (plan of analytics()!.revenueByPlan; track plan.planId) {
                  <tr>
                    <td class="font-medium">{{ plan.planName }}</td>
                    <td>£{{ plan.revenue | number:'1.2-2' }}</td>
                    <td>{{ plan.subscriberCount }}</td>
                    <td>£{{ plan.averageLTV | number:'1.2-2' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- Revenue Trend Chart -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="300">
          <h2 class="text-xl font-semibold mb-4">Revenue Trend</h2>
          <div class="overflow-x-auto">
            <div class="flex items-end gap-1 h-48 min-w-[600px]">
              @for (trend of getCombinedTrends(); track trend.date) {
                <div class="flex-1 flex flex-col items-center justify-end h-full">
                  <div class="w-full bg-primary rounded-t transition-all duration-300"
                       [style.height.%]="getTrendBarHeight(trend.revenue)">
                  </div>
                  <div class="text-xs text-base-content/50 mt-1 truncate w-full text-center">
                    {{ trend.date | date:'MM/dd' }}
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Subscriber Growth -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="400">
          <h2 class="text-xl font-semibold mb-4">Subscriber Growth</h2>
          <div class="overflow-x-auto">
            <table class="table table-sm">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Total</th>
                  <th>New</th>
                  <th>Churned</th>
                  <th>Net Growth</th>
                </tr>
              </thead>
              <tbody>
                @for (growth of analytics()!.subscriberGrowth.slice(-20); track growth.date) {
                  <tr>
                    <td>{{ growth.date | date:'mediumDate' }}</td>
                    <td class="font-bold">{{ growth.totalSubscribers }}</td>
                    <td class="text-success">+{{ growth.newSubscribers }}</td>
                    <td class="text-error">-{{ growth.churned }}</td>
                    <td [class.text-success]="growth.netGrowth >= 0" [class.text-error]="growth.netGrowth < 0">
                      {{ growth.netGrowth >= 0 ? '+' : '' }}{{ growth.netGrowth }}
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- Forecast -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="500">
          <h2 class="text-xl font-semibold mb-4">Growth Forecast</h2>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <div class="text-sm text-base-content/60">Subscribers (30d)</div>
              <div class="text-xl font-bold">{{ analytics()!.forecast.projectedSubscribers30Days }}</div>
            </div>
            <div>
              <div class="text-sm text-base-content/60">Subscribers (90d)</div>
              <div class="text-xl font-bold">{{ analytics()!.forecast.projectedSubscribers90Days }}</div>
            </div>
            <div>
              <div class="text-sm text-base-content/60">Projected MRR (30d)</div>
              <div class="text-xl font-bold text-primary">£{{ analytics()!.forecast.projectedMRR30Days | number:'1.2-2' }}</div>
            </div>
            <div>
              <div class="text-sm text-base-content/60">Projected MRR (90d)</div>
              <div class="text-xl font-bold text-primary">£{{ analytics()!.forecast.projectedMRR90Days | number:'1.2-2' }}</div>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class AnalyticsComponent implements OnInit {
  private readonly analyticsService = inject(AnalyticsService);

  readonly analytics = signal<RevenueAnalyticsDto | null>(null);
  readonly loading = signal(true);

  granularity = 'monthly';
  provider = '';
  startDate = '';
  endDate = '';

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.loading.set(true);
    this.analyticsService
      .getRevenueAnalytics({
        granularity: this.granularity,
        provider: this.provider || undefined,
        startDate: this.startDate || undefined,
        endDate: this.endDate || undefined,
      })
      .subscribe({
        next: (data) => {
          this.analytics.set(data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  getCombinedTrends() {
    return (this.analytics()?.revenueTrends ?? [])
      .filter((t) => t.provider === 'Combined')
      .slice(-20);
  }

  getTrendBarHeight(revenue: number): number {
    const trends = this.getCombinedTrends();
    const max = Math.max(...trends.map((t) => t.revenue), 1);
    return Math.max(5, (revenue / max) * 80);
  }
}
