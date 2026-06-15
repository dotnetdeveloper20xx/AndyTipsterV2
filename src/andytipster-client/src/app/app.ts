import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { CookieConsentComponent } from './shared/components/cookie-consent/cookie-consent.component';
import { OfflineIndicatorComponent } from './shared/components/offline-indicator/offline-indicator.component';
import { BottomNavComponent } from './shared/components/bottom-nav/bottom-nav.component';
import { ToastContainerComponent } from './shared/components/toast/toast-container.component';
import { routeTransitionAnimation } from './shared/animations/route-transition.animation';
import { AuthActions } from './store/auth/auth.actions';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, CookieConsentComponent, OfflineIndicatorComponent, BottomNavComponent, ToastContainerComponent],
  template: `
    <a class="skip-to-content" href="#main-content">Skip to main content</a>
    <app-offline-indicator />
    <app-navbar />
    <main id="main-content" class="pb-16 lg:pb-0" [@routeAnimation]="getRouteAnimationData(outlet)">
      <router-outlet #outlet="outlet" />
    </main>
    <app-bottom-nav />
    <app-cookie-consent />
    <app-toast-container />
  `,
  styleUrl: './app.scss',
  animations: [routeTransitionAnimation],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  constructor() {
    inject(Store).dispatch(AuthActions.initAuth());
  }

  getRouteAnimationData(outlet: RouterOutlet): string {
    return outlet?.activatedRouteData?.['animation'] ?? '';
  }
}
