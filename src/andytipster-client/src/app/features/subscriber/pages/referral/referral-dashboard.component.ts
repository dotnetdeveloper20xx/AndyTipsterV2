import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReferralService, ReferralDashboardDto } from '../../../../core/services/referral.service';

@Component({
  selector: 'app-referral-dashboard',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="container mx-auto p-4 max-w-4xl">
      <h1 class="text-2xl font-bold mb-6">Referral Program</h1>

      @if (dashboard()) {
        <!-- Referral Link -->
        <div class="card bg-base-100 shadow-md mb-6">
          <div class="card-body">
            <h2 class="card-title text-lg">Your Referral Link</h2>
            <div class="flex gap-2 items-center">
              <input
                type="text"
                [value]="dashboard()!.referralUrl"
                readonly
                class="input input-bordered flex-1 text-sm"
                aria-label="Referral URL" />
              <button
                class="btn btn-primary btn-sm"
                (click)="copyLink()"
                aria-label="Copy referral link">
                {{ copied() ? '✓ Copied' : 'Copy' }}
              </button>
            </div>
            <p class="text-sm text-base-content/60 mt-2">
              Share this link with friends. When they subscribe, you both earn rewards!
            </p>
          </div>
        </div>

        <!-- Stats -->
        <div class="stats shadow w-full mb-6">
          <div class="stat">
            <div class="stat-title">Total Clicks</div>
            <div class="stat-value text-primary">{{ dashboard()!.totalClicks }}</div>
          </div>
          <div class="stat">
            <div class="stat-title">Conversions</div>
            <div class="stat-value text-secondary">{{ dashboard()!.totalConversions }}</div>
          </div>
          <div class="stat">
            <div class="stat-title">Rewards Earned</div>
            <div class="stat-value text-accent">£{{ dashboard()!.totalRewardsEarned | number:'1.2-2' }}</div>
          </div>
        </div>

        <!-- Referral History -->
        <div class="card bg-base-100 shadow-md">
          <div class="card-body">
            <h2 class="card-title text-lg">Referral History</h2>
            @if (dashboard()!.referrals.length === 0) {
              <p class="text-base-content/60 py-4 text-center">No referrals yet. Share your link to get started!</p>
            } @else {
              <div class="overflow-x-auto">
                <table class="table table-sm" aria-label="Referral history">
                  <thead>
                    <tr>
                      <th>Date</th>
                      <th>Status</th>
                      <th>Converted</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (referral of dashboard()!.referrals; track referral.id) {
                      <tr>
                        <td>{{ referral.createdAt | date:'mediumDate' }}</td>
                        <td>
                          <span [class]="referral.isConverted ? 'badge badge-success badge-sm' : 'badge badge-warning badge-sm'">
                            {{ referral.isConverted ? 'Converted' : 'Pending' }}
                          </span>
                        </td>
                        <td>{{ referral.convertedAt ? (referral.convertedAt | date:'mediumDate') : '—' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      } @else {
        <div class="flex justify-center py-12">
          <span class="loading loading-spinner loading-lg"></span>
        </div>
      }
    </div>
  `
})
export class ReferralDashboardComponent implements OnInit {
  private readonly referralService = inject(ReferralService);

  dashboard = signal<ReferralDashboardDto | null>(null);
  copied = signal(false);

  ngOnInit(): void {
    this.referralService.getDashboard().subscribe(data => {
      this.dashboard.set(data);
    });
  }

  copyLink(): void {
    const url = this.dashboard()?.referralUrl;
    if (url) {
      navigator.clipboard.writeText(url);
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    }
  }
}
