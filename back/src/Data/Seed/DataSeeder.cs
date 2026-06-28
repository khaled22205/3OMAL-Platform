using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using src.Models;

namespace src.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<IdentityUser<int>> userManager)
    {
        await SeedRoles(context);
        await SeedAdminUser(userManager);
        await SeedCategories(context);
    }

    private static async Task SeedRoles(AppDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new IdentityRole<int> { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Name = "Customer", NormalizedName = "CUSTOMER" },
                new IdentityRole<int> { Name = "Worker", NormalizedName = "WORKER" }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAdminUser(UserManager<IdentityUser<int>> userManager)
    {
        if (await userManager.FindByEmailAsync("admin@3omal.com") == null)
        {
            var admin = new IdentityUser<int>
            {
                UserName = "admin",
                Email = "admin@3omal.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task SeedCategories(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Plumbing", SeoUrl = "plumbing", Description = "Plumbing services", SortOrder = 1, IsActive = true },
            new() { Name = "Electrical", SeoUrl = "electrical", Description = "Electrical services", SortOrder = 2, IsActive = true },
            new() { Name = "Carpentry", SeoUrl = "carpentry", Description = "Carpentry services", SortOrder = 3, IsActive = true },
            new() { Name = "Painting", SeoUrl = "painting", Description = "Painting services", SortOrder = 4, IsActive = true },
            new() { Name = "AC & Refrigeration", SeoUrl = "ac-refrigeration", Description = "AC and refrigeration services", SortOrder = 5, IsActive = true },
            new() { Name = "Cleaning", SeoUrl = "cleaning", Description = "Cleaning services", SortOrder = 6, IsActive = true },
            new() { Name = "Moving Services", SeoUrl = "moving-services", Description = "Moving and relocation services", SortOrder = 7, IsActive = true },
            new() { Name = "Technology & Smart Home", SeoUrl = "technology-smart-home", Description = "Technology and smart home services", SortOrder = 8, IsActive = true },
            new() { Name = "Gardening", SeoUrl = "gardening", Description = "Gardening services", SortOrder = 9, IsActive = true },
            new() { Name = "Pest Control", SeoUrl = "pest-control", Description = "Pest control services", SortOrder = 10, IsActive = true },
            new() { Name = "Automotive", SeoUrl = "automotive", Description = "Mobile car mechanic services", SortOrder = 11, IsActive = true },
            new() { Name = "General Maintenance", SeoUrl = "general-maintenance", Description = "General maintenance services", SortOrder = 12, IsActive = true }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}