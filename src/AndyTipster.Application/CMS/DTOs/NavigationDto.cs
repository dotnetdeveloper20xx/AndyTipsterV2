namespace AndyTipster.Application.CMS.DTOs;

public class NavigationMenuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<MenuItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public Guid? ParentItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Target { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public string? RequiredRole { get; set; }
    public string? RequiredSubscriptionStatus { get; set; }
    public List<MenuItemDto> Children { get; set; } = new();
}

public class CreateMenuRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class UpdateMenuRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public List<MenuItemDto>? Items { get; set; }
}

public class CreateMenuItemRequest
{
    public Guid MenuId { get; set; }
    public Guid? ParentItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Target { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public string? RequiredRole { get; set; }
    public string? RequiredSubscriptionStatus { get; set; }
}
