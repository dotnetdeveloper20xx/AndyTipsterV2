import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SubscriptionService, RevenueAnalytics, TransactionListResult, PaymentHistoryItem } from '../../../../core/services/subscription.service';

@Component({
  selector: 'app-paypal-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6">
      <!-- Environment Banner -->
      <div class="alert alert-warning mb-6">
        <span>⚠️ PayPal Environment: <strong>Sandbox</strong> — Transactions are not real</span>
      </div>

      <h1 class="text-3xl font-bold mb-6">PayPal Dashboard</h1>

      @if (loading()) {
        <div class="flex justify-center py-8"><span class="loading loading-spinner loading-lg"></span></div>
      } @else {
      <!-- Revenue Analytics Cards -->
      @if (analytics()) {
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div class="stat bg-base-100 shadow rounded-box">
            <div class="stat-title">MRR</div>
            <div class="stat-value text-primary">£{{ analytics()!.mrr | number:'1.2-2' }}</div>
          </div>
          <div class="stat bg-base-100 shadow rounded-box">
            <div class="stat-title">ARR</div>
            <div class="stat-value">£{{ analytics()!.arr | number:'1.2-2' }}</div>
          </div>
          <div class="stat bg-base-100 shadow rounded-box">
            <div class="stat-title">Churn Rate</div>
            <div class="stat-value text-error">{{ analytics()!.churnRate | number:'1.1-1' }}%</div>
          </div>
          <div class="stat bg-base-100 shadow rounded-box">
            <div class="stat-title">Active Subscribers</div>
            <div class="stat-value text-success">{{ analytics()!.activeSubscribers }}</div>
          </div>
        </div>

        <!-- Revenue by Plan -->
        <div class="card bg-base-100 shadow-xl mb-6">
          <div class="card-body">
            <h2 class="card-title">Revenue by Plan</h2>
            <div class="overflow-x-auto">
              <table class="table">
                <thead><tr><th>Plan</th><th>Monthly Revenue</th></tr></thead>
                <tbody>
                  @for (entry of revenueByPlanEntries(); track entry.key) {
                    <tr><td>{{ entry.key }}</td><td>£{{ entry.value | number:'1.2-2' }}</td></tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>
      }

      <!-- Transaction History -->
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title">Transaction History</h2>
          <!-- Filters -->
          <div class="flex flex-wrap gap-2 mt-2 mb-4">
            <input type="text" placeholder="Search..." class="input input-bordered input-sm"
              [(ngModel)]="searchTerm" (ngModelChange)="loadTransactions()" />
            <input type="date" class="input input-bordered input-sm" [(ngModel)]="startDate" (change)="loadTransactions()" />
            <input type="date" class="input input-bordered input-sm" [(ngModel)]="endDate" (change)="loadTransactions()" />
            <select class="select select-bordered select-sm" [(ngModel)]="statusFilter" (change)="loadTransactions()">
              <option value="">All Statuses</option>
              <option value="completed">Completed</option>
              <option value="succeeded">Succeeded</option>
              <option value="failed">Failed</option>
            </select>
            <button class="btn btn-sm btn-outline" (click)="exportCsv()">Export CSV</button>
          </div>

          @if (transactions()) {
            <div class="overflow-x-auto">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Date</th><th>Transaction ID</th><th>Amount</th><th>Fees</th><th>Net</th><th>Status</th><th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (tx of transactions()!.items; track tx.id) {
                    <tr>
                      <td>{{ tx.paidAt | date:'short' }}</td>
                      <td class="font-mono text-xs">{{ tx.id }}</td>
                      <td>{{ tx.currency }} {{ tx.amount | number:'1.2-2' }}</td>
                      <td>—</td>
                      <td>{{ tx.currency }} {{ tx.amount | number:'1.2-2' }}</td>
                      <td><span class="badge badge-sm" [class.badge-success]="tx.status === 'completed' || tx.status === 'succeeded'">{{ tx.status }}</span></td>
                      <td><button class="btn btn-xs btn-ghost" (click)="refund(tx)">Refund</button></td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            <!-- Pagination -->
            <div class="flex justify-between items-center mt-4">
              <span class="text-sm">{{ transactions()!.totalCount }} transactions</span>
              <div class="join">
                <button class="join-item btn btn-sm" [disabled]="page <= 1" (click)="page = page - 1; loadTransactions()">«</button>
                <button class="join-item btn btn-sm">Page {{ page }}</button>
                <button class="join-item btn btn-sm" (click)="page = page + 1; loadTransactions()">»</button>
              </div>
            </div>
          }
        </div>
      </div>
      }
    </section>
  `,
})
export class PayPalDashboardComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);

  analytics = signal<RevenueAnalytics | null>(null);
  transactions = signal<TransactionListResult | null>(null);
  revenueByPlanEntries = signal<{ key: string; value: number }[]>([]);
  loading = signal(true);

  searchTerm = '';
  startDate = '';
  endDate = '';
  statusFilter = '';
  page = 1;

  ngOnInit() {
    this.subscriptionService.getRevenueAnalytics().subscribe({
      next: (data) => {
        this.analytics.set(data);
        this.revenueByPlanEntries.set(
          Object.entries(data.revenueByPlan).map(([key, value]) => ({ key, value }))
        );
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
    this.loadTransactions();
  }

  loadTransactions() {
    this.subscriptionService.getTransactions({
      searchTerm: this.searchTerm || undefined,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      status: this.statusFilter || undefined,
      page: this.page,
      pageSize: 25,
    }).subscribe({
      next: (result) => this.transactions.set(result),
      error: () => {},
    });
  }

  refund(tx: PaymentHistoryItem) {
    if (confirm(`Refund transaction ${tx.id}?`)) {
      this.subscriptionService.processRefund(tx.id).subscribe({
        next: () => this.loadTransactions(),
        error: (err) => { alert(err.error?.errorMessage || 'Refund failed.'); },
      });
    }
  }

  exportCsv() {
    // Simple CSV export of visible data
    const items = this.transactions()?.items ?? [];
    const csv = 'Date,TransactionID,Amount,Currency,Status,Provider\n' +
      items.map(tx => `${tx.paidAt},${tx.id},${tx.amount},${tx.currency},${tx.status},${tx.provider}`).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'transactions.csv'; a.click();
    URL.revokeObjectURL(url);
  }
}
