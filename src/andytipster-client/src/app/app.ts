import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { CookieConsentComponent } from './shared/components/cookie-consent/cookie-consent.component';
import { routeTransitionAnimation } from './shared/animations/route-transition.animation';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, CookieConsentComponent],
  template: `
    <a class="skip-to-content" href="#main-content">Skip to main content</a>
    <app-navbar />
    <main id="main-content" [@routeAnimation]="getRouteAnimationData(outlet)">
      <router-outlet #outlet="outlet" />
    </main>
    <app-cookie-consent />
  `,
  styleUrl: './app.scss',
  animations: [routeTransitionAnimation],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  getRouteAnimationData(outlet: RouterOutlet): string {
    return outlet?.activatedRouteData?.['animation'] ?? '';
  }
}
