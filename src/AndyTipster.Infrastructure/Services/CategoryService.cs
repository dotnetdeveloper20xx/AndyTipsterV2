using AndyTipster.Application.Tips.DTOs;
using AndyTipster.Application.Tips.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Exceptions;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AndyTipsterDbContext _context;

    public CategoryService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(bool includeInactive = false)
    {
        var query = _context.TipCategories.AsQueryable();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query.OrderBy(c => c.Name).ToListAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid categoryId)
    {
        var category = await _context.TipCategories.FindAsync(categoryId)
            ?? throw new NotFoundException("Category not found.");
        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = new[] { "Category name is required." }
            });

        var slug = GenerateSlug(dto.Name);

        var existing = await _context.TipCategories.AnyAsync(c => c.Slug == slug);
        if (existing)
            throw new BusinessRuleException($"A category with name '{dto.Name}' already exists.");

        var category = new TipCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = dto.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.TipCategories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto)
    {
        var category = await _context.TipCategories.FindAsync(categoryId)
            ?? throw new NotFoundException("Category not found.");

        if (dto.Name != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["name"] = new[] { "Category name cannot be empty." }
                });

            category.Name = dto.Name.Trim();
            category.Slug = GenerateSlug(dto.Name);
        }

        if (dto.Description != null)
            category.Description = dto.Description.Trim();

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await _context.TipCategories
            .Include(c => c.Tips)
            .FirstOrDefaultAsync(c => c.Id == categoryId)
            ?? throw new NotFoundException("Category not found.");

        if (category.Tips.Any())
            throw new BusinessRuleException("Cannot delete a category that has tips assigned to it.");

        _context.TipCategories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task SeedDefaultCategoriesAsync()
    {
        var defaults = new[]
        {
            ("UK Horse Racing", "uk-horse-racing"),
            ("Irish Horse Racing", "irish-horse-racing"),
            ("Other Sports", "other-sports")
        };

        foreach (var (name, slug) in defaults)
        {
            var exists = await _context.TipCategories.AnyAsync(c => c.Slug == slug);
            if (!exists)
            {
                _context.TipCategories.Add(new TipCategory
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Slug = slug,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private static string GenerateSlug(string name)
    {
        return name.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
    }

    private static CategoryDto MapToDto(TipCategory category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}
