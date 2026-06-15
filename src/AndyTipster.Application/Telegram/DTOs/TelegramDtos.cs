namespace AndyTipster.Application.Telegram.DTOs;

public record TelegramLinkDto
{
    public string ConnectionCode { get; init; } = string.Empty;
    public string BotUsername { get; init; } = string.Empty;
    public string LinkUrl { get; init; } = string.Empty;
}

public record TelegramStatusDto
{
    public bool IsLinked { get; init; }
    public string? TelegramUsername { get; init; }
    public DateTime? LinkedAt { get; init; }
}

public record TelegramUnlinkDto
{
    public Guid UserId { get; init; }
}
