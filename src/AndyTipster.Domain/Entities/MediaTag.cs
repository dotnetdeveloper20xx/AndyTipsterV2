namespace AndyTipster.Domain.Entities;

public class MediaTag
{
    public Guid Id { get; set; }
    public Guid MediaAssetId { get; set; }
    public string Tag { get; set; } = string.Empty;

    public MediaAsset MediaAsset { get; set; } = null!;
}
