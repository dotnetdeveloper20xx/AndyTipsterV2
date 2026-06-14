import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-pricing',
  standalone: true,
  template: `<section><h1>Pricing</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PricingComponent {}
