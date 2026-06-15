import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  template: `
    <div class="toast toast-end toast-bottom z-[100]" aria-live="polite" aria-atomic="true">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="alert shadow-lg animate-slide-in-right"
          [class.alert-success]="toast.type === 'success'"
          [class.alert-error]="toast.type === 'error'"
          [class.alert-info]="toast.type === 'info'"
          [class.alert-warning]="toast.type === 'warning'"
          role="alert"
        >
          <span>{{ toast.message }}</span>
          <button
            class="btn btn-ghost btn-xs"
            (click)="toastService.dismiss(toast.id)"
            aria-label="Dismiss notification"
          >
            ✕
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slideInRight {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .animate-slide-in-right {
      animation: slideInRight 0.3s ease-out;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);
}
