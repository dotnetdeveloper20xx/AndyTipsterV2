namespace AndyTipster.Application.Audit.Services;

/// <summary>
/// Service interface for append-only audit logging.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an admin action to the audit trail. Append-only — no updates or deletes.
    /// </summary>
    Task LogActionAsync(
        Guid actorUserId,
        string actionType,
        string targetEntity,
        string? targetEntityId = null,
        string? beforeJson = null,
        string? afterJson = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated audit logs with optional filters. Read-only access.
    /// </summary>
    Task<AuditLogResponse> GetAuditLogsAsync(AuditLogRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request DTO for querying audit logs.
/// </summary>
public class AuditLogRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
    public string? ActionTypeFilter { get; set; }
    public string? ActorFilter { get; set; }
    public string? TargetEntityFilter { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string SortBy { get; set; } = "Timestamp";
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Paginated response DTO for audit logs.
/// </summary>
public class AuditLogResponse
{
    public List<AuditLogEntryDto> Entries { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AuditLogEntryDto
{
    public long Id { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetEntityId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
