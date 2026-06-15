using AndyTipster.Application.Notifications.DTOs;
using AndyTipster.Application.Notifications.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AndyTipsterDbContext _context;
    private readonly INotificationPreferencesService _preferencesService;
    private readonly ILogger<NotificationService> _logger;

    private const int MaxRetries = 3;

    public NotificationService(
        AndyTipsterDbContext context,
        INotificationPreferencesService preferencesService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _preferencesService = preferencesService;
        _logger = logger;
    }

    public async Task SendNotificationAsync(SendNotificationDto dto)
    {
        // Create in-app notification
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = Enum.Parse<NotificationType>(dto.Type),
            Title = dto.Title,
            Body = dto.Body,
            ActionUrl = dto.ActionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Attempt multi-channel delivery with retry
        await DeliverToChannelsAsync(dto);
    }

    public async Task SendBroadcastAsync(BroadcastDto dto)
    {
        var activeSubscribers = await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in activeSubscribers)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Broadcast,
                Title = dto.Title,
                Body = dto.Body,
                ActionUrl = dto.ActionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Broadcast sent to {Count} subscribers: {Title}", activeSubscribers.Count, dto.Title);
    }

    public async Task<NotificationListDto> GetUserNotificationsAsync(Guid userId, int count = 20)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type.ToString(),
                Title = n.Title,
                Body = n.Body,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync();

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return new NotificationListDto
        {
            Items = notifications,
            UnreadCount = unreadCount
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SendTipPublicationAlertAsync(Guid tipId)
    {
        var tip = await _context.Tips.FindAsync(tipId);
        if (tip == null) return;

        var subscribers = await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in subscribers)
        {
            if (!await _preferencesService.ShouldSendAsync(userId, "NewTip", "InApp"))
                continue;

            await SendNotificationAsync(new SendNotificationDto
            {
                UserId = userId,
                Type = NotificationType.NewTip.ToString(),
                Title = "New Tip Published",
                Body = $"A new tip has been published: {tip.Selection} @ {tip.Odds}",
                ActionUrl = $"/tips/{tipId}"
            });
        }
    }

    public async Task SendRenewalReminderAsync(Guid subscriptionId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription == null) return;

        await SendNotificationAsync(new SendNotificationDto
        {
            UserId = subscription.UserId,
            Type = NotificationType.RenewalReminder.ToString(),
            Title = "Subscription Renewal Reminder",
            Body = $"Your {subscription.Plan.Name} subscription will renew on {subscription.CurrentPeriodEnd:d}.",
            ActionUrl = "/settings/billing"
        });
    }

    public async Task SendPaymentFailureAlertAsync(Guid userId, string reason)
    {
        await SendNotificationAsync(new SendNotificationDto
        {
            UserId = userId,
            Type = NotificationType.PaymentFailed.ToString(),
            Title = "Payment Failed",
            Body = $"Your payment could not be processed: {reason}. Please update your payment method.",
            ActionUrl = "/settings/billing"
        });
    }

    public async Task SendTipResultUpdateAsync(Guid tipId)
    {
        var tip = await _context.Tips.FindAsync(tipId);
        if (tip == null) return;

        var subscribers = await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in subscribers)
        {
            if (!await _preferencesService.ShouldSendAsync(userId, "TipResult", "InApp"))
                continue;

            await SendNotificationAsync(new SendNotificationDto
            {
                UserId = userId,
                Type = NotificationType.TipResult.ToString(),
                Title = "Tip Result Updated",
                Body = $"{tip.Selection}: Result is {tip.Result}",
                ActionUrl = $"/tips/{tipId}"
            });
        }
    }

    private async Task DeliverToChannelsAsync(SendNotificationDto dto)
    {
        var channels = new[] { "Email", "WebPush", "Telegram" };

        foreach (var channel in channels)
        {
            if (!await _preferencesService.ShouldSendAsync(dto.UserId, dto.Type, channel))
                continue;

            var delivered = false;
            for (int attempt = 1; attempt <= MaxRetries && !delivered; attempt++)
            {
                try
                {
                    await DeliverToChannelAsync(channel, dto);
                    delivered = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deliver notification via {Channel}, attempt {Attempt}/{MaxRetries}",
                        channel, attempt, MaxRetries);

                    if (attempt < MaxRetries)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        await Task.Delay(delay);
                    }
                }
            }

            if (!delivered)
            {
                _logger.LogError("Failed to deliver notification via {Channel} after {MaxRetries} retries for user {UserId}",
                    channel, MaxRetries, dto.UserId);
            }
        }
    }

    private Task DeliverToChannelAsync(string channel, SendNotificationDto dto)
    {
        // Stub implementations for external delivery channels
        return channel switch
        {
            "Email" => SendEmailNotificationAsync(dto),
            "WebPush" => SendWebPushNotificationAsync(dto),
            "Telegram" => SendTelegramNotificationAsync(dto),
            _ => Task.CompletedTask
        };
    }

    private Task SendEmailNotificationAsync(SendNotificationDto dto)
    {
        // SendGrid stub - would integrate with SendGrid API
        _logger.LogInformation("SendGrid: Sending email notification to user {UserId}: {Title}", dto.UserId, dto.Title);
        return Task.CompletedTask;
    }

    private Task SendWebPushNotificationAsync(SendNotificationDto dto)
    {
        // Web Push stub - would send browser push notification
        _logger.LogInformation("WebPush: Sending push notification to user {UserId}: {Title}", dto.UserId, dto.Title);
        return Task.CompletedTask;
    }

    private Task SendTelegramNotificationAsync(SendNotificationDto dto)
    {
        // Telegram stub - would send via Telegram Bot API
        _logger.LogInformation("Telegram: Sending notification to user {UserId}: {Title}", dto.UserId, dto.Title);
        return Task.CompletedTask;
    }
}
