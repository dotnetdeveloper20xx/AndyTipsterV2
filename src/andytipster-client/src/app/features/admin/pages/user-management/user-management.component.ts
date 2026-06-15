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

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

      <!-- Search and Filters -->
      <div class="card bg-base-200 p-4">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div class="form-control">
            <label class="label" for="search-input"><span class="label-text">Search</span></label>
            <input id="search-input" type="text" class="input input-bordered input-sm"
              placeholder="Name or email..." [(ngModel)]="searchTerm"
              (input)="onSearchChange()" aria-label="Search users by name or email" />
          </div>
          <div class="form-control">
            <label class="label" for="role-filter"><span class="label-text">Role</span></label>
            <select id="role-filter" class="select select-bordered select-sm" [(ngModel)]="roleFilter" (change)="loadUsers()">
              <option value="">All Roles</option>
              <option value="Super Admin">Super Admin</option>
              <option value="Admin">Admin</option>
              <option value="Moderator">Moderator</option>
              <option value="Subscriber">Subscriber</option>
              <option value="Free User">Free User</option>
            </select>
          </div>
          <div class="form-control">
            <label class="label" for="status-filter"><span class="label-text">Status</span></label>
            <select id="status-filter" class="select select-bordered select-sm" [(ngModel)]="statusFilter" (change)="loadUsers()">
              <option value="">All Statuses</option>
              <option value="active">Active</option>
              <option value="suspended">Suspended</option>
              <option value="unverified">Unverified</option>
            </select>
          </div>
          <div class="form-control">
            <label class="label" for="page-size"><span class="label-text">Page Size</span></label>
            <select id="page-size" class="select select-bordered select-sm" [(ngModel)]="pageSize" (change)="loadUsers()">
              <option [ngValue]="10">10</option>
              <option [ngValue]="25">25</option>
              <option [ngValue]="50">50</option>
              <option [ngValue]="100">100</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Bulk Actions Bar -->
      @if (selectedUsers().length > 0) {
        <div class="alert shadow-md flex items-center justify-between">
          <span>{{ selectedUsers().length }} user(s) selected</span>
          <div class="flex gap-2">
            <button class="btn btn-warning btn-sm" (click)="showBulkConfirmation('suspend')">Suspend</button>
            <button class="btn btn-info btn-sm" (click)="showBulkConfirmation('role_change')">Change Role</button>
            <button class="btn btn-ghost btn-sm" (click)="clearSelection()">Clear</button>
          </div>
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
      @if (loading()) {
        <div class="space-y-2">
          @for (i of [1,2,3,4,5]; track i) {
            <div class="skeleton h-12 w-full"></div>
          }
        </div>
      } @else if (error()) {
        <div class="alert alert-error">
          <span>{{ error() }}</span>
          <button class="btn btn-sm btn-ghost" (click)="loadUsers()">Retry</button>
        </div>
      } @else {
        <div class="overflow-x-auto">
          <table class="table table-zebra w-full" aria-label="User management table">
            <thead>
              <tr>
                <th>
                  <input type="checkbox" class="checkbox checkbox-sm"
                    [checked]="allSelected()"
                    (change)="toggleSelectAll()" aria-label="Select all users" />
                </th>
                <th class="cursor-pointer" (click)="sortBy('displayName')">
                  Name
                  @if (currentSort() === 'displayName') {
                    <span>{{ currentSortDir() === 'asc' ? '↑' : '↓' }}</span>
                  }
                </th>
                <th class="cursor-pointer" (click)="sortBy('email')">
                  Email
                  @if (currentSort() === 'email') {
                    <span>{{ currentSortDir() === 'asc' ? '↑' : '↓' }}</span>
                  }
                </th>
                <th>Roles</th>
                <th>Status</th>
                <th>Plan</th>
                <th class="cursor-pointer" (click)="sortBy('createdAt')">
                  Registered
                  @if (currentSort() === 'createdAt') {
                    <span>{{ currentSortDir() === 'asc' ? '↑' : '↓' }}</span>
                  }
                </th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (user of users(); track user.id) {
                <tr>
                  <td>
                    <input type="checkbox" class="checkbox checkbox-sm"
                      [checked]="isSelected(user.id)"
                      (change)="toggleUser(user.id)" [attr.aria-label]="'Select ' + user.displayName" />
                  </td>
                  <td>
                    <div class="flex items-center gap-2">
                      <div class="avatar placeholder">
                        <div class="w-8 h-8 rounded-full bg-neutral text-neutral-content">
                          @if (user.avatarUrl) {
                            <img [src]="user.avatarUrl" [alt]="user.displayName + ' avatar'" />
                          } @else {
                            <span class="text-xs">{{ user.displayName.charAt(0) }}</span>
                          }
                        </div>
                      </div>
                      <span>{{ user.displayName }}</span>
                    </div>
                  </td>
                  <td>{{ user.email }}</td>
                  <td>
                    @for (role of user.roles; track role) {
                      <span class="badge badge-sm badge-outline mr-1">{{ role }}</span>
                    }
                  </td>
                  <td>
                    <span class="badge badge-sm" [class.badge-success]="user.status === 'Active'"
                      [class.badge-error]="user.status === 'Suspended'"
                      [class.badge-warning]="user.status === 'Unverified'">
                      {{ user.status }}
                    </span>
                  </td>
                  <td>{{ user.plan || '—' }}</td>
                  <td>{{ user.createdAt | date:'mediumDate' }}</td>
                  <td>
                    <div class="flex gap-1">
                      <button class="btn btn-xs btn-ghost" (click)="impersonateUser(user.id)"
                        title="Impersonate" aria-label="Impersonate user">👤</button>
                      <button class="btn btn-xs btn-ghost" (click)="suspendUser(user.id)"
                        [disabled]="user.status === 'Suspended'"
                        title="Suspend" aria-label="Suspend user">🚫</button>
                    </div>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="8" class="text-center py-8">
                    <p class="text-base-content/60">No users found matching your criteria.</p>
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
            of {{ totalCount() }} users
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
export class UserManagementComponent implements OnInit {
  private readonly usersService = inject(AdminUsersService);
  protected readonly Math = Math;

  // State
  users = signal<UserSummary[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  currentPage = signal(1);
  loading = signal(false);
  error = signal<string | null>(null);
  exporting = signal(false);
  impersonation = signal<ImpersonateResponse | null>(null);
  bulkConfirmation = signal<string | null>(null);
  currentSort = signal('createdAt');
  currentSortDir = signal<'asc' | 'desc'>('desc');

  // Filters
  searchTerm = '';
  roleFilter = '';
  statusFilter = '';
  pageSize = 25;
  bulkRoleName = 'Subscriber';

  // Selection
  private selectedSet = signal(new Set<string>());
  selectedUsers = computed(() => [...this.selectedSet()]);
  allSelected = computed(() => this.users().length > 0 && this.users().every(u => this.selectedSet().has(u.id)));

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
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    const request: UserListRequest = {
      page: this.currentPage(),
      pageSize: this.pageSize,
      search: this.searchTerm || undefined,
      roleFilter: this.roleFilter || undefined,
      statusFilter: this.statusFilter || undefined,
      sortBy: this.currentSort(),
      sortDirection: this.currentSortDir(),
    };

    this.usersService.getUsers(request).subscribe({
      next: (res) => {
        this.users.set(res.users);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load users. Please try again.');
        this.loading.set(false);
      },
    });
  }

  onSearchChange(): void {
    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadUsers();
    }, 300);
  }

  sortBy(column: string): void {
    if (this.currentSort() === column) {
      this.currentSortDir.set(this.currentSortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.currentSort.set(column);
      this.currentSortDir.set('asc');
    }
    this.loadUsers();
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.loadUsers();
  }

  // Selection
  isSelected(id: string): boolean {
    return this.selectedSet().has(id);
  }

  toggleUser(id: string): void {
    const set = new Set(this.selectedSet());
    if (set.has(id)) set.delete(id);
    else set.add(id);
    this.selectedSet.set(set);
  }

  toggleSelectAll(): void {
    if (this.allSelected()) {
      this.selectedSet.set(new Set());
    } else {
      this.selectedSet.set(new Set(this.users().map(u => u.id)));
    }
  }

  clearSelection(): void {
    this.selectedSet.set(new Set());
  }

  // Bulk Actions
  showBulkConfirmation(action: string): void {
    this.bulkConfirmation.set(action);
  }

  cancelBulkAction(): void {
    this.bulkConfirmation.set(null);
  }

  executeBulkAction(): void {
    const action = this.bulkConfirmation();
    if (!action) return;

    this.usersService.bulkAction({
      userIds: this.selectedUsers(),
      action,
      roleName: action === 'role_change' ? this.bulkRoleName : undefined,
    }).subscribe({
      next: (result: BulkActionResponse) => {
        this.bulkConfirmation.set(null);
        this.clearSelection();
        this.loadUsers();
      },
      error: () => {
        this.bulkConfirmation.set(null);
      },
    });
  }

  // Quick Actions
  suspendUser(id: string): void {
    if (!confirm('Are you sure you want to suspend this user?')) return;
    this.usersService.suspendUser(id).subscribe({
      next: () => this.loadUsers(),
    });
  }

  impersonateUser(id: string): void {
    this.usersService.impersonateUser(id).subscribe({
      next: (response) => {
        this.impersonation.set(response);
        // Store impersonation token for session
        sessionStorage.setItem('impersonationToken', response.impersonationToken);
      },
    });
  }

  endImpersonation(): void {
    this.impersonation.set(null);
    sessionStorage.removeItem('impersonationToken');
  }

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
