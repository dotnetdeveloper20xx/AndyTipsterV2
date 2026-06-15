using AndyTipster.Application.Tips.DTOs;

namespace AndyTipster.Application.Tips.Services;

public interface ITipService
{
    Task<TipDto> CreateTipAsync(CreateTipDto dto, Guid userId);
    Task<TipDto> UpdateTipAsync(Guid tipId, UpdateTipDto dto);
    Task<TipDto> GetTipByIdAsync(Guid tipId);
    Task<(List<TipDto> Items, int TotalCount)> GetTipsAsync(TipFilterDto filter);
    Task DeleteTipAsync(Guid tipId);
    Task<TipDto> PublishTipAsync(Guid tipId, DateTime? scheduledPublishAt);
    Task<TipDto> ArchiveTipAsync(Guid tipId);
    Task<TipDto> RecordResultAsync(Guid tipId, string result);
    Task<PnLSummaryDto> GetPnLSummaryAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId, string groupBy = "month");
    Task<BulkImportResultDto> BulkImportAsync(Stream csvStream, Guid userId);
}
