using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Tests;

public class JwtServiceTests
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public JwtServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyThatIsAtLeast32BytesLong!",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenExpirationMinutes"] = "60",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    private JwtService CreateService()
    {
        return new JwtService(_config, _context);
    }

    [Fact]
    public void GenerateTokens_Should_return_access_token_with_correct_claims()
    {
        var service = CreateService();
        var (accessToken, _, _) = service.GenerateTokens(1, "test@test.com", new List<string> { "Customer" });

        accessToken.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(accessToken);

        jsonToken.Subject.Should().BeNull();
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@test.com");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Customer");
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateTokens_Should_return_valid_expiration()
    {
        var service = CreateService();
        var (accessToken, _, expiresAt) = service.GenerateTokens(1, "test@test.com", new List<string> { "Customer" });

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(accessToken);

        jsonToken.ValidTo.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateTokens_Should_handle_multiple_roles()
    {
        var service = CreateService();
        var (accessToken, _, _) = service.GenerateTokens(1, "test@test.com", new List<string> { "Admin", "Worker" });

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(accessToken);

        var roles = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        roles.Should().Contain(new[] { "Admin", "Worker" });
    }

    [Fact]
    public void GenerateTokens_Should_store_refresh_token_in_database()
    {
        var service = CreateService();
        var (_, refreshToken, _) = service.GenerateTokens(1, "test@test.com", new List<string> { "Customer" });

        refreshToken.Should().NotBeNullOrEmpty();

        var stored = _context.Set<RefreshToken>().FirstOrDefault(rt => rt.Token == refreshToken);
        stored.Should().NotBeNull();
        stored!.UserId.Should().Be(1);
        stored.IsActive.Should().BeTrue();
        stored.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void GenerateTokens_Should_set_refresh_token_expiry()
    {
        var service = CreateService();
        var (_, refreshToken, _) = service.GenerateTokens(1, "test@test.com", new List<string> { "Customer" });

        var stored = _context.Set<RefreshToken>().First(rt => rt.Token == refreshToken);
        stored.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RevokeRefreshTokensAsync_Should_revoke_all_active_tokens_for_user()
    {
        var service = CreateService();
        service.GenerateTokens(1, "test@test.com", new List<string> { "Customer" });
        service.GenerateTokens(1, "test@test.com", new List<string> { "Customer" });

        await service.RevokeRefreshTokensAsync(1);

        var tokens = _context.Set<RefreshToken>().Where(rt => rt.UserId == 1).ToList();
        tokens.Should().HaveCount(2);
        tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public async Task RevokeRefreshTokensAsync_Should_not_revoke_tokens_for_other_users()
    {
        var service = CreateService();
        service.GenerateTokens(1, "user1@test.com", new List<string>());
        service.GenerateTokens(2, "user2@test.com", new List<string>());

        await service.RevokeRefreshTokensAsync(1);

        var user1Tokens = _context.Set<RefreshToken>().Where(rt => rt.UserId == 1).ToList();
        var user2Tokens = _context.Set<RefreshToken>().Where(rt => rt.UserId == 2).ToList();

        user1Tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
        user2Tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeFalse());
    }

    [Fact]
    public async Task RevokeRefreshTokensAsync_Should_not_fail_when_no_active_tokens()
    {
        var service = CreateService();
        await service.RevokeRefreshTokensAsync(999);
    }

    [Fact]
    public void GenerateTokens_Should_not_accept_short_key()
    {
        var shortConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Short",
                ["Jwt:Issuer"] = "Issuer",
                ["Jwt:Audience"] = "Audience",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            })
            .Build();

        var act = () => new JwtService(shortConfig, _context)
            .GenerateTokens(1, "test@test.com", new List<string>());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
