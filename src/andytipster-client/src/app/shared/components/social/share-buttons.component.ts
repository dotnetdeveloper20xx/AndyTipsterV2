import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-share-buttons',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex gap-2 items-center" role="group" aria-label="Share options">
      <button
        class="btn btn-sm btn-outline"
        (click)="shareOnTwitter()"
        aria-label="Share on X (Twitter)">
        𝕏
      </button>
      <button
        class="btn btn-sm btn-outline"
        (click)="shareOnFacebook()"
        aria-label="Share on Facebook">
        f
      </button>
      <button
        class="btn btn-sm btn-outline"
        (click)="copyLink()"
        aria-label="Copy link">
        🔗
      </button>
    </div>
  `
})
export class ShareButtonsComponent {
  @Input() url = '';
  @Input() title = '';
  @Input() description = '';

  shareOnTwitter(): void {
    const text = encodeURIComponent(`${this.title} - ${this.description}`);
    const shareUrl = encodeURIComponent(this.url || window.location.href);
    window.open(`https://twitter.com/intent/tweet?text=${text}&url=${shareUrl}`, '_blank', 'width=600,height=400');
  }

  shareOnFacebook(): void {
    const shareUrl = encodeURIComponent(this.url || window.location.href);
    window.open(`https://www.facebook.com/sharer/sharer.php?u=${shareUrl}`, '_blank', 'width=600,height=400');
  }

  copyLink(): void {
    navigator.clipboard.writeText(this.url || window.location.href);
  }
}
