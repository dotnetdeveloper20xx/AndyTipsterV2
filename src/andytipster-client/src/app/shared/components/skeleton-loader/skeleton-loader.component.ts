import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
  signal,
  OnInit,
  OnDestroy,
} from '@angular/core';

export type SkeletonShape = 'text' | 'avatar' | 'card';

@Component({
  selector: 'app-skeleton-loader',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (hasTimedOut()) {
      <div class="flex flex-col items-center justify-center gap-4 p-8 text-center" role="alert" aria-live="assertive">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-12 w-12 text-error"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          aria-hidden="true"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.999L13.732 4.001c-.77-1.333-2.694-1.333-3.464 0L3.34 16.001C2.57 17.335 3.532 19 5.072 19z"
          />
        </svg>
        <p class="text-base-content/70 text-sm">Loading timed out. Please try again.</p>
        <button class="btn btn-primary btn-sm transition-micro" (click)="onRetry()">
          Retry
        </button>
      </div>
    } @else {
      <div class="flex flex-col gap-3" [attr.aria-label]="'Loading content'" role="status">
        @for (row of rows(); track $index) {
          @switch (shape()) {
            @case ('text') {
              <div
                class="skeleton-shimmer h-4 rounded"
                [class.w-full]="$index < rows().length - 1"
                [class.w-3/4]="$index === rows().length - 1"
              ></div>
            }
            @case ('avatar') {
              <div class="flex items-center gap-3">
                <div class="skeleton-shimmer h-10 w-10 rounded-full shrink-0"></div>
                <div class="flex flex-col gap-2 flex-1">
                  <div class="skeleton-shimmer h-4 w-3/4 rounded"></div>
                  <div class="skeleton-shimmer h-3 w-1/2 rounded"></div>
                </div>
              </div>
            }
            @case ('card') {
              <div class="skeleton-shimmer h-32 w-full rounded-lg"></div>
            }
          }
        }
        <span class="sr-only">Loading...</span>
      </div>
    }
  `,
})
export class SkeletonLoaderComponent implements OnInit, OnDestroy {
  /** Number of skeleton rows to display */
  readonly count = input<number>(3);

  /** Shape of the skeleton elements */
  readonly shape = input<SkeletonShape>('text');

  /** Timeout in milliseconds before showing error state (default: 10s) */
  readonly timeout = input<number>(10000);

  /** Emitted when the user clicks the retry button */
  readonly retry = output<void>();

  readonly hasTimedOut = signal(false);
  private timeoutId: ReturnType<typeof setTimeout> | null = null;

  get rows(): () => number[] {
    return () => Array.from({ length: this.count() }, (_, i) => i);
  }

  ngOnInit(): void {
    this.startTimeout();
  }

  ngOnDestroy(): void {
    this.clearTimeout();
  }

  onRetry(): void {
    this.hasTimedOut.set(false);
    this.startTimeout();
    this.retry.emit();
  }

  private startTimeout(): void {
    this.clearTimeout();
    this.timeoutId = setTimeout(() => {
      this.hasTimedOut.set(true);
    }, this.timeout());
  }

  private clearTimeout(): void {
    if (this.timeoutId !== null) {
      clearTimeout(this.timeoutId);
      this.timeoutId = null;
    }
  }
}
