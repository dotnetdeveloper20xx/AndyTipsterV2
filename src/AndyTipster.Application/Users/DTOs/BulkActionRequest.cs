namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Request DTO for bulk user actions.
/// </summary>
public class BulkActionRequest
{
    public List<Guid> UserIds { get; set; } = [];
    public string Action { get; set; } = string.Empty; // "suspend", "role_change", "export"
    public string? RoleName { get; set; } // Required for role_change action
}

/// <summary>
/// Response DTO for bulk action results.
/// </summary>
public class BulkActionResponse
{
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public List<BulkActionFailure> Failures { get; set; } = [];
}

public class BulkActionFailure
{
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
