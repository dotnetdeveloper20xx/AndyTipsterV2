/**
 * UserManagementComponent
 *
 * Pattern: Admin pages use signals + ngModel for simple filter/search bindings.
 * Auth pages use ReactiveFormsModule for complex forms with validation.
 * The DataTable component handles search, sort, filter, and pagination via its outputs.
 */
import { ChangeDetectionStrategy, Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  AdminUsersService,
  UserSummary,
  UserListRequest,
  BulkActionResponse,
  ImpersonateResponse,
} from '../../../../core/services/admin-users.service';
import {
  DataTableComponent,
  DataTableColumn,
  PageChangeEvent,
  SortState,
  FilterState,
  BulkAction,
  BulkActionEvent,
} from '../../../../shared/components/data-table/index';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="p-6 space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-bold">User Management</h1>
        <button class="btn btn-primary btn-sm" (click)="exportUsers()" [disabled]="exporting()">
          {{ exporting() ? 'Exporting...' : 'Export CSV' }}
        </button>
      </div>

      <!-- Impersonation Banner -->
      @if (impersonation()) {
        <div class="alert alert-warning shadow-lg" role="alert">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          <span>Impersonating: <strong>{{ impersonation()!.impersonatedUserName }}</strong> ({{ impersonation()!.impersonatedUserEmail }})</span>
          <button class="btn btn-sm btn-ghost" (click)="endImpersonation()">End Impersonation</button>
        </div>
      }

      <!-- Bulk Action Confirmation Dialog -->
      @if (bulkConfirmation()) {
        <div class="modal modal-open" role="dialog" aria-modal="true" aria-labelledby="bulk-dialog-title">
          <div class="modal-box">
            <h3 class="font-bold text-lg" id="bulk-dialog-title">Confirm Bulk Action</h3>
            <p class="py-4">
              Are you sure you want to <strong>{{ bulkConfirmation() }}</strong> {{ selectedUsers().length }} user(s)?
            </p>
            @if (bulkConfirmation() === 'role_change') {
              <div class="form-control">
                <label class="label" for="bulk-role"><span class="label-text">New Role</span></label>
                <select id="bulk-role" class="select select-bordered" [(ngModel)]="bulkRoleName">
                  <option value="Subscriber">Subscriber</option>
                  <option value="Free User">Free User</option>
                  <option value="Moderator">Moderator</option>
                </select>
              </div>
            }
            <div class="modal-action">
              <button class="btn btn-ghost" (click)="cancelBulkAction()">Cancel</button>
              <button class="btn btn-primary" (click)="executeBulkAction()">Confirm</button>
            </div>
          </div>
          <div class="modal-backdrop" (click)="cancelBulkAction()"></div>
        </div>
      }

      <!-- Data Table -->
      <app-data-table
        [columns]="columns"
        [data]="users()"
        [loading]="loading()"
        [error]="error()"
        [totalCount]="totalCount()"
        [pageSize]="pageSize"
        [selectable]="true"
        [exportable]="false"
        [bulkActions]="tableBulkActions"
        (pageChange)="onPageChange($event)"
        (sortChange)="onSortChange($event)"
        (filterChange)="onFilterChange($event)"
        (selectionChange)="onSelectionChange($event)"
        (search)="onSearch($event)"
        (bulkAction)="onBulkAction($event)"
        (retry)="loadUsers()"
      />
    </section>
  `,
})
export class UserManagementComponent implements OnInit {
  private readonly usersService = inject(AdminUsersService);

  // State
  users = signal<UserSummary[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);
  exporting = signal(false);
  impersonation = signal<ImpersonateResponse | null>(null);
  bulkConfirmation = signal<string | null>(null);

  // Filters & Sort
  private searchTerm = '';
  private roleFilter = '';
  private statusFilter = '';
  private currentSort = 'createdAt';
  private currentSortDir: 'asc' | 'desc' = 'desc';
  private currentPage = 1;
  pageSize = 25;
  bulkRoleName = 'Subscriber';

  // Selection
  private selectedSet = signal<UserSummary[]>([]);
  selectedUsers = computed(() => this.selectedSet());

  // Table column definitions
  columns: DataTableColumn<UserSummary>[] = [
    {
      field: 'displayName',
      header: 'Name',
      sortable: true,
      filterable: true,
      filterType: 'text',
    },
    {
      field: 'email',
      header: 'Email',
      sortable: true,
      filterable: true,
      filterType: 'text',
    },
    {
      field: 'roles',
      header: 'Roles',
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Super Admin', value: 'Super Admin' },
        { label: 'Admin', value: 'Admin' },
        { label: 'Moderator', value: 'Moderator' },
        { label: 'Subscriber', value: 'Subscriber' },
        { label: 'Free User', value: 'Free User' },
      ],
      render: (row) => row.roles?.join(', ') ?? '',
    },
    {
      field: 'status',
      header: 'Status',
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Active', value: 'active' },
        { label: 'Suspended', value: 'suspended' },
        { label: 'Unverified', value: 'unverified' },
      ],
    },
    {
      field: 'plan',
      header: 'Plan',
      render: (row) => row.plan || '—',
    },
    {
      field: 'createdAt',
      header: 'Registration Date',
      sortable: true,
      render: (row) => row.createdAt ? new Date(row.createdAt).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : '',
    },
  ];

  // Bulk actions shown in the data table
  tableBulkActions: BulkAction[] = [
    { id: 'suspend', label: 'Suspend', variant: 'warning' },
    { id: 'role_change', label: 'Change Role', variant: 'secondary' },
  ];

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    const request: UserListRequest = {
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchTerm || undefined,
      roleFilter: this.roleFilter || undefined,
      statusFilter: this.statusFilter || undefined,
      sortBy: this.currentSort,
      sortDirection: this.currentSortDir,
    };

    this.usersService.getUsers(request).subscribe({
      next: (res) => {
        this.users.set(res.users);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load users. Please try again.');
        this.loading.set(false);
      },
    });
  }

  // DataTable event handlers
  onPageChange(event: PageChangeEvent): void {
    this.currentPage = event.page;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  onSortChange(sorts: SortState[]): void {
    if (sorts.length > 0) {
      this.currentSort = sorts[0].column;
      this.currentSortDir = sorts[0].direction;
    } else {
      this.currentSort = 'createdAt';
      this.currentSortDir = 'desc';
    }
    this.loadUsers();
  }

  onFilterChange(filters: FilterState[]): void {
    this.roleFilter = '';
    this.statusFilter = '';
    for (const f of filters) {
      if (f.column === 'roles') this.roleFilter = f.value;
      if (f.column === 'status') this.statusFilter = f.value;
    }
    this.currentPage = 1;
    this.loadUsers();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadUsers();
  }

  onSelectionChange(selected: UserSummary[]): void {
    this.selectedSet.set(selected);
  }

  onBulkAction(event: BulkActionEvent): void {
    this.bulkConfirmation.set(event.action);
  }

  // Bulk Actions
  cancelBulkAction(): void {
    this.bulkConfirmation.set(null);
  }

  executeBulkAction(): void {
    const action = this.bulkConfirmation();
    if (!action) return;

    const userIds = this.selectedUsers().map(u => u.id);
    this.usersService.bulkAction({
      userIds,
      action,
      roleName: action === 'role_change' ? this.bulkRoleName : undefined,
    }).subscribe({
      next: () => {
        this.bulkConfirmation.set(null);
        this.selectedSet.set([]);
        this.loadUsers();
      },
      error: () => {
        this.bulkConfirmation.set(null);
      },
    });
  }

  // Impersonation
  impersonateUser(id: string): void {
    this.usersService.impersonateUser(id).subscribe({
      next: (response) => {
        this.impersonation.set(response);
        sessionStorage.setItem('impersonationToken', response.impersonationToken);
      },
    });
  }

  endImpersonation(): void {
    this.impersonation.set(null);
    sessionStorage.removeItem('impersonationToken');
  }

  // Export
  exportUsers(): void {
    this.exporting.set(true);
    this.usersService.exportUsers({
      search: this.searchTerm || undefined,
      roleFilter: this.roleFilter || undefined,
      statusFilter: this.statusFilter || undefined,
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `users-export-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.exporting.set(false);
      },
      error: () => this.exporting.set(false),
    });
  }
}
