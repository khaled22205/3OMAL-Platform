using Domain.Entities;

namespace TestCommon.Builders;

public class CategoryBuilder
{
    private string _name = "Test Category";
    private string? _seoUrl = "test-category";
    private string? _icon;
    private string? _banner;
    private string? _description;
    private int? _parentCategoryId;
    private Category? _parentCategory;
    private int _sortOrder;
    private bool _isActive = true;
    private readonly List<Category> _subCategories = [];
    private bool _isDeleted;

    public CategoryBuilder WithName(string name) { _name = name; return this; }
    public CategoryBuilder WithSeoUrl(string? url) { _seoUrl = url; return this; }
    public CategoryBuilder WithIcon(string? icon) { _icon = icon; return this; }
    public CategoryBuilder WithBanner(string? banner) { _banner = banner; return this; }
    public CategoryBuilder WithDescription(string? desc) { _description = desc; return this; }
    public CategoryBuilder WithParent(int parentId, Category? parent = null)
    {
        _parentCategoryId = parentId; _parentCategory = parent; return this;
    }
    public CategoryBuilder WithSortOrder(int order) { _sortOrder = order; return this; }
    public CategoryBuilder NotActive() { _isActive = false; return this; }
    public CategoryBuilder WithSubCategory(Category sub)
    {
        _subCategories.Add(sub); return this;
    }
    public CategoryBuilder Deleted()
    {
        _isDeleted = true; return this;
    }

    public Category Build()
    {
        var cat = new Category
        {
            Name = _name,
            SeoUrl = _seoUrl,
            Icon = _icon,
            Banner = _banner,
            Description = _description,
            ParentCategoryId = _parentCategoryId,
            ParentCategory = _parentCategory,
            SortOrder = _sortOrder,
            IsActive = _isActive,
            IsDeleted = _isDeleted,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var sub in _subCategories)
        {
            sub.ParentCategoryId = cat.Id;
            sub.ParentCategory = cat;
            cat.SubCategories.Add(sub);
        }
        return cat;
    }

    public static Category Create(string name = "Test Category", string seoUrl = "test-category")
        => new CategoryBuilder().WithName(name).WithSeoUrl(seoUrl).Build();
}
