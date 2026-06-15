namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Paginated response DTO for user activity log.
/// </summary>
public class ActivityLogResponse
{
    public List<ActivityLogEntry> Entries { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ActivityLogEntry
{
    public long Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
