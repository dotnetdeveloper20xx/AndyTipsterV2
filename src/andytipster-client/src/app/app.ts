import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs/operators';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';
import { TopbarComponent } from './shared/components/topbar/topbar.component';
import { PublicNavComponent } from './shared/components/public-nav/public-nav.component';
import { CookieConsentComponent } from './shared/components/cookie-consent/cookie-consent.component';
import { OfflineIndicatorComponent } from './shared/components/offline-indicator/offline-indicator.component';
import { ToastContainerComponent } from './shared/components/toast/toast-container.component';
import { selectIsAuthenticated } from './store/auth/auth.selectors';
import { AuthActions } from './store/auth/auth.actions';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    SidebarComponent,
    TopbarComponent,
    PublicNavComponent,
    CookieConsentComponent,
    OfflineIndicatorComponent,
    ToastContainerComponent,
  ],
  template: `
    <a class="skip-to-content" href="#main-content">Skip to main content</a>
    <app-offline-indicator />

    @if (showSidebarLayout()) {
      <!-- Authenticated layout: sidebar + topbar -->
      <div class="flex h-screen bg-base-100">
        <!-- Sidebar -->
        <app-sidebar [mobileOpen]="mobileMenuOpen()" (closeMobile)="mobileMenuOpen.set(false)" />

        <!-- Main content area -->
        <div class="flex-1 flex flex-col overflow-hidden lg:ml-64">
          <!-- Top bar -->
          <app-topbar (toggleSidebar)="mobileMenuOpen.set(!mobileMenuOpen())" />

          <!-- Page content -->
          <main id="main-content" class="flex-1 overflow-y-auto p-4 lg:p-6 animate-fade-in">
            <router-outlet />
          </main>
        </div>
      </div>
    } @else {
      <!-- Public layout: simple top nav, full width -->
      <app-public-nav />
      <main id="main-content">
        <router-outlet />
      </main>
    }

    <app-cookie-consent />
    <app-toast-container />
  `,
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly store = inject(Store);
  private readonly router = inject(Router);

  readonly mobileMenuOpen = signal(false);
  readonly showSidebarLayout = signal(false);

  private isAuthenticated = false;

  constructor() {
    this.store.dispatch(AuthActions.initAuth());

    this.store.select(selectIsAuthenticated).subscribe((auth) => {
      this.isAuthenticated = auth;
      this.updateLayout(this.router.url);
    });

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event) => {
        const navEnd = event as NavigationEnd;
        this.updateLayout(navEnd.urlAfterRedirects);
        this.mobileMenuOpen.set(false);
      });
  }

  private updateLayout(url: string): void {
    // Public routes get simple top nav (no sidebar)
    const publicPrefixes = ['/', '/pricing', '/stats', '/blog', '/faq', '/auth', '/unauthorized'];
    const isPublicRoute = publicPrefixes.some((prefix) => {
      if (prefix === '/') return url === '/';
      return url.startsWith(prefix);
    });

    this.showSidebarLayout.set(this.isAuthenticated && !isPublicRoute);
  }
}
