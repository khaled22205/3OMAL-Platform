using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Bookings;
using Application.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class BookingsControllerTests
{
    private readonly Mock<IBookingService> _bookingServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private BookingsController CreateController()
    {
        var controller = new BookingsController(_bookingServiceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task GetById_Should_return_200_when_found()
    {
        _bookingServiceMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Pending" });
        var controller = CreateController();

        var result = await controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_404_when_not_found()
    {
        _bookingServiceMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((BookingResponse?)null);
        var controller = CreateController();

        var result = await controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMyBookings_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.GetCustomerBookingsAsync(1, It.IsAny<BookingFilterRequest>()))
            .ReturnsAsync(new PagedResult<BookingResponse> { Items = new List<BookingResponse>(), TotalCount = 0 });
        var controller = CreateController();

        var result = await controller.GetMyBookings(new BookingFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetWorkerBookings_Should_return_200()
    {
        _bookingServiceMock.Setup(x => x.GetWorkerBookingsAsync(1, It.IsAny<BookingFilterRequest>()))
            .ReturnsAsync(new PagedResult<BookingResponse> { Items = new List<BookingResponse>(), TotalCount = 0 });
        var controller = CreateController();

        var result = await controller.GetWorkerBookings(1, new BookingFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Should_return_201()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.CreateAsync(1, It.IsAny<CreateBookingRequest>()))
            .ReturnsAsync(new BookingResponse { Id = 1 });
        var controller = CreateController();

        var result = await controller.Create(new CreateBookingRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Accept_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.AcceptAsync(1, 1))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Accepted" });
        var controller = CreateController();

        var result = await controller.Accept(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Reject_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.RejectAsync(1, 1, null))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Rejected" });
        var controller = CreateController();

        var result = await controller.Reject(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Cancel_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.CancelAsync(1, 1, null))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Cancelled" });
        var controller = CreateController();

        var result = await controller.Cancel(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Reschedule_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        var newDate = DateTime.UtcNow.AddDays(3);
        _bookingServiceMock.Setup(x => x.RescheduleAsync(1, 1, newDate))
            .ReturnsAsync(new BookingResponse { Id = 1, ScheduledAt = newDate });
        var controller = CreateController();

        var result = await controller.Reschedule(1, newDate);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task MarkOnTheWay_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.MarkOnTheWayAsync(1, 1))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "OnTheWay" });
        var controller = CreateController();

        var result = await controller.MarkOnTheWay(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task StartJob_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.StartJobAsync(1, 1))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Started" });
        var controller = CreateController();

        var result = await controller.StartJob(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PauseJob_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.PauseJobAsync(1, 1))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Paused" });
        var controller = CreateController();

        var result = await controller.PauseJob(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CompleteJob_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _bookingServiceMock.Setup(x => x.CompleteJobAsync(1, 1))
            .ReturnsAsync(new BookingResponse { Id = 1, Status = "Completed" });
        var controller = CreateController();

        var result = await controller.CompleteJob(1);

        result.Should().BeOfType<OkObjectResult>();
    }
}
