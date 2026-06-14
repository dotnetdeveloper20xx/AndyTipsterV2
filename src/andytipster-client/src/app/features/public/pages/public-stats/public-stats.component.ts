import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-public-stats',
  standalone: true,
  template: `<section><h1>Performance Statistics</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicStatsComponent {}
