import { ChangeDetectionStrategy, Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService, AuditLogEntry, AuditLogRequest } from '../../../../core/services/audit.service';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6 space-y-6">
      <h1 class="text-2xl font-bold">Audit Log</h1>
      <p class="text-sm text-base-content/60">
        Append-only audit trail. 2-year retention. Not editable by any user.
      </p>

      <!-- Search and Filters -->
      <div class="card bg-base-200 p-4">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div class="form-control">
            <label class="label" for="audit-search"><span class="label-text">Search</span></label>
            <input id="audit-search" type="text" class="input input-bordered input-sm"
              placeholder="Action, entity, actor..." [(ngModel)]="searchTerm"
              (input)="onSearchChange()" aria-label="Search audit logs" />
          </div>
          <div class="form-control">
            <label class="label" for="action-filter"><span class="label-text">Action Type</span></label>
            <select id="action-filter" class="select select-bordered select-sm" [(ngModel)]="actionTypeFilter" (change)="loadLogs()">
              <option value="">All Actions</option>
              <option value="UserSuspended">User Suspended</option>
              <option value="UserImpersonation">User Impersonation</option>
              <option value="BulkRoleChange">Bulk Role Change</option>
              <option value="RoleCreated">Role Created</option>
              <option value="RoleAssigned">Role Assigned</option>
              <option value="RoleRemoved">Role Removed</option>
              <option value="Login">Login</option>
            </select>
          </div>
          <div class="form-control">
            <label class="label" for="entity-filter"><span class="label-text">Target Entity</span></label>
            <select id="entity-filter" class="select select-bordered select-sm" [(ngModel)]="targetEntityFilter" (change)="loadLogs()">
              <option value="">All Entities</option>
              <option value="User">User</option>
              <option value="Role">Role</option>
              <option value="Plan">Plan</option>
              <option value="Subscription">Subscription</option>
            </select>
          </div>
          <div class="form-control">
            <label class="label" for="date-from"><span class="label-text">Date From</span></label>
            <input id="date-from" type="date" class="input input-bordered input-sm"
              [(ngModel)]="dateFrom" (change)="loadLogs()" />
          </div>
        </div>
      </div>

      <!-- Data Table -->
      @if (loading()) {
        <div class="space-y-2">
          @for (i of [1,2,3,4,5]; track i) {
            <div class="skeleton h-12 w-full"></div>
          }
        </div>
      } @else if (error()) {
        <div class="alert alert-error">
          <span>{{ error() }}</span>
          <button class="btn btn-sm btn-ghost" (click)="loadLogs()">Retry</button>
        </div>
      } @else {
        <div class="overflow-x-auto">
          <table class="table table-zebra w-full" aria-label="Audit log table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Actor</th>
                <th>Action</th>
                <th>Target Entity</th>
                <th>Target ID</th>
                <th>IP Address</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              @for (entry of entries(); track entry.id) {
                <tr>
                  <td class="whitespace-nowrap">{{ entry.timestamp | date:'medium' }}</td>
                  <td>{{ entry.actorName }}</td>
                  <td><span class="badge badge-sm badge-outline">{{ entry.actionType }}</span></td>
                  <td>{{ entry.targetEntity }}</td>
                  <td class="font-mono text-xs">{{ entry.targetEntityId ? entry.targetEntityId.slice(0, 8) + '...' : '—' }}</td>
                  <td class="font-mono text-xs">{{ entry.ipAddress || '—' }}</td>
                  <td>
                    @if (entry.beforeJson || entry.afterJson) {
                      <button class="btn btn-xs btn-ghost" (click)="toggleDetails(entry.id)">
                        {{ expandedEntry() === entry.id ? 'Hide' : 'Show' }}
                      </button>
                    }
                  </td>
                </tr>
                @if (expandedEntry() === entry.id) {
                  <tr>
                    <td colspan="7" class="bg-base-200">
                      <div class="grid grid-cols-2 gap-4 p-2">
                        @if (entry.beforeJson) {
                          <div>
                            <strong class="text-sm">Before:</strong>
                            <pre class="text-xs bg-base-300 p-2 rounded mt-1 overflow-auto max-h-40">{{ entry.beforeJson | json }}</pre>
                          </div>
                        }
                        @if (entry.afterJson) {
                          <div>
                            <strong class="text-sm">After:</strong>
                            <pre class="text-xs bg-base-300 p-2 rounded mt-1 overflow-auto max-h-40">{{ entry.afterJson | json }}</pre>
                          </div>
                        }
                      </div>
                    </td>
                  </tr>
                }
              } @empty {
                <tr>
                  <td colspan="7" class="text-center py-8">
                    <p class="text-base-content/60">No audit log entries found.</p>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="flex items-center justify-between mt-4">
          <span class="text-sm text-base-content/60">
            Showing {{ ((currentPage() - 1) * pageSize) + 1 }}–{{ Math.min(currentPage() * pageSize, totalCount()) }}
            of {{ totalCount() }} entries
          </span>
          <div class="join">
            <button class="join-item btn btn-sm" [disabled]="currentPage() <= 1" (click)="goToPage(currentPage() - 1)">«</button>
            @for (p of pageNumbers(); track p) {
              <button class="join-item btn btn-sm" [class.btn-active]="p === currentPage()" (click)="goToPage(p)">{{ p }}</button>
            }
            <button class="join-item btn btn-sm" [disabled]="currentPage() >= totalPages()" (click)="goToPage(currentPage() + 1)">»</button>
          </div>
        </div>
      }
    </section>
  `,
})
export class AuditLogComponent implements OnInit {
  private readonly auditService = inject(AuditService);
  protected readonly Math = Math;

  entries = signal<AuditLogEntry[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  currentPage = signal(1);
  loading = signal(false);
  error = signal<string | null>(null);
  expandedEntry = signal<number | null>(null);

  searchTerm = '';
  actionTypeFilter = '';
  targetEntityFilter = '';
  dateFrom = '';
  pageSize = 25;

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  });

  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.loading.set(true);
    this.error.set(null);

    const request: AuditLogRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize,
      search: this.searchTerm || undefined,
      actionTypeFilter: this.actionTypeFilter || undefined,
      targetEntityFilter: this.targetEntityFilter || undefined,
      dateFrom: this.dateFrom || undefined,
    };

    this.auditService.getAuditLogs(request).subscribe({
      next: (res) => {
        this.entries.set(res.entries);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load audit logs. Please try again.');
        this.loading.set(false);
      },
    });
  }

  onSearchChange(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadLogs();
    }, 300);
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.loadLogs();
  }

  toggleDetails(id: number): void {
    this.expandedEntry.set(this.expandedEntry() === id ? null : id);
  }
}
