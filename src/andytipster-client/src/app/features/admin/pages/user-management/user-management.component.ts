import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-user-management',
  standalone: true,
  template: `<section><h1>User Management</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserManagementComponent {}
