namespace AndyTipster.Application.CMS.DTOs;

public class MediaAssetDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string? CdnUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }
    public Guid? FolderId { get; set; }
    public string? FolderName { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
}

public class UploadMediaRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class MediaSearchRequest
{
    public string? Query { get; set; }
    public Guid? FolderId { get; set; }
    public string? ContentType { get; set; }
    public List<string>? Tags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class MediaEditRequest
{
    public string? AltText { get; set; }
    public string? FileName { get; set; }
    public Guid? FolderId { get; set; }
    public List<string>? Tags { get; set; }
}

public class ImageTransformRequest
{
    public int? CropX { get; set; }
    public int? CropY { get; set; }
    public int? CropWidth { get; set; }
    public int? CropHeight { get; set; }
    public int? ResizeWidth { get; set; }
    public int? ResizeHeight { get; set; }
    public int? RotateDegrees { get; set; }
}

public class BatchUploadResultDto
{
    public List<MediaAssetDto> Succeeded { get; set; } = new();
    public List<BatchUploadErrorDto> Failed { get; set; } = new();
}

public class BatchUploadErrorDto
{
    public string FileName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
