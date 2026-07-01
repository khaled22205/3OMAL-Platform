using Domain.Common;

namespace Domain.Entities;

public class Category : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Banner { get; set; }
    public string? Description { get; set; }
    public string? SeoUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
