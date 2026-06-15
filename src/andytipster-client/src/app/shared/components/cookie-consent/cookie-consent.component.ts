import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CookieConsentService } from '../../../core/services/cookie-consent.service';

@Component({
  selector: 'app-cookie-consent',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (cookieConsentService.showBanner()) {
      <div class="fixed bottom-0 left-0 right-0 z-50 p-4 bg-base-200 shadow-lg border-t border-base-300"
           role="dialog" aria-label="Cookie consent" aria-describedby="cookie-desc">
        @if (!showCustomise()) {
          <div class="max-w-7xl mx-auto flex flex-col md:flex-row items-center gap-4">
            <p id="cookie-desc" class="flex-1 text-sm text-base-content">
              We use cookies to enhance your experience. Essential cookies are always active.
              You can customise your preferences or accept/reject all non-essential cookies.
            </p>
            <div class="flex gap-2 flex-wrap">
              <button class="btn btn-primary btn-sm" (click)="acceptAll()" aria-label="Accept all cookies">
                Accept All
              </button>
              <button class="btn btn-outline btn-sm" (click)="rejectAll()" aria-label="Reject all non-essential cookies">
                Reject All
              </button>
              <button class="btn btn-ghost btn-sm" (click)="showCustomise.set(true)" aria-label="Customise cookie preferences">
                Customise
              </button>
            </div>
          </div>
        } @else {
          <div class="max-w-7xl mx-auto">
            <h3 class="font-semibold text-base-content mb-3">Cookie Preferences</h3>
            <div class="grid gap-3 mb-4">
              <label class="flex items-center gap-3">
                <input type="checkbox" checked disabled class="checkbox checkbox-sm" aria-label="Essential cookies (always on)" />
                <div>
                  <span class="font-medium text-sm">Essential</span>
                  <span class="text-xs text-base-content/70 block">Required for basic site functionality (always on)</span>
                </div>
              </label>
              <label class="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" [checked]="analyticsEnabled()" (change)="analyticsEnabled.set(!analyticsEnabled())"
                       class="checkbox checkbox-sm" aria-label="Analytics cookies" />
                <div>
                  <span class="font-medium text-sm">Analytics</span>
                  <span class="text-xs text-base-content/70 block">Help us understand how visitors use the site</span>
                </div>
              </label>
              <label class="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" [checked]="marketingEnabled()" (change)="marketingEnabled.set(!marketingEnabled())"
                       class="checkbox checkbox-sm" aria-label="Marketing cookies" />
                <div>
                  <span class="font-medium text-sm">Marketing</span>
                  <span class="text-xs text-base-content/70 block">Used for targeted advertising and promotions</span>
                </div>
              </label>
              <label class="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" [checked]="preferencesEnabled()" (change)="preferencesEnabled.set(!preferencesEnabled())"
                       class="checkbox checkbox-sm" aria-label="Preference cookies" />
                <div>
                  <span class="font-medium text-sm">Preferences</span>
                  <span class="text-xs text-base-content/70 block">Remember your settings and preferences</span>
                </div>
              </label>
            </div>
            <div class="flex gap-2">
              <button class="btn btn-primary btn-sm" (click)="saveCustom()" aria-label="Save cookie preferences">
                Save Preferences
              </button>
              <button class="btn btn-ghost btn-sm" (click)="showCustomise.set(false)" aria-label="Back to simple options">
                Back
              </button>
            </div>
          </div>
        }
      </div>
    }
  `,
})
export class CookieConsentComponent {
  readonly cookieConsentService = inject(CookieConsentService);

  readonly showCustomise = signal(false);
  readonly analyticsEnabled = signal(false);
  readonly marketingEnabled = signal(false);
  readonly preferencesEnabled = signal(false);

  acceptAll(): void {
    this.cookieConsentService.acceptAll();
  }

  rejectAll(): void {
    this.cookieConsentService.rejectAll();
  }

  saveCustom(): void {
    this.cookieConsentService.saveCustomPreferences({
      analytics: this.analyticsEnabled(),
      marketing: this.marketingEnabled(),
      preferences: this.preferencesEnabled(),
    });
    this.showCustomise.set(false);
  }
}
