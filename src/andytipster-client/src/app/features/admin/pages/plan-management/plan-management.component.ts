import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-plan-management',
  standalone: true,
  template: `<section><h1>Plan Management</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlanManagementComponent {}
