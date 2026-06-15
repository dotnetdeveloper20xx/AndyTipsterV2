using AndyTipster.Application.Tips.DTOs;

namespace AndyTipster.Application.Tips.Services;

public record AccessCheckResult
{
    public bool HasAccess { get; init; }
    public string? DenialReason { get; init; }
    public bool IsTipOfTheDay { get; init; }
    public bool ShowPaywall { get; init; }
}

public interface IAccessGatingService
{
    Task<AccessCheckResult> CheckAccessAsync(Guid userId, Guid tipId);
    Task<AccessCheckResult> CheckCategoryAccessAsync(Guid userId, Guid categoryId);
    Task<TipDto?> GetTipOfTheDayAsync();
    Task<(List<TipDto> Items, int TotalCount)> GetAccessibleTipsAsync(Guid userId, TipFilterDto filter);
}
