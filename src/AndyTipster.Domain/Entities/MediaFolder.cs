namespace AndyTipster.Domain.Entities;

public class MediaFolder
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public DateTime CreatedAt { get; set; }

    public MediaFolder? ParentFolder { get; set; }
    public ICollection<MediaFolder> SubFolders { get; set; } = new List<MediaFolder>();
    public ICollection<MediaAsset> Assets { get; set; } = new List<MediaAsset>();
}
