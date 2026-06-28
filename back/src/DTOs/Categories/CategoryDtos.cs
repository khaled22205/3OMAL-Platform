namespace src.DTOs.Categories;

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Banner { get; set; }
    public string? SeoUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Banner { get; set; }
    public string? SeoUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ServicesCount { get; set; }
    public List<CategoryResponse> SubCategories { get; set; } = [];
}

public class CategoryTreeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SeoUrl { get; set; }
    public int SortOrder { get; set; }
    public List<CategoryTreeResponse> Children { get; set; } = [];
}