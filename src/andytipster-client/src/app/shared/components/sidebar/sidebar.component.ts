import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectIsAuthenticated } from '../../../store/auth/auth.selectors';
import { selectUserRoleNames } from '../../../store/roles/roles.selectors';
import { AuthActions } from '../../../store/auth/auth.actions';

export interface SidebarNavItem {
  label: string;
  route?: string;
  icon: string;
  children?: SidebarNavItem[];
  roles?: string[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Desktop sidebar -->
    <aside
      class="hidden lg:flex flex-col w-64 h-screen fixed left-0 top-0 sidebar-gradient z-40 overflow-y-auto"
      aria-label="Main navigation"
    >
      <!-- Logo -->
      <div class="flex items-center gap-2 px-5 py-5 border-b border-white/10">
        <div class="w-9 h-9 rounded-lg bg-amber-400 flex items-center justify-center">
          <span class="text-sm font-bold text-neutral">AT</span>
        </div>
        <span class="text-white font-semibold text-lg tracking-tight">ANDYTIPSTER</span>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 px-3 py-4 space-y-1">
        @for (item of visibleNavItems; track item.label) {
          @if (item.children && item.children.length > 0) {
            <!-- Expandable parent -->
            <div>
              <button
                class="sidebar-nav-item w-full text-left"
                (click)="toggleSubmenu(item.label)"
                [attr.aria-expanded]="isExpanded(item.label)"
              >
                <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path [attr.d]="getIconPath(item.icon)" />
                </svg>
                <span class="flex-1">{{ item.label }}</span>
                <svg class="w-4 h-4 transition-transform" [class.rotate-90]="isExpanded(item.label)" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                  <path d="M9 18l6-6-6-6" />
                </svg>
              </button>
              @if (isExpanded(item.label)) {
                <div class="ml-4 mt-1 space-y-0.5">
                  @for (child of getVisibleChildren(item); track child.label) {
                    <a
                      [routerLink]="child.route"
                      routerLinkActive="active"
                      class="sidebar-nav-item text-sm pl-6"
                      (click)="closeMobile.emit()"
                    >
                      <span>{{ child.label }}</span>
                    </a>
                  }
                </div>
              }
            </div>
          } @else {
            <!-- Direct link -->
            <a
              [routerLink]="item.route"
              routerLinkActive="active"
              [routerLinkActiveOptions]="{ exact: item.route === '/subscriber/dashboard' || item.route === '/admin/dashboard' }"
              class="sidebar-nav-item"
              (click)="closeMobile.emit()"
            >
              <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <path [attr.d]="getIconPath(item.icon)" />
              </svg>
              <span>{{ item.label }}</span>
            </a>
          }
        }
      </nav>

      <!-- Bottom section -->
      <div class="px-3 py-4 border-t border-white/10">
        <button
          class="sidebar-nav-item w-full text-left text-red-300 hover:text-red-200 hover:bg-red-500/10"
          (click)="logout()"
        >
          <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" />
          </svg>
          <span>Logout</span>
        </button>
      </div>
    </aside>

    <!-- Mobile drawer overlay -->
    @if (mobileOpen()) {
      <div class="fixed inset-0 z-50 lg:hidden">
        <!-- Backdrop -->
        <div
          class="absolute inset-0 bg-black/50"
          (click)="closeMobile.emit()"
          aria-hidden="true"
        ></div>

        <!-- Drawer -->
        <aside
          class="absolute left-0 top-0 h-full w-64 sidebar-gradient overflow-y-auto animate-slide-in-left"
          aria-label="Mobile navigation"
        >
          <!-- Logo -->
          <div class="flex items-center justify-between px-5 py-5 border-b border-white/10">
            <div class="flex items-center gap-2">
              <div class="w-9 h-9 rounded-lg bg-amber-400 flex items-center justify-center">
                <span class="text-sm font-bold text-neutral">AT</span>
              </div>
              <span class="text-white font-semibold text-lg">ANDYTIPSTER</span>
            </div>
            <button (click)="closeMobile.emit()" class="text-white/70 hover:text-white" aria-label="Close menu">
              <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M18 6L6 18M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Navigation -->
          <nav class="flex-1 px-3 py-4 space-y-1">
            @for (item of visibleNavItems; track item.label) {
              @if (item.children && item.children.length > 0) {
                <div>
                  <button
                    class="sidebar-nav-item w-full text-left"
                    (click)="toggleSubmenu(item.label)"
                    [attr.aria-expanded]="isExpanded(item.label)"
                  >
                    <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                      <path [attr.d]="getIconPath(item.icon)" />
                    </svg>
                    <span class="flex-1">{{ item.label }}</span>
                    <svg class="w-4 h-4 transition-transform" [class.rotate-90]="isExpanded(item.label)" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                      <path d="M9 18l6-6-6-6" />
                    </svg>
                  </button>
                  @if (isExpanded(item.label)) {
                    <div class="ml-4 mt-1 space-y-0.5">
                      @for (child of getVisibleChildren(item); track child.label) {
                        <a
                          [routerLink]="child.route"
                          routerLinkActive="active"
                          class="sidebar-nav-item text-sm pl-6"
                          (click)="closeMobile.emit()"
                        >
                          <span>{{ child.label }}</span>
                        </a>
                      }
                    </div>
                  }
                </div>
              } @else {
                <a
                  [routerLink]="item.route"
                  routerLinkActive="active"
                  [routerLinkActiveOptions]="{ exact: item.route === '/subscriber/dashboard' || item.route === '/admin/dashboard' }"
                  class="sidebar-nav-item"
                  (click)="closeMobile.emit()"
                >
                  <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                    <path [attr.d]="getIconPath(item.icon)" />
                  </svg>
                  <span>{{ item.label }}</span>
                </a>
              }
            }
          </nav>

          <!-- Bottom section -->
          <div class="px-3 py-4 border-t border-white/10">
            <button
              class="sidebar-nav-item w-full text-left text-red-300 hover:text-red-200 hover:bg-red-500/10"
              (click)="logout()"
            >
              <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" />
              </svg>
              <span>Logout</span>
            </button>
          </div>
        </aside>
      </div>
    }
  `,
  styles: [`
    :host { display: contents; }

    @keyframes slide-in-left {
      from { transform: translateX(-100%); }
      to { transform: translateX(0); }
    }

    .animate-slide-in-left {
      animation: slide-in-left 0.2s ease-out;
    }
  `],
})
export class SidebarComponent {
  private readonly store = inject(Store);
  private readonly router = inject(Router);

  readonly mobileOpen = input<boolean>(false);
  readonly closeMobile = output<void>();

  private roles: string[] = [];
  private expandedMenus = new Set<string>();

  private readonly adminNavItems: SidebarNavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'dashboard' },
    {
      label: 'Tips', icon: 'tips', children: [
        { label: "Today's Tips", route: '/admin/tips', icon: '' },
        { label: 'Results', route: '/subscriber/results', icon: '' },
      ]
    },
    {
      label: 'Subscriptions', icon: 'subscriptions', children: [
        { label: 'Packages', route: '/admin/plans', icon: '' },
        { label: 'Billing', route: '/admin/paypal-dashboard', icon: '' },
      ]
    },
    { label: 'Members', route: '/admin/users', icon: 'members' },
    { label: 'Performance', route: '/admin/analytics', icon: 'performance' },
    { label: 'Reports', route: '/admin/audit', icon: 'reports' },
    {
      label: 'Content', icon: 'content', children: [
        { label: 'CMS', route: '/admin/cms', icon: '' },
        { label: 'Blog', route: '/admin/blog', icon: '' },
        { label: 'Media', route: '/admin/media-library', icon: '' },
      ]
    },
    { label: 'Settings', route: '/admin/navigation', icon: 'settings' },
  ];

  private readonly subscriberNavItems: SidebarNavItem[] = [
    { label: 'Dashboard', route: '/subscriber/tips', icon: 'dashboard' },
    {
      label: 'Tips', icon: 'tips', children: [
        { label: "Today's Tips", route: '/subscriber/tips', icon: '' },
        { label: 'Results', route: '/subscriber/results', icon: '' },
      ]
    },
    { label: 'Performance', route: '/subscriber/results', icon: 'performance' },
    { label: 'Billing', route: '/subscriber/billing', icon: 'billing' },
    { label: 'Profile', route: '/subscriber/profile', icon: 'profile' },
    { label: 'Referrals', route: '/subscriber/referrals', icon: 'referrals' },
  ];

  private readonly freeUserNavItems: SidebarNavItem[] = [
    { label: 'Dashboard', route: '/', icon: 'dashboard' },
    { label: 'Pricing', route: '/pricing', icon: 'pricing' },
    { label: 'Profile', route: '/subscriber/profile', icon: 'profile' },
  ];

  constructor() {
    this.store.select(selectUserRoleNames).subscribe((roles) => {
      this.roles = roles;
    });
  }

  get visibleNavItems(): SidebarNavItem[] {
    if (this.roles.includes('Super Admin') || this.roles.includes('Admin') || this.roles.includes('Moderator')) {
      return this.adminNavItems;
    }
    if (this.roles.includes('Subscriber')) {
      return this.subscriberNavItems;
    }
    return this.freeUserNavItems;
  }

  getVisibleChildren(item: SidebarNavItem): SidebarNavItem[] {
    return item.children ?? [];
  }

  toggleSubmenu(label: string): void {
    if (this.expandedMenus.has(label)) {
      this.expandedMenus.delete(label);
    } else {
      this.expandedMenus.add(label);
    }
  }

  isExpanded(label: string): boolean {
    return this.expandedMenus.has(label);
  }

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }

  getIconPath(icon: string): string {
    const icons: Record<string, string> = {
      dashboard: 'M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z M9 22V12h6v10',
      tips: 'M12 2L2 7l10 5 10-5-10-5z M2 17l10 5 10-5 M2 12l10 5 10-5',
      subscriptions: 'M21 4H3a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h18a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2z M1 10h22',
      members: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2 M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z M23 21v-2a4 4 0 0 0-3-3.87 M16 3.13a4 4 0 0 1 0 7.75',
      performance: 'M18 20V10 M12 20V4 M6 20v-6',
      reports: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z M14 2v6h6 M16 13H8 M16 17H8 M10 9H8',
      content: 'M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7 M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z',
      settings: 'M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z',
      billing: 'M21 4H3a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h18a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2z M1 10h22',
      profile: 'M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2 M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z',
      pricing: 'M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6',
      referrals: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2 M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z M19 8v6 M22 11h-6',
    };
    return icons[icon] || 'M12 2L2 7l10 5 10-5-10-5z';
  }
}
