using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Features.Auth;
using Application.Common.Mappings;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IIdentityService identity,
        IJwtService jwt,
        AppDbContext context,
        ILogger<AuthService> logger)
    {
        _identity = identity;
        _jwt = jwt;
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _identity.UserExistsAsync(request.Email))
        {
            _logger.LogWarning("Registration failed: email {Email} already exists", request.Email);
            return new AuthResponse { Success = false, Message = "Email already registered" };
        }

        var (success, errors) = await _identity.CreateUserAsync(request.Email, request.Password, request.PhoneNumber);
        if (!success)
        {
            var errorList = errors.ToList();
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errorList);
            return new AuthResponse
            {
                Success = false,
                Message = "Registration failed",
                Errors = errorList
            };
        }

        var userId = await _identity.GetUserIdByEmailAsync(request.Email);
        if (userId == null)
        {
            _logger.LogError("User created but not found by email: {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Registration failed" };
        }

        var role = request.UserType == "Worker" ? "Worker" : "Customer";
        if (!await _identity.RoleExistsAsync(role))
        {
            await _identity.CreateRoleAsync(role);
            _logger.LogInformation("Created role {Role}", role);
        }

        await _identity.AddToRoleAsync(userId.Value, role);
        _logger.LogInformation("User {UserId} registered as {Role}", userId.Value, role);

        if (role == "Worker")
        {
            _context.WorkerProfiles.Add(new WorkerProfile { UserId = userId.Value });
            await _context.SaveChangesAsync();
        }

        var roles = (await _identity.GetUserRolesAsync(userId.Value)).ToList();
        _logger.LogInformation("Login after registration for UserId={UserId}, Roles={Roles}, Redirect={Redirect}",
            userId.Value, roles, role == "Admin" ? "/admin" : role == "Worker" ? "/worker" : "/customer");

        var tokens = _jwt.GenerateTokens(userId.Value, request.Email, roles);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful. Please verify your email.",
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            ExpiresAt = tokens.expiresAt,
            User = MappingHelper.ToUserInfo((userId.Value, request.Email, request.PhoneNumber, roles))
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var userId = await _identity.GetUserIdByEmailAsync(request.Email);
        if (userId == null)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        if (await _identity.IsLockedOutAsync(request.Email))
            return new AuthResponse { Success = false, Message = "Account is locked. Try again later." };

        if (!await _identity.CheckPasswordAsync(request.Email, request.Password))
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        var roles = (await _identity.GetUserRolesAsync(userId.Value)).ToList();
        _logger.LogInformation("Login UserId={UserId}, Email={Email}, Roles={Roles}",
            userId.Value, request.Email, roles);

        var tokens = _jwt.GenerateTokens(userId.Value, request.Email, roles);

        var email = await _identity.GetUserEmailAsync(userId.Value);
        var phone = request.Email; // We don't have phone from login, just email

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            ExpiresAt = tokens.expiresAt,
            User = MappingHelper.ToUserInfo((userId.Value, request.Email, (string?)null, roles))
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

        if (token == null)
            return new AuthResponse { Success = false, Message = "Invalid or expired refresh token" };

        var userEmail = await _identity.GetUserEmailAsync(token.UserId);
        if (userEmail == null)
            return new AuthResponse { Success = false, Message = "User not found" };

        token.Revoke();
        await _context.SaveChangesAsync();

        var roles = (await _identity.GetUserRolesAsync(token.UserId)).ToList();
        var tokens = _jwt.GenerateTokens(token.UserId, userEmail, roles);

        return new AuthResponse
        {
            Success = true,
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            ExpiresAt = tokens.expiresAt,
            User = MappingHelper.ToUserInfo((token.UserId, userEmail, (string?)null, roles))
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        return await _identity.ChangePasswordAsync(userId, currentPassword, newPassword);
    }

    public async Task<bool> VerifyEmailAsync(int userId, string token)
    {
        return await _identity.VerifyEmailAsync(userId, token);
    }

    public async Task LogoutAsync(int userId)
    {
        await _jwt.RevokeRefreshTokensAsync(userId);
    }
}
