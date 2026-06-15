using AndyTipster.Application.Analytics.DTOs;

namespace AndyTipster.Application.Analytics.Services;

public interface IPublicAnalyticsService
{
    Task<PublicStatsDto> GetPublicStatsAsync();
    Task<Last30DaysSummaryDto> GetLast30DaysSummaryAsync();
    Task<byte[]> ExportResultsCsvAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId);
    Task<byte[]> ExportResultsPdfAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId);
}
