using API.Controllers;
using Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class TestableBaseApiController : BaseApiController
{
    public new IActionResult OkResult<T>(T data, string? message = null) => base.OkResult(data, message);
    public new IActionResult CreatedResult<T>(T data, string? message = null) => base.CreatedResult(data, message);
    public new IActionResult BadRequestResult(string message, List<string>? errors = null) => base.BadRequestResult(message, errors);
    public new IActionResult NotFoundResult(string message = "Resource not found") => base.NotFoundResult(message);

    public int CallGetUserId() => GetUserId();
}

public class BaseApiControllerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private TestableBaseApiController CreateController()
    {
        var controller = new TestableBaseApiController();
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public void OkResult_Should_return_200_with_success_true()
    {
        var controller = CreateController();
        var result = controller.OkResult(new { id = 1 }, "Success");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value;
        var dict = value!.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
        dict["success"].Should().Be(true);
        dict["message"].Should().Be("Success");
    }

    [Fact]
    public void CreatedResult_Should_return_201_with_success_true()
    {
        var controller = CreateController();
        var result = controller.CreatedResult(new { id = 1 }, "Created");

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var value = createdResult.Value;
        var dict = value!.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
        dict["success"].Should().Be(true);
        dict["message"].Should().Be("Created");
    }

    [Fact]
    public void BadRequestResult_Should_return_400_with_success_false()
    {
        var controller = CreateController();
        var result = controller.BadRequestResult("Error occurred", new List<string> { "Error1" });

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var value = badRequest.Value;
        var dict = value!.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
        dict["success"].Should().Be(false);
        dict["message"].Should().Be("Error occurred");
    }

    [Fact]
    public void NotFoundResult_Should_return_404_with_success_false()
    {
        var controller = CreateController();
        var result = controller.NotFoundResult("Not found");

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var value = notFound.Value;
        var dict = value!.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(value));
        dict["success"].Should().Be(false);
        dict["message"].Should().Be("Not found");
    }

    [Fact]
    public void GetUserId_Should_return_userId_from_service()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(42);
        var controller = CreateController();

        var userId = controller.CallGetUserId();

        userId.Should().Be(42);
    }

    [Fact]
    public void GetUserId_Should_throw_when_service_returns_null()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns((int?)null);
        var controller = CreateController();

        var act = () => controller.CallGetUserId();

        act.Should().Throw<UnauthorizedAccessException>().WithMessage("User not authenticated");
    }
}
