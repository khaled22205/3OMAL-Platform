using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Reviews;
using Application.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class ReviewsControllerTests
{
    private readonly Mock<IReviewService> _reviewServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private ReviewsController CreateController()
    {
        var controller = new ReviewsController(_reviewServiceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task GetWorkerReviews_Should_return_200()
    {
        _reviewServiceMock.Setup(x => x.GetWorkerReviewsAsync(1, 1, 10))
            .ReturnsAsync(new PagedResult<ReviewResponse> { Items = new List<ReviewResponse>(), TotalCount = 0 });
        var controller = CreateController();

        var result = await controller.GetWorkerReviews(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_200_when_found()
    {
        _reviewServiceMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ReviewResponse { Id = 1, Rating = 5 });
        var controller = CreateController();

        var result = await controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_404_when_not_found()
    {
        _reviewServiceMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((ReviewResponse?)null);
        var controller = CreateController();

        var result = await controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_Should_return_201()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _reviewServiceMock.Setup(x => x.CreateAsync(1, It.IsAny<CreateReviewRequest>()))
            .ReturnsAsync(new ReviewResponse { Id = 1, Rating = 5 });
        var controller = CreateController();

        var result = await controller.Create(new CreateReviewRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Update_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _reviewServiceMock.Setup(x => x.UpdateAsync(1, 1, It.IsAny<UpdateReviewRequest>()))
            .ReturnsAsync(new ReviewResponse { Id = 1, Rating = 4 });
        var controller = CreateController();

        var result = await controller.Update(1, new UpdateReviewRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Reply_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _reviewServiceMock.Setup(x => x.ReplyAsync(1, 1, "Thank you!")).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Reply(1, new WorkerReplyRequest { Reply = "Thank you!" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Reply_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _reviewServiceMock.Setup(x => x.ReplyAsync(1, 999, "Thanks")).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.Reply(999, new WorkerReplyRequest { Reply = "Thanks" });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _reviewServiceMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Delete(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Should_return_404_when_not_found()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _reviewServiceMock.Setup(x => x.DeleteAsync(1, 999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
