using AndyTipster.Application.Community.DTOs;

namespace AndyTipster.Application.Community.Services;

public interface IPollService
{
    Task<PollDto> CreatePollAsync(CreatePollDto dto, Guid createdByUserId);
    Task<PollDto> GetPollAsync(Guid pollId, Guid? userId = null);
    Task<List<PollDto>> GetActivePollsAsync(Guid? userId = null);
    Task<PollDto> VoteAsync(VoteDto dto, Guid userId);
    Task ClosePollAsync(Guid pollId);
}
