namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Response returned when a user initiates 2FA setup.
/// Contains QR code URI for authenticator app and manual entry key.
/// </summary>
public record Enable2FAResponse(
    string QrCodeUri,
    string ManualEntryKey
);
