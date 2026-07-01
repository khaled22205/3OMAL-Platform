using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TestCommon.Factories;

namespace Integration.Tests.Database;

public class MigrationSmokeTests
{
    [Fact]
    public void Database_Can_Be_Created_And_Seeded()
    {
        using var context = TestDbContextFactory.Create();
        context.Database.EnsureCreated();
        context.Database.CanConnect().Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Filter_Should_Apply()
    {
        using var context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Set<Domain.Entities.Category>().Add(new Domain.Entities.Category
            {
                Name = "Active",
                SeoUrl = "active",
                IsDeleted = false
            });
            ctx.Set<Domain.Entities.Category>().Add(new Domain.Entities.Category
            {
                Name = "Deleted",
                SeoUrl = "deleted",
                IsDeleted = true
            });
        });

        var categories = context.Set<Domain.Entities.Category>().ToList();

        categories.Should().ContainSingle(c => c.Name == "Active");
        categories.Should().NotContain(c => c.Name == "Deleted");
    }
}
