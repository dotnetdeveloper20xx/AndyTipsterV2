import { Injectable, signal } from '@angular/core';

export interface CookieConsentPreferences {
  essential: boolean; // Always true, cannot be toggled
  analytics: boolean;
  marketing: boolean;
  preferences: boolean;
  timestamp: string;
  policyVersion: string;
}

const CONSENT_STORAGE_KEY = 'cookie-consent';
const CURRENT_POLICY_VERSION = '1.0';
const CONSENT_EXPIRY_MONTHS = 12;

@Injectable({ providedIn: 'root' })
export class CookieConsentService {
  readonly showBanner = signal(false);
  readonly preferences = signal<CookieConsentPreferences | null>(null);

  constructor() {
    this.checkConsentStatus();
  }

  private checkConsentStatus(): void {
    const stored = localStorage.getItem(CONSENT_STORAGE_KEY);
    if (!stored) {
      this.showBanner.set(true);
      return;
    }

    try {
      const consent: CookieConsentPreferences = JSON.parse(stored);

      // Re-prompt if policy version changed
      if (consent.policyVersion !== CURRENT_POLICY_VERSION) {
        this.showBanner.set(true);
        return;
      }

      // Re-prompt after 12 months
      const consentDate = new Date(consent.timestamp);
      const expiryDate = new Date(consentDate);
      expiryDate.setMonth(expiryDate.getMonth() + CONSENT_EXPIRY_MONTHS);
      if (new Date() > expiryDate) {
        this.showBanner.set(true);
        return;
      }

      this.preferences.set(consent);
      this.applyConsent(consent);
    } catch {
      this.showBanner.set(true);
    }
  }

  acceptAll(): void {
    const consent: CookieConsentPreferences = {
      essential: true,
      analytics: true,
      marketing: true,
      preferences: true,
      timestamp: new Date().toISOString(),
      policyVersion: CURRENT_POLICY_VERSION,
    };
    this.saveConsent(consent);
  }

  rejectAll(): void {
    const consent: CookieConsentPreferences = {
      essential: true,
      analytics: false,
      marketing: false,
      preferences: false,
      timestamp: new Date().toISOString(),
      policyVersion: CURRENT_POLICY_VERSION,
    };
    this.saveConsent(consent);
  }

  saveCustomPreferences(prefs: Omit<CookieConsentPreferences, 'essential' | 'timestamp' | 'policyVersion'>): void {
    const consent: CookieConsentPreferences = {
      essential: true,
      analytics: prefs.analytics,
      marketing: prefs.marketing,
      preferences: prefs.preferences,
      timestamp: new Date().toISOString(),
      policyVersion: CURRENT_POLICY_VERSION,
    };
    this.saveConsent(consent);
  }

  private saveConsent(consent: CookieConsentPreferences): void {
    localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(consent));
    this.preferences.set(consent);
    this.showBanner.set(false);
    this.applyConsent(consent);
  }

  private applyConsent(consent: CookieConsentPreferences): void {
    // Block or enable non-essential scripts based on consent
    if (consent.analytics) {
      this.enableAnalyticsScripts();
    } else {
      this.disableAnalyticsScripts();
    }

    if (consent.marketing) {
      this.enableMarketingScripts();
    } else {
      this.disableMarketingScripts();
    }
  }

  private enableAnalyticsScripts(): void {
    // Enable analytics scripts (e.g., Google Analytics, Application Insights)
    document.querySelectorAll('script[data-consent="analytics"]').forEach((script) => {
      const el = script as HTMLScriptElement;
      if (el.dataset['src']) {
        el.src = el.dataset['src'];
      }
    });
  }

  private disableAnalyticsScripts(): void {
    // Analytics scripts are blocked by default until consent
  }

  private enableMarketingScripts(): void {
    // Enable marketing scripts (e.g., Facebook Pixel, Google Ads)
    document.querySelectorAll('script[data-consent="marketing"]').forEach((script) => {
      const el = script as HTMLScriptElement;
      if (el.dataset['src']) {
        el.src = el.dataset['src'];
      }
    });
  }

  private disableMarketingScripts(): void {
    // Marketing scripts are blocked by default until consent
  }

  hasConsent(type: 'analytics' | 'marketing' | 'preferences'): boolean {
    const prefs = this.preferences();
    if (!prefs) return false;
    return prefs[type];
  }

  getConsentTimestamp(): string | null {
    const prefs = this.preferences();
    return prefs?.timestamp ?? null;
  }
}
