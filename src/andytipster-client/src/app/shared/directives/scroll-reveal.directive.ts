import { Directive, ElementRef, inject, input, OnDestroy, OnInit } from '@angular/core';

/**
 * Scroll-triggered reveal animation directive using Intersection Observer.
 * Elements fade in and translate up when they enter the viewport.
 *
 * Usage:
 * <div appScrollReveal>Content</div>
 * <div appScrollReveal [revealDelay]="200">Delayed content</div>
 * <div appScrollReveal revealAnimation="slide-left">Slides from left</div>
 */
@Directive({
  selector: '[appScrollReveal]',
  standalone: true,
})
export class ScrollRevealDirective implements OnInit, OnDestroy {
  private readonly el = inject(ElementRef<HTMLElement>);
  private observer: IntersectionObserver | null = null;

  /** Delay in ms before animation starts */
  readonly revealDelay = input(0);

  /** Animation type: 'fade-up' | 'fade-in' | 'slide-left' | 'slide-right' | 'scale' */
  readonly revealAnimation = input<string>('fade-up');

  /** Threshold for intersection (0 to 1) */
  readonly revealThreshold = input(0.1);

  ngOnInit(): void {
    const element = this.el.nativeElement;

    // Apply initial hidden state
    element.style.opacity = '0';
    element.style.transition = `opacity 0.6s ease, transform 0.6s ease`;
    element.style.transitionDelay = `${this.revealDelay()}ms`;

    this.setInitialTransform(element);

    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            element.style.opacity = '1';
            element.style.transform = 'translate(0, 0) scale(1)';
            // Once revealed, stop observing
            this.observer?.unobserve(element);
          }
        });
      },
      { threshold: this.revealThreshold() }
    );

    this.observer.observe(element);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    this.observer = null;
  }

  private setInitialTransform(element: HTMLElement): void {
    switch (this.revealAnimation()) {
      case 'fade-up':
        element.style.transform = 'translateY(30px)';
        break;
      case 'fade-in':
        element.style.transform = 'translateY(0)';
        break;
      case 'slide-left':
        element.style.transform = 'translateX(-30px)';
        break;
      case 'slide-right':
        element.style.transform = 'translateX(30px)';
        break;
      case 'scale':
        element.style.transform = 'scale(0.9)';
        break;
      default:
        element.style.transform = 'translateY(30px)';
    }
  }
}
