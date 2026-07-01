using Microsoft.EntityFrameworkCore;
using Domain.DomainServices;
using Domain.Entities;
using Application.Features.Categories;
using Application.Common.Mappings;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryTreeResponse>> GetTreeAsync()
    {
        var categories = await _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return categories.Select(c => c.ToTreeResponse()).ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category?.ToResponse();
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            Banner = request.Banner,
            SeoUrl = request.SeoUrl ?? StringHelper.ToSeoUrl(request.Name),
            ParentCategoryId = request.ParentCategoryId,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category.ToResponse();
    }

    public async Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category with id {id} not found");

        category.Name = request.Name;
        category.Description = request.Description;
        category.Icon = request.Icon;
        category.Banner = request.Banner;
        category.SeoUrl = request.SeoUrl ?? StringHelper.ToSeoUrl(request.Name);
        category.ParentCategoryId = request.ParentCategoryId;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return category.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        if (category.SubCategories.Count != 0)
            throw new InvalidOperationException("Cannot delete category with subcategories");

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSortOrderAsync(int id, int sortOrder)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.SortOrder = sortOrder;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
