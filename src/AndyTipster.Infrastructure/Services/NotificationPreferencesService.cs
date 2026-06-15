using AndyTipster.Application.Notifications.DTOs;
using AndyTipster.Application.Notifications.Services;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class NotificationPreferencesService : INotificationPreferencesService
{
    private readonly AndyTipsterDbContext _context;

    // In-memory store for preferences (would be a DB table in production)
    private static readonly Dictionary<Guid, NotificationPreferencesDto> _preferences = new();
    private static readonly object _lock = new();

    public NotificationPreferencesService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    public Task<NotificationPreferencesDto> GetPreferencesAsync(Guid userId)
    {
        lock (_lock)
        {
            if (_preferences.TryGetValue(userId, out var prefs))
                return Task.FromResult(prefs);

            // Return defaults
            var defaults = new NotificationPreferencesDto
            {
                UserId = userId,
                EmailEnabled = true,
                WebPushEnabled = true,
                TelegramEnabled = false,
                InAppEnabled = true,
                NewTipEnabled = true,
                TipResultEnabled = true,
                PaymentAlertEnabled = true,
                RenewalReminderEnabled = true,
                BroadcastEnabled = true
            };

            _preferences[userId] = defaults;
            return Task.FromResult(defaults);
        }
    }

    public Task<NotificationPreferencesDto> UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesDto dto)
    {
        lock (_lock)
        {
            var existing = _preferences.GetValueOrDefault(userId) ?? new NotificationPreferencesDto { UserId = userId };

            var updated = existing with
            {
                EmailEnabled = dto.EmailEnabled ?? existing.EmailEnabled,
                WebPushEnabled = dto.WebPushEnabled ?? existing.WebPushEnabled,
                TelegramEnabled = dto.TelegramEnabled ?? existing.TelegramEnabled,
                InAppEnabled = dto.InAppEnabled ?? existing.InAppEnabled,
                NewTipEnabled = dto.NewTipEnabled ?? existing.NewTipEnabled,
                TipResultEnabled = dto.TipResultEnabled ?? existing.TipResultEnabled,
                PaymentAlertEnabled = dto.PaymentAlertEnabled ?? existing.PaymentAlertEnabled,
                RenewalReminderEnabled = dto.RenewalReminderEnabled ?? existing.RenewalReminderEnabled,
                BroadcastEnabled = dto.BroadcastEnabled ?? existing.BroadcastEnabled,
                QuietHoursStart = dto.QuietHoursStart ?? existing.QuietHoursStart,
                QuietHoursEnd = dto.QuietHoursEnd ?? existing.QuietHoursEnd,
                Timezone = dto.Timezone ?? existing.Timezone
            };

            _preferences[userId] = updated;
            return Task.FromResult(updated);
        }
    }

    public Task<bool> ShouldSendAsync(Guid userId, string notificationType, string channel)
    {
        lock (_lock)
        {
            if (!_preferences.TryGetValue(userId, out var prefs))
                return Task.FromResult(true); // Default to sending

            // Check channel toggle
            var channelEnabled = channel switch
            {
                "Email" => prefs.EmailEnabled,
                "WebPush" => prefs.WebPushEnabled,
                "Telegram" => prefs.TelegramEnabled,
                "InApp" => prefs.InAppEnabled,
                _ => true
            };

            if (!channelEnabled)
                return Task.FromResult(false);

            // Check category toggle
            var categoryEnabled = notificationType switch
            {
                "NewTip" => prefs.NewTipEnabled,
                "TipResult" => prefs.TipResultEnabled,
                "PaymentFailed" => prefs.PaymentAlertEnabled,
                "RenewalReminder" => prefs.RenewalReminderEnabled,
                "Broadcast" => prefs.BroadcastEnabled,
                _ => true
            };

            if (!categoryEnabled)
                return Task.FromResult(false);

            // Check quiet hours
            if (prefs.QuietHoursStart.HasValue && prefs.QuietHoursEnd.HasValue)
            {
                var now = TimeOnly.FromDateTime(DateTime.UtcNow);
                var start = prefs.QuietHoursStart.Value;
                var end = prefs.QuietHoursEnd.Value;

                if (start <= end)
                {
                    // e.g., 22:00 to 08:00 doesn't wrap around
                    if (now >= start && now <= end)
                        return Task.FromResult(false);
                }
                else
                {
                    // Wraps around midnight, e.g., 22:00 to 08:00
                    if (now >= start || now <= end)
                        return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }
    }
}
