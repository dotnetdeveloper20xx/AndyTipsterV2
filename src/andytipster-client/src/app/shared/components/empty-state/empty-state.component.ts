import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center gap-6 p-12 text-center" role="status" aria-live="polite">
      <!-- Inline SVG illustration -->
      <svg
        xmlns="http://www.w3.org/2000/svg"
        class="h-32 w-32 text-base-content/20"
        fill="none"
        viewBox="0 0 120 120"
        aria-hidden="true"
      >
        <rect x="20" y="30" width="80" height="60" rx="8" stroke="currentColor" stroke-width="2" fill="none" />
        <path d="M20 50h80" stroke="currentColor" stroke-width="1.5" />
        <circle cx="60" cy="72" r="8" stroke="currentColor" stroke-width="2" fill="none" />
        <path d="M56 72l3 3 5-6" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
        <path d="M40 40h40" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
        <path d="M45 45h30" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" opacity="0.5" />
      </svg>

      <!-- Message -->
      <div class="max-w-sm">
        <h3 class="text-lg font-semibold text-base-content">{{ title() }}</h3>
        <p class="mt-2 text-sm text-base-content/60">{{ message() }}</p>
      </div>

      <!-- CTA Button -->
      @if (ctaText() && ctaRoute()) {
        <a
          [routerLink]="ctaRoute()"
          class="btn btn-primary transition-standard"
        >
          {{ ctaText() }}
        </a>
      }
    </div>
  `,
})
export class EmptyStateComponent {
  /** Title displayed above the message */
  readonly title = input<string>('Nothing here yet');

  /** Descriptive message explaining why the state is empty */
  readonly message = input<string>('');

  /** Text for the call-to-action button */
  readonly ctaText = input<string>('');

  /** Router link destination for the CTA button */
  readonly ctaRoute = input<string>('');
}
