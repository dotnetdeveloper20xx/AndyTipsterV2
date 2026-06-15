import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TipsService, TipDto, CategoryDto, TipFilterDto } from '../../../../core/services/tips.service';

@Component({
  selector: 'app-tips-feed',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6 space-y-6">
      <h1 class="text-2xl font-bold">Tips Feed</h1>

      <!-- Tip of the Day -->
      @if (tipOfTheDay()) {
        <div class="card bg-gradient-to-r from-primary/10 to-secondary/10 shadow-md">
          <div class="card-body">
            <div class="flex items-center gap-2">
              <span class="badge badge-primary">Tip of the Day</span>
              <span class="text-sm opacity-70">Free Preview</span>
            </div>
            <h2 class="card-title mt-2">{{ tipOfTheDay()!.selection }}</h2>
            <p class="text-sm">{{ tipOfTheDay()!.raceName }} • {{ tipOfTheDay()!.eventDate | date:'mediumDate' }}</p>
            <div class="flex gap-4 mt-2">
              <span class="font-semibold">Odds: {{ tipOfTheDay()!.odds }}</span>
              <span>Stake: {{ tipOfTheDay()!.stake }}/10</span>
              <span class="badge badge-ghost">{{ tipOfTheDay()!.categoryName }}</span>
            </div>
            @if (tipOfTheDay()!.commentary) {
              <p class="mt-2 text-sm opacity-80">{{ tipOfTheDay()!.commentary }}</p>
            }
            @if (tipOfTheDay()!.result) {
              <span class="badge mt-2" [class.badge-success]="tipOfTheDay()!.result === 'Won'"
                    [class.badge-error]="tipOfTheDay()!.result === 'Lost'">
                {{ tipOfTheDay()!.result }}
              </span>
            }
          </div>
        </div>
      }

      <!-- Filters -->
      <div class="flex flex-wrap gap-3 items-end">
        <select class="select select-bordered select-sm" [(ngModel)]="filter.categoryId" (change)="loadFeed()">
          <option value="">All Categories</option>
          @for (cat of categories(); track cat.id) {
            <option [value]="cat.id">{{ cat.name }}</option>
          }
        </select>
        <select class="select select-bordered select-sm" [(ngModel)]="filter.result" (change)="loadFeed()">
          <option value="">All Results</option>
          <option value="Won">Won</option>
          <option value="Lost">Lost</option>
          <option value="Void">Void</option>
          <option value="Push">Push</option>
        </select>
        <input type="date" class="input input-bordered input-sm" [(ngModel)]="filter.startDate" (change)="loadFeed()" placeholder="From" />
        <input type="date" class="input input-bordered input-sm" [(ngModel)]="filter.endDate" (change)="loadFeed()" placeholder="To" />
      </div>

      <!-- Tips List -->
      @if (tips().length === 0 && !loading()) {
        <div class="text-center py-12">
          <p class="text-lg opacity-60">No tips available for your subscription.</p>
          <a routerLink="/dashboard/billing" class="btn btn-primary btn-sm mt-4">Upgrade Plan</a>
        </div>
      }

      <div class="grid gap-4">
        @for (tip of tips(); track tip.id) {
          <div class="card bg-base-100 shadow">
            <div class="card-body py-4">
              <div class="flex items-start justify-between">
                <div>
                  <h3 class="font-semibold text-lg">{{ tip.selection }}</h3>
                  <p class="text-sm opacity-70">{{ tip.raceName }} • {{ tip.eventDate | date:'mediumDate' }}</p>
                </div>
                <div class="text-right">
                  @if (tip.result) {
                    <span class="badge" [class.badge-success]="tip.result === 'Won'"
                          [class.badge-error]="tip.result === 'Lost'"
                          [class.badge-warning]="tip.result === 'Void' || tip.result === 'Push'">
                      {{ tip.result }}
                    </span>
                    <p class="text-sm mt-1" [class.text-success]="(tip.profitLoss ?? 0) > 0"
                       [class.text-error]="(tip.profitLoss ?? 0) < 0">
                      {{ tip.profitLoss != null ? (tip.profitLoss > 0 ? '+' : '') + (tip.profitLoss | number:'1.2-2') : '' }}
                    </p>
                  }
                </div>
              </div>
              <div class="flex gap-4 mt-2 text-sm">
                <span><strong>Odds:</strong> {{ tip.odds }}</span>
                <span><strong>Stake:</strong> {{ tip.stake }}/10</span>
                <span class="badge badge-ghost badge-sm">{{ tip.categoryName }}</span>
              </div>
              @if (tip.commentary) {
                <p class="mt-2 text-sm opacity-80">{{ tip.commentary }}</p>
              }
            </div>
          </div>
        }
      </div>

      <!-- Pagination -->
      @if (totalCount() > 0) {
        <div class="flex justify-between items-center">
          <span class="text-sm opacity-70">Showing {{ tips().length }} of {{ totalCount() }}</span>
          <div class="join">
            <button class="join-item btn btn-sm" [disabled]="filter.page === 1" (click)="filter.page = filter.page! - 1; loadFeed()">«</button>
            <button class="join-item btn btn-sm">Page {{ filter.page }}</button>
            <button class="join-item btn btn-sm" [disabled]="tips().length < filter.pageSize!" (click)="filter.page = filter.page! + 1; loadFeed()">»</button>
          </div>
        </div>
      }

      <!-- Paywall -->
      @if (showPaywall()) {
        <div class="card bg-base-200 shadow-lg">
          <div class="card-body items-center text-center">
            <h2 class="card-title">Unlock All Tips</h2>
            <p>Subscribe to access our full range of tips and expert selections.</p>
            <div class="card-actions mt-4">
              <a routerLink="/pricing" class="btn btn-primary">View Plans</a>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class TipsFeedComponent implements OnInit {
  private readonly tipsService = inject(TipsService);

  tips = signal<TipDto[]>([]);
  totalCount = signal(0);
  tipOfTheDay = signal<TipDto | null>(null);
  categories = signal<CategoryDto[]>([]);
  loading = signal(false);
  showPaywall = signal(false);

  filter: TipFilterDto = { page: 1, pageSize: 20, categoryId: '', result: '' };

  ngOnInit(): void {
    this.loadFeed();
    this.loadTipOfTheDay();
    this.loadCategories();
  }

  loadFeed(): void {
    this.loading.set(true);
    this.tipsService.getTipsFeed(this.filter).subscribe({
      next: (res) => {
        this.tips.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
        this.showPaywall.set(res.items.length === 0 && res.totalCount === 0);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 403) {
          this.showPaywall.set(true);
        }
      }
    });
  }

  loadTipOfTheDay(): void {
    this.tipsService.getTipOfTheDay().subscribe({
      next: (tip) => this.tipOfTheDay.set(tip),
      error: () => {} // Silently handle if no tip of the day
    });
  }

  loadCategories(): void {
    this.tipsService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats)
    });
  }
}
