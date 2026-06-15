using AndyTipster.Application.Notifications.DTOs;

namespace AndyTipster.Application.Notifications.Services;

public interface INotificationService
{
    Task SendNotificationAsync(SendNotificationDto dto);
    Task SendBroadcastAsync(BroadcastDto dto);
    Task<NotificationListDto> GetUserNotificationsAsync(Guid userId, int count = 20);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task SendTipPublicationAlertAsync(Guid tipId);
    Task SendRenewalReminderAsync(Guid subscriptionId);
    Task SendPaymentFailureAlertAsync(Guid userId, string reason);
    Task SendTipResultUpdateAsync(Guid tipId);
}
