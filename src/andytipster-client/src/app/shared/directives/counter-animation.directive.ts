import { Directive, ElementRef, inject, input, OnDestroy, OnInit } from '@angular/core';

/**
 * Number counter animation directive.
 * Animates a number from zero to target over 1 second when the element enters the viewport.
 *
 * Usage:
 * <span appCounterAnimation [counterTarget]="1500"></span>
 * <span appCounterAnimation [counterTarget]="85.5" [counterDecimals]="1" counterSuffix="%"></span>
 */
@Directive({
  selector: '[appCounterAnimation]',
  standalone: true,
})
export class CounterAnimationDirective implements OnInit, OnDestroy {
  private readonly el = inject(ElementRef<HTMLElement>);
  private observer: IntersectionObserver | null = null;
  private animationFrame: number | null = null;

  /** Target number to animate to */
  readonly counterTarget = input(0);

  /** Number of decimal places */
  readonly counterDecimals = input(0);

  /** Duration in ms (default: 1000ms) */
  readonly counterDuration = input(1000);

  /** Prefix (e.g., '£', '$') */
  readonly counterPrefix = input('');

  /** Suffix (e.g., '%', '+') */
  readonly counterSuffix = input('');

  ngOnInit(): void {
    const element = this.el.nativeElement;
    element.textContent = `${this.counterPrefix()}0${this.counterSuffix()}`;

    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            this.startAnimation();
            this.observer?.unobserve(element);
          }
        });
      },
      { threshold: 0.1 }
    );

    this.observer.observe(element);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    this.observer = null;
    if (this.animationFrame !== null) {
      cancelAnimationFrame(this.animationFrame);
    }
  }

  private startAnimation(): void {
    const target = this.counterTarget();
    const duration = this.counterDuration();
    const decimals = this.counterDecimals();
    const prefix = this.counterPrefix();
    const suffix = this.counterSuffix();
    const element = this.el.nativeElement;

    const startTime = performance.now();

    const update = (currentTime: number): void => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);

      // Ease-out cubic for smooth deceleration
      const easedProgress = 1 - Math.pow(1 - progress, 3);
      const currentValue = target * easedProgress;

      element.textContent = `${prefix}${currentValue.toFixed(decimals)}${suffix}`;

      if (progress < 1) {
        this.animationFrame = requestAnimationFrame(update);
      } else {
        element.textContent = `${prefix}${target.toFixed(decimals)}${suffix}`;
      }
    };

    this.animationFrame = requestAnimationFrame(update);
  }
}
