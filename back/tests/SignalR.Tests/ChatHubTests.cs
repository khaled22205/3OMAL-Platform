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
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
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

namespace SignalR.Tests;

public class ChatHubTests : IAsyncLifetime
{
    private CustomSignalRFactory _factory = null!;
    private HubConnection _connection1 = null!;
    private HubConnection _connection2 = null!;

    public async Task InitializeAsync()
    {
        _factory = new CustomSignalRFactory();
        _connection1 = CreateConnection(_factory, 1, "Customer");
        _connection2 = CreateConnection(_factory, 2, "Worker");
        await _connection1.StartAsync();
        await _connection2.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection1.DisposeAsync();
        await _connection2.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task SendMessage_Should_Broadcast_To_Group()
    {
        var tcs = new TaskCompletionSource<object?>();
        _connection1.On<object?>("NewMessage", msg =>
        {
            tcs.TrySetResult(msg);
        });

        await _connection2.InvokeAsync("JoinConversationGroup", 1);
        await _connection1.InvokeAsync("JoinConversationGroup", 1);

        await _connection2.InvokeAsync("SendMessage", new
        {
            conversationId = 1,
            messageType = "Text",
            content = "Hello from worker",
            replyToMessageId = (int?)null
        });

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10))) == tcs.Task;
        completed.Should().BeTrue("message should be received within timeout");
    }

    [Fact]
    public async Task JoinConversationGroup_Should_Succeed()
    {
        await _connection1.InvokeAsync("JoinConversationGroup", 1);
    }

    private static HubConnection CreateConnection(CustomSignalRFactory factory, int userId, string role)
    {
        var token = GenerateJwt(userId, role);
        var client = factory.CreateClient();
        return new HubConnectionBuilder()
            .WithUrl($"http://localhost/hubs/chat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                options.HttpMessageHandlerFactory = _ => factory.CreateHandler();
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
            .Build();
    }

    private static string GenerateJwt(int userId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key-1234567890-test-key-1234567890"));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class CustomSignalRFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"testdb_{Guid.NewGuid():N}";
    private TestServer _testServer = null!;

    public HttpMessageHandler CreateHandler() => _testServer.CreateHandler();

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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
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

            context.Conversations.Add(new Domain.Entities.Conversation());
            context.SaveChanges();

            var conv = context.Conversations.First();
            context.ConversationParticipants.AddRange(
                new Domain.Entities.ConversationParticipant { ConversationId = conv.Id, UserId = 1, JoinedAt = DateTime.UtcNow },
                new Domain.Entities.ConversationParticipant { ConversationId = conv.Id, UserId = 2, JoinedAt = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

        app.Start();
        _testServer = app.GetTestServer();
        return app;
    }
}
