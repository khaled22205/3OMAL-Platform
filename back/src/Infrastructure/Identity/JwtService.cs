using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Identity;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public JwtService(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public (string accessToken, string refreshToken, DateTime expiresAt) GenerateTokens(int userId, string email, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("AccessTokenExpirationMinutes"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateAndStoreRefreshToken(userId);

        return (accessToken, refreshToken, expiration);
    }

    private string GenerateAndStoreRefreshToken(int userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        _context.Set<RefreshToken>().Add(new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetSection("Jwt").GetValue<int>("RefreshTokenExpirationDays")),
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
        return token;
    }

    public async Task RevokeRefreshTokensAsync(int userId)
    {
        var tokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.Revoke();

        await _context.SaveChangesAsync();
    }
}
