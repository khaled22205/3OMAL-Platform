using System.Security.Claims;
using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private AuthController CreateController()
    {
        var controller = new AuthController(_authServiceMock.Object, _currentUserMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() } };
        return controller;
    }

    [Fact]
    public async Task Register_Should_return_201_when_successful()
    {
        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new AuthResponse { Success = true, Message = "Registration successful. Please verify your email." });
        var controller = CreateController();

        var result = await controller.Register(new RegisterRequest());

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Register_Should_return_400_when_fails()
    {
        _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new AuthResponse { Success = false, Message = "Email already registered" });
        var controller = CreateController();

        var result = await controller.Register(new RegisterRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_Should_return_200_when_successful()
    {
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new AuthResponse { Success = true });
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_Should_return_400_when_fails()
    {
        _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new AuthResponse { Success = false });
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_Should_return_200_when_successful()
    {
        _authServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AuthResponse { Success = true });
        var controller = CreateController();

        var result = await controller.RefreshToken(new RefreshTokenRequest { RefreshToken = "token" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_Should_return_400_when_fails()
    {
        _authServiceMock.Setup(x => x.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AuthResponse { Success = false });
        var controller = CreateController();

        var result = await controller.RefreshToken(new RefreshTokenRequest { RefreshToken = "invalid" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_Should_return_200_when_successful()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _authServiceMock.Setup(x => x.ChangePasswordAsync(1, "old", "new")).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.ChangePassword(new ChangePasswordRequest { CurrentPassword = "old", NewPassword = "new" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_Should_return_400_when_fails()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _authServiceMock.Setup(x => x.ChangePasswordAsync(1, "wrong", "new")).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.ChangePassword(new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "new" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Logout_Should_return_200()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
        _authServiceMock.Setup(x => x.LogoutAsync(1)).Returns(Task.CompletedTask);
        var controller = CreateController();

        var result = await controller.Logout();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetCurrentUser_Should_return_user_info()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(42);
        _currentUserMock.Setup(x => x.GetUserEmail()).Returns("test@test.com");
        _currentUserMock.Setup(x => x.GetUserRoles()).Returns(new List<string> { "Customer" });
        var controller = CreateController();

        var result = controller.GetCurrentUser();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }
}
