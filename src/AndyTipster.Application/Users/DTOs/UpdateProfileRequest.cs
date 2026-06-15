namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Request DTO for updating user profile.
/// </summary>
public class UpdateProfileRequest
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? TimeZone { get; set; }
}
