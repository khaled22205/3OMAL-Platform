using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Application.Common.Interfaces;
using Application.Features.Auth;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identityMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    private AuthService CreateService()
    {
        return new AuthService(_identityMock.Object, _jwtMock.Object, _context, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_Should_return_failure_when_email_already_exists()
    {
        _identityMock.Setup(x => x.UserExistsAsync("existing@test.com")).ReturnsAsync(true);
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest { Email = "existing@test.com", Password = "pass123", UserType = "Customer" });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already registered");
    }

    [Fact]
    public async Task RegisterAsync_Should_return_failure_when_user_creation_fails()
    {
        _identityMock.Setup(x => x.UserExistsAsync("test@test.com")).ReturnsAsync(false);
        _identityMock.Setup(x => x.CreateUserAsync("test@test.com", "pass123", "1234567890"))
            .ReturnsAsync((false, new[] { "Error creating user" }));
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "test@test.com",
            Password = "pass123",
            PhoneNumber = "1234567890",
            UserType = "Customer"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.Errors.Should().Contain("Error creating user");
    }

    [Fact]
    public async Task RegisterAsync_Should_return_failure_when_userId_not_found()
    {
        _identityMock.Setup(x => x.UserExistsAsync("test@test.com")).ReturnsAsync(false);
        _identityMock.Setup(x => x.CreateUserAsync("test@test.com", "pass123", "1234567890"))
            .ReturnsAsync((true, Enumerable.Empty<string>()));
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("test@test.com")).ReturnsAsync((int?)null);
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "test@test.com",
            Password = "pass123",
            PhoneNumber = "1234567890",
            UserType = "Customer"
        });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_Should_create_worker_profile_for_worker_role()
    {
        _identityMock.Setup(x => x.UserExistsAsync("worker@test.com")).ReturnsAsync(false);
        _identityMock.Setup(x => x.CreateUserAsync("worker@test.com", "pass123", "1234567890"))
            .ReturnsAsync((true, Enumerable.Empty<string>()));
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("worker@test.com")).ReturnsAsync(42);
        _identityMock.Setup(x => x.RoleExistsAsync("Worker")).ReturnsAsync(true);
        _identityMock.Setup(x => x.AddToRoleAsync(42, "Worker")).ReturnsAsync(true);
        _identityMock.Setup(x => x.GetUserRolesAsync(42)).ReturnsAsync(new List<string> { "Worker" });
        _jwtMock.Setup(x => x.GenerateTokens(42, "worker@test.com", It.IsAny<IList<string>>()))
            .Returns(("access", "refresh", DateTime.UtcNow.AddHours(1)));
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "worker@test.com",
            Password = "pass123",
            PhoneNumber = "1234567890",
            UserType = "Worker"
        });

        result.Success.Should().BeTrue();
        _context.WorkerProfiles.Should().ContainSingle(wp => wp.UserId == 42);
    }

    [Fact]
    public async Task RegisterAsync_Should_not_create_worker_profile_for_customer()
    {
        _identityMock.Setup(x => x.UserExistsAsync("cust@test.com")).ReturnsAsync(false);
        _identityMock.Setup(x => x.CreateUserAsync("cust@test.com", "pass123", "1234567890"))
            .ReturnsAsync((true, Enumerable.Empty<string>()));
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("cust@test.com")).ReturnsAsync(43);
        _identityMock.Setup(x => x.RoleExistsAsync("Customer")).ReturnsAsync(true);
        _identityMock.Setup(x => x.AddToRoleAsync(43, "Customer")).ReturnsAsync(true);
        _identityMock.Setup(x => x.GetUserRolesAsync(43)).ReturnsAsync(new List<string> { "Customer" });
        _jwtMock.Setup(x => x.GenerateTokens(43, "cust@test.com", It.IsAny<IList<string>>()))
            .Returns(("access", "refresh", DateTime.UtcNow.AddHours(1)));
        var service = CreateService();

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "cust@test.com",
            Password = "pass123",
            PhoneNumber = "1234567890",
            UserType = "Customer"
        });

        result.Success.Should().BeTrue();
        _context.WorkerProfiles.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginAsync_Should_return_failure_when_user_not_found()
    {
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("unknown@test.com")).ReturnsAsync((int?)null);
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest { Email = "unknown@test.com", Password = "pass123" });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_Should_return_failure_when_account_is_locked()
    {
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("locked@test.com")).ReturnsAsync(1);
        _identityMock.Setup(x => x.IsLockedOutAsync("locked@test.com")).ReturnsAsync(true);
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest { Email = "locked@test.com", Password = "pass123" });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Account is locked. Try again later.");
    }

    [Fact]
    public async Task LoginAsync_Should_return_failure_when_password_is_wrong()
    {
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("test@test.com")).ReturnsAsync(1);
        _identityMock.Setup(x => x.IsLockedOutAsync("test@test.com")).ReturnsAsync(false);
        _identityMock.Setup(x => x.CheckPasswordAsync("test@test.com", "wrong")).ReturnsAsync(false);
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "wrong" });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_Should_return_success_with_tokens()
    {
        _identityMock.Setup(x => x.GetUserIdByEmailAsync("test@test.com")).ReturnsAsync(1);
        _identityMock.Setup(x => x.IsLockedOutAsync("test@test.com")).ReturnsAsync(false);
        _identityMock.Setup(x => x.CheckPasswordAsync("test@test.com", "correct")).ReturnsAsync(true);
        _identityMock.Setup(x => x.GetUserRolesAsync(1)).ReturnsAsync(new List<string> { "Customer" });
        _identityMock.Setup(x => x.GetUserEmailAsync(1)).ReturnsAsync("test@test.com");
        _jwtMock.Setup(x => x.GenerateTokens(1, "test@test.com", It.IsAny<IList<string>>()))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddHours(1)));
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "correct" });

        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_return_failure_when_token_is_invalid()
    {
        var service = CreateService();

        var result = await service.RefreshTokenAsync("nonexistent-token");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_return_failure_when_token_is_revoked()
    {
        _context.Set<RefreshToken>().Add(new RefreshToken
        {
            Token = "revoked-token",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        });
        await _context.SaveChangesAsync();
        var service = CreateService();

        var result = await service.RefreshTokenAsync("revoked-token");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_return_failure_when_user_not_found()
    {
        _context.Set<RefreshToken>().Add(new RefreshToken
        {
            Token = "valid-token",
            UserId = 999,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        _identityMock.Setup(x => x.GetUserEmailAsync(999)).ReturnsAsync((string?)null);
        var service = CreateService();

        var result = await service.RefreshTokenAsync("valid-token");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_rotate_token()
    {
        _context.Set<RefreshToken>().Add(new RefreshToken
        {
            Token = "old-token",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        _identityMock.Setup(x => x.GetUserEmailAsync(1)).ReturnsAsync("test@test.com");
        _identityMock.Setup(x => x.GetUserRolesAsync(1)).ReturnsAsync(new List<string> { "Customer" });
        _jwtMock.Setup(x => x.GenerateTokens(1, "test@test.com", It.IsAny<IList<string>>()))
            .Returns(("new_access", "new_refresh", DateTime.UtcNow.AddHours(1)));
        var service = CreateService();

        var result = await service.RefreshTokenAsync("old-token");

        result.Success.Should().BeTrue();
        result.AccessToken.Should().Be("new_access");
        result.RefreshToken.Should().Be("new_refresh");

        var stored = await _context.Set<RefreshToken>().FirstAsync(rt => rt.Token == "old-token");
        stored.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_delegate_to_identity()
    {
        _identityMock.Setup(x => x.ChangePasswordAsync(1, "old", "new")).ReturnsAsync(true);
        var service = CreateService();

        var result = await service.ChangePasswordAsync(1, "old", "new");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_return_false_when_identity_fails()
    {
        _identityMock.Setup(x => x.ChangePasswordAsync(1, "wrong", "new")).ReturnsAsync(false);
        var service = CreateService();

        var result = await service.ChangePasswordAsync(1, "wrong", "new");

        result.Should().BeFalse();
    }
}
