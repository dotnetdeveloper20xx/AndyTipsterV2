import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-register',
  standalone: true,
  template: `
    <section class="auth-page">
      <h1>Create Account</h1>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {}
