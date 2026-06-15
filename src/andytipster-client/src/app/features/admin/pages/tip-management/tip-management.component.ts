import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  TipsService,
  TipDto,
  CategoryDto,
  CreateTipDto,
  BulkImportResultDto,
  TipFilterDto,
  PnLSummaryDto
} from '../../../../core/services/tips.service';

@Component({
  selector: 'app-tip-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6 space-y-6">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-bold">Tip Management</h1>
        <div class="flex gap-2">
          <button class="btn btn-outline btn-sm" (click)="showImportModal = true">CSV Import</button>
          <button class="btn btn-primary btn-sm" (click)="openCreateForm()">Create Tip</button>
        </div>
      </div>

      <!-- Filters -->
      <div class="flex flex-wrap gap-3 items-end">
        <select class="select select-bordered select-sm" [(ngModel)]="filter.status" (change)="loadTips()">
          <option value="">All Statuses</option>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Archived">Archived</option>
        </select>
        <select class="select select-bordered select-sm" [(ngModel)]="filter.categoryId" (change)="loadTips()">
          <option value="">All Categories</option>
          @for (cat of categories(); track cat.id) {
            <option [value]="cat.id">{{ cat.name }}</option>
          }
        </select>
        <select class="select select-bordered select-sm" [(ngModel)]="filter.result" (change)="loadTips()">
          <option value="">All Results</option>
          <option value="Won">Won</option>
          <option value="Lost">Lost</option>
          <option value="Void">Void</option>
          <option value="Push">Push</option>
        </select>
        <button class="btn btn-ghost btn-sm" (click)="loadPnL()">View P&L</button>
      </div>

      <!-- P&L Summary -->
      @if (pnlSummary()) {
        <div class="stats shadow w-full">
          <div class="stat">
            <div class="stat-title">Total P&L</div>
            <div class="stat-value" [class.text-success]="pnlSummary()!.totalProfitLoss > 0"
                 [class.text-error]="pnlSummary()!.totalProfitLoss < 0">
              {{ pnlSummary()!.totalProfitLoss | number:'1.2-2' }}
            </div>
          </div>
          <div class="stat">
            <div class="stat-title">Strike Rate</div>
            <div class="stat-value">{{ pnlSummary()!.strikeRate }}%</div>
          </div>
          <div class="stat">
            <div class="stat-title">Won/Lost</div>
            <div class="stat-value text-sm">{{ pnlSummary()!.won }}W / {{ pnlSummary()!.lost }}L / {{ pnlSummary()!.void }}V</div>
          </div>
          <div class="stat">
            <div class="stat-title">Total Tips</div>
            <div class="stat-value">{{ pnlSummary()!.totalTips }}</div>
          </div>
        </div>
      }

      <!-- Tips Table -->
      <div class="overflow-x-auto">
        <table class="table table-zebra w-full">
          <thead>
            <tr>
              <th>Date</th>
              <th>Race</th>
              <th>Selection</th>
              <th>Odds</th>
              <th>Stake</th>
              <th>Category</th>
              <th>Status</th>
              <th>Result</th>
              <th>P&L</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (tip of tips(); track tip.id) {
              <tr>
                <td>{{ tip.eventDate | date:'shortDate' }}</td>
                <td>{{ tip.raceName }}</td>
                <td>{{ tip.selection }}</td>
                <td>{{ tip.odds }}</td>
                <td>{{ tip.stake }}</td>
                <td>{{ tip.categoryName }}</td>
                <td>
                  <span class="badge" [class.badge-ghost]="tip.status === 'Draft'"
                        [class.badge-success]="tip.status === 'Published'"
                        [class.badge-neutral]="tip.status === 'Archived'">
                    {{ tip.status }}
                  </span>
                </td>
                <td>
                  @if (tip.result) {
                    <span class="badge" [class.badge-success]="tip.result === 'Won'"
                          [class.badge-error]="tip.result === 'Lost'"
                          [class.badge-warning]="tip.result === 'Void' || tip.result === 'Push'">
                      {{ tip.result }}
                    </span>
                  }
                </td>
                <td [class.text-success]="(tip.profitLoss ?? 0) > 0"
                    [class.text-error]="(tip.profitLoss ?? 0) < 0">
                  {{ tip.profitLoss != null ? (tip.profitLoss | number:'1.2-2') : '-' }}
                </td>
                <td>
                  <div class="flex gap-1">
                    @if (tip.status === 'Draft') {
                      <button class="btn btn-xs btn-success" (click)="publishTip(tip.id)">Publish</button>
                      <button class="btn btn-xs btn-ghost" (click)="deleteTip(tip.id)">Delete</button>
                    }
                    @if (tip.status === 'Published' && !tip.result) {
                      <button class="btn btn-xs btn-info" (click)="openResultModal(tip)">Record Result</button>
                      <button class="btn btn-xs btn-neutral" (click)="archiveTip(tip.id)">Archive</button>
                    }
                    @if (tip.status === 'Published' && tip.result) {
                      <button class="btn btn-xs btn-neutral" (click)="archiveTip(tip.id)">Archive</button>
                    }
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="flex justify-between items-center">
        <span class="text-sm opacity-70">Total: {{ totalCount() }}</span>
        <div class="join">
          <button class="join-item btn btn-sm" [disabled]="filter.page === 1" (click)="filter.page = filter.page! - 1; loadTips()">«</button>
          <button class="join-item btn btn-sm">Page {{ filter.page }}</button>
          <button class="join-item btn btn-sm" [disabled]="tips().length < filter.pageSize!" (click)="filter.page = filter.page! + 1; loadTips()">»</button>
        </div>
      </div>

      <!-- Create/Edit Modal -->
      @if (showCreateForm) {
        <div class="modal modal-open">
          <div class="modal-box max-w-lg">
            <h3 class="font-bold text-lg">Create Tip</h3>
            <div class="form-control mt-4 space-y-3">
              <label class="label"><span class="label-text">Event Date</span></label>
              <input type="date" class="input input-bordered" [(ngModel)]="newTip.eventDate" />

              <label class="label"><span class="label-text">Race Name</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newTip.raceName" maxlength="200" placeholder="e.g. 3:15 Cheltenham" />

              <label class="label"><span class="label-text">Selection</span></label>
              <input type="text" class="input input-bordered" [(ngModel)]="newTip.selection" maxlength="200" placeholder="e.g. Lucky Star" />

              <label class="label"><span class="label-text">Odds</span></label>
              <input type="number" class="input input-bordered" [(ngModel)]="newTip.odds" min="1.01" max="1000" step="0.01" />

              <label class="label"><span class="label-text">Stake (1-10)</span></label>
              <input type="number" class="input input-bordered" [(ngModel)]="newTip.stake" min="1" max="10" />

              <label class="label"><span class="label-text">Category</span></label>
              <select class="select select-bordered" [(ngModel)]="newTip.categoryId">
                @for (cat of categories(); track cat.id) {
                  <option [value]="cat.id">{{ cat.name }}</option>
                }
              </select>

              <label class="label"><span class="label-text">Commentary (optional)</span></label>
              <textarea class="textarea textarea-bordered" [(ngModel)]="newTip.commentary" maxlength="5000" rows="3"></textarea>
            </div>
            @if (formError()) {
              <div class="alert alert-error mt-3"><span>{{ formError() }}</span></div>
            }
            <div class="modal-action">
              <button class="btn" (click)="showCreateForm = false">Cancel</button>
              <button class="btn btn-primary" (click)="createTip()">Create</button>
            </div>
          </div>
        </div>
      }

      <!-- Result Modal -->
      @if (showResultModal) {
        <div class="modal modal-open">
          <div class="modal-box">
            <h3 class="font-bold text-lg">Record Result for: {{ selectedTip?.selection }}</h3>
            <div class="flex gap-2 mt-4">
              <button class="btn btn-success" (click)="recordResult('Won')">Won</button>
              <button class="btn btn-error" (click)="recordResult('Lost')">Lost</button>
              <button class="btn btn-warning" (click)="recordResult('Void')">Void</button>
              <button class="btn btn-info" (click)="recordResult('Push')">Push</button>
            </div>
            <div class="modal-action">
              <button class="btn" (click)="showResultModal = false">Cancel</button>
            </div>
          </div>
        </div>
      }

      <!-- CSV Import Modal -->
      @if (showImportModal) {
        <div class="modal modal-open">
          <div class="modal-box">
            <h3 class="font-bold text-lg">Bulk Import Tips (CSV)</h3>
            <p class="text-sm opacity-70 mt-2">Format: EventDate, RaceName, Selection, Odds, Stake, Category, Commentary (optional)</p>
            <p class="text-sm opacity-70">Max 500 rows, 5MB file size.</p>
            <input type="file" class="file-input file-input-bordered w-full mt-4" accept=".csv" (change)="onFileSelect($event)" />
            @if (importResult()) {
              <div class="mt-3 p-3 bg-base-200 rounded">
                <p>Imported: {{ importResult()!.successCount }}/{{ importResult()!.totalRows }}</p>
                @if (importResult()!.errorCount > 0) {
                  <p class="text-error">Errors: {{ importResult()!.errorCount }} rows</p>
                  <ul class="text-sm mt-1">
                    @for (err of importResult()!.errors.slice(0, 10); track err.rowNumber + err.field) {
                      <li>Row {{ err.rowNumber }}: {{ err.field }} - {{ err.error }}</li>
                    }
                  </ul>
                }
              </div>
            }
            <div class="modal-action">
              <button class="btn" (click)="showImportModal = false; importResult.set(null)">Close</button>
              <button class="btn btn-primary" [disabled]="!selectedFile" (click)="importCsv()">Import</button>
            </div>
          </div>
        </div>
      }
    </section>
  `,
})
export class TipManagementComponent implements OnInit {
  private readonly tipsService = inject(TipsService);

  tips = signal<TipDto[]>([]);
  totalCount = signal(0);
  categories = signal<CategoryDto[]>([]);
  pnlSummary = signal<PnLSummaryDto | null>(null);
  formError = signal<string | null>(null);
  importResult = signal<BulkImportResultDto | null>(null);

  filter: TipFilterDto = { page: 1, pageSize: 25, status: '', categoryId: '', result: '' };
  showCreateForm = false;
  showResultModal = false;
  showImportModal = false;
  selectedTip: TipDto | null = null;
  selectedFile: File | null = null;

  newTip: CreateTipDto = { eventDate: '', raceName: '', selection: '', odds: 0, stake: 1, categoryId: '' };

  ngOnInit(): void {
    this.loadTips();
    this.loadCategories();
  }

  loadTips(): void {
    this.tipsService.getTips(this.filter).subscribe({
      next: (res) => {
        this.tips.set(res.items);
        this.totalCount.set(res.totalCount);
      }
    });
  }

  loadCategories(): void {
    this.tipsService.getCategories(true).subscribe({
      next: (cats) => this.categories.set(cats)
    });
  }

  loadPnL(): void {
    this.tipsService.getPnLSummary(undefined, undefined, this.filter.categoryId || undefined).subscribe({
      next: (summary) => this.pnlSummary.set(summary)
    });
  }

  openCreateForm(): void {
    this.newTip = { eventDate: '', raceName: '', selection: '', odds: 0, stake: 1, categoryId: this.categories()[0]?.id ?? '' };
    this.formError.set(null);
    this.showCreateForm = true;
  }

  createTip(): void {
    this.formError.set(null);
    this.tipsService.createTip(this.newTip).subscribe({
      next: () => {
        this.showCreateForm = false;
        this.loadTips();
      },
      error: (err) => {
        const detail = err.error?.detail || err.error?.message || 'Validation failed.';
        this.formError.set(detail);
      }
    });
  }

  publishTip(tipId: string): void {
    this.tipsService.publishTip(tipId).subscribe({ next: () => this.loadTips() });
  }

  archiveTip(tipId: string): void {
    this.tipsService.archiveTip(tipId).subscribe({ next: () => this.loadTips() });
  }

  deleteTip(tipId: string): void {
    if (confirm('Delete this tip?')) {
      this.tipsService.deleteTip(tipId).subscribe({ next: () => this.loadTips() });
    }
  }

  openResultModal(tip: TipDto): void {
    this.selectedTip = tip;
    this.showResultModal = true;
  }

  recordResult(result: string): void {
    if (!this.selectedTip) return;
    this.tipsService.recordResult(this.selectedTip.id, result).subscribe({
      next: () => {
        this.showResultModal = false;
        this.loadTips();
      }
    });
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  importCsv(): void {
    if (!this.selectedFile) return;
    this.tipsService.bulkImport(this.selectedFile).subscribe({
      next: (result) => {
        this.importResult.set(result);
        this.loadTips();
      },
      error: (err) => {
        this.importResult.set({ totalRows: 0, successCount: 0, errorCount: 1, errors: [{ rowNumber: 0, field: '', error: err.error?.detail || 'Import failed.' }] });
      }
    });
  }
}
