using AndyTipster.Application.CMS.DTOs;

namespace AndyTipster.Application.CMS.Services;

public interface IMediaService
{
    Task<MediaAssetDto> UploadAsync(Stream fileStream, UploadMediaRequest request, Guid userId);
    Task<BatchUploadResultDto> BatchUploadAsync(List<(Stream Stream, UploadMediaRequest Request)> files, Guid userId);
    Task<MediaAssetDto> GetByIdAsync(Guid assetId);
    Task<List<MediaAssetDto>> SearchAsync(MediaSearchRequest request);
    Task<MediaAssetDto> UpdateAsync(Guid assetId, MediaEditRequest request);
    Task DeleteAsync(Guid assetId);
    Task<MediaAssetDto> TransformAsync(Guid assetId, ImageTransformRequest request, Guid userId);
    Task<bool> IsAssetInUseAsync(Guid assetId);
    Task<List<string>> GetReferencingPagesAsync(Guid assetId);
}
