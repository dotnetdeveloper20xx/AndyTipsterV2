using System.Text.Json;
using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.CMS;

public class PageService : IPageService
{
    private readonly AndyTipsterDbContext _db;

    public PageService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<PageDto> CreatePageAsync(CreatePageRequest request, Guid userId)
    {
        var page = new CmsPage
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = request.Slug,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            OgImageUrl = request.OgImageUrl,
            CanonicalUrl = request.CanonicalUrl,
            NoIndex = request.NoIndex,
            Status = PageStatus.Draft,
            BlocksJson = JsonSerializer.Serialize(request.Blocks),
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _db.CmsPages.Add(page);

        // Create initial version
        var version = new PageVersion
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            VersionNumber = 1,
            BlocksJson = page.BlocksJson,
            ChangeSummary = "Initial version",
            AuthorUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.PageVersions.Add(version);

        await _db.SaveChangesAsync();

        return MapToDto(page);
    }

    public async Task<PageDto> GetPageByIdAsync(Guid pageId)
    {
        var page = await _db.CmsPages
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        return MapToDto(page);
    }

    public async Task<PageDto?> GetPageBySlugAsync(string slug)
    {
        var page = await _db.CmsPages
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PageStatus.Published);

        return page == null ? null : MapToDto(page);
    }

    public async Task<List<PageDto>> GetPagesAsync(string? status = null, int page = 1, int pageSize = 25)
    {
        var query = _db.CmsPages.Include(p => p.CreatedByUser).AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PageStatus>(status, true, out var pageStatus))
        {
            query = query.Where(p => p.Status == pageStatus);
        }

        var pages = await query
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return pages.Select(MapToDto).ToList();
    }

    public async Task<PageDto> UpdatePageAsync(Guid pageId, UpdatePageRequest request, Guid userId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        if (request.Title != null) page.Title = request.Title;
        if (request.Slug != null) page.Slug = request.Slug;
        if (request.MetaTitle != null) page.MetaTitle = request.MetaTitle;
        if (request.MetaDescription != null) page.MetaDescription = request.MetaDescription;
        if (request.OgImageUrl != null) page.OgImageUrl = request.OgImageUrl;
        if (request.CanonicalUrl != null) page.CanonicalUrl = request.CanonicalUrl;
        if (request.NoIndex.HasValue) page.NoIndex = request.NoIndex.Value;

        if (request.Blocks != null)
        {
            page.BlocksJson = JsonSerializer.Serialize(request.Blocks);
        }

        page.UpdatedAt = DateTime.UtcNow;
        page.CurrentVersion++;

        // Create version snapshot
        var version = new PageVersion
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            VersionNumber = page.CurrentVersion,
            BlocksJson = page.BlocksJson,
            ChangeSummary = request.ChangeSummary ?? "Content updated",
            AuthorUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.PageVersions.Add(version);

        await _db.SaveChangesAsync();
        return MapToDto(page);
    }

    public async Task DeletePageAsync(Guid pageId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        _db.CmsPages.Remove(page);
        await _db.SaveChangesAsync();
    }

    public async Task<PageDto> PublishPageAsync(Guid pageId, PublishPageRequest request, Guid userId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        if (request.ScheduledPublishAt.HasValue)
        {
            page.Status = PageStatus.Scheduled;
            page.ScheduledPublishAt = request.ScheduledPublishAt.Value.ToUniversalTime();
        }
        else
        {
            page.Status = PageStatus.Published;
            page.PublishedAt = DateTime.UtcNow;
        }

        if (request.ExpiresAt.HasValue)
        {
            page.ExpiresAt = request.ExpiresAt.Value.ToUniversalTime();
        }

        page.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapToDto(page);
    }

    public async Task<PageDto> UnpublishPageAsync(Guid pageId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        page.Status = PageStatus.Draft;
        page.PublishedAt = null;
        page.ScheduledPublishAt = null;
        page.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapToDto(page);
    }

    public async Task<List<PageVersionDto>> GetVersionHistoryAsync(Guid pageId)
    {
        var versions = await _db.PageVersions
            .Where(v => v.PageId == pageId)
            .Include(v => v.Author)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return versions.Select(v => new PageVersionDto
        {
            Id = v.Id,
            VersionNumber = v.VersionNumber,
            BlocksJson = v.BlocksJson,
            ChangeSummary = v.ChangeSummary,
            AuthorUserName = v.Author?.UserName ?? "Unknown",
            CreatedAt = v.CreatedAt
        }).ToList();
    }

    public async Task<PageVersionDto> GetVersionAsync(Guid pageId, int versionNumber)
    {
        var version = await _db.PageVersions
            .Include(v => v.Author)
            .FirstOrDefaultAsync(v => v.PageId == pageId && v.VersionNumber == versionNumber)
            ?? throw new KeyNotFoundException($"Version {versionNumber} not found for page {pageId}");

        return new PageVersionDto
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            BlocksJson = version.BlocksJson,
            ChangeSummary = version.ChangeSummary,
            AuthorUserName = version.Author?.UserName ?? "Unknown",
            CreatedAt = version.CreatedAt
        };
    }

    public async Task<PageDto> RollbackToVersionAsync(Guid pageId, int versionNumber, Guid userId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        var version = await _db.PageVersions
            .FirstOrDefaultAsync(v => v.PageId == pageId && v.VersionNumber == versionNumber)
            ?? throw new KeyNotFoundException($"Version {versionNumber} not found");

        // Restore as draft with version's content
        page.BlocksJson = version.BlocksJson;
        page.Status = PageStatus.Draft;
        page.UpdatedAt = DateTime.UtcNow;
        page.CurrentVersion++;

        // Create new version snapshot for the rollback
        var rollbackVersion = new PageVersion
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            VersionNumber = page.CurrentVersion,
            BlocksJson = page.BlocksJson,
            ChangeSummary = $"Rolled back to version {versionNumber}",
            AuthorUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.PageVersions.Add(rollbackVersion);

        await _db.SaveChangesAsync();
        return MapToDto(page);
    }

    public async Task<List<PublishingQueueItemDto>> GetPublishingQueueAsync()
    {
        var items = await _db.CmsPages
            .Where(p => p.Status == PageStatus.Scheduled || p.ExpiresAt != null)
            .OrderBy(p => p.ScheduledPublishAt ?? p.ExpiresAt)
            .Select(p => new PublishingQueueItemDto
            {
                PageId = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                ScheduledPublishAt = p.ScheduledPublishAt,
                ExpiresAt = p.ExpiresAt,
                Status = p.Status.ToString()
            })
            .ToListAsync();

        return items;
    }

    public async Task ProcessScheduledPublishingAsync()
    {
        var now = DateTime.UtcNow;
        var scheduledPages = await _db.CmsPages
            .Where(p => p.Status == PageStatus.Scheduled && p.ScheduledPublishAt <= now)
            .ToListAsync();

        foreach (var page in scheduledPages)
        {
            page.Status = PageStatus.Published;
            page.PublishedAt = now;
            page.ScheduledPublishAt = null;
            page.UpdatedAt = now;
        }

        if (scheduledPages.Any())
        {
            await _db.SaveChangesAsync();
        }
    }

    public async Task ProcessContentExpiryAsync()
    {
        var now = DateTime.UtcNow;
        var expiredPages = await _db.CmsPages
            .Where(p => p.Status == PageStatus.Published && p.ExpiresAt != null && p.ExpiresAt <= now)
            .ToListAsync();

        foreach (var page in expiredPages)
        {
            page.Status = PageStatus.Expired;
            page.UpdatedAt = now;
        }

        if (expiredPages.Any())
        {
            await _db.SaveChangesAsync();
        }
    }

    private static PageDto MapToDto(CmsPage page)
    {
        var blocks = new List<BlockDto>();
        try
        {
            blocks = JsonSerializer.Deserialize<List<BlockDto>>(page.BlocksJson) ?? new();
        }
        catch { /* fallback to empty */ }

        return new PageDto
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Status = page.Status.ToString(),
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            OgImageUrl = page.OgImageUrl,
            CanonicalUrl = page.CanonicalUrl,
            NoIndex = page.NoIndex,
            PublishedAt = page.PublishedAt,
            ScheduledPublishAt = page.ScheduledPublishAt,
            ExpiresAt = page.ExpiresAt,
            CurrentVersion = page.CurrentVersion,
            Blocks = blocks,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt,
            CreatedByUserName = page.CreatedByUser?.UserName ?? "Unknown"
        };
    }
}
