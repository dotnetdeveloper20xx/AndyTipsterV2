import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectIsAuthenticated } from '../../../store/auth/auth.selectors';
import { selectUserRoleNames } from '../../../store/roles/roles.selectors';
import { AuthActions } from '../../../store/auth/auth.actions';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-public-nav',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, ThemeToggleComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav class="bg-primary shadow-sm sticky top-0 z-50" aria-label="Main navigation">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <a routerLink="/" class="flex items-center gap-2 hover:opacity-90 transition-micro">
            <div class="w-9 h-9 rounded-lg bg-amber-400 flex items-center justify-center">
              <span class="text-sm font-bold text-neutral">AT</span>
            </div>
            <span class="text-primary-content font-semibold text-lg tracking-tight">ANDYTIPSTER</span>
          </a>

          <!-- Desktop nav links -->
          <div class="hidden md:flex items-center gap-1">
            <a routerLink="/" routerLinkActive="bg-white/15" [routerLinkActiveOptions]="{exact: true}"
               class="px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10 transition-micro">
              Home
            </a>
            <a routerLink="/pricing" routerLinkActive="bg-white/15"
               class="px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10 transition-micro">
              Pricing
            </a>
            <a routerLink="/stats" routerLinkActive="bg-white/15"
               class="px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10 transition-micro">
              Stats
            </a>
            <a routerLink="/blog" routerLinkActive="bg-white/15"
               class="px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10 transition-micro">
              Blog
            </a>
            <a routerLink="/faq" routerLinkActive="bg-white/15"
               class="px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10 transition-micro">
              FAQ
            </a>
          </div>

          <!-- Right side -->
          <div class="flex items-center gap-2">
            <app-theme-toggle />

            @if (isAuthenticated$ | async) {
              <a routerLink="/subscriber/tips" class="btn btn-accent btn-sm text-accent-content">
                Go to Dashboard
              </a>
            } @else {
              <a routerLink="/auth/login" class="btn btn-ghost btn-sm text-primary-content hover:bg-white/10">
                Sign In
              </a>
              <a routerLink="/auth/register" class="btn btn-warning btn-sm text-warning-content font-semibold">
                Sign Up
              </a>
            }

            <!-- Mobile menu button -->
            <button
              class="md:hidden btn btn-ghost btn-sm btn-square text-primary-content"
              (click)="mobileMenuOpen = !mobileMenuOpen"
              aria-label="Toggle mobile menu"
            >
              <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                @if (mobileMenuOpen) {
                  <path d="M18 6L6 18M6 6l12 12" />
                } @else {
                  <path d="M4 6h16M4 12h16M4 18h16" />
                }
              </svg>
            </button>
          </div>
        </div>

        <!-- Mobile menu -->
        @if (mobileMenuOpen) {
          <div class="md:hidden border-t border-white/10 py-3 space-y-1">
            <a routerLink="/" (click)="mobileMenuOpen = false"
               class="block px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10">
              Home
            </a>
            <a routerLink="/pricing" (click)="mobileMenuOpen = false"
               class="block px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10">
              Pricing
            </a>
            <a routerLink="/stats" (click)="mobileMenuOpen = false"
               class="block px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10">
              Stats
            </a>
            <a routerLink="/blog" (click)="mobileMenuOpen = false"
               class="block px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10">
              Blog
            </a>
            <a routerLink="/faq" (click)="mobileMenuOpen = false"
               class="block px-3 py-2 rounded-lg text-sm font-medium text-primary-content/80 hover:text-primary-content hover:bg-white/10">
              FAQ
            </a>
          </div>
        }
      </div>
    </nav>
  `,
  styles: [`:host { display: contents; }`],
})
export class PublicNavComponent {
  private readonly store = inject(Store);

  readonly isAuthenticated$ = this.store.select(selectIsAuthenticated);
  mobileMenuOpen = false;
}
