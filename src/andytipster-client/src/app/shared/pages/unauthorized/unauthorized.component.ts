import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="unauthorized-page">
      <h1>Access Denied</h1>
      <p>You do not have permission to access this page.</p>
      <a routerLink="/">Return to Home</a>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnauthorizedComponent {}
