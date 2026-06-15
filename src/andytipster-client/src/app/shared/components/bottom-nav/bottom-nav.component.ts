import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-bottom-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav
      class="btm-nav btm-nav-md lg:hidden z-40 bg-secondary border-t border-secondary-content/10"
      aria-label="Mobile navigation"
    >
      @for (item of navItems; track item.route) {
        <a
          [routerLink]="item.route"
          routerLinkActive="active"
          [routerLinkActiveOptions]="{ exact: item.route === '/' }"
          class="text-secondary-content/60 hover:text-secondary-content"
          [attr.aria-label]="item.label"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            @switch (item.icon) {
              @case ('home') {
                <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
                <polyline points="9 22 9 12 15 12 15 22"/>
              }
              @case ('tips') {
                <path d="M12 2L2 7l10 5 10-5-10-5z"/>
                <path d="M2 17l10 5 10-5"/>
                <path d="M2 12l10 5 10-5"/>
              }
              @case ('plans') {
                <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                <line x1="3" y1="9" x2="21" y2="9"/>
                <line x1="9" y1="21" x2="9" y2="9"/>
              }
              @case ('profile') {
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                <circle cx="12" cy="7" r="4"/>
              }
              @case ('menu') {
                <line x1="3" y1="12" x2="21" y2="12"/>
                <line x1="3" y1="6" x2="21" y2="6"/>
                <line x1="3" y1="18" x2="21" y2="18"/>
              }
            }
          </svg>
          <span class="btm-nav-label text-xs">{{ item.label }}</span>
        </a>
      }
    </nav>
  `,
  styles: `
    :host {
      display: contents;
    }
    .btm-nav a.active {
      color: oklch(var(--a));
      border-top-color: oklch(var(--a));
      background: transparent;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BottomNavComponent {
  readonly navItems: NavItem[] = [
    { label: 'Home', route: '/', icon: 'home' },
    { label: 'Tips', route: '/subscriber/tips', icon: 'tips' },
    { label: 'Plans', route: '/pricing', icon: 'plans' },
    { label: 'Profile', route: '/subscriber/profile', icon: 'profile' },
    { label: 'Menu', route: '/subscriber/dashboard', icon: 'menu' },
  ];
}
