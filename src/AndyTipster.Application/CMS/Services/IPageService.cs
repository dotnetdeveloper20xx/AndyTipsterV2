using AndyTipster.Application.CMS.DTOs;

namespace AndyTipster.Application.CMS.Services;

public interface IPageService
{
    Task<PageDto> CreatePageAsync(CreatePageRequest request, Guid userId);
    Task<PageDto> GetPageByIdAsync(Guid pageId);
    Task<PageDto?> GetPageBySlugAsync(string slug);
    Task<List<PageDto>> GetPagesAsync(string? status = null, int page = 1, int pageSize = 25);
    Task<PageDto> UpdatePageAsync(Guid pageId, UpdatePageRequest request, Guid userId);
    Task DeletePageAsync(Guid pageId);
    Task<PageDto> PublishPageAsync(Guid pageId, PublishPageRequest request, Guid userId);
    Task<PageDto> UnpublishPageAsync(Guid pageId);
    Task<List<PageVersionDto>> GetVersionHistoryAsync(Guid pageId);
    Task<PageVersionDto> GetVersionAsync(Guid pageId, int versionNumber);
    Task<PageDto> RollbackToVersionAsync(Guid pageId, int versionNumber, Guid userId);
    Task<List<PublishingQueueItemDto>> GetPublishingQueueAsync();
    Task ProcessScheduledPublishingAsync();
    Task ProcessContentExpiryAsync();
}
