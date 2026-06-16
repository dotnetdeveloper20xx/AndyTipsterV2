import { ChangeDetectionStrategy, Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TipsService, TipDto, TipFilterDto } from '../../../../core/services/tips.service';
import { ProfitTrackerComponent } from '../../../../shared/components/profit-tracker/profit-tracker.component';
import { ScrollRevealDirective } from '../../../../shared/directives/scroll-reveal.directive';

interface DayGroup {
  date: string;
  label: string;
  tips: TipDto[];
  totalPnL: number;
  won: number;
  lost: number;
  pending: number;
  total: number;
}

@Component({
  selector: 'app-tips-feed',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ProfitTrackerComponent, ScrollRevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="max-w-4xl mx-auto px-4 py-6 space-y-6">

      <!-- Profit Tracker (Feature 2) -->
      <app-profit-tracker />

      <!-- Today Header -->
      <div class="animate-fade-in">
        <div class="flex items-baseline justify-between mb-1">
          <h1 class="text-2xl font-bold text-base-content font-[Poppins]">
            Today — {{ todayFormatted() }}
          </h1>
        </div>
        @if (todayGroup()) {
          <p class="text-sm text-base-content/70">
            {{ todayGroup()!.total }} {{ todayGroup()!.total === 1 ? 'selection' : 'selections' }}
            @if (todayGroup()!.won > 0) { • <span class="text-success font-medium">{{ todayGroup()!.won }} won</span> }
            @if (todayGroup()!.lost > 0) { • <span class="text-error/70">{{ todayGroup()!.lost }} lost</span> }
            @if (todayGroup()!.pending > 0) { • <span class="text-base-content/60">{{ todayGroup()!.pending }} pending</span> }
          </p>
        } @else {
          <p class="text-sm text-base-content/60">No selections today yet.</p>
        }
      </div>

      <!-- Today's P&L Banner -->
      @if (todayGroup() && todayGroup()!.total > 0) {
        <div class="card bg-[#F4F6F5] border border-base-200 shadow-sm animate-fade-in">
          <div class="card-body py-3 px-5">
            <div class="flex items-center justify-between">
              <span class="text-sm font-medium text-base-content/70">Today's P&L</span>
              <span class="text-xl font-bold"
                    [class.text-success]="todayGroup()!.totalPnL >= 0"
                    [class.text-error]="todayGroup()!.totalPnL < 0">
                {{ todayGroup()!.totalPnL >= 0 ? '+' : '' }}£{{ todayGroup()!.totalPnL | number:'1.2-2' }}
              </span>
            </div>
          </div>
        </div>
      }

      <!-- Loading -->
      @if (loading()) {
        <div class="flex justify-center py-12">
          <span class="loading loading-spinner loading-lg text-primary"></span>
        </div>
      }

      <!-- Today's Tips Cards -->
      @if (!loading() && todayGroup() && todayGroup()!.tips.length > 0) {
        <div class="space-y-3" appScrollReveal revealAnimation="fade-up">
          @for (tip of todayGroup()!.tips; track tip.id) {
            <div class="card bg-white shadow-sm border border-base-200 hover:shadow-md transition-shadow duration-200">
              <div class="card-body py-4 px-5">
                <div class="flex items-start gap-3">
                  <!-- Result Emoji -->
                  <div class="text-2xl mt-0.5 shrink-0">{{ getResultEmoji(tip) }}</div>

                  <!-- Main Content -->
                  <div class="flex-1 min-w-0">
                    <div class="flex items-start justify-between gap-2">
                      <div>
                        <h3 class="font-semibold text-base-content">
                          {{ getRaceTime(tip) }} {{ tip.raceName }}
                        </h3>
                        <p class="text-lg font-bold text-base-content mt-0.5">{{ tip.selection }}</p>
                      </div>
                    </div>

                    <!-- Result line -->
                    <div class="flex flex-wrap items-center gap-2 mt-2 text-sm">
                      @if (tip.result) {
                        <span class="font-semibold uppercase"
                              [class.text-success]="tip.result === 'Won'"
                              [class.text-error]="tip.result === 'Lost'"
                              [class.text-warning]="tip.result === 'Void' || tip.result === 'Push'">
                          {{ tip.result }}
                        </span>
                        <span class="text-base-content/60">at {{ tip.odds }}/1</span>
                      } @else {
                        <span class="text-base-content/60">Odds: {{ tip.odds }}/1</span>
                      }
                      <span class="text-base-content/40">•</span>
                      <span class="text-base-content/60">Stake: {{ tip.stake }}pt</span>

                      @if (tip.profitLoss != null) {
                        <span class="text-base-content/40">•</span>
                        <span class="font-semibold"
                              [class.text-success]="tip.profitLoss > 0"
                              [class.text-error]="tip.profitLoss < 0">
                          {{ tip.profitLoss > 0 ? '+' : '' }}£{{ tip.profitLoss | number:'1.2-2' }}
                        </span>
                      } @else {
                        <span class="text-base-content/40">•</span>
                        <span class="text-base-content/50 italic">Pending</span>
                      }
                    </div>

                    <!-- Commentary -->
                    @if (tip.commentary) {
                      <p class="mt-2 text-sm text-base-content/70 italic">"{{ tip.commentary }}"</p>
                    }
                  </div>
                </div>
              </div>
            </div>
          }
        </div>
      }

      <!-- No tips today but not loading -->
      @if (!loading() && (!todayGroup() || todayGroup()!.tips.length === 0) && !showPaywall()) {
        <div class="card bg-[#F4F6F5] border border-base-200 shadow-sm">
          <div class="card-body items-center text-center py-12">
            <span class="text-4xl mb-3">🏇</span>
            <p class="text-base-content/60">No tips published today yet. Check back later!</p>
          </div>
        </div>
      }

      <!-- Previous Days Section -->
      @if (!loading() && previousDays().length > 0) {
        <div class="mt-8" appScrollReveal [revealDelay]="150">
          <h2 class="text-lg font-semibold text-base-content mb-3">Previous Days</h2>
          <div class="space-y-2">
            @for (day of previousDays(); track day.date) {
              <div class="collapse collapse-arrow bg-white border border-base-200 shadow-sm rounded-lg">
                <input type="checkbox" />
                <div class="collapse-title py-3 px-5 min-h-0">
                  <div class="flex items-center justify-between">
                    <span class="font-medium text-base-content">{{ day.label }}</span>
                    <div class="flex items-center gap-3 text-sm">
                      <span class="text-base-content/70">{{ day.won }} from {{ day.total }}</span>
                      <span class="font-semibold"
                            [class.text-success]="day.totalPnL >= 0"
                            [class.text-error]="day.totalPnL < 0">
                        {{ day.totalPnL >= 0 ? '+' : '' }}£{{ day.totalPnL | number:'1.2-2' }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="collapse-content px-5 pb-4">
                  <div class="space-y-2 pt-2">
                    @for (tip of day.tips; track tip.id) {
                      <div class="flex items-center gap-3 py-2 border-b border-base-200 last:border-0">
                        <span class="text-lg">{{ getResultEmoji(tip) }}</span>
                        <div class="flex-1 min-w-0">
                          <span class="font-medium text-sm">{{ tip.selection }}</span>
                          <span class="text-xs text-base-content/60 ml-2">{{ tip.raceName }}</span>
                        </div>
                        <div class="text-right text-sm">
                          @if (tip.profitLoss != null) {
                            <span class="font-semibold"
                                  [class.text-success]="tip.profitLoss > 0"
                                  [class.text-error]="tip.profitLoss < 0">
                              {{ tip.profitLoss > 0 ? '+' : '' }}£{{ tip.profitLoss | number:'1.2-2' }}
                            </span>
                          } @else {
                            <span class="text-base-content/50">—</span>
                          }
                        </div>
                      </div>
                    }
                  </div>
                </div>
              </div>
            }
          </div>
        </div>
      }

      <!-- Paywall -->
      @if (showPaywall()) {
        <div class="card bg-white border border-base-200 shadow-lg mt-8">
          <div class="card-body items-center text-center">
            <h2 class="card-title text-xl">Unlock All Tips</h2>
            <p class="text-base-content/70">Subscribe to access our full range of tips and expert selections.</p>
            <div class="card-actions mt-4">
              <a routerLink="/pricing" class="btn btn-primary">View Plans</a>
            </div>
          </div>
        </div>
      }
    </section>
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
export class TipsFeedComponent implements OnInit {
  private readonly tipsService = inject(TipsService);

  allTips = signal<TipDto[]>([]);
  loading = signal(false);
  showPaywall = signal(false);

  // Computed: today formatted string
  todayFormatted = computed(() => {
    const now = new Date();
    return now.toLocaleDateString('en-GB', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    });
  });

  // Computed: today's date string (YYYY-MM-DD)
  private todayDateStr = computed(() => {
    const now = new Date();
    return now.toISOString().split('T')[0];
  });

  // Computed: group tips by day
  private dayGroups = computed(() => {
    const tips = this.allTips();
    const groups = new Map<string, TipDto[]>();

    for (const tip of tips) {
      const dateKey = tip.eventDate?.split('T')[0] ?? '';
      if (!groups.has(dateKey)) {
        groups.set(dateKey, []);
      }
      groups.get(dateKey)!.push(tip);
    }

    const result: DayGroup[] = [];
    const sortedKeys = [...groups.keys()].sort((a, b) => b.localeCompare(a));

    for (const dateKey of sortedKeys) {
      const dayTips = groups.get(dateKey)!;
      const won = dayTips.filter((t) => t.result === 'Won').length;
      const lost = dayTips.filter((t) => t.result === 'Lost').length;
      const pending = dayTips.filter((t) => !t.result).length;
      const totalPnL = dayTips.reduce((sum, t) => sum + (t.profitLoss ?? 0), 0);

      result.push({
        date: dateKey,
        label: this.getDayLabel(dateKey),
        tips: dayTips,
        totalPnL,
        won,
        lost,
        pending,
        total: dayTips.length,
      });
    }

    return result;
  });

  // Computed: today's group
  todayGroup = computed(() => {
    return this.dayGroups().find((g) => g.date === this.todayDateStr()) ?? null;
  });

  // Computed: previous days (last 7, excluding today)
  previousDays = computed(() => {
    return this.dayGroups()
      .filter((g) => g.date !== this.todayDateStr())
      .slice(0, 7);
  });

  ngOnInit(): void {
    this.loadFeed();
  }

  loadFeed(): void {
    this.loading.set(true);

    // Load last 8 days of tips
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 7);

    const filter: TipFilterDto = {
      page: 1,
      pageSize: 100,
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
    };

    this.tipsService.getTipsFeed(filter).subscribe({
      next: (res) => {
        this.allTips.set(res.items);
        this.loading.set(false);
        this.showPaywall.set(res.items.length === 0 && res.totalCount === 0);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 403) {
          this.showPaywall.set(true);
        }
      },
    });
  }

  getResultEmoji(tip: TipDto): string {
    switch (tip.result) {
      case 'Won': return '✅';
      case 'Lost': return '❌';
      case 'Void': return '⬜';
      case 'Push': return '⬜';
      default: return '⏳';
    }
  }

  getRaceTime(tip: TipDto): string {
    // Extract time from raceName if it starts with a time pattern (e.g., "3:15 Cheltenham")
    const match = tip.raceName?.match(/^(\d{1,2}:\d{2})/);
    if (match) return '';
    return '';
  }

  private getDayLabel(dateStr: string): string {
    const date = new Date(dateStr + 'T00:00:00');
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.getTime() === today.getTime()) {
      return 'Today';
    } else if (date.getTime() === yesterday.getTime()) {
      return 'Yesterday';
    } else {
      return date.toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'short' });
    }
  }
}
