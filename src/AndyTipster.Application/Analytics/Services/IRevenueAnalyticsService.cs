using AndyTipster.Application.Analytics.DTOs;

namespace AndyTipster.Application.Analytics.Services;

public interface IRevenueAnalyticsService
{
    Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(RevenueFilterDto? filter);
    Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(RevenueFilterDto filter);
    Task<SubscriberForecastDto> GetSubscriberForecastAsync();
}
