import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-tip-management',
  standalone: true,
  template: `<section><h1>Tip Management</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TipManagementComponent {}
