namespace AndyTipster.Application.Auth.DTOs;

public record RegisterResponse(
    Guid UserId,
    string Email,
    string Message
);
