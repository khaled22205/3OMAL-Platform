using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Workers;
using Application.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class WorkersControllerTests
{
    private readonly Mock<IWorkerService> _workerServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private WorkersController CreateController()
    {
        var controller = new WorkersController(_workerServiceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task Search_Should_return_200()
    {
        _workerServiceMock.Setup(x => x.SearchAsync(It.IsAny<WorkerSearchRequest>()))
            .ReturnsAsync(new PagedResult<WorkerSummaryResponse>
            {
                Items = new List<WorkerSummaryResponse> { new() { Id = 1, FirstName = "John" } },
                TotalCount = 1
            });
        var controller = CreateController();

        var result = await controller.Search(new WorkerSearchRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_200_when_found()
    {
        _workerServiceMock.Setup(x => x.GetProfileByIdAsync(1))
            .ReturnsAsync(new WorkerProfileResponse { Id = 1, FirstName = "John" });
        var controller = CreateController();

        var result = await controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_404_when_not_found()
    {
        _workerServiceMock.Setup(x => x.GetProfileByIdAsync(999)).ReturnsAsync((WorkerProfileResponse?)null);
        var controller = CreateController();

        var result = await controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMyProfile_Should_return_200_when_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.GetProfileAsync(1))
            .ReturnsAsync(new WorkerProfileResponse { Id = 1, FirstName = "John" });
        var controller = CreateController();

        var result = await controller.GetMyProfile();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyProfile_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.GetProfileAsync(1)).ReturnsAsync((WorkerProfileResponse?)null);
        var controller = CreateController();

        var result = await controller.GetMyProfile();

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateProfile_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.CreateOrUpdateProfileAsync(1, It.IsAny<WorkerProfileRequest>()))
            .ReturnsAsync(new WorkerProfileResponse { Id = 1 });
        var controller = CreateController();

        var result = await controller.UpdateProfile(new WorkerProfileRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateAvailability_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.UpdateAvailabilityStatusAsync(1, true)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.UpdateAvailability(new WorkerStatusRequest { IsAvailable = true });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateAvailability_Should_return_400_when_fails()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.UpdateAvailabilityStatusAsync(1, true)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.UpdateAvailability(new WorkerStatusRequest { IsAvailable = true });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddAvailabilitySlot_Should_return_201()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.AddAvailabilityAsync(1, It.IsAny<WorkerAvailabilityRequest>()))
            .ReturnsAsync(new WorkerAvailabilityResponse { Id = 1 });
        var controller = CreateController();

        var result = await controller.AddAvailabilitySlot(new WorkerAvailabilityRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task RemoveAvailabilitySlot_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.RemoveAvailabilityAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.RemoveAvailabilitySlot(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveAvailabilitySlot_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.RemoveAvailabilityAsync(1, 999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.RemoveAvailabilitySlot(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddPortfolioItem_Should_return_201()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.AddPortfolioItemAsync(1, It.IsAny<WorkerPortfolioRequest>()))
            .ReturnsAsync(new WorkerPortfolioResponse { Id = 1 });
        var controller = CreateController();

        var result = await controller.AddPortfolioItem(new WorkerPortfolioRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task RemovePortfolioItem_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.RemovePortfolioItemAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.RemovePortfolioItem(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemovePortfolioItem_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _workerServiceMock.Setup(x => x.RemovePortfolioItemAsync(1, 999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.RemovePortfolioItem(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
