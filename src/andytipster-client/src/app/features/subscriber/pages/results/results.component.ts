import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-results',
  standalone: true,
  template: `<section><h1>P&amp;L Results</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResultsComponent {}
