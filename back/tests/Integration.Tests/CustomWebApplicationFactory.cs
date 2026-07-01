using System.Text;
using API.Hubs;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Application.Common.Interfaces;
using Application.Features.Auth;
using Application.Features.Categories;
using Application.Features.Workers;
using Application.Features.Services;
using Application.Features.Bookings;
using Application.Features.Reviews;
using Application.Features.Payments;
using Application.Features.Favorites;
using Application.Features.Admin;
using Application.Features.Chat;

namespace Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"testdb_{Guid.NewGuid():N}";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var appBuilder = WebApplication.CreateBuilder();
        appBuilder.WebHost.UseTestServer();
        appBuilder.Environment.EnvironmentName = "Testing";

        appBuilder.Configuration["Jwt:Key"] = "test-key-1234567890-test-key-1234567890";
        appBuilder.Configuration["Jwt:Issuer"] = "TestIssuer";
        appBuilder.Configuration["Jwt:Audience"] = "TestAudience";
        appBuilder.Configuration["Jwt:AccessTokenExpirationMinutes"] = "15";
        appBuilder.Configuration["Jwt:RefreshTokenExpirationDays"] = "7";

        appBuilder.Services.AddControllers()
            .AddApplicationPart(typeof(Program).Assembly);
        appBuilder.Services.AddSignalR();
        appBuilder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(_dbName));
        appBuilder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        appBuilder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        });
        appBuilder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key-1234567890-test-key-1234567890")),
                    ValidateIssuer = true,
                    ValidIssuer = "TestIssuer",
                    ValidateAudience = true,
                    ValidAudience = "TestAudience",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        appBuilder.Services.AddAuthorization();
        appBuilder.Services.AddCors(options =>
            options.AddPolicy("AllowAll", opt =>
                opt.SetIsOriginAllowed(_ => true)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials()));
        appBuilder.Services.AddScoped<IIdentityService, IdentityService>();
        appBuilder.Services.AddScoped<IJwtService, JwtService>();
        appBuilder.Services.AddScoped<IFileService, FileService>();
        appBuilder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        appBuilder.Services.AddScoped<IAuthService, AuthService>();
        appBuilder.Services.AddScoped<ICategoryService, CategoryService>();
        appBuilder.Services.AddScoped<IWorkerService, WorkerService>();
        appBuilder.Services.AddScoped<IWorkerServiceService, WorkerServiceService>();
        appBuilder.Services.AddScoped<IBookingService, BookingService>();
        appBuilder.Services.AddScoped<IReviewService, ReviewService>();
        appBuilder.Services.AddScoped<IPaymentService, PaymentService>();
        appBuilder.Services.AddScoped<IFavoriteService, FavoriteService>();
        appBuilder.Services.AddScoped<IAdminService, AdminService>();
        appBuilder.Services.AddScoped<IChatService, ChatService>();
        appBuilder.Services.AddSingleton<ConnectionManager>();
        appBuilder.Services.AddHttpContextAccessor();

        var app = appBuilder.Build();

        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<ChatHub>("/hubs/chat");

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        app.Start();
        return app;
    }

    public string GenerateTestJwt(int userId, string role)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("test-key-1234567890-test-key-1234567890");
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Role, role)
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public HttpClient CreateAuthenticatedClient(int userId, string role)
    {
        var client = CreateClient();
        var token = GenerateTestJwt(userId, role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
