import { ChangeDetectionStrategy, Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService, SubscriberPerformanceDto, MonthlyPerformanceSummaryDto } from '../../../../core/services/analytics.service';
import { ScrollRevealDirective } from '../../../../shared/directives/scroll-reveal.directive';

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [CommonModule, FormsModule, ScrollRevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-7xl mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold text-base-content mb-2">My Performance</h1>
      <p class="text-base-content/70 mb-6">Personal P&amp;L at level stakes for your subscribed categories</p>

      <!-- Streak & Performance Highlight Section (Feature 4) -->
      @if (performance()) {
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6" appScrollReveal revealAnimation="fade-up">
          <div class="stat bg-base-200 rounded-box shadow-sm p-4">
            <div class="stat-title text-xs uppercase tracking-wide">Current Streak</div>
            <div class="stat-value text-2xl"
                 [class.text-success]="performance()!.streakType === 'winning'"
                 [class.text-error]="performance()!.streakType === 'losing'">
              {{ performance()!.streakType === 'winning' ? '🔥 ' : '' }}{{ performance()!.currentStreak }}{{ performance()!.streakType === 'winning' ? 'W' : 'L' }}
            </div>
            <div class="stat-desc text-xs">
              {{ performance()!.currentStreak }} consecutive {{ performance()!.streakType === 'winning' ? 'winners' : 'losers' }}
            </div>
          </div>
          <div class="stat bg-base-200 rounded-box shadow-sm p-4">
            <div class="stat-title text-xs uppercase tracking-wide">This Month</div>
            <div class="stat-value text-2xl"
                 [class.text-success]="currentMonthPnL() >= 0"
                 [class.text-error]="currentMonthPnL() < 0">
              {{ currentMonthPnL() >= 0 ? '+' : '' }}£{{ currentMonthPnL() | number:'1.0-0' }}
            </div>
            <div class="stat-desc text-xs">
              {{ currentMonthWon() }} from {{ currentMonthTotal() }} ({{ currentMonthStrikeRate() | number:'1.0-0' }}%)
            </div>
          </div>
          <div class="stat bg-base-200 rounded-box shadow-sm p-4">
            <div class="stat-title text-xs uppercase tracking-wide">Best Month</div>
            <div class="stat-value text-2xl">
              {{ bestMonth() ? '+£' + (bestMonth()!.profitLoss | number:'1.0-0') : '—' }}
            </div>
            <div class="stat-desc text-xs">
              {{ bestMonth() ? bestMonthLabel() : 'No data yet' }}
            </div>
          </div>
          <div class="stat bg-base-200 rounded-box shadow-sm p-4">
            <div class="stat-title text-xs uppercase tracking-wide">All Time ROI</div>
            <div class="stat-value text-2xl text-accent"
                 [class.text-success]="allTimeRoi() >= 0"
                 [class.text-error]="allTimeRoi() < 0">
              {{ allTimeRoi() >= 0 ? '+' : '' }}{{ allTimeRoi() | number:'1.0-0' }}%
            </div>
            <div class="stat-desc text-xs">Since joining</div>
          </div>
        </div>
      }

      <!-- Filters -->
      <div class="flex flex-wrap gap-3 mb-6">
        <select class="select select-bordered select-sm" [(ngModel)]="selectedCategory"
                (change)="loadPerformance()" aria-label="Filter by category">
          <option value="">All Categories</option>
          @for (cat of performance()?.categoryBreakdown ?? []; track cat.categoryId) {
            <option [value]="cat.categoryId">{{ cat.categoryName }}</option>
          }
        </select>
        <input type="date" class="input input-bordered input-sm" [(ngModel)]="startDate"
               (change)="loadPerformance()" aria-label="Start date" />
        <input type="date" class="input input-bordered input-sm" [(ngModel)]="endDate"
               (change)="loadPerformance()" aria-label="End date" />
        <button class="btn btn-ghost btn-sm" (click)="clearFilters()">Clear</button>
      </div>

      @if (loading()) {
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          @for (i of [1,2,3]; track i) {
            <div class="card bg-base-200 animate-pulse h-24"></div>
          }
        </div>
      }

      @if (performance()) {
        <!-- Summary Cards -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8" appScrollReveal [revealDelay]="100">
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Total P&L</div>
            <div class="text-2xl font-bold" [class.text-success]="performance()!.totalProfitLoss >= 0"
                 [class.text-error]="performance()!.totalProfitLoss < 0">
              £{{ performance()!.totalProfitLoss | number:'1.2-2' }}
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Strike Rate</div>
            <div class="text-2xl font-bold text-primary">{{ performance()!.strikeRate | number:'1.1-1' }}%</div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Total Tips</div>
            <div class="text-2xl font-bold">{{ performance()!.totalTips }}</div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">W/L Record</div>
            <div class="text-2xl font-bold">
              <span class="text-success">{{ performance()!.won }}</span>/<span class="text-error">{{ performance()!.lost }}</span>
            </div>
          </div>
        </div>

        <!-- Streak Display -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="200">
          <h2 class="text-xl font-semibold mb-4">Streaks</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="text-center">
              <div class="text-sm text-base-content/60">Current Streak</div>
              <div class="text-3xl font-bold" [class.text-success]="performance()!.streakType === 'winning'"
                   [class.text-error]="performance()!.streakType === 'losing'">
                {{ performance()!.currentStreak }}
              </div>
              <div class="text-xs badge" [class.badge-success]="performance()!.streakType === 'winning'"
                   [class.badge-error]="performance()!.streakType === 'losing'">
                {{ performance()!.streakType }}
              </div>
            </div>
            <div class="text-center">
              <div class="text-sm text-base-content/60">Longest Win Streak</div>
              <div class="text-3xl font-bold text-success">{{ performance()!.longestWinStreak }}</div>
            </div>
            <div class="text-center">
              <div class="text-sm text-base-content/60">Longest Lose Streak</div>
              <div class="text-3xl font-bold text-error">{{ performance()!.longestLoseStreak }}</div>
            </div>
          </div>
        </div>

        <!-- Monthly Performance Summaries -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="300">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-semibold">Monthly Performance</h2>
            <button class="btn btn-outline btn-sm" (click)="sendDigest()"
                    [disabled]="sendingDigest()" aria-label="Send monthly email digest">
              {{ sendingDigest() ? 'Sending...' : '📧 Email Digest' }}
            </button>
          </div>
          <div class="overflow-x-auto">
            <table class="table table-sm">
              <thead>
                <tr>
                  <th>Month</th>
                  <th>P&L</th>
                  <th>Tips</th>
                  <th>Won</th>
                  <th>Lost</th>
                  <th>Strike Rate</th>
                  <th>ROI</th>
                </tr>
              </thead>
              <tbody>
                @for (month of performance()!.monthlySummaries; track month.year + '-' + month.month) {
                  <tr>
                    <td>{{ month.year }}-{{ month.month | number:'2.0-0' }}</td>
                    <td [class.text-success]="month.profitLoss >= 0" [class.text-error]="month.profitLoss < 0">
                      £{{ month.profitLoss | number:'1.2-2' }}
                    </td>
                    <td>{{ month.tipCount }}</td>
                    <td class="text-success">{{ month.won }}</td>
                    <td class="text-error">{{ month.lost }}</td>
                    <td>{{ month.strikeRate | number:'1.1-1' }}%</td>
                    <td [class.text-success]="month.roi >= 0" [class.text-error]="month.roi < 0">
                      {{ month.roi | number:'1.1-1' }}%
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- Category Breakdown -->
        @if (performance()!.categoryBreakdown.length > 0) {
          <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="400">
            <h2 class="text-xl font-semibold mb-4">Category Breakdown</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              @for (cat of performance()!.categoryBreakdown; track cat.categoryId) {
                <div class="card bg-base-200 p-4">
                  <h3 class="font-semibold mb-2">{{ cat.categoryName }}</h3>
                  <div class="space-y-1 text-sm">
                    <div class="flex justify-between">
                      <span>P&L</span>
                      <span class="font-bold" [class.text-success]="cat.profitLoss >= 0"
                            [class.text-error]="cat.profitLoss < 0">£{{ cat.profitLoss | number:'1.2-2' }}</span>
                    </div>
                    <div class="flex justify-between">
                      <span>Strike Rate</span>
                      <span class="font-bold">{{ cat.strikeRate | number:'1.1-1' }}%</span>
                    </div>
                    <div class="flex justify-between">
                      <span>ROI</span>
                      <span class="font-bold">{{ cat.roi | number:'1.1-1' }}%</span>
                    </div>
                    <div class="flex justify-between">
                      <span>Tips</span>
                      <span class="font-bold">{{ cat.tipCount }}</span>
                    </div>
                  </div>
                </div>
              }
            </div>
          </div>
        }
      }
    </section>
  `,
})
export class ResultsComponent implements OnInit {
  private readonly analyticsService = inject(AnalyticsService);

  readonly performance = signal<SubscriberPerformanceDto | null>(null);
  readonly loading = signal(true);
  readonly sendingDigest = signal(false);

  selectedCategory = '';
  startDate = '';
  endDate = '';

  // Computed metrics for the highlight section
  currentMonthPnL = computed(() => {
    const perf = this.performance();
    if (!perf) return 0;
    const now = new Date();
    const current = perf.monthlySummaries.find(
      (m) => m.year === now.getFullYear() && m.month === now.getMonth() + 1
    );
    return current?.profitLoss ?? 0;
  });

  currentMonthWon = computed(() => {
    const perf = this.performance();
    if (!perf) return 0;
    const now = new Date();
    const current = perf.monthlySummaries.find(
      (m) => m.year === now.getFullYear() && m.month === now.getMonth() + 1
    );
    return current?.won ?? 0;
  });

  currentMonthTotal = computed(() => {
    const perf = this.performance();
    if (!perf) return 0;
    const now = new Date();
    const current = perf.monthlySummaries.find(
      (m) => m.year === now.getFullYear() && m.month === now.getMonth() + 1
    );
    return current?.tipCount ?? 0;
  });

  currentMonthStrikeRate = computed(() => {
    const perf = this.performance();
    if (!perf) return 0;
    const now = new Date();
    const current = perf.monthlySummaries.find(
      (m) => m.year === now.getFullYear() && m.month === now.getMonth() + 1
    );
    return current?.strikeRate ?? 0;
  });

  bestMonth = computed<MonthlyPerformanceSummaryDto | null>(() => {
    const perf = this.performance();
    if (!perf || perf.monthlySummaries.length === 0) return null;
    return perf.monthlySummaries.reduce((best, m) =>
      m.profitLoss > (best?.profitLoss ?? -Infinity) ? m : best
    , perf.monthlySummaries[0]);
  });

  bestMonthLabel = computed(() => {
    const best = this.bestMonth();
    if (!best) return '';
    const date = new Date(best.year, best.month - 1);
    return date.toLocaleDateString('en-GB', { month: 'long', year: 'numeric' });
  });

  allTimeRoi = computed(() => {
    const perf = this.performance();
    if (!perf || perf.totalTips === 0) return 0;
    // ROI = (totalProfitLoss / totalStaked) * 100
    // Assume level stakes of 1 unit per tip
    return (perf.totalProfitLoss / perf.totalTips) * 100;
  });

  ngOnInit(): void {
    this.loadPerformance();
  }

  loadPerformance(): void {
    this.loading.set(true);
    this.analyticsService
      .getSubscriberPerformance(
        this.selectedCategory || undefined,
        this.startDate || undefined,
        this.endDate || undefined
      )
      .subscribe({
        next: (data) => {
          this.performance.set(data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  clearFilters(): void {
    this.selectedCategory = '';
    this.startDate = '';
    this.endDate = '';
    this.loadPerformance();
  }

  sendDigest(): void {
    this.sendingDigest.set(true);
    this.analyticsService.sendMonthlyDigest().subscribe({
      next: () => this.sendingDigest.set(false),
      error: () => this.sendingDigest.set(false),
    });
  }
}
