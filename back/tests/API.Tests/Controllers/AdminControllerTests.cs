using API.Controllers.V1;
using Application.Features.Admin;
using Application.Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _adminServiceMock = new();

    private AdminController CreateController()
    {
        var controller = new AdminController(_adminServiceMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public async Task GetDashboard_Should_return_200()
    {
        _adminServiceMock.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(new DashboardStatsResponse());
        var controller = CreateController();

        var result = await controller.GetDashboard();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUsers_Should_return_200()
    {
        _adminServiceMock.Setup(x => x.GetUsersAsync(1, 10, null, null))
            .ReturnsAsync(new PagedResult<UserManagementResponse> { Items = new List<UserManagementResponse>(), TotalCount = 0 });
        var controller = CreateController();

        var result = await controller.GetUsers();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUser_Should_return_200_when_found()
    {
        _adminServiceMock.Setup(x => x.GetUserByIdAsync(1))
            .ReturnsAsync(new UserManagementResponse { Id = 1, Email = "test@test.com" });
        var controller = CreateController();

        var result = await controller.GetUser(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUser_Should_return_404_when_not_found()
    {
        _adminServiceMock.Setup(x => x.GetUserByIdAsync(999)).ReturnsAsync((UserManagementResponse?)null);
        var controller = CreateController();

        var result = await controller.GetUser(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_Should_return_200_when_successful()
    {
        _adminServiceMock.Setup(x => x.UpdateUserAsync(1, It.IsAny<AdminUserUpdateRequest>())).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.UpdateUser(1, new AdminUserUpdateRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_Should_return_404_when_not_found()
    {
        _adminServiceMock.Setup(x => x.UpdateUserAsync(999, It.IsAny<AdminUserUpdateRequest>())).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.UpdateUser(999, new AdminUserUpdateRequest());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task LockUser_Should_return_200_when_successful()
    {
        _adminServiceMock.Setup(x => x.LockoutUserAsync(1, null)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.LockUser(1, null);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task LockUser_Should_return_404_when_not_found()
    {
        _adminServiceMock.Setup(x => x.LockoutUserAsync(999, null)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.LockUser(999, null);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_Should_return_200_when_successful()
    {
        _adminServiceMock.Setup(x => x.DeleteUserAsync(1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.DeleteUser(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_Should_return_404_when_not_found()
    {
        _adminServiceMock.Setup(x => x.DeleteUserAsync(999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.DeleteUser(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ExportBookings_Should_return_file()
    {
        _adminServiceMock.Setup(x => x.ExportBookingsAsync(null, null))
            .ReturnsAsync(new byte[] { 1, 2, 3 });
        var controller = CreateController();

        var result = await controller.ExportBookings(null, null);

        result.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)result;
        fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileDownloadName.Should().Be("bookings.xlsx");
    }

    [Fact]
    public async Task ExportUsers_Should_return_file()
    {
        _adminServiceMock.Setup(x => x.ExportUsersAsync(null))
            .ReturnsAsync(new byte[] { 1, 2, 3 });
        var controller = CreateController();

        var result = await controller.ExportUsers(null);

        result.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)result;
        fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileDownloadName.Should().Be("users.xlsx");
    }
}
