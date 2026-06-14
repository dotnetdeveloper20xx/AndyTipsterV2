import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-cms',
  standalone: true,
  template: `<section><h1>CMS</h1></section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CmsComponent {}
