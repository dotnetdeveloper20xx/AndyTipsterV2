import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SubscriptionService, AdminDashboardSummary } from '../../../../core/services/subscription.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6">
      <h1 class="text-3xl font-bold mb-6">Admin Dashboard</h1>

      @if (loading()) {
        <div class="flex justify-center"><span class="loading loading-spinner loading-lg"></span></div>
      } @else if (summary()) {
        <!-- Summary Cards -->
        <div class="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
          <div class="stat bg-base-200 shadow rounded-box border-l-4 border-primary">
            <div class="stat-title">Subscribers</div>
            <div class="stat-value text-primary">{{ summary()!.totalSubscribers }}</div>
          </div>
          <div class="stat bg-base-200 shadow rounded-box border-l-4 border-success">
            <div class="stat-title">MRR</div>
            <div class="stat-value text-success">£{{ summary()!.mrr | number:'1.0-0' }}</div>
          </div>
          <div class="stat bg-base-200 shadow rounded-box border-l-4 border-warning">
            <div class="stat-title">Tips Today</div>
            <div class="stat-value text-warning">{{ summary()!.tipsPublishedToday }}</div>
          </div>
          <div class="stat bg-base-200 shadow rounded-box border-l-4 border-info">
            <div class="stat-title">Recent Signups</div>
            <div class="stat-value text-info">{{ summary()!.recentSignups }}</div>
          </div>
          <div class="stat bg-base-200 shadow rounded-box border-l-4 border-error">
            <div class="stat-title">Payment Alerts</div>
            <div class="stat-value" [class.text-error]="summary()!.paymentAlerts > 0">{{ summary()!.paymentAlerts }}</div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex flex-wrap gap-2 mb-6">
          <button class="btn btn-sm btn-primary" (click)="navigate('/admin/tips')">Create Tips</button>
          <button class="btn btn-sm btn-secondary" (click)="navigate('/admin/plans')">Manage Plans</button>
          <button class="btn btn-sm btn-accent" (click)="navigate('/admin/users')">View Subscribers</button>
          <button class="btn btn-sm btn-outline" (click)="navigate('/admin/paypal-dashboard')">PayPal Dashboard</button>
        </div>

        <!-- Revenue Trend -->
        @if (summary()!.revenueTrend.length > 0) {
          <div class="card bg-base-100 shadow-xl mb-6">
            <div class="card-body">
              <h2 class="card-title">Revenue Trend</h2>
              <div class="flex items-end gap-1 h-32">
                @for (point of summary()!.revenueTrend; track point.date) {
                  <div class="flex-1 bg-primary rounded-t-sm"
                    [style.height.%]="getBarHeight(point.revenue)"
                    [title]="point.date + ': £' + point.revenue"></div>
                }
              </div>
            </div>
          </div>
        }

        <!-- Recent Activity -->
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h2 class="card-title">Recent Activity</h2>
            @if (summary()!.recentActivity.length === 0) {
              <p class="text-base-content/60">No recent activity.</p>
            } @else {
              <ul class="timeline timeline-vertical timeline-compact">
                @for (activity of summary()!.recentActivity; track activity.timestamp) {
                  <li class="py-2">
                    <div class="flex justify-between w-full">
                      <div>
                        <span class="font-medium">{{ activity.action }}</span>
                        <span class="text-base-content/60 ml-2">{{ activity.description }}</span>
                      </div>
                      <span class="text-xs text-base-content/50">{{ activity.timestamp | date:'short' }}</span>
                    </div>
                  </li>
                }
              </ul>
            }
          </div>
        </div>
      } @else {
        <!-- Onboarding Cards for Fresh Install -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="card bg-base-100 shadow-xl">
            <div class="card-body">
              <h2 class="card-title">🎯 Create Plans</h2>
              <p>Set up subscription plans for your tipsters.</p>
              <div class="card-actions justify-end">
                <button class="btn btn-primary btn-sm" (click)="navigate('/admin/plans')">Get Started</button>
              </div>
            </div>
          </div>
          <div class="card bg-base-100 shadow-xl">
            <div class="card-body">
              <h2 class="card-title">📝 Publish Tips</h2>
              <p>Start publishing horse racing tips for subscribers.</p>
              <div class="card-actions justify-end">
                <button class="btn btn-primary btn-sm" (click)="navigate('/admin/tips')">Create Tip</button>
              </div>
            </div>
          </div>
          <div class="card bg-base-100 shadow-xl">
            <div class="card-body">
              <h2 class="card-title">🎨 Customize Site</h2>
              <p>Build landing pages with the CMS page builder.</p>
              <div class="card-actions justify-end">
                <button class="btn btn-primary btn-sm" (click)="navigate('/admin/cms')">Open CMS</button>
              </div>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class DashboardComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly router = inject(Router);

  summary = signal<AdminDashboardSummary | null>(null);
  loading = signal(true);

  ngOnInit() {
    this.subscriptionService.getDashboardSummary().subscribe({
      next: (data) => { this.summary.set(data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  getBarHeight(revenue: number): number {
    const max = Math.max(...(this.summary()?.revenueTrend.map(p => p.revenue) ?? [1]));
    return max > 0 ? (revenue / max) * 100 : 0;
  }
}
