import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService, SubscriberPerformanceDto } from '../../../core/services/analytics.service';
import { ScrollRevealDirective } from '../../directives/scroll-reveal.directive';

@Component({
  selector: 'app-profit-tracker',
  standalone: true,
  imports: [CommonModule, ScrollRevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (performance()) {
      <div class="card bg-white shadow-sm border border-base-200 mb-6 animate-fade-in" appScrollReveal revealAnimation="fade-in">
        <div class="card-body py-4 px-6">
          <!-- Header line -->
          <div class="flex flex-wrap items-center gap-2 text-sm text-base-content/70 mb-3">
            <span class="font-medium text-base-content">Your Performance</span>
            <span class="text-base-content/50">•</span>
            <span>Total profit:
              <span class="font-semibold" [class.text-success]="performance()!.totalProfitLoss >= 0"
                    [class.text-error]="performance()!.totalProfitLoss < 0">
                {{ performance()!.totalProfitLoss >= 0 ? '+' : '' }}£{{ performance()!.totalProfitLoss | number:'1.2-2' }}
              </span>
            </span>
            <span class="text-base-content/50">•</span>
            <span>This month:
              <span class="font-semibold" [class.text-success]="currentMonthPnL() >= 0"
                    [class.text-error]="currentMonthPnL() < 0">
                {{ currentMonthPnL() >= 0 ? '+' : '' }}£{{ currentMonthPnL() | number:'1.2-2' }}
              </span>
            </span>
          </div>

          <!-- Stat bar -->
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div class="text-center">
              <div class="text-xs text-base-content/60 uppercase tracking-wide">Total Tips</div>
              <div class="text-xl font-bold text-base-content mt-0.5">{{ performance()!.totalTips }}</div>
            </div>
            <div class="text-center">
              <div class="text-xs text-base-content/60 uppercase tracking-wide">Total Profit</div>
              <div class="text-xl font-bold mt-0.5"
                   [class.text-success]="performance()!.totalProfitLoss >= 0"
                   [class.text-error]="performance()!.totalProfitLoss < 0">
                {{ performance()!.totalProfitLoss >= 0 ? '+' : '' }}£{{ performance()!.totalProfitLoss | number:'1.2-2' }}
              </div>
            </div>
            <div class="text-center">
              <div class="text-xs text-base-content/60 uppercase tracking-wide">Win Rate</div>
              <div class="text-xl font-bold text-primary mt-0.5">{{ performance()!.strikeRate | number:'1.1-1' }}%</div>
            </div>
            <div class="text-center">
              <div class="text-xs text-base-content/60 uppercase tracking-wide">Current Streak</div>
              <div class="text-xl font-bold mt-0.5"
                   [class.text-success]="performance()!.streakType === 'winning'"
                   [class.text-error]="performance()!.streakType === 'losing'">
                {{ performance()!.streakType === 'winning' ? '🔥' : '' }}{{ performance()!.currentStreak }}{{ performance()!.streakType === 'winning' ? 'W' : 'L' }}
              </div>
            </div>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .animate-fade-in {
      animation: fadeIn 0.4s ease-out;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(8px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `],
})
export class ProfitTrackerComponent implements OnInit {
  private readonly analyticsService = inject(AnalyticsService);

  readonly performance = signal<SubscriberPerformanceDto | null>(null);
  readonly currentMonthPnL = signal<number>(0);

  ngOnInit(): void {
    this.analyticsService.getSubscriberPerformance().subscribe({
      next: (data) => {
        this.performance.set(data);
        // Calculate current month P&L from monthly summaries
        const now = new Date();
        const currentMonth = data.monthlySummaries.find(
          (m) => m.year === now.getFullYear() && m.month === now.getMonth() + 1
        );
        this.currentMonthPnL.set(currentMonth?.profitLoss ?? 0);
      },
    });
  }
}
