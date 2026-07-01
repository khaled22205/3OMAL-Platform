using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Integration.Tests.Controllers;

public class CategoriesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CategoriesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Should_Return_202_When_Not_Admin()
    {
        var client = _factory.CreateAuthenticatedClient(1, "Customer");
        var request = new { name = "New Category", seoUrl = "new-category" };

        var response = await client.PostAsJsonAsync("/api/v1/categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
