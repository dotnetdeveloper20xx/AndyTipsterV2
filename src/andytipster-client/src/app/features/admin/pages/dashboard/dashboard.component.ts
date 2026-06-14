import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  template: `<section><h1>Admin Dashboard</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {}
