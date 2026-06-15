import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectIsAuthenticated, selectAuthUser } from '../../../store/auth/auth.selectors';
import { selectUserRoleNames } from '../../../store/roles/roles.selectors';
import { selectUserPermissions } from '../../../store/permissions/permissions.selectors';
import { selectUserProfile } from '../../../store/user/user.selectors';
import { AuthActions } from '../../../store/auth/auth.actions';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';

export interface NavItem {
  label: string;
  route: string;
  icon?: string;
  roles?: string[];
  permissions?: string[];
  authRequired?: boolean;
  guestOnly?: boolean;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, ThemeToggleComponent],
  template: `
    <nav class="navbar bg-base-100 shadow-sm sticky top-0 z-50" aria-label="Main navigation">
      <div class="navbar-start">
        <!-- Mobile menu toggle -->
        <div class="dropdown">
          <label tabindex="0" class="btn btn-ghost lg:hidden" aria-label="Open menu">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h8m-8 6h16" />
            </svg>
          </label>
          <ul tabindex="0" class="menu menu-sm dropdown-content mt-3 z-[1] p-2 shadow bg-base-100 rounded-box w-52" role="menu">
            @for (item of visibleNavItems; track item.route) {
              <li role="none">
                <a [routerLink]="item.route" routerLinkActive="active" role="menuitem">{{ item.label }}</a>
              </li>
            }
          </ul>
        </div>
        <a routerLink="/" class="btn btn-ghost text-xl font-bold">AndyTipster</a>
      </div>

      <!-- Desktop nav links -->
      <div class="navbar-center hidden lg:flex">
        <ul class="menu menu-horizontal px-1" role="menubar">
          @for (item of visibleNavItems; track item.route) {
            <li role="none">
              <a [routerLink]="item.route" routerLinkActive="active" role="menuitem">{{ item.label }}</a>
            </li>
          }
        </ul>
      </div>

      <div class="navbar-end gap-2">
        <app-theme-toggle />

        @if (isAuthenticated$ | async) {
          <!-- Authenticated user dropdown -->
          <div class="dropdown dropdown-end">
            <label tabindex="0" class="btn btn-ghost btn-circle avatar" aria-label="User menu">
              <div class="w-10 rounded-full bg-primary text-primary-content flex items-center justify-center">
                @if (userAvatar) {
                  <img [src]="userAvatar" [alt]="userDisplayName + ' avatar'" class="rounded-full" />
                } @else {
                  <span class="text-lg font-bold">{{ userInitials }}</span>
                }
              </div>
            </label>
            <ul tabindex="0" class="menu menu-sm dropdown-content mt-3 z-[1] p-2 shadow bg-base-100 rounded-box w-52" role="menu">
              <li class="menu-title">
                <span>{{ userDisplayName }}</span>
              </li>
              <li role="none"><a routerLink="/profile" role="menuitem">Profile</a></li>
              <li role="none"><a routerLink="/settings" role="menuitem">Settings</a></li>
              @if (isAdmin) {
                <div class="divider my-0"></div>
                <li role="none"><a routerLink="/admin" role="menuitem">Admin Dashboard</a></li>
              }
              <div class="divider my-0"></div>
              <li role="none"><a (click)="logout()" role="menuitem" class="text-error">Logout</a></li>
            </ul>
          </div>
        } @else {
          <!-- Guest buttons -->
          <a routerLink="/auth/login" class="btn btn-ghost btn-sm">Sign In</a>
          <a routerLink="/auth/register" class="btn btn-primary btn-sm">Sign Up</a>
        }
      </div>
    </nav>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  private readonly store = inject(Store);

  readonly isAuthenticated$ = this.store.select(selectIsAuthenticated);

  private readonly navItems: NavItem[] = [
    { label: 'Home', route: '/', authRequired: false },
    { label: 'Tips', route: '/tips', authRequired: true },
    { label: 'Plans', route: '/plans', authRequired: false },
    { label: 'Admin', route: '/admin', roles: ['admin', 'super-admin'], authRequired: true },
  ];

  private roles: string[] = [];
  private permissions: string[] = [];
  private authenticated = false;
  userDisplayName = '';
  userAvatar: string | null = null;
  isAdmin = false;

  constructor() {
    this.store.select(selectIsAuthenticated).subscribe((auth) => {
      this.authenticated = auth;
    });
    this.store.select(selectUserRoleNames).subscribe((roles) => {
      this.roles = roles;
      this.isAdmin = roles.includes('admin') || roles.includes('super-admin');
    });
    this.store.select(selectUserPermissions).subscribe((permissions) => {
      this.permissions = permissions;
    });
    this.store.select(selectUserProfile).subscribe((profile) => {
      this.userDisplayName = profile?.displayName ?? '';
      this.userAvatar = profile?.avatarUrl ?? null;
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

  get visibleNavItems(): NavItem[] {
    return this.navItems.filter((item) => {
      // Guest-only items
      if (item.guestOnly && this.authenticated) return false;

      // Auth-required items
      if (item.authRequired && !this.authenticated) return false;

      // Role-restricted items
      if (item.roles && item.roles.length > 0) {
        if (!item.roles.some((role) => this.roles.includes(role))) return false;
      }

      // Permission-restricted items
      if (item.permissions && item.permissions.length > 0) {
        if (!item.permissions.some((perm) => this.permissions.includes(perm))) return false;
      }

      return true;
    });
  }

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
