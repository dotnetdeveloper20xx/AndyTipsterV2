import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type Theme = 'andytipster-light' | 'andytipster-dark';

const THEME_STORAGE_KEY = 'andytipster-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  private readonly _theme = signal<Theme>(this.getInitialTheme());

  readonly theme = this._theme.asReadonly();
  readonly isDarkMode = computed(() => this._theme() === 'andytipster-dark');

  constructor() {
    if (this.isBrowser) {
      this.applyTheme(this._theme());
      this.listenToSystemChanges();
    }
  }

  toggleTheme(): void {
    const newTheme: Theme =
      this._theme() === 'andytipster-light' ? 'andytipster-dark' : 'andytipster-light';
    this.setTheme(newTheme);
  }

  setTheme(theme: Theme): void {
    this._theme.set(theme);
    this.applyTheme(theme);
    this.persistTheme(theme);
  }

  private getInitialTheme(): Theme {
    if (!this.isBrowser) {
      return 'andytipster-light';
    }

    // Check localStorage first
    const stored = this.readStoredTheme();
    if (stored) {
      return stored;
    }

    // Fall back to system preference
    return this.getSystemPreference();
  }

  private readStoredTheme(): Theme | null {
    try {
      const stored = localStorage.getItem(THEME_STORAGE_KEY);
      if (stored === 'andytipster-light' || stored === 'andytipster-dark') {
        return stored;
      }
      return null;
    } catch {
      // localStorage unavailable — continue without error (Req 7.4)
      return null;
    }
  }

  private getSystemPreference(): Theme {
    if (window.matchMedia?.('(prefers-color-scheme: dark)').matches) {
      return 'andytipster-dark';
    }
    return 'andytipster-light';
  }

  private applyTheme(theme: Theme): void {
    if (!this.isBrowser) return;
    document.documentElement.setAttribute('data-theme', theme);
  }

  private persistTheme(theme: Theme): void {
    try {
      localStorage.setItem(THEME_STORAGE_KEY, theme);
    } catch {
      // localStorage unavailable — continue without error (Req 7.4)
    }
  }

  private listenToSystemChanges(): void {
    const mediaQuery = window.matchMedia?.('(prefers-color-scheme: dark)');
    if (!mediaQuery) return;

    mediaQuery.addEventListener('change', (event) => {
      // Only follow system preference if no manual override in storage
      const stored = this.readStoredTheme();
      if (!stored) {
        const newTheme: Theme = event.matches ? 'andytipster-dark' : 'andytipster-light';
        this._theme.set(newTheme);
        this.applyTheme(newTheme);
      }
    });
  }
}
