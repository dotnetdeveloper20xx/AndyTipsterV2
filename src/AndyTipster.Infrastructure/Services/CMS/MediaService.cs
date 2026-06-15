using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.CMS;

public class MediaService : IMediaService
{
    private readonly AndyTipsterDbContext _db;
    private static readonly Dictionary<string, long> MaxFileSizes = new()
    {
        { "image", 10 * 1024 * 1024 },     // 10MB
        { "document", 50 * 1024 * 1024 },   // 50MB
        { "video", 500 * 1024 * 1024 }      // 500MB
    };

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/svg+xml", "image/gif"
    };

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private static readonly HashSet<string> AllowedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4"
    };

    public MediaService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<MediaAssetDto> UploadAsync(Stream fileStream, UploadMediaRequest request, Guid userId)
    {
        ValidateFileType(request.ContentType);
        ValidateFileSize(fileStream.Length, request.ContentType);

        if (IsImageType(request.ContentType) && string.IsNullOrWhiteSpace(request.AltText))
        {
            throw new InvalidOperationException("Alt text is required for image uploads (1-125 characters).");
        }

        if (!string.IsNullOrEmpty(request.AltText) && request.AltText.Length > 125)
        {
            throw new InvalidOperationException("Alt text must not exceed 125 characters.");
        }

        // Stub: In production, upload to Azure Blob Storage, compress, convert to WebP
        var blobUrl = $"https://andytipster.blob.core.windows.net/media/{Guid.NewGuid()}/{request.FileName}";
        var cdnUrl = blobUrl.Replace("blob.core.windows.net", "azureedge.net");

        var asset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            ContentType = request.ContentType,
            BlobUrl = blobUrl,
            CdnUrl = cdnUrl,
            FileSizeBytes = fileStream.Length,
            AltText = request.AltText,
            FolderId = request.FolderId,
            UploadedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.MediaAssets.Add(asset);

        // Add tags
        foreach (var tag in request.Tags)
        {
            _db.MediaTags.Add(new MediaTag
            {
                Id = Guid.NewGuid(),
                MediaAssetId = asset.Id,
                Tag = tag
            });
        }

        await _db.SaveChangesAsync();

        return MapToDto(asset, request.Tags);
    }

    public async Task<BatchUploadResultDto> BatchUploadAsync(List<(Stream Stream, UploadMediaRequest Request)> files, Guid userId)
    {
        if (files.Count > 20)
        {
            throw new InvalidOperationException("Batch upload limited to 20 files.");
        }

        var result = new BatchUploadResultDto();

        foreach (var (stream, request) in files)
        {
            try
            {
                var asset = await UploadAsync(stream, request, userId);
                result.Succeeded.Add(asset);
            }
            catch (Exception ex)
            {
                result.Failed.Add(new BatchUploadErrorDto
                {
                    FileName = request.FileName,
                    Reason = ex.Message
                });
            }
        }

        return result;
    }

    public async Task<MediaAssetDto> GetByIdAsync(Guid assetId)
    {
        var asset = await _db.MediaAssets
            .Include(a => a.Folder)
            .Include(a => a.UploadedByUser)
            .FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw new KeyNotFoundException($"Media asset {assetId} not found");

        var tags = await _db.MediaTags
            .Where(t => t.MediaAssetId == assetId)
            .Select(t => t.Tag)
            .ToListAsync();

        return MapToDto(asset, tags);
    }

    public async Task<List<MediaAssetDto>> SearchAsync(MediaSearchRequest request)
    {
        var query = _db.MediaAssets
            .Include(a => a.Folder)
            .Include(a => a.UploadedByUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Query))
        {
            query = query.Where(a => a.FileName.Contains(request.Query));
        }

        if (request.FolderId.HasValue)
        {
            query = query.Where(a => a.FolderId == request.FolderId.Value);
        }

        if (!string.IsNullOrEmpty(request.ContentType))
        {
            query = query.Where(a => a.ContentType.StartsWith(request.ContentType));
        }

        if (request.Tags != null && request.Tags.Any())
        {
            var assetIds = await _db.MediaTags
                .Where(t => request.Tags.Contains(t.Tag))
                .Select(t => t.MediaAssetId)
                .Distinct()
                .ToListAsync();

            query = query.Where(a => assetIds.Contains(a.Id));
        }

        var assets = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var allTags = await _db.MediaTags
            .Where(t => assets.Select(a => a.Id).Contains(t.MediaAssetId))
            .ToListAsync();

        return assets.Select(a => MapToDto(a, allTags.Where(t => t.MediaAssetId == a.Id).Select(t => t.Tag).ToList())).ToList();
    }

    public async Task<MediaAssetDto> UpdateAsync(Guid assetId, MediaEditRequest request)
    {
        var asset = await _db.MediaAssets.FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw new KeyNotFoundException($"Media asset {assetId} not found");

        if (request.AltText != null) asset.AltText = request.AltText;
        if (request.FileName != null) asset.FileName = request.FileName;
        if (request.FolderId.HasValue) asset.FolderId = request.FolderId;

        if (request.Tags != null)
        {
            var existingTags = await _db.MediaTags.Where(t => t.MediaAssetId == assetId).ToListAsync();
            _db.MediaTags.RemoveRange(existingTags);

            foreach (var tag in request.Tags)
            {
                _db.MediaTags.Add(new MediaTag
                {
                    Id = Guid.NewGuid(),
                    MediaAssetId = assetId,
                    Tag = tag
                });
            }
        }

        await _db.SaveChangesAsync();

        var tags = await _db.MediaTags.Where(t => t.MediaAssetId == assetId).Select(t => t.Tag).ToListAsync();
        return MapToDto(asset, tags);
    }

    public async Task DeleteAsync(Guid assetId)
    {
        if (await IsAssetInUseAsync(assetId))
        {
            throw new InvalidOperationException("Cannot delete asset that is referenced by one or more pages.");
        }

        var asset = await _db.MediaAssets.FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw new KeyNotFoundException($"Media asset {assetId} not found");

        var tags = await _db.MediaTags.Where(t => t.MediaAssetId == assetId).ToListAsync();
        _db.MediaTags.RemoveRange(tags);
        _db.MediaAssets.Remove(asset);
        await _db.SaveChangesAsync();
    }

    public async Task<MediaAssetDto> TransformAsync(Guid assetId, ImageTransformRequest request, Guid userId)
    {
        var original = await _db.MediaAssets.FirstOrDefaultAsync(a => a.Id == assetId)
            ?? throw new KeyNotFoundException($"Media asset {assetId} not found");

        // Stub: In production, apply crop/resize/rotate and create new asset
        var transformedBlobUrl = $"https://andytipster.blob.core.windows.net/media/{Guid.NewGuid()}/transformed_{original.FileName}";
        var transformedCdnUrl = transformedBlobUrl.Replace("blob.core.windows.net", "azureedge.net");

        var transformed = new MediaAsset
        {
            Id = Guid.NewGuid(),
            FileName = $"transformed_{original.FileName}",
            ContentType = "image/webp",
            BlobUrl = transformedBlobUrl,
            CdnUrl = transformedCdnUrl,
            FileSizeBytes = original.FileSizeBytes,
            Width = request.ResizeWidth ?? request.CropWidth ?? original.Width,
            Height = request.ResizeHeight ?? request.CropHeight ?? original.Height,
            AltText = original.AltText,
            FolderId = original.FolderId,
            UploadedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.MediaAssets.Add(transformed);
        await _db.SaveChangesAsync();

        return MapToDto(transformed, new List<string>());
    }

    public async Task<bool> IsAssetInUseAsync(Guid assetId)
    {
        var cdnUrl = await _db.MediaAssets
            .Where(a => a.Id == assetId)
            .Select(a => a.CdnUrl ?? a.BlobUrl)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(cdnUrl)) return false;

        return await _db.CmsPages.AnyAsync(p => p.BlocksJson.Contains(cdnUrl));
    }

    public async Task<List<string>> GetReferencingPagesAsync(Guid assetId)
    {
        var cdnUrl = await _db.MediaAssets
            .Where(a => a.Id == assetId)
            .Select(a => a.CdnUrl ?? a.BlobUrl)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(cdnUrl)) return new List<string>();

        return await _db.CmsPages
            .Where(p => p.BlocksJson.Contains(cdnUrl))
            .Select(p => p.Title)
            .ToListAsync();
    }

    private static void ValidateFileType(string contentType)
    {
        if (!AllowedImageTypes.Contains(contentType) &&
            !AllowedDocumentTypes.Contains(contentType) &&
            !AllowedVideoTypes.Contains(contentType))
        {
            throw new InvalidOperationException($"File type '{contentType}' is not allowed.");
        }
    }

    private static void ValidateFileSize(long sizeBytes, string contentType)
    {
        var category = GetFileCategory(contentType);
        if (MaxFileSizes.TryGetValue(category, out var maxSize) && sizeBytes > maxSize)
        {
            throw new InvalidOperationException($"File exceeds maximum size for {category} ({maxSize / (1024 * 1024)}MB).");
        }
    }

    private static string GetFileCategory(string contentType)
    {
        if (AllowedImageTypes.Contains(contentType)) return "image";
        if (AllowedDocumentTypes.Contains(contentType)) return "document";
        if (AllowedVideoTypes.Contains(contentType)) return "video";
        return "unknown";
    }

    private static bool IsImageType(string contentType) => AllowedImageTypes.Contains(contentType);

    private static MediaAssetDto MapToDto(MediaAsset asset, List<string> tags)
    {
        return new MediaAssetDto
        {
            Id = asset.Id,
            FileName = asset.FileName,
            ContentType = asset.ContentType,
            BlobUrl = asset.BlobUrl,
            CdnUrl = asset.CdnUrl,
            FileSizeBytes = asset.FileSizeBytes,
            Width = asset.Width,
            Height = asset.Height,
            AltText = asset.AltText,
            FolderId = asset.FolderId,
            FolderName = asset.Folder?.Name,
            Tags = tags,
            CreatedAt = asset.CreatedAt,
            UploadedByUserName = asset.UploadedByUser?.UserName ?? "Unknown"
        };
    }
}
