using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Favorites;
using Application.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class FavoritesControllerTests
{
    private readonly Mock<IFavoriteService> _favoriteServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private FavoritesController CreateController()
    {
        var controller = new FavoritesController(_favoriteServiceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task GetAll_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _favoriteServiceMock.Setup(x => x.GetUserFavoritesAsync(1, 1, 10))
            .ReturnsAsync(new PagedResult<FavoriteResponse> { Items = new List<FavoriteResponse>(), TotalCount = 0 });
        var controller = CreateController();

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Add_Should_return_201()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _favoriteServiceMock.Setup(x => x.AddAsync(1, It.IsAny<AddFavoriteRequest>()))
            .ReturnsAsync(new FavoriteResponse { Id = 1 });
        var controller = CreateController();

        var result = await controller.Add(new AddFavoriteRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Remove_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _favoriteServiceMock.Setup(x => x.RemoveAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Remove(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Remove_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _favoriteServiceMock.Setup(x => x.RemoveAsync(1, 999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.Remove(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
