using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Application.Features.Categories;

namespace Infrastructure.Tests;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new CategoryService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private Category CreateCategory(string name, string seoUrl, int sortOrder = 1,
        bool isActive = true, int? parentId = null)
    {
        return new Category
        {
            Name = name,
            SeoUrl = seoUrl,
            SortOrder = sortOrder,
            IsActive = isActive,
            ParentCategoryId = parentId
        };
    }

    [Fact]
    public async Task GetTreeAsync_Should_return_only_root_categories()
    {
        _context.Categories.Add(CreateCategory("Root", "root"));
        _context.Categories.Add(CreateCategory("Sub", "sub", parentId: 1));
        await _context.SaveChangesAsync();

        var result = await _service.GetTreeAsync();

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Root");
    }

    [Fact]
    public async Task GetTreeAsync_Should_order_by_sort_order()
    {
        _context.Categories.Add(CreateCategory("Second", "second", sortOrder: 2));
        _context.Categories.Add(CreateCategory("First", "first", sortOrder: 1));
        await _context.SaveChangesAsync();

        var result = await _service.GetTreeAsync();

        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Second");
    }

    [Fact]
    public async Task GetTreeAsync_Should_include_children()
    {
        var parent = CreateCategory("Root", "root");
        _context.Categories.Add(parent);
        await _context.SaveChangesAsync();

        var child = CreateCategory("Sub", "sub", parentId: parent.Id);
        _context.Categories.Add(child);
        await _context.SaveChangesAsync();

        var result = await _service.GetTreeAsync();

        result.Should().ContainSingle();
        result[0].Children.Should().ContainSingle();
        result[0].Children[0].Name.Should().Be("Sub");
    }

    [Fact]
    public async Task GetByIdAsync_Should_return_category()
    {
        var cat = CreateCategory("Plumbing", "plumbing");
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(cat.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Plumbing");
    }

    [Fact]
    public async Task GetByIdAsync_Should_return_null_for_missing()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_create_with_provided_seo_url()
    {
        var request = new CategoryRequest
        {
            Name = "Plumbing",
            SeoUrl = "plumbing-services",
            SortOrder = 1,
            IsActive = true
        };

        var result = await _service.CreateAsync(request);

        result.Name.Should().Be("Plumbing");
        result.SeoUrl.Should().Be("plumbing-services");
    }

    [Fact]
    public async Task CreateAsync_Should_generate_seo_url_when_not_provided()
    {
        var request = new CategoryRequest
        {
            Name = "Plumbing Services",
            SortOrder = 1,
            IsActive = true
        };

        var result = await _service.CreateAsync(request);

        result.SeoUrl.Should().Be("plumbing-services");
    }

    [Fact]
    public async Task CreateAsync_Should_store_in_database()
    {
        var request = new CategoryRequest { Name = "Test", IsActive = true };

        await _service.CreateAsync(request);

        var count = await _context.Categories.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_Should_update_existing_category()
    {
        var cat = CreateCategory("Old", "old");
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var request = new CategoryRequest
        {
            Name = "Updated",
            Description = "New desc",
            SortOrder = 5,
            IsActive = false
        };

        var result = await _service.UpdateAsync(cat.Id, request);

        result.Name.Should().Be("Updated");
        result.SortOrder.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_Should_throw_for_missing()
    {
        var request = new CategoryRequest { Name = "Nope", IsActive = true };
        var act = () => _service.UpdateAsync(999, request);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Category with id 999 not found");
    }

    [Fact]
    public async Task DeleteAsync_Should_return_false_when_not_found()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_throw_when_has_subcategories()
    {
        var parent = CreateCategory("Parent", "parent");
        _context.Categories.Add(parent);
        await _context.SaveChangesAsync();

        var child = CreateCategory("Child", "child", parentId: parent.Id);
        _context.Categories.Add(child);
        await _context.SaveChangesAsync();

        var act = () => _service.DeleteAsync(parent.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete category with subcategories");
    }

    [Fact]
    public async Task DeleteAsync_Should_soft_delete()
    {
        var cat = CreateCategory("ToDelete", "to-delete");
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(cat.Id);
        result.Should().BeTrue();

        var inDb = await _context.Categories.IgnoreQueryFilters().FirstAsync(c => c.Id == cat.Id);
        inDb.IsDeleted.Should().BeTrue();
        inDb.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ToggleActiveAsync_Should_return_false_when_not_found()
    {
        var result = await _service.ToggleActiveAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleActiveAsync_Should_toggle_active_flag()
    {
        var cat = CreateCategory("ToggleTest", "toggle-test", isActive: false);
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var result = await _service.ToggleActiveAsync(cat.Id);
        result.Should().BeTrue();

        var inDb = await _context.Categories.FindAsync(cat.Id);
        inDb!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleActiveAsync_Should_toggle_from_active_to_inactive()
    {
        var cat = CreateCategory("ToggleTest2", "toggle-test2");
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        await _service.ToggleActiveAsync(cat.Id);

        var inDb = await _context.Categories.FindAsync(cat.Id);
        inDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSortOrderAsync_Should_return_false_when_not_found()
    {
        var result = await _service.UpdateSortOrderAsync(999, 5);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSortOrderAsync_Should_update_sort_order()
    {
        var cat = CreateCategory("Reorder", "reorder");
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var result = await _service.UpdateSortOrderAsync(cat.Id, 42);
        result.Should().BeTrue();

        var inDb = await _context.Categories.FindAsync(cat.Id);
        inDb!.SortOrder.Should().Be(42);
    }
}
