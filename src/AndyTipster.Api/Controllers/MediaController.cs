using System.Security.Claims;
using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Policy = "Permission:CMS.Edit")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<MediaAssetDto>> Upload(IFormFile file, [FromForm] string? altText, [FromForm] Guid? folderId, [FromForm] string? tags)
    {
        var userId = GetUserId();

        var request = new UploadMediaRequest
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            AltText = altText ?? string.Empty,
            FolderId = folderId,
            Tags = string.IsNullOrEmpty(tags) ? new() : tags.Split(',').Select(t => t.Trim()).ToList()
        };

        using var stream = file.OpenReadStream();
        var result = await _mediaService.UploadAsync(stream, request, userId);
        return Ok(result);
    }

    [HttpPost("upload/batch")]
    public async Task<ActionResult<BatchUploadResultDto>> BatchUpload(List<IFormFile> files, [FromForm] string? altText, [FromForm] Guid? folderId)
    {
        var userId = GetUserId();
        var uploadItems = new List<(Stream Stream, UploadMediaRequest Request)>();

        foreach (var file in files)
        {
            uploadItems.Add((file.OpenReadStream(), new UploadMediaRequest
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                AltText = altText ?? string.Empty,
                FolderId = folderId
            }));
        }

        var result = await _mediaService.BatchUploadAsync(uploadItems, userId);
        return Ok(result);
    }

    [HttpGet("{assetId:guid}")]
    public async Task<ActionResult<MediaAssetDto>> GetAsset(Guid assetId)
    {
        var result = await _mediaService.GetByIdAsync(assetId);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<MediaAssetDto>>> Search([FromQuery] MediaSearchRequest request)
    {
        var results = await _mediaService.SearchAsync(request);
        return Ok(results);
    }

    [HttpPatch("{assetId:guid}")]
    public async Task<ActionResult<MediaAssetDto>> UpdateAsset(Guid assetId, [FromBody] MediaEditRequest request)
    {
        var result = await _mediaService.UpdateAsync(assetId, request);
        return Ok(result);
    }

    [HttpDelete("{assetId:guid}")]
    public async Task<IActionResult> DeleteAsset(Guid assetId)
    {
        await _mediaService.DeleteAsync(assetId);
        return NoContent();
    }

    [HttpPost("{assetId:guid}/transform")]
    public async Task<ActionResult<MediaAssetDto>> Transform(Guid assetId, [FromBody] ImageTransformRequest request)
    {
        var userId = GetUserId();
        var result = await _mediaService.TransformAsync(assetId, request, userId);
        return Ok(result);
    }

    [HttpGet("{assetId:guid}/in-use")]
    public async Task<ActionResult<object>> CheckInUse(Guid assetId)
    {
        var inUse = await _mediaService.IsAssetInUseAsync(assetId);
        var pages = inUse ? await _mediaService.GetReferencingPagesAsync(assetId) : new List<string>();
        return Ok(new { inUse, pages });
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
