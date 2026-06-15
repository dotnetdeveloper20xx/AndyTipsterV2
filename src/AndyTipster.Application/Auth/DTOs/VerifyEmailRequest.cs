namespace AndyTipster.Application.Auth.DTOs;

public record VerifyEmailRequest(
    string UserId,
    string Token
);
