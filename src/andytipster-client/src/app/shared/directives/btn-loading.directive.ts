import { Directive, ElementRef, inject, Input, OnChanges, Renderer2, SimpleChanges } from '@angular/core';

@Directive({
  selector: '[btnLoading]',
  standalone: true,
})
export class BtnLoadingDirective implements OnChanges {
  @Input({ alias: 'btnLoading' }) isLoading = false;

  private readonly el = inject(ElementRef<HTMLButtonElement>);
  private readonly renderer = inject(Renderer2);
  private spinnerEl: HTMLElement | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ('isLoading' in changes) {
      this.updateState();
    }
  }

  private updateState(): void {
    const button = this.el.nativeElement;

    if (this.isLoading) {
      this.renderer.setAttribute(button, 'disabled', 'true');
      this.addSpinner();
    } else {
      this.renderer.removeAttribute(button, 'disabled');
      this.removeSpinner();
    }
  }

  private addSpinner(): void {
    if (this.spinnerEl) return;

    this.spinnerEl = this.renderer.createElement('span');
    this.renderer.addClass(this.spinnerEl, 'loading');
    this.renderer.addClass(this.spinnerEl, 'loading-spinner');
    this.renderer.addClass(this.spinnerEl, 'loading-xs');

    const button = this.el.nativeElement;
    this.renderer.insertBefore(button, this.spinnerEl, button.firstChild);
  }

  private removeSpinner(): void {
    if (!this.spinnerEl) return;

    const button = this.el.nativeElement;
    this.renderer.removeChild(button, this.spinnerEl);
    this.spinnerEl = null;
  }
}
