using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Integration.Tests.Auth;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Should_Create_User_And_Return_Tokens()
    {
        var client = _factory.CreateClient();
        var request = new
        {
            email = "newuser@test.com",
            password = "Password123",
            firstName = "New",
            lastName = "User",
            phoneNumber = "01000000000",
            userType = "Customer"
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_Should_Return_400_When_Credentials_Invalid()
    {
        var client = _factory.CreateClient();
        var request = new { email = "nonexistent@test.com", password = "wrong" };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_401_When_Unauthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_200_When_Authenticated()
    {
        var client = _factory.CreateAuthenticatedClient(1, "Customer");

        var response = await client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
