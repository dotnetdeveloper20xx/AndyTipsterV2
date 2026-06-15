using AndyTipster.Application.Telegram.DTOs;
using AndyTipster.Application.Telegram.Services;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly AndyTipsterDbContext _context;
    private readonly ILogger<TelegramService> _logger;

    // In-memory store for Telegram links (would be a DB table in production)
    private static readonly Dictionary<Guid, TelegramAccountLink> _links = new();
    private static readonly Dictionary<string, Guid> _connectionCodes = new();
    private static readonly object _lock = new();

    private const string BotUsername = "AndyTipsterBot";

    public TelegramService(AndyTipsterDbContext context, ILogger<TelegramService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<TelegramLinkDto> GenerateConnectionCodeAsync(Guid userId)
    {
        var code = GenerateUniqueCode();

        lock (_lock)
        {
            // Remove any existing codes for this user
            var existingCodes = _connectionCodes.Where(c => c.Value == userId).Select(c => c.Key).ToList();
            foreach (var existing in existingCodes)
                _connectionCodes.Remove(existing);

            _connectionCodes[code] = userId;
        }

        return Task.FromResult(new TelegramLinkDto
        {
            ConnectionCode = code,
            BotUsername = BotUsername,
            LinkUrl = $"https://t.me/{BotUsername}?start={code}"
        });
    }

    public Task<TelegramStatusDto> GetLinkStatusAsync(Guid userId)
    {
        lock (_lock)
        {
            if (_links.TryGetValue(userId, out var link))
            {
                return Task.FromResult(new TelegramStatusDto
                {
                    IsLinked = true,
                    TelegramUsername = link.Username,
                    LinkedAt = link.LinkedAt
                });
            }
        }

        return Task.FromResult(new TelegramStatusDto { IsLinked = false });
    }

    public Task LinkAccountAsync(string connectionCode, string telegramChatId, string telegramUsername)
    {
        lock (_lock)
        {
            if (!_connectionCodes.TryGetValue(connectionCode, out var userId))
                throw new InvalidOperationException("Invalid or expired connection code.");

            _connectionCodes.Remove(connectionCode);

            _links[userId] = new TelegramAccountLink
            {
                UserId = userId,
                ChatId = telegramChatId,
                Username = telegramUsername,
                LinkedAt = DateTime.UtcNow
            };
        }

        _logger.LogInformation("Telegram account linked: {Username} -> User {UserId}", telegramUsername, "userId");
        return Task.CompletedTask;
    }

    public Task UnlinkAccountAsync(Guid userId)
    {
        lock (_lock)
        {
            _links.Remove(userId);
        }

        _logger.LogInformation("Telegram account unlinked for user {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task SendTipMessageAsync(Guid userId, string formattedMessage)
    {
        lock (_lock)
        {
            if (!_links.TryGetValue(userId, out var link))
            {
                _logger.LogWarning("Cannot send Telegram message: user {UserId} not linked", userId);
                return Task.CompletedTask;
            }

            // Telegram Bot API stub - would POST to https://api.telegram.org/bot<token>/sendMessage
            _logger.LogInformation("Telegram: Sending message to chat {ChatId}: {Message}", link.ChatId, formattedMessage[..Math.Min(50, formattedMessage.Length)]);
        }

        return Task.CompletedTask;
    }

    public async Task BroadcastTipAsync(Guid tipId)
    {
        var tip = await _context.Tips.FindAsync(tipId);
        if (tip == null) return;

        var message = FormatTipMessage(tip);

        List<TelegramAccountLink> linkedUsers;
        lock (_lock)
        {
            linkedUsers = _links.Values.ToList();
        }

        foreach (var link in linkedUsers)
        {
            await SendTipMessageAsync(link.UserId, message);
        }

        _logger.LogInformation("Telegram broadcast for tip {TipId} sent to {Count} users", tipId, linkedUsers.Count);
    }

    private static string FormatTipMessage(Domain.Entities.Tip tip)
    {
        return $"🏇 *New Tip*\n\n" +
               $"📅 {tip.EventDate:d}\n" +
               $"🏁 {tip.RaceName}\n" +
               $"🎯 *{tip.Selection}*\n" +
               $"📊 Odds: {tip.Odds}\n" +
               $"💰 Stake: {tip.Stake}/10\n" +
               $"{(!string.IsNullOrEmpty(tip.Commentary) ? $"\n💬 {tip.Commentary}" : "")}";
    }

    private static string GenerateUniqueCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private class TelegramAccountLink
    {
        public Guid UserId { get; set; }
        public string ChatId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime LinkedAt { get; set; }
    }
}
