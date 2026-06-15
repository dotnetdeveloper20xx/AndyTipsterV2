import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-faq',
  standalone: true,
  template: `<section><h1>FAQ</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FaqComponent {}
