import { ChangeDetectionStrategy, Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CmsService, NavigationMenuDto, MenuItemDto } from '../../../../core/services/cms.service';

@Component({
  selector: 'app-navigation-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold">Navigation & Menu Management</h1>
        <button class="btn btn-primary btn-sm" (click)="showCreateMenu = true">+ New Menu</button>
      </div>

      <!-- Create menu dialog -->
      @if (showCreateMenu) {
        <div class="card bg-base-100 shadow-lg mb-4 p-4">
          <h3 class="font-semibold mb-3">Create Menu</h3>
          <div class="flex gap-3">
            <input class="input input-sm input-bordered flex-1" [(ngModel)]="newMenuName" placeholder="Menu name" aria-label="Menu name" />
            <select class="select select-sm select-bordered" [(ngModel)]="newMenuLocation" aria-label="Menu location">
              <option value="header">Header</option>
              <option value="footer">Footer</option>
              <option value="sidebar">Sidebar</option>
              <option value="mobile">Mobile</option>
            </select>
            <button class="btn btn-sm btn-primary" (click)="createMenu()">Create</button>
            <button class="btn btn-sm btn-ghost" (click)="showCreateMenu = false">Cancel</button>
          </div>
        </div>
      }

      <!-- Menu tabs -->
      <div role="tablist" class="tabs tabs-bordered mb-4">
        @for (menu of menus(); track menu.id) {
          <button
            role="tab"
            class="tab"
            [class.tab-active]="selectedMenu()?.id === menu.id"
            (click)="selectMenu(menu)"
          >
            {{ menu.name }} ({{ menu.location }})
          </button>
        }
      </div>

      <!-- Selected menu tree editor -->
      @if (selectedMenu()) {
        <div class="card bg-base-100 shadow-sm p-4">
          <div class="flex justify-between items-center mb-4">
            <div>
              <h2 class="font-semibold">{{ selectedMenu()!.name }}</h2>
              <p class="text-xs text-base-content/60">Location: {{ selectedMenu()!.location }}</p>
            </div>
            <div class="flex gap-2">
              <button class="btn btn-sm btn-primary" (click)="addItem()">+ Add Item</button>
              <button class="btn btn-sm btn-error btn-outline" (click)="deleteMenu()">Delete Menu</button>
            </div>
          </div>

          <!-- Tree view -->
          <div class="pl-2">
            @for (item of selectedMenu()!.items; track item.id; let i = $index) {
              <ng-container *ngTemplateOutlet="menuItemTemplate; context: { $implicit: item, index: i, depth: 0 }"></ng-container>
            }

            @if (selectedMenu()!.items.length === 0) {
              <p class="text-base-content/50 text-sm py-4">No menu items. Click "+ Add Item" to get started.</p>
            }
          </div>
        </div>

        <!-- Item editor -->
        @if (editingItem()) {
          <div class="card bg-base-100 shadow-sm p-4 mt-4">
            <h3 class="font-semibold mb-3">Edit Menu Item</h3>
            <div class="grid grid-cols-2 gap-3">
              <div class="form-control">
                <label class="label"><span class="label-text text-xs">Label</span></label>
                <input class="input input-sm input-bordered" [(ngModel)]="editingItem()!.label" aria-label="Item label" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-xs">URL</span></label>
                <input class="input input-sm input-bordered" [(ngModel)]="editingItem()!.url" aria-label="Item URL" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-xs">Icon</span></label>
                <input class="input input-sm input-bordered" [(ngModel)]="editingItem()!.icon" placeholder="Optional" aria-label="Item icon" />
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-xs">Target</span></label>
                <select class="select select-sm select-bordered" [(ngModel)]="editingItem()!.target" aria-label="Link target">
                  <option value="_self">Same window</option>
                  <option value="_blank">New window</option>
                </select>
              </div>
              <div class="form-control">
                <label class="label"><span class="label-text text-xs">Required Role</span></label>
                <input class="input input-sm input-bordered" [(ngModel)]="editingItem()!.requiredRole" placeholder="Optional" aria-label="Required role" />
              </div>
              <div class="form-control flex items-end">
                <label class="label cursor-pointer gap-2">
                  <span class="label-text text-xs">Visible</span>
                  <input type="checkbox" class="toggle toggle-sm" [(ngModel)]="editingItem()!.isVisible" aria-label="Visible" />
                </label>
              </div>
            </div>
            <div class="flex gap-2 mt-4">
              <button class="btn btn-sm btn-primary" (click)="saveItem()">Save Item</button>
              <button class="btn btn-sm btn-ghost" (click)="editingItem.set(null)">Cancel</button>
            </div>
          </div>
        }
      }
    </div>

    <!-- Recursive menu item template -->
    <ng-template #menuItemTemplate let-item let-index="index" let-depth="depth">
      <div class="flex items-center gap-2 py-1.5 px-2 rounded hover:bg-base-200 group" [style.padding-left.px]="depth * 24 + 8">
        <span class="text-base-content/40 cursor-move">⠿</span>
        <span class="flex-1 text-sm">{{ item.label }}</span>
        <span class="text-xs text-base-content/50">{{ item.url }}</span>
        @if (!item.isVisible) {
          <span class="badge badge-xs badge-ghost">Hidden</span>
        }
        @if (item.requiredRole) {
          <span class="badge badge-xs badge-info">{{ item.requiredRole }}</span>
        }
        <div class="opacity-0 group-hover:opacity-100 flex gap-1">
          <button class="btn btn-xs btn-ghost" (click)="moveItemUp(item)" [disabled]="index === 0" aria-label="Move up">⬆</button>
          <button class="btn btn-xs btn-ghost" (click)="moveItemDown(item)" aria-label="Move down">⬇</button>
          <button class="btn btn-xs btn-ghost" (click)="editItem(item)" aria-label="Edit item">✏️</button>
          <button class="btn btn-xs btn-ghost btn-error" (click)="removeItem(item)" aria-label="Remove item">✕</button>
        </div>
      </div>
      @for (child of item.children; track child.id; let ci = $index) {
        <ng-container *ngTemplateOutlet="menuItemTemplate; context: { $implicit: child, index: ci, depth: depth + 1 }"></ng-container>
      }
    </ng-template>
  `
})
export class NavigationEditorComponent implements OnInit {
  private readonly cmsService = inject(CmsService);

  menus = signal<NavigationMenuDto[]>([]);
  selectedMenu = signal<NavigationMenuDto | null>(null);
  editingItem = signal<MenuItemDto | null>(null);

  showCreateMenu = false;
  newMenuName = '';
  newMenuLocation = 'header';

  ngOnInit(): void {
    this.loadMenus();
  }

  loadMenus(): void {
    this.cmsService.getMenus().subscribe(menus => {
      this.menus.set(menus);
      if (menus.length > 0 && !this.selectedMenu()) {
        this.selectMenu(menus[0]);
      }
    });
  }

  selectMenu(menu: NavigationMenuDto): void {
    this.selectedMenu.set(menu);
    this.editingItem.set(null);
  }

  createMenu(): void {
    if (!this.newMenuName) return;
    this.cmsService.createMenu({ name: this.newMenuName, location: this.newMenuLocation }).subscribe(() => {
      this.showCreateMenu = false;
      this.newMenuName = '';
      this.loadMenus();
    });
  }

  deleteMenu(): void {
    const menu = this.selectedMenu();
    if (!menu) return;
    if (confirm(`Delete menu "${menu.name}"?`)) {
      this.cmsService.deleteMenu(menu.id).subscribe(() => {
        this.selectedMenu.set(null);
        this.loadMenus();
      });
    }
  }

  addItem(): void {
    const newItem: MenuItemDto = {
      id: crypto.randomUUID(),
      label: 'New Item',
      url: '/',
      icon: undefined,
      target: '_self',
      sortOrder: this.selectedMenu()?.items.length ?? 0,
      isVisible: true,
      requiredRole: undefined,
      requiredSubscriptionStatus: undefined,
      children: [],
    };
    this.editingItem.set(newItem);
  }

  editItem(item: MenuItemDto): void {
    this.editingItem.set({ ...item });
  }

  saveItem(): void {
    const item = this.editingItem();
    const menu = this.selectedMenu();
    if (!item || !menu) return;

    // Check if this is a new or existing item
    const exists = this.findItem(menu.items, item.id);
    if (exists) {
      // Update in place
      Object.assign(exists, item);
    } else {
      // Add to menu
      menu.items.push(item);
    }

    // Save entire menu structure
    this.cmsService.updateMenu(menu.id, { items: menu.items }).subscribe(updated => {
      this.selectedMenu.set(updated);
      this.editingItem.set(null);
    });
  }

  removeItem(item: MenuItemDto): void {
    const menu = this.selectedMenu();
    if (!menu) return;
    if (!confirm(`Remove "${item.label}"?`)) return;

    const filtered = this.removeFromTree(menu.items, item.id);
    this.cmsService.updateMenu(menu.id, { items: filtered }).subscribe(updated => {
      this.selectedMenu.set(updated);
    });
  }

  moveItemUp(item: MenuItemDto): void {
    const menu = this.selectedMenu();
    if (!menu) return;
    const items = [...menu.items];
    const idx = items.findIndex(i => i.id === item.id);
    if (idx > 0) {
      [items[idx - 1], items[idx]] = [items[idx], items[idx - 1]];
      this.cmsService.updateMenu(menu.id, { items }).subscribe(updated => {
        this.selectedMenu.set(updated);
      });
    }
  }

  moveItemDown(item: MenuItemDto): void {
    const menu = this.selectedMenu();
    if (!menu) return;
    const items = [...menu.items];
    const idx = items.findIndex(i => i.id === item.id);
    if (idx < items.length - 1) {
      [items[idx], items[idx + 1]] = [items[idx + 1], items[idx]];
      this.cmsService.updateMenu(menu.id, { items }).subscribe(updated => {
        this.selectedMenu.set(updated);
      });
    }
  }

  private findItem(items: MenuItemDto[], id: string): MenuItemDto | null {
    for (const item of items) {
      if (item.id === id) return item;
      const found = this.findItem(item.children, id);
      if (found) return found;
    }
    return null;
  }

  private removeFromTree(items: MenuItemDto[], id: string): MenuItemDto[] {
    return items
      .filter(i => i.id !== id)
      .map(i => ({ ...i, children: this.removeFromTree(i.children, id) }));
  }
}
