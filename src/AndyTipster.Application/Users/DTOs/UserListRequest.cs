namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Request DTO for paginated user listing with search and filters.
/// </summary>
public class UserListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
    public string? RoleFilter { get; set; }
    public string? PlanFilter { get; set; }
    public string? StatusFilter { get; set; }
    public DateTime? RegisteredFrom { get; set; }
    public DateTime? RegisteredTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "desc";
}
