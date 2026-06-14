namespace AndyTipster.Domain.Entities;

public class MediaAsset
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
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public MediaFolder? Folder { get; set; }
    public ApplicationUser UploadedByUser { get; set; } = null!;
}
