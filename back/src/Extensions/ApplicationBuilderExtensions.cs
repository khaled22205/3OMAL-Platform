using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Data.Seed;
using src.Middleware;

namespace src.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser<int>>>();

        await context.Database.MigrateAsync();
        await DataSeeder.SeedAsync(context, userManager);
    }

    public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
    {
        app.UseExceptionHandling();
        app.UseRequestLogging();
        return app;
    }
}