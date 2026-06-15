using AndyTipster.Application.HelpBot.DTOs;

namespace AndyTipster.Application.HelpBot.Services;

public interface IHelpBotService
{
    Task<BotResponseDto> ProcessMessageAsync(UserMessageDto dto, Guid? userId);
    Task<ConversationDto> GetConversationAsync(Guid sessionId);
    Task<List<FlowDto>> GetFlowsAsync();
    Task<FlowDto> CreateFlowAsync(CreateFlowDto dto);
    Task<FlowDto> UpdateFlowAsync(Guid flowId, UpdateFlowDto dto);
    Task DeleteFlowAsync(Guid flowId);
    Task EscalateToTicketAsync(Guid sessionId, string summary);
}
