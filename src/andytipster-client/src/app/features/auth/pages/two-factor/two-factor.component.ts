import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-two-factor',
  standalone: true,
  template: `
    <section class="auth-page">
      <h1>Two-Factor Authentication</h1>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TwoFactorComponent {}
