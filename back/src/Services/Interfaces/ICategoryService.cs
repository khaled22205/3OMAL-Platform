using src.DTOs.Categories;

namespace src.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryTreeResponse>> GetTreeAsync();
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<CategoryResponse> CreateAsync(CategoryRequest request);
    Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleActiveAsync(int id);
    Task<bool> UpdateSortOrderAsync(int id, int sortOrder);
}