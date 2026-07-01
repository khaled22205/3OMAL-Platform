using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Services;
using Application.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class ServicesControllerTests
{
    private readonly Mock<IWorkerServiceService> _serviceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private ServicesController CreateController()
    {
        var controller = new ServicesController(_serviceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task Search_Should_return_200()
    {
        _serviceMock.Setup(x => x.SearchAsync(null, null, 1, 10))
            .ReturnsAsync(new PagedResult<ServiceResponse> { Items = new List<ServiceResponse>(), TotalCount = 0 });
        var controller = CreateController();

        var result = await controller.Search(null, null, 1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_200_when_found()
    {
        _serviceMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ServiceResponse { Id = 1, Title = "Fix Pipe" });
        var controller = CreateController();

        var result = await controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_404_when_not_found()
    {
        _serviceMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((ServiceResponse?)null);
        var controller = CreateController();

        var result = await controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByWorker_Should_return_200()
    {
        _serviceMock.Setup(x => x.GetByWorkerAsync(1))
            .ReturnsAsync(new List<ServiceResponse> { new() { Id = 1 } });
        var controller = CreateController();

        var result = await controller.GetByWorker(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Should_return_201()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _serviceMock.Setup(x => x.CreateAsync(1, It.IsAny<ServiceRequest>()))
            .ReturnsAsync(new ServiceResponse { Id = 1, Title = "New Service" });
        var controller = CreateController();

        var result = await controller.Create(new ServiceRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Update_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _serviceMock.Setup(x => x.UpdateAsync(1, 1, It.IsAny<ServiceRequest>()))
            .ReturnsAsync(new ServiceResponse { Id = 1, Title = "Updated" });
        var controller = CreateController();

        var result = await controller.Update(1, new ServiceRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _serviceMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Delete(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _serviceMock.Setup(x => x.DeleteAsync(1, 999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ToggleActive_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _serviceMock.Setup(x => x.ToggleActiveAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.ToggleActive(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ToggleActive_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _serviceMock.Setup(x => x.ToggleActiveAsync(1, 999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.ToggleActive(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
