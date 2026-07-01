using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class RefreshTokenTests
{
    private static RefreshToken CreateActiveToken() => new()
    {
        UserId = 1,
        Token = "test-token",
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedAt = DateTime.UtcNow
    };

    public class IsActive
    {
        [Fact]
        public void Should_be_active_when_not_revoked_and_not_expired()
        {
            var token = CreateActiveToken();
            token.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Should_be_inactive_when_expired()
        {
            var token = CreateActiveToken();
            token.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            token.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Should_be_inactive_when_revoked()
        {
            var token = CreateActiveToken();
            token.Revoke();
            token.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Should_be_inactive_when_revoked_and_expired()
        {
            var token = CreateActiveToken();
            token.Revoke();
            token.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            token.IsActive.Should().BeFalse();
        }
    }

    public class IsExpired
    {
        [Fact]
        public void Should_be_false_when_not_expired()
        {
            var token = CreateActiveToken();
            token.IsExpired.Should().BeFalse();
        }

        [Fact]
        public void Should_be_true_when_expired()
        {
            var token = CreateActiveToken();
            token.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            token.IsExpired.Should().BeTrue();
        }

        [Fact]
        public void Should_be_true_at_exact_expiry()
        {
            var token = CreateActiveToken();
            token.ExpiresAt = DateTime.UtcNow;
            token.IsExpired.Should().BeTrue();
        }
    }

    public class Revoke
    {
        [Fact]
        public void Should_set_IsRevoked_true()
        {
            var token = CreateActiveToken();
            token.Revoke();
            token.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public void Should_make_token_inactive()
        {
            var token = CreateActiveToken();
            token.Revoke();
            token.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Should_be_idempotent()
        {
            var token = CreateActiveToken();
            token.Revoke();
            token.Revoke();
            token.IsRevoked.Should().BeTrue();
        }
    }
}
