using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Features.Chat;
using Application.Common.Models;

namespace API.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _chatServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(42);
        _controller = new ChatController(_chatServiceMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task GetConversations_Should_return_ok_with_paged_result()
    {
        var pagedResult = new PagedResult<ConversationResponse>
        {
            Items = [new ConversationResponse { Id = 1, OtherUser = new UserBriefResponse { UserId = 2, FirstName = "A", LastName = "B" } }],
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };
        _chatServiceMock.Setup(x => x.GetConversationsAsync(42, 1, 20)).ReturnsAsync(pagedResult);

        var result = await _controller.GetConversations(1, 20);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetConversation_Should_return_ok()
    {
        var response = new ConversationResponse
        {
            Id = 1,
            OtherUser = new UserBriefResponse { UserId = 2, FirstName = "A", LastName = "B" }
        };
        _chatServiceMock.Setup(x => x.GetConversationAsync(1, 42)).ReturnsAsync(response);

        var result = await _controller.GetConversation(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUnreadCount_Should_return_ok()
    {
        _chatServiceMock.Setup(x => x.GetUnreadCountAsync(42)).ReturnsAsync(new UnreadCountResponse { Count = 5 });

        var result = await _controller.GetUnreadCount();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Should().NotBeNull();
    }
}
