using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Seed;
using Infrastructure.FileStorage;
using Infrastructure.Identity;
using Infrastructure.Services;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name)));

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<Application.Features.Auth.IAuthService, AuthService>();
        services.AddScoped<Application.Features.Categories.ICategoryService, CategoryService>();
        services.AddScoped<Application.Features.Workers.IWorkerService, WorkerService>();
        services.AddScoped<Application.Features.Services.IWorkerServiceService, WorkerServiceService>();
        services.AddScoped<Application.Features.Bookings.IBookingService, BookingService>();
        services.AddScoped<Application.Features.Reviews.IReviewService, ReviewService>();
        services.AddScoped<Application.Features.Payments.IPaymentService, PaymentService>();
        services.AddScoped<Application.Features.Favorites.IFavoriteService, FavoriteService>();
        services.AddScoped<Application.Features.Admin.IAdminService, AdminService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
