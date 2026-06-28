using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using src.Data;
using src.DTOs.Auth;
using src.Helpers;
using src.Models;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly SignInManager<IdentityUser<int>> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser<int>> userManager,
        SignInManager<IdentityUser<int>> signInManager,
        RoleManager<IdentityRole<int>> roleManager,
        AppDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return new AuthResponse { Success = false, Message = "Email already registered" };

        var user = new IdentityUser<int>
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return new AuthResponse
            {
                Success = false,
                Message = "Registration failed",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };

        var role = request.UserType == "Worker" ? "Worker" : "Customer";
        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole<int>(role));

        await _userManager.AddToRoleAsync(user, role);

        if (role == "Worker")
        {
            _context.WorkerProfiles.Add(new WorkerProfile
            {
                UserId = user.Id
            });
            await _context.SaveChangesAsync();
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var tokens = await GenerateTokensAsync(user, roles);

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful. Please verify your email.",
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiration()),
            User = user.ToUserInfo(roles)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (result.IsLockedOut)
            return new AuthResponse { Success = false, Message = "Account is locked. Try again later." };
        if (!result.Succeeded)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var tokens = await GenerateTokensAsync(user, roles);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiration()),
            User = user.ToUserInfo(roles)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

        if (storedToken == null)
            return new AuthResponse { Success = false, Message = "Invalid or expired refresh token" };

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user == null)
            return new AuthResponse { Success = false, Message = "User not found" };

        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var tokens = await GenerateTokensAsync(user, roles);

        return new AuthResponse
        {
            Success = true,
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiration()),
            User = user.ToUserInfo(roles)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> VerifyEmailAsync(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task LogoutAsync(int userId)
    {
        var tokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(IdentityUser<int> user, List<string> roles)
    {
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);
        return (accessToken, refreshToken);
    }

    private string GenerateAccessToken(IdentityUser<int> user, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiration()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(int userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

            _context.Set<RefreshToken>().Add(new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return token;
    }

    private int GetAccessTokenExpiration() =>
        _configuration.GetSection("Jwt").GetValue<int>("AccessTokenExpirationMinutes");

    private int GetRefreshTokenExpirationDays() =>
        _configuration.GetSection("Jwt").GetValue<int>("RefreshTokenExpirationDays");
}