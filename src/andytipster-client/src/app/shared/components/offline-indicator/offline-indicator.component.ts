import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, signal } from '@angular/core';

@Component({
  selector: 'app-offline-indicator',
  standalone: true,
  template: `
    @if (isOffline()) {
      <div
        class="fixed top-0 left-0 right-0 z-50 bg-warning text-warning-content px-4 py-2 text-center text-sm font-medium shadow-md transition-transform duration-300"
        role="alert"
        aria-live="polite"
      >
        <div class="flex items-center justify-center gap-2">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
            <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
          </svg>
          <span>You're offline. Showing cached content.</span>
        </div>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OfflineIndicatorComponent implements OnInit, OnDestroy {
  readonly isOffline = signal(false);

  private handleOnline = () => this.isOffline.set(false);
  private handleOffline = () => this.isOffline.set(true);

  ngOnInit(): void {
    this.isOffline.set(!navigator.onLine);
    window.addEventListener('online', this.handleOnline);
    window.addEventListener('offline', this.handleOffline);
  }

  ngOnDestroy(): void {
    window.removeEventListener('online', this.handleOnline);
    window.removeEventListener('offline', this.handleOffline);
  }
}
