using AndyTipster.Application.Telegram.DTOs;

namespace AndyTipster.Application.Telegram.Services;

public interface ITelegramService
{
    Task<TelegramLinkDto> GenerateConnectionCodeAsync(Guid userId);
    Task<TelegramStatusDto> GetLinkStatusAsync(Guid userId);
    Task LinkAccountAsync(string connectionCode, string telegramChatId, string telegramUsername);
    Task UnlinkAccountAsync(Guid userId);
    Task SendTipMessageAsync(Guid userId, string formattedMessage);
    Task BroadcastTipAsync(Guid tipId);
}
