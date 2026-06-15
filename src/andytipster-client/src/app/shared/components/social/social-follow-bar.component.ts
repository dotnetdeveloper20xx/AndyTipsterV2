import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface SocialLink {
  platform: string;
  url: string;
  label: string;
  isVisible: boolean;
}

interface SocialFollowBar {
  links: SocialLink[];
  isVisible: boolean;
}

@Component({
  selector: 'app-social-follow-bar',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (followBar && followBar.isVisible) {
      <div class="flex flex-wrap gap-2 items-center" role="navigation" aria-label="Social media links">
        @for (link of followBar.links; track link.platform) {
          @if (link.isVisible) {
            <a
              [href]="link.url"
              target="_blank"
              rel="noopener noreferrer"
              class="btn btn-sm btn-ghost gap-1"
              [attr.aria-label]="link.label">
              <span class="text-sm">{{ getPlatformIcon(link.platform) }}</span>
              <span class="hidden sm:inline text-xs">{{ link.label }}</span>
            </a>
          }
        }
      </div>
    }
  `
})
export class SocialFollowBarComponent implements OnInit {
  private readonly http = inject(HttpClient);
  followBar: SocialFollowBar | null = null;

  ngOnInit(): void {
    this.http.get<SocialFollowBar>(`${environment.apiUrl}/api/social/follow-bar`)
      .subscribe(data => this.followBar = data);
  }

  getPlatformIcon(platform: string): string {
    const icons: Record<string, string> = {
      'Twitter': '𝕏',
      'Facebook': 'f',
      'Instagram': '📷',
      'Telegram': '✈️',
      'YouTube': '▶️'
    };
    return icons[platform] || '🔗';
  }
}
