namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Response containing recovery codes generated during 2FA activation.
/// </summary>
public record RecoveryCodesResponse(IReadOnlyList<string> Codes);
