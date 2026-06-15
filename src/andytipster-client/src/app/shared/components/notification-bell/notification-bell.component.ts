import { Component, inject, OnInit, OnDestroy, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NotificationsService, NotificationDto } from '../../../core/services/notifications.service';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="relative">
      <!-- Bell button -->
      <button
        class="btn btn-ghost btn-circle"
        (click)="toggleDropdown()"
        [attr.aria-expanded]="isDropdownOpen()"
        aria-label="Notifications">
        <div class="indicator">
          <span class="text-lg">🔔</span>
          @if (unreadCount() > 0) {
            <span class="badge badge-xs badge-primary indicator-item">
              {{ displayCount() }}
            </span>
          }
        </div>
      </button>

      <!-- Dropdown -->
      @if (isDropdownOpen()) {
        <div
          class="absolute right-0 mt-2 w-80 bg-base-100 border border-base-300 rounded-lg shadow-xl z-50 max-h-96 overflow-hidden"
          role="menu"
          aria-label="Notification list">
          <!-- Header -->
          <div class="p-3 border-b border-base-300 flex justify-between items-center">
            <span class="font-semibold text-sm">Notifications</span>
            @if (unreadCount() > 0) {
              <button
                class="btn btn-ghost btn-xs"
                (click)="markAllAsRead()">
                Mark all read
              </button>
            }
          </div>

          <!-- Notification list -->
          <div class="overflow-y-auto max-h-72">
            @if (notifications().length === 0) {
              <div class="p-6 text-center text-base-content/60 text-sm">
                No notifications yet
              </div>
            }
            @for (notification of notifications(); track notification.id) {
              <button
                class="w-full text-left p-3 hover:bg-base-200 border-b border-base-200 transition-colors duration-150"
                [class.bg-primary/5]="!notification.isRead"
                (click)="onNotificationClick(notification)">
                <div class="flex gap-2">
                  <span class="text-lg">{{ getTypeIcon(notification.type) }}</span>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium truncate">{{ notification.title }}</p>
                    <p class="text-xs text-base-content/60 truncate">{{ notification.body }}</p>
                    <p class="text-xs text-base-content/40 mt-1">{{ formatTime(notification.createdAt) }}</p>
                  </div>
                  @if (!notification.isRead) {
                    <span class="w-2 h-2 bg-primary rounded-full mt-1 shrink-0"></span>
                  }
                </div>
              </button>
            }
          </div>
        </div>
      }
    </div>
  `
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  private readonly notificationsService = inject(NotificationsService);
  private pollSubscription?: Subscription;

  isDropdownOpen = signal(false);
  notifications = signal<NotificationDto[]>([]);
  unreadCount = signal(0);

  get displayCount(): () => string {
    return () => this.unreadCount() > 99 ? '99+' : this.unreadCount().toString();
  }

  ngOnInit(): void {
    this.loadNotifications();
    this.loadUnreadCount();

    // Poll for updates every 30 seconds
    this.pollSubscription = interval(30000).subscribe(() => {
      this.loadUnreadCount();
    });
  }

  ngOnDestroy(): void {
    this.pollSubscription?.unsubscribe();
  }

  toggleDropdown(): void {
    const newState = !this.isDropdownOpen();
    this.isDropdownOpen.set(newState);
    if (newState) {
      this.loadNotifications();
    }
  }

  onNotificationClick(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.notificationsService.markAsRead(notification.id).subscribe(() => {
        this.loadNotifications();
        this.loadUnreadCount();
      });
    }
    if (notification.actionUrl) {
      window.location.href = notification.actionUrl;
    }
  }

  markAllAsRead(): void {
    this.notificationsService.markAllAsRead().subscribe(() => {
      this.loadNotifications();
      this.loadUnreadCount();
    });
  }

  getTypeIcon(type: string): string {
    const icons: Record<string, string> = {
      'NewTip': '🏇',
      'TipResult': '📊',
      'PaymentFailed': '⚠️',
      'RenewalReminder': '🔄',
      'Broadcast': '📢',
      'System': 'ℹ️'
    };
    return icons[type] || '📌';
  }

  formatTime(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }

  private loadNotifications(): void {
    this.notificationsService.getNotifications(20).subscribe(result => {
      this.notifications.set(result.items);
      this.unreadCount.set(result.unreadCount);
    });
  }

  private loadUnreadCount(): void {
    this.notificationsService.getUnreadCount().subscribe(result => {
      this.unreadCount.set(result.rawCount);
    });
  }
}
