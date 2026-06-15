using AndyTipster.Application.Analytics.DTOs;

namespace AndyTipster.Application.Analytics.Services;

public interface ISubscriberAnalyticsService
{
    Task<SubscriberPerformanceDto> GetSubscriberPerformanceAsync(Guid userId, SubscriberPerformanceFilterDto? filter);
    Task<List<MonthlyPerformanceSummaryDto>> GetMonthlySummariesAsync(Guid userId, int months = 12);
    Task SendMonthlyDigestEmailAsync(Guid userId);
}
