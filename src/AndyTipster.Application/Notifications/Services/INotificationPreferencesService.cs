using AndyTipster.Application.Notifications.DTOs;

namespace AndyTipster.Application.Notifications.Services;

public interface INotificationPreferencesService
{
    Task<NotificationPreferencesDto> GetPreferencesAsync(Guid userId);
    Task<NotificationPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesDto dto);
    Task<bool> ShouldSendAsync(Guid userId, string notificationType, string channel);
}
