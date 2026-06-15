import { Directive, ElementRef, HostListener, inject, input } from '@angular/core';

/**
 * Card hover effect directive.
 * Applies scale + shadow lift on hover with 150ms transition.
 *
 * Usage:
 * <div appCardHover>Hoverable card</div>
 * <div appCardHover [hoverScale]="1.03">Custom scale</div>
 */
@Directive({
  selector: '[appCardHover]',
  standalone: true,
})
export class CardHoverDirective {
  private readonly el = inject(ElementRef<HTMLElement>);

  /** Scale factor on hover (default: 1.02) */
  readonly hoverScale = input(1.02);

  constructor() {
    const element = this.el.nativeElement;
    element.style.transition = 'transform 150ms ease, box-shadow 150ms ease';
    element.style.cursor = 'pointer';
  }

  @HostListener('mouseenter')
  onMouseEnter(): void {
    const element = this.el.nativeElement;
    element.style.transform = `scale(${this.hoverScale()})`;
    element.style.boxShadow = '0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)';
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    const element = this.el.nativeElement;
    element.style.transform = 'scale(1)';
    element.style.boxShadow = '';
  }
}
