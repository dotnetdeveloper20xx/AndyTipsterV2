import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationDto {
  id: string;
  userId: string;
  type: string;
  title: string;
  body: string;
  actionUrl?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export interface NotificationListDto {
  items: NotificationDto[];
  unreadCount: number;
}

export interface UnreadCountDto {
  count: string;
  rawCount: number;
}

export interface NotificationPreferencesDto {
  userId: string;
  emailEnabled: boolean;
  webPushEnabled: boolean;
  telegramEnabled: boolean;
  inAppEnabled: boolean;
  newTipEnabled: boolean;
  tipResultEnabled: boolean;
  paymentAlertEnabled: boolean;
  renewalReminderEnabled: boolean;
  broadcastEnabled: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  timezone?: string;
}

export interface UpdateNotificationPreferencesDto {
  emailEnabled?: boolean;
  webPushEnabled?: boolean;
  telegramEnabled?: boolean;
  inAppEnabled?: boolean;
  newTipEnabled?: boolean;
  tipResultEnabled?: boolean;
  paymentAlertEnabled?: boolean;
  renewalReminderEnabled?: boolean;
  broadcastEnabled?: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  timezone?: string;
}

export interface BroadcastDto {
  title: string;
  body: string;
  actionUrl?: string;
  channels: string[];
}

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/notifications`;

  getNotifications(count = 20): Observable<NotificationListDto> {
    return this.http.get<NotificationListDto>(this.apiUrl, { params: { count: count.toString() } });
  }

  getUnreadCount(): Observable<UnreadCountDto> {
    return this.http.get<UnreadCountDto>(`${this.apiUrl}/unread-count`);
  }

  markAsRead(notificationId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${notificationId}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/read-all`, {});
  }

  sendBroadcast(dto: BroadcastDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/broadcast`, dto);
  }

  getPreferences(): Observable<NotificationPreferencesDto> {
    return this.http.get<NotificationPreferencesDto>(`${this.apiUrl}/preferences`);
  }

  updatePreferences(dto: UpdateNotificationPreferencesDto): Observable<NotificationPreferencesDto> {
    return this.http.patch<NotificationPreferencesDto>(`${this.apiUrl}/preferences`, dto);
  }
}
