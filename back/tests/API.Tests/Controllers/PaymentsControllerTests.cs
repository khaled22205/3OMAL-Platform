using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private PaymentsController CreateController()
    {
        var controller = new PaymentsController(_paymentServiceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task GetByBookingId_Should_return_200_when_found()
    {
        _paymentServiceMock.Setup(x => x.GetByBookingIdAsync(1))
            .ReturnsAsync(new PaymentResponse { Id = 1, Amount = 500 });
        var controller = CreateController();

        var result = await controller.GetByBookingId(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByBookingId_Should_return_404_when_not_found()
    {
        _paymentServiceMock.Setup(x => x.GetByBookingIdAsync(999)).ReturnsAsync((PaymentResponse?)null);
        var controller = CreateController();

        var result = await controller.GetByBookingId(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ProcessPayment_Should_return_200()
    {
        _paymentServiceMock.Setup(x => x.ProcessPaymentAsync(1, "CreditCard"))
            .ReturnsAsync(new PaymentResponse { Id = 1, Status = "Completed" });
        var controller = CreateController();

        var result = await controller.ProcessPayment(new ProcessPaymentRequest { BookingId = 1, PaymentMethod = "CreditCard" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Refund_Should_return_200_when_successful()
    {
        _paymentServiceMock.Setup(x => x.ProcessRefundAsync(1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Refund(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Refund_Should_return_400_when_fails()
    {
        _paymentServiceMock.Setup(x => x.ProcessRefundAsync(1)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.Refund(1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
