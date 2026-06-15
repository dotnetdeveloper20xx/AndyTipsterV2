using AndyTipster.Application.Tips.DTOs;
using AndyTipster.Application.Tips.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all active categories (public).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] bool includeInactive = false)
    {
        var categories = await _categoryService.GetCategoriesAsync(includeInactive);
        return Ok(categories);
    }

    /// <summary>
    /// Get a specific category by ID.
    /// </summary>
    [HttpGet("{categoryId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        return Ok(category);
    }

    /// <summary>
    /// Create a new category (admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Tips.Create")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { categoryId = category.Id }, category);
    }

    /// <summary>
    /// Update a category (admin only).
    /// </summary>
    [HttpPatch("{categoryId:guid}")]
    [Authorize(Policy = "Permission:Tips.Edit")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid categoryId, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateCategoryAsync(categoryId, dto);
        return Ok(category);
    }

    /// <summary>
    /// Delete a category (admin only).
    /// </summary>
    [HttpDelete("{categoryId:guid}")]
    [Authorize(Policy = "Permission:Tips.Delete")]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        await _categoryService.DeleteCategoryAsync(categoryId);
        return NoContent();
    }

    /// <summary>
    /// Seed default categories (admin only, for initial setup).
    /// </summary>
    [HttpPost("seed")]
    [Authorize(Policy = "Permission:Tips.Create")]
    public async Task<IActionResult> SeedDefaults()
    {
        await _categoryService.SeedDefaultCategoriesAsync();
        return Ok(new { message = "Default categories seeded successfully." });
    }
}
