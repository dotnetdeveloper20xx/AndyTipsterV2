namespace AndyTipster.Domain.Entities;

public class NavigationMenu
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // e.g., "header", "footer", "sidebar"
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
