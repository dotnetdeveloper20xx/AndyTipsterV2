namespace AndyTipster.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public Guid? ParentItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Target { get; set; } // "_blank", "_self"
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public string? RequiredRole { get; set; }
    public DateTime CreatedAt { get; set; }

    public NavigationMenu Menu { get; set; } = null!;
    public MenuItem? ParentItem { get; set; }
    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
}
