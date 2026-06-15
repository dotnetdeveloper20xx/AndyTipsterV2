using AndyTipster.Application.CMS.DTOs;

namespace AndyTipster.Application.CMS.Services;

public interface INavigationService
{
    Task<NavigationMenuDto> CreateMenuAsync(CreateMenuRequest request);
    Task<NavigationMenuDto> GetMenuByIdAsync(Guid menuId);
    Task<NavigationMenuDto?> GetMenuByLocationAsync(string location);
    Task<List<NavigationMenuDto>> GetAllMenusAsync();
    Task<NavigationMenuDto> UpdateMenuAsync(Guid menuId, UpdateMenuRequest request);
    Task DeleteMenuAsync(Guid menuId);
    Task<MenuItemDto> AddMenuItemAsync(CreateMenuItemRequest request);
    Task<MenuItemDto> UpdateMenuItemAsync(Guid itemId, CreateMenuItemRequest request);
    Task DeleteMenuItemAsync(Guid itemId);
    Task ReorderMenuItemsAsync(Guid menuId, List<Guid> orderedItemIds);
}
