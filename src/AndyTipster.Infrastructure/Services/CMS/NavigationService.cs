using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.CMS;

public class NavigationService : INavigationService
{
    private readonly AndyTipsterDbContext _db;

    public NavigationService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<NavigationMenuDto> CreateMenuAsync(CreateMenuRequest request)
    {
        var menu = new NavigationMenu
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Location = request.Location,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.NavigationMenus.Add(menu);
        await _db.SaveChangesAsync();

        return MapToDto(menu);
    }

    public async Task<NavigationMenuDto> GetMenuByIdAsync(Guid menuId)
    {
        var menu = await _db.NavigationMenus
            .Include(m => m.Items.Where(i => i.ParentItemId == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Children.OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(m => m.Id == menuId)
            ?? throw new KeyNotFoundException($"Menu {menuId} not found");

        return MapToDto(menu);
    }

    public async Task<NavigationMenuDto?> GetMenuByLocationAsync(string location)
    {
        var menu = await _db.NavigationMenus
            .Include(m => m.Items.Where(i => i.ParentItemId == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Children.OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(m => m.Location == location && m.IsActive);

        return menu == null ? null : MapToDto(menu);
    }

    public async Task<List<NavigationMenuDto>> GetAllMenusAsync()
    {
        var menus = await _db.NavigationMenus
            .Include(m => m.Items.Where(i => i.ParentItemId == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Children.OrderBy(c => c.SortOrder))
            .OrderBy(m => m.Location)
            .ToListAsync();

        return menus.Select(MapToDto).ToList();
    }

    public async Task<NavigationMenuDto> UpdateMenuAsync(Guid menuId, UpdateMenuRequest request)
    {
        var menu = await _db.NavigationMenus
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == menuId)
            ?? throw new KeyNotFoundException($"Menu {menuId} not found");

        if (request.Name != null) menu.Name = request.Name;
        if (request.IsActive.HasValue) menu.IsActive = request.IsActive.Value;

        if (request.Items != null)
        {
            // Replace all items with the new tree structure
            _db.MenuItems.RemoveRange(menu.Items);

            foreach (var itemDto in request.Items)
            {
                AddMenuItemFromDto(menuId, null, itemDto);
            }
        }

        menu.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetMenuByIdAsync(menuId);
    }

    public async Task DeleteMenuAsync(Guid menuId)
    {
        var menu = await _db.NavigationMenus
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == menuId)
            ?? throw new KeyNotFoundException($"Menu {menuId} not found");

        _db.MenuItems.RemoveRange(menu.Items);
        _db.NavigationMenus.Remove(menu);
        await _db.SaveChangesAsync();
    }

    public async Task<MenuItemDto> AddMenuItemAsync(CreateMenuItemRequest request)
    {
        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            MenuId = request.MenuId,
            ParentItemId = request.ParentItemId,
            Label = request.Label,
            Url = request.Url,
            Icon = request.Icon,
            Target = request.Target,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible,
            RequiredRole = request.RequiredRole,
            CreatedAt = DateTime.UtcNow
        };

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();

        return MapItemToDto(item);
    }

    public async Task<MenuItemDto> UpdateMenuItemAsync(Guid itemId, CreateMenuItemRequest request)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Menu item {itemId} not found");

        item.Label = request.Label;
        item.Url = request.Url;
        item.Icon = request.Icon;
        item.Target = request.Target;
        item.SortOrder = request.SortOrder;
        item.IsVisible = request.IsVisible;
        item.RequiredRole = request.RequiredRole;
        item.ParentItemId = request.ParentItemId;

        await _db.SaveChangesAsync();
        return MapItemToDto(item);
    }

    public async Task DeleteMenuItemAsync(Guid itemId)
    {
        var item = await _db.MenuItems
            .Include(i => i.Children)
            .FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Menu item {itemId} not found");

        // Remove children first
        if (item.Children.Any())
        {
            _db.MenuItems.RemoveRange(item.Children);
        }

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ReorderMenuItemsAsync(Guid menuId, List<Guid> orderedItemIds)
    {
        var items = await _db.MenuItems
            .Where(i => i.MenuId == menuId && orderedItemIds.Contains(i.Id))
            .ToListAsync();

        for (int i = 0; i < orderedItemIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == orderedItemIds[i]);
            if (item != null) item.SortOrder = i;
        }

        await _db.SaveChangesAsync();
    }

    private void AddMenuItemFromDto(Guid menuId, Guid? parentId, MenuItemDto dto)
    {
        var item = new MenuItem
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            MenuId = menuId,
            ParentItemId = parentId,
            Label = dto.Label,
            Url = dto.Url,
            Icon = dto.Icon,
            Target = dto.Target,
            SortOrder = dto.SortOrder,
            IsVisible = dto.IsVisible,
            RequiredRole = dto.RequiredRole,
            CreatedAt = DateTime.UtcNow
        };

        _db.MenuItems.Add(item);

        foreach (var child in dto.Children)
        {
            AddMenuItemFromDto(menuId, item.Id, child);
        }
    }

    private static NavigationMenuDto MapToDto(NavigationMenu menu)
    {
        return new NavigationMenuDto
        {
            Id = menu.Id,
            Name = menu.Name,
            Location = menu.Location,
            IsActive = menu.IsActive,
            Items = menu.Items
                .Where(i => i.ParentItemId == null)
                .OrderBy(i => i.SortOrder)
                .Select(MapItemToDto)
                .ToList(),
            CreatedAt = menu.CreatedAt,
            UpdatedAt = menu.UpdatedAt
        };
    }

    private static MenuItemDto MapItemToDto(MenuItem item)
    {
        return new MenuItemDto
        {
            Id = item.Id,
            ParentItemId = item.ParentItemId,
            Label = item.Label,
            Url = item.Url,
            Icon = item.Icon,
            Target = item.Target,
            SortOrder = item.SortOrder,
            IsVisible = item.IsVisible,
            RequiredRole = item.RequiredRole,
            Children = item.Children?.OrderBy(c => c.SortOrder).Select(MapItemToDto).ToList() ?? new()
        };
    }
}
