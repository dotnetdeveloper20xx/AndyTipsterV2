using AndyTipster.Application.Tips.DTOs;

namespace AndyTipster.Application.Tips.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync(bool includeInactive = false);
    Task<CategoryDto> GetCategoryByIdAsync(Guid categoryId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(Guid categoryId);
    Task SeedDefaultCategoriesAsync();
}
