import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-landing',
  standalone: true,
  template: `
    <section class="landing-page">
      <h1>Welcome to AndyTipster</h1>
      <p>UK &amp; Ireland Horse Racing Tips</p>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent {}
