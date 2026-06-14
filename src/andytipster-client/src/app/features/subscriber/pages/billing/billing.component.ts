import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-billing',
  standalone: true,
  template: `<section><h1>Billing</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingComponent {}
