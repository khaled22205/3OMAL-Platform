using System.Security.Claims;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Infrastructure.Tests;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

    private CurrentUserService CreateService(ClaimsPrincipal? user)
    {
        var httpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        return new CurrentUserService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void GetUserId_Should_return_userId_when_claim_exists()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        }));
        var service = CreateService(user);

        var result = service.GetUserId();

        result.Should().Be(42);
    }

    [Fact]
    public void GetUserId_Should_return_null_when_claim_missing()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var service = CreateService(user);

        var result = service.GetUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_Should_return_null_when_claim_is_not_an_integer()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number")
        }));
        var service = CreateService(user);

        var result = service.GetUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_Should_return_null_when_HttpContext_is_null()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        var result = service.GetUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserEmail_Should_return_email_when_claim_exists()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@test.com")
        }));
        var service = CreateService(user);

        var result = service.GetUserEmail();

        result.Should().Be("test@test.com");
    }

    [Fact]
    public void GetUserEmail_Should_return_null_when_claim_missing()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var service = CreateService(user);

        var result = service.GetUserEmail();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserEmail_Should_return_null_when_HttpContext_is_null()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        var result = service.GetUserEmail();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserRoles_Should_return_all_role_claims()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Worker")
        }));
        var service = CreateService(user);

        var result = service.GetUserRoles();

        result.Should().BeEquivalentTo(new[] { "Admin", "Worker" });
    }

    [Fact]
    public void GetUserRoles_Should_return_empty_list_when_no_role_claims()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var service = CreateService(user);

        var result = service.GetUserRoles();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetUserRoles_Should_return_empty_list_when_HttpContext_is_null()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        var result = service.GetUserRoles();

        result.Should().BeEmpty();
    }

    [Fact]
    public void IsInRole_Should_return_true_when_user_is_in_role()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }));
        var service = CreateService(user);

        var result = service.IsInRole("Admin");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInRole_Should_return_false_when_user_is_not_in_role()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var service = CreateService(user);

        var result = service.IsInRole("Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_Should_return_false_when_HttpContext_is_null()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        var result = service.IsInRole("Admin");

        result.Should().BeFalse();
    }
}
