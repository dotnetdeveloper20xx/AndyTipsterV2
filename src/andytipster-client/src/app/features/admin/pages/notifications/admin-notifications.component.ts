import { Component, inject, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationsService, BroadcastDto } from '../../../../core/services/notifications.service';

@Component({
  selector: 'app-admin-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="container mx-auto p-4 max-w-3xl">
      <h1 class="text-2xl font-bold mb-6">Broadcast Notifications</h1>

      <div class="card bg-base-100 shadow-md">
        <div class="card-body">
          <h2 class="card-title text-lg">Send Broadcast to All Subscribers</h2>
          <p class="text-sm text-base-content/60 mb-4">This will send a notification to all active subscribers within 5 minutes.</p>

          <div class="form-control mb-4">
            <label class="label" for="broadcast-title">
              <span class="label-text">Title</span>
            </label>
            <input
              id="broadcast-title"
              type="text"
              class="input input-bordered"
              [(ngModel)]="title"
              placeholder="Notification title"
              maxlength="100" />
          </div>

          <div class="form-control mb-4">
            <label class="label" for="broadcast-body">
              <span class="label-text">Message</span>
            </label>
            <textarea
              id="broadcast-body"
              class="textarea textarea-bordered h-32"
              [(ngModel)]="body"
              placeholder="Notification message..."
              maxlength="500"></textarea>
          </div>

          <div class="form-control mb-4">
            <label class="label" for="broadcast-url">
              <span class="label-text">Action URL (optional)</span>
            </label>
            <input
              id="broadcast-url"
              type="text"
              class="input input-bordered"
              [(ngModel)]="actionUrl"
              placeholder="/tips or https://..." />
          </div>

          <div class="form-control mb-4">
            <label class="label">
              <span class="label-text">Channels</span>
            </label>
            <div class="flex flex-wrap gap-4">
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" class="checkbox checkbox-primary checkbox-sm" [(ngModel)]="channelEmail" />
                <span class="text-sm">Email</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" class="checkbox checkbox-primary checkbox-sm" [(ngModel)]="channelInApp" />
                <span class="text-sm">In-App</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" class="checkbox checkbox-primary checkbox-sm" [(ngModel)]="channelWebPush" />
                <span class="text-sm">Web Push</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" class="checkbox checkbox-primary checkbox-sm" [(ngModel)]="channelTelegram" />
                <span class="text-sm">Telegram</span>
              </label>
            </div>
          </div>

          @if (successMessage()) {
            <div class="alert alert-success text-sm mb-4">
              <span>{{ successMessage() }}</span>
            </div>
          }

          @if (errorMessage()) {
            <div class="alert alert-error text-sm mb-4">
              <span>{{ errorMessage() }}</span>
            </div>
          }

          <div class="card-actions justify-end">
            <button
              class="btn btn-primary"
              (click)="sendBroadcast()"
              [disabled]="isSending() || !title.trim() || !body.trim()">
              @if (isSending()) {
                <span class="loading loading-spinner loading-sm"></span>
              }
              Send Broadcast
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminNotificationsComponent {
  private readonly notificationsService = inject(NotificationsService);

  title = '';
  body = '';
  actionUrl = '';
  channelEmail = true;
  channelInApp = true;
  channelWebPush = false;
  channelTelegram = false;

  isSending = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  sendBroadcast(): void {
    const channels: string[] = [];
    if (this.channelEmail) channels.push('Email');
    if (this.channelInApp) channels.push('InApp');
    if (this.channelWebPush) channels.push('WebPush');
    if (this.channelTelegram) channels.push('Telegram');

    const dto: BroadcastDto = {
      title: this.title.trim(),
      body: this.body.trim(),
      actionUrl: this.actionUrl.trim() || undefined,
      channels
    };

    this.isSending.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');

    this.notificationsService.sendBroadcast(dto).subscribe({
      next: (result) => {
        this.successMessage.set(result.message);
        this.title = '';
        this.body = '';
        this.actionUrl = '';
        this.isSending.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.detail || 'Failed to send broadcast.');
        this.isSending.set(false);
      }
    });
  }
}
