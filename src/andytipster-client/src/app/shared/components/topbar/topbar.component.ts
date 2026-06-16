import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectUserProfile } from '../../../store/user/user.selectors';
import { selectUserRoleNames } from '../../../store/roles/roles.selectors';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { NotificationBellComponent } from '../notification-bell/notification-bell.component';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterLink, ThemeToggleComponent, NotificationBellComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="h-16 bg-base-200 border-b border-base-300 flex items-center px-4 lg:px-6 gap-4 sticky top-0 z-30">
      <!-- Mobile hamburger -->
      <button
        class="lg:hidden btn btn-ghost btn-sm btn-square"
        (click)="toggleSidebar.emit()"
        aria-label="Open navigation menu"
      >
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <line x1="3" y1="6" x2="21" y2="6" />
          <line x1="3" y1="12" x2="21" y2="12" />
          <line x1="3" y1="18" x2="21" y2="18" />
        </svg>
      </button>

      <!-- Search -->
      <div class="flex-1 max-w-md">
        <div class="relative">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-base-content/40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            type="search"
            placeholder="Search..."
            class="input input-sm input-bordered w-full pl-9 bg-base-100 border-base-300 focus:border-primary"
            aria-label="Search"
          />
        </div>
      </div>

      <!-- Right side -->
      <div class="flex items-center gap-2">
        <app-theme-toggle />
        <app-notification-bell />

        <!-- User avatar + name -->
        <a routerLink="/subscriber/profile" class="flex items-center gap-2 hover:bg-base-100 rounded-lg px-2 py-1 transition-micro">
          <div class="w-8 h-8 rounded-full bg-primary text-primary-content flex items-center justify-center text-sm font-semibold">
            {{ userInitials }}
          </div>
          <div class="hidden md:flex flex-col">
            <span class="text-sm font-medium text-base-content leading-tight">{{ userDisplayName }}</span>
            <span class="text-xs text-base-content/60 leading-tight">{{ primaryRole }}</span>
          </div>
        </a>
      </div>
    </header>
  `,
  styles: [`:host { display: contents; }`],
})
export class TopbarComponent {
  private readonly store = inject(Store);

  readonly toggleSidebar = output<void>();

  userDisplayName = '';
  primaryRole = '';

  constructor() {
    this.store.select(selectUserProfile).subscribe((profile) => {
      this.userDisplayName = profile?.displayName ?? '';
    });
    this.store.select(selectUserRoleNames).subscribe((roles) => {
      this.primaryRole = roles[0] ?? 'Member';
    });
  }

  get userInitials(): string {
    if (!this.userDisplayName) return '?';
    return this.userDisplayName
      .split(' ')
      .map((n) => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }
}
