import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService, PublicStatsDto } from '../../../../core/services/analytics.service';
import { ScrollRevealDirective } from '../../../../shared/directives/scroll-reveal.directive';
import { CounterAnimationDirective } from '../../../../shared/directives/counter-animation.directive';

@Component({
  selector: 'app-public-stats',
  standalone: true,
  imports: [CommonModule, ScrollRevealDirective, CounterAnimationDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-7xl mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold text-base-content mb-2">Performance Statistics</h1>
      <p class="text-base-content/70 mb-8">Verified results from all published tips</p>

      @if (loading()) {
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          @for (i of [1,2,3,4]; track i) {
            <div class="card bg-base-200 animate-pulse h-24"></div>
          }
        </div>
      }

      @if (stats()) {
        <!-- Summary Cards -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8" appScrollReveal>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Total P&L</div>
            <div class="text-2xl font-bold" [class.text-success]="stats()!.totalProfitLoss >= 0"
                 [class.text-error]="stats()!.totalProfitLoss < 0">
              <span appCounterAnimation [counterTarget]="stats()!.totalProfitLoss" [counterDecimals]="2"
                    counterPrefix="£"></span>
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Strike Rate</div>
            <div class="text-2xl font-bold text-primary">
              <span appCounterAnimation [counterTarget]="stats()!.strikeRate" [counterDecimals]="1"
                    counterSuffix="%"></span>
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">ROI</div>
            <div class="text-2xl font-bold" [class.text-success]="stats()!.roi >= 0"
                 [class.text-error]="stats()!.roi < 0">
              <span appCounterAnimation [counterTarget]="stats()!.roi" [counterDecimals]="1"
                    counterSuffix="%"></span>
            </div>
          </div>
          <div class="card bg-base-100 shadow-sm p-4">
            <div class="text-sm text-base-content/60">Total Tips</div>
            <div class="text-2xl font-bold">
              <span appCounterAnimation [counterTarget]="stats()!.totalTips"></span>
            </div>
          </div>
        </div>

        <!-- Last 30 Days Summary -->
        <div class="card bg-primary/10 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="100">
          <h2 class="text-xl font-semibold mb-4">Last 30 Days</h2>
          <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
            <div>
              <span class="text-sm text-base-content/60">P&L</span>
              <div class="text-lg font-bold" [class.text-success]="stats()!.last30Days.profitLoss >= 0"
                   [class.text-error]="stats()!.last30Days.profitLoss < 0">
                £{{ stats()!.last30Days.profitLoss | number:'1.2-2' }}
              </div>
            </div>
            <div>
              <span class="text-sm text-base-content/60">Strike Rate</span>
              <div class="text-lg font-bold">{{ stats()!.last30Days.strikeRate | number:'1.1-1' }}%</div>
            </div>
            <div>
              <span class="text-sm text-base-content/60">Tips</span>
              <div class="text-lg font-bold">{{ stats()!.last30Days.totalTips }} (W:{{ stats()!.last30Days.won }} L:{{ stats()!.last30Days.lost }})</div>
            </div>
          </div>
        </div>

        <!-- Profit Over Time Chart (simple bar representation) -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="200">
          <h2 class="text-xl font-semibold mb-4">Profit Over Time</h2>
          <div class="overflow-x-auto">
            <div class="flex items-end gap-1 h-48 min-w-[600px]">
              @for (point of stats()!.profitOverTime.slice(-30); track point.date) {
                <div class="flex-1 flex flex-col items-center justify-end h-full">
                  <div class="w-full rounded-t transition-all duration-300"
                       [class.bg-success]="point.dailyProfit >= 0"
                       [class.bg-error]="point.dailyProfit < 0"
                       [style.height.%]="getBarHeight(point.dailyProfit)">
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Win Rate Trends -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="300">
          <h2 class="text-xl font-semibold mb-4">Win Rate Trends</h2>
          <div class="overflow-x-auto">
            <table class="table table-sm">
              <thead>
                <tr>
                  <th>Period</th>
                  <th>Win Rate</th>
                  <th>Tips</th>
                </tr>
              </thead>
              <tbody>
                @for (trend of stats()!.winRateTrends; track trend.period) {
                  <tr>
                    <td>{{ trend.period }}</td>
                    <td>
                      <div class="flex items-center gap-2">
                        <progress class="progress progress-primary w-20" [value]="trend.winRate" max="100"></progress>
                        <span class="text-sm">{{ trend.winRate | number:'1.1-1' }}%</span>
                      </div>
                    </td>
                    <td>{{ trend.tipCount }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <!-- Category Comparison -->
        <div class="card bg-base-100 shadow-sm p-6 mb-8" appScrollReveal [revealDelay]="400">
          <h2 class="text-xl font-semibold mb-4">Category Comparison</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            @for (cat of stats()!.categoryComparisons; track cat.categoryId) {
              <div class="card bg-base-200 p-4">
                <h3 class="font-semibold mb-2">{{ cat.categoryName }}</h3>
                <div class="grid grid-cols-2 gap-2 text-sm">
                  <div>P&L: <span class="font-bold" [class.text-success]="cat.profitLoss >= 0"
                       [class.text-error]="cat.profitLoss < 0">£{{ cat.profitLoss | number:'1.2-2' }}</span></div>
                  <div>Strike: <span class="font-bold">{{ cat.strikeRate | number:'1.1-1' }}%</span></div>
                  <div>ROI: <span class="font-bold">{{ cat.roi | number:'1.1-1' }}%</span></div>
                  <div>Tips: <span class="font-bold">{{ cat.tipCount }}</span></div>
                </div>
              </div>
            }
          </div>
        </div>

        <!-- Export Buttons -->
        <div class="flex gap-3 mb-8" appScrollReveal [revealDelay]="500">
          <button class="btn btn-outline btn-sm" (click)="exportCsv()" aria-label="Export results as CSV">
            📄 Export CSV
          </button>
          <button class="btn btn-outline btn-sm" (click)="exportPdf()" aria-label="Export results as PDF">
            📑 Export PDF
          </button>
        </div>
      }

      @if (error()) {
        <div class="alert alert-error">
          <span>{{ error() }}</span>
          <button class="btn btn-sm btn-ghost" (click)="loadStats()">Retry</button>
        </div>
      }
    </section>
  `,
})
export class PublicStatsComponent implements OnInit {
  private readonly analyticsService = inject(AnalyticsService);

  readonly stats = signal<PublicStatsDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.loading.set(true);
    this.error.set(null);
    this.analyticsService.getPublicStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load performance statistics. Please try again.');
        this.loading.set(false);
      },
    });
  }

  getBarHeight(value: number): number {
    if (!this.stats()) return 0;
    const maxProfit = Math.max(
      ...this.stats()!.profitOverTime.slice(-30).map((p) => Math.abs(p.dailyProfit)),
      1
    );
    return Math.max(5, (Math.abs(value) / maxProfit) * 80);
  }

  exportCsv(): void {
    this.analyticsService.exportCsv().subscribe((blob) => {
      this.downloadBlob(blob, 'andytipster-results.csv');
    });
  }

  exportPdf(): void {
    this.analyticsService.exportPdf().subscribe((blob) => {
      this.downloadBlob(blob, 'andytipster-results.pdf');
    });
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}
