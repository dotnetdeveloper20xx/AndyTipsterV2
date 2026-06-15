import { Directive, ElementRef, inject, isDevMode, OnInit } from '@angular/core';

/**
 * Directive that enforces alt text on <img> elements.
 * In development mode, logs a console warning if an image has an empty or missing alt attribute.
 * Applies automatically to all <img> elements when imported.
 *
 * Usage: Import RequiredAltDirective in the component's imports array.
 * It will automatically apply to all img tags in that component's template.
 */
@Directive({
  selector: 'img',
  standalone: true,
})
export class RequiredAltDirective implements OnInit {
  private readonly el = inject(ElementRef<HTMLImageElement>);

  ngOnInit(): void {
    if (isDevMode()) {
      this.checkAltAttribute();
    }
  }

  private checkAltAttribute(): void {
    const imgElement = this.el.nativeElement;
    const alt = imgElement.getAttribute('alt');

    if (alt === null || alt.trim() === '') {
      const src = imgElement.getAttribute('src') || 'unknown source';
      console.warn(
        `[Accessibility] Image is missing alt text. ` +
          `All images must have descriptive alt attributes for screen readers. ` +
          `Source: "${src}"`
      );
    }
  }
}
