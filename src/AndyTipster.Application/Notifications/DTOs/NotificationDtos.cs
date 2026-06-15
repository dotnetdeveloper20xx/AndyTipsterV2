namespace AndyTipster.Application.Notifications.DTOs;

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}

public record NotificationListDto
{
    public List<NotificationDto> Items { get; init; } = new();
    public int UnreadCount { get; init; }
}

public record NotificationPreferencesDto
{
    public Guid UserId { get; init; }
    public bool EmailEnabled { get; init; } = true;
    public bool WebPushEnabled { get; init; } = true;
    public bool TelegramEnabled { get; init; }
    public bool InAppEnabled { get; init; } = true;
    public bool NewTipEnabled { get; init; } = true;
    public bool TipResultEnabled { get; init; } = true;
    public bool PaymentAlertEnabled { get; init; } = true;
    public bool RenewalReminderEnabled { get; init; } = true;
    public bool BroadcastEnabled { get; init; } = true;
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public string? Timezone { get; init; }
}

public record UpdateNotificationPreferencesDto
{
    public bool? EmailEnabled { get; init; }
    public bool? WebPushEnabled { get; init; }
    public bool? TelegramEnabled { get; init; }
    public bool? InAppEnabled { get; init; }
    public bool? NewTipEnabled { get; init; }
    public bool? TipResultEnabled { get; init; }
    public bool? PaymentAlertEnabled { get; init; }
    public bool? RenewalReminderEnabled { get; init; }
    public bool? BroadcastEnabled { get; init; }
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public string? Timezone { get; init; }
}

public record BroadcastDto
{
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public List<string> Channels { get; init; } = new();
}

public record SendNotificationDto
{
    public Guid UserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
}

public record NotificationDeliveryStatus
{
    public string Channel { get; init; } = string.Empty;
    public bool IsDelivered { get; init; }
    public int RetryCount { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? DeliveredAt { get; init; }
}
