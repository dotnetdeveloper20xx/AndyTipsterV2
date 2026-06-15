using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/navigation")]
public class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    public NavigationController(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<NavigationMenuDto>>> GetAllMenus()
    {
        var menus = await _navigationService.GetAllMenusAsync();
        return Ok(menus);
    }

    [HttpGet("{menuId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<NavigationMenuDto>> GetMenu(Guid menuId)
    {
        var menu = await _navigationService.GetMenuByIdAsync(menuId);
        return Ok(menu);
    }

    [HttpGet("by-location/{location}")]
    [AllowAnonymous]
    public async Task<ActionResult<NavigationMenuDto>> GetMenuByLocation(string location)
    {
        var menu = await _navigationService.GetMenuByLocationAsync(location);
        if (menu == null) return NotFound();
        return Ok(menu);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<NavigationMenuDto>> CreateMenu([FromBody] CreateMenuRequest request)
    {
        var menu = await _navigationService.CreateMenuAsync(request);
        return CreatedAtAction(nameof(GetMenu), new { menuId = menu.Id }, menu);
    }

    [HttpPatch("{menuId:guid}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<NavigationMenuDto>> UpdateMenu(Guid menuId, [FromBody] UpdateMenuRequest request)
    {
        var menu = await _navigationService.UpdateMenuAsync(menuId, request);
        return Ok(menu);
    }

    [HttpDelete("{menuId:guid}")]
    [Authorize(Policy = "Permission:CMS.Delete")]
    public async Task<IActionResult> DeleteMenu(Guid menuId)
    {
        await _navigationService.DeleteMenuAsync(menuId);
        return NoContent();
    }

    [HttpPost("items")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<MenuItemDto>> AddMenuItem([FromBody] CreateMenuItemRequest request)
    {
        var item = await _navigationService.AddMenuItemAsync(request);
        return Ok(item);
    }

    [HttpPut("items/{itemId:guid}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<MenuItemDto>> UpdateMenuItem(Guid itemId, [FromBody] CreateMenuItemRequest request)
    {
        var item = await _navigationService.UpdateMenuItemAsync(itemId, request);
        return Ok(item);
    }

    [HttpDelete("items/{itemId:guid}")]
    [Authorize(Policy = "Permission:CMS.Delete")]
    public async Task<IActionResult> DeleteMenuItem(Guid itemId)
    {
        await _navigationService.DeleteMenuItemAsync(itemId);
        return NoContent();
    }

    [HttpPost("{menuId:guid}/reorder")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<IActionResult> ReorderItems(Guid menuId, [FromBody] List<Guid> orderedItemIds)
    {
        await _navigationService.ReorderMenuItemsAsync(menuId, orderedItemIds);
        return NoContent();
    }
}
