import {
  ChangeDetectionStrategy,
  Component,
  ContentChild,
  Input,
  TemplateRef,
} from '@angular/core';
import { CdkVirtualScrollViewport, CdkVirtualForOf, CdkFixedSizeVirtualScroll } from '@angular/cdk/scrolling';
import { NgTemplateOutlet } from '@angular/common';

/**
 * Generic virtual scrolling list component using Angular CDK.
 * Use this for any list that may exceed 100 items to maintain performance.
 *
 * Usage:
 * ```html
 * <app-virtual-list [items]="tips" [itemSize]="72" [containerHeight]="'600px'">
 *   <ng-template #itemTemplate let-item>
 *     <div class="p-4 border-b">{{ item.name }}</div>
 *   </ng-template>
 * </app-virtual-list>
 * ```
 */
@Component({
  selector: 'app-virtual-list',
  standalone: true,
  imports: [CdkVirtualScrollViewport, CdkFixedSizeVirtualScroll, CdkVirtualForOf, NgTemplateOutlet],
  template: `
    <cdk-virtual-scroll-viewport
      [itemSize]="itemSize"
      [style.height]="containerHeight"
      class="w-full"
    >
      <div *cdkVirtualFor="let item of items; trackBy: trackByFn" class="virtual-list-item">
        <ng-container
          [ngTemplateOutlet]="itemTemplate"
          [ngTemplateOutletContext]="{ $implicit: item }"
        ></ng-container>
      </div>
    </cdk-virtual-scroll-viewport>
  `,
  styles: `
    :host {
      display: block;
    }
    cdk-virtual-scroll-viewport {
      overflow-y: auto;
    }
    .virtual-list-item {
      contain: layout style;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VirtualListComponent<T> {
  @Input() items: T[] = [];
  @Input() itemSize = 64;
  @Input() containerHeight = '500px';
  @Input() trackByFn: (index: number, item: T) => unknown = (index) => index;

  @ContentChild('itemTemplate', { static: false }) itemTemplate!: TemplateRef<{ $implicit: T }>;
}
