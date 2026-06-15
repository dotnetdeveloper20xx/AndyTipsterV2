namespace AndyTipster.Application.GDPR.DTOs;

public record DataExportRequestDto
{
    public string Format { get; init; } = "json"; // "json" or "csv"
}

public record DataExportStatusDto
{
    public Guid RequestId { get; init; }
    public string Status { get; init; } = string.Empty; // "pending", "processing", "ready", "expired"
    public DateTime RequestedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? DownloadUrl { get; init; }
}

public record AccountDeletionRequestDto
{
    public string Password { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record AccountDeletionStatusDto
{
    public Guid UserId { get; init; }
    public string Status { get; init; } = string.Empty; // "scheduled", "cancelled", "completed"
    public DateTime ScheduledDeletionDate { get; init; }
    public DateTime? CancelledAt { get; init; }
    public bool CanCancel { get; init; }
}

public record RectificationRequestDto
{
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string? Bio { get; init; }
    public string? Timezone { get; init; }
}

public record ConsentRecordDto
{
    public Guid Id { get; init; }
    public string ConsentType { get; init; } = string.Empty;
    public bool IsGranted { get; init; }
    public DateTime GrantedAt { get; init; }
    public DateTime? RevokedAt { get; init; }
    public string? IpAddress { get; init; }
}

public record BreachNotificationDto
{
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public List<Guid>? AffectedUserIds { get; init; }
    public bool NotifyAll { get; init; }
}

public record DataProcessingRecordDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string ProcessingType { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string? LegalBasis { get; init; }
}
