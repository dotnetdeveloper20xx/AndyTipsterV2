import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-tips-feed',
  standalone: true,
  template: `<section><h1>Tips Feed</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TipsFeedComponent {}
