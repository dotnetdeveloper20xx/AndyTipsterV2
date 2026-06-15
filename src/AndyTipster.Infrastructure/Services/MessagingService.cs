using AndyTipster.Application.Community.DTOs;
using AndyTipster.Application.Community.Services;
using AndyTipster.Domain.Exceptions;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class MessagingService : IMessagingService
{
    private readonly AndyTipsterDbContext _context;

    // In-memory message store (would be DB table in production)
    private static readonly List<MessageEntity> _messages = new();
    private static readonly object _lock = new();

    public MessagingService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageDto dto, Guid senderId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["content"] = new[] { "Message content is required." }
            });

        var sender = await _context.Users.FindAsync(senderId)
            ?? throw new NotFoundException("Sender not found.");

        var recipient = await _context.Users.FindAsync(dto.RecipientId)
            ?? throw new NotFoundException("Recipient not found.");

        var message = new MessageEntity
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            SenderName = sender.DisplayName ?? sender.UserName ?? "",
            RecipientId = dto.RecipientId,
            RecipientName = recipient.DisplayName ?? recipient.UserName ?? "",
            Content = dto.Content.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        lock (_lock)
        {
            _messages.Add(message);
        }

        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            RecipientId = message.RecipientId,
            RecipientName = message.RecipientName,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }

    public Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid participantId, int page = 1, int pageSize = 20)
    {
        lock (_lock)
        {
            var messages = _messages
                .Where(m => (m.SenderId == userId && m.RecipientId == participantId)
                         || (m.SenderId == participantId && m.RecipientId == userId))
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    RecipientId = m.RecipientId,
                    RecipientName = m.RecipientName,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToList();

            return Task.FromResult(messages);
        }
    }

    public Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        lock (_lock)
        {
            var conversations = _messages
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Select(g =>
                {
                    var lastMessage = g.OrderByDescending(m => m.CreatedAt).First();
                    var participantId = g.Key;
                    var participantName = lastMessage.SenderId == participantId
                        ? lastMessage.SenderName
                        : lastMessage.RecipientName;

                    return new ConversationDto
                    {
                        ParticipantId = participantId,
                        ParticipantName = participantName,
                        ParticipantAvatarUrl = null,
                        LastMessage = lastMessage.Content,
                        LastMessageAt = lastMessage.CreatedAt,
                        UnreadCount = g.Count(m => m.RecipientId == userId && !m.IsRead)
                    };
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return Task.FromResult(conversations);
        }
    }

    public Task MarkConversationAsReadAsync(Guid userId, Guid participantId)
    {
        lock (_lock)
        {
            var unread = _messages
                .Where(m => m.SenderId == participantId && m.RecipientId == userId && !m.IsRead)
                .ToList();

            foreach (var msg in unread)
            {
                msg.IsRead = true;
            }
        }

        return Task.CompletedTask;
    }

    private class MessageEntity
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid RecipientId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
