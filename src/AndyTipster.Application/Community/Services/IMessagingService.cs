using AndyTipster.Application.Community.DTOs;

namespace AndyTipster.Application.Community.Services;

public interface IMessagingService
{
    Task<MessageDto> SendMessageAsync(SendMessageDto dto, Guid senderId);
    Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid participantId, int page = 1, int pageSize = 20);
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task MarkConversationAsReadAsync(Guid userId, Guid participantId);
}
