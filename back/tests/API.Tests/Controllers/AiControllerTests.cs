using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using API.Controllers.V1;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.AiAssistant;

namespace API.Tests.Controllers;

public class AiControllerTests
{
    private readonly Mock<IAiAssistantService> _assistantMock = new();
    private readonly Mock<IAiConversationService> _conversationMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly AiController _controller;

    public AiControllerTests()
    {
        _controller = new AiController(
            _assistantMock.Object, _conversationMock.Object, _currentUserMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(_currentUserMock.Object);
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    public class StartConversation : AiControllerTests
    {
        [Fact]
        public async Task Should_return_created_for_authenticated_user()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _assistantMock.Setup(x => x.StartConversationAsync(1, null, "Customer",
                It.IsAny<StartConversationRequest>()))
                .ReturnsAsync(new AiConversationSummaryResponse { Id = 1 });

            var result = await _controller.StartConversation(
                new StartConversationRequest { Title = "Help", FirstMessage = "Hi" });

            result.Should().BeOfType<CreatedResult>();
            var created = result as CreatedResult;
            created!.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Should_pass_sessionId_for_guest()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns((int?)null);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns([]);
            _assistantMock.Setup(x => x.StartConversationAsync(null, "sess-abc", "Guest",
                It.Is<StartConversationRequest>(r => r.SessionId == "sess-abc")))
                .ReturnsAsync(new AiConversationSummaryResponse { Id = 2 });

            var result = await _controller.StartConversation(
                new StartConversationRequest { SessionId = "sess-abc" });

            result.Should().BeOfType<CreatedResult>();
        }
    }

    public class GetConversations : AiControllerTests
    {
        [Fact]
        public async Task Should_return_paged_result_for_authenticated_user()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.GetConversationsAsync(1, null, "Customer", 1, 20))
                .ReturnsAsync(new PagedResult<AiConversationSummaryResponse>
                {
                    Items = [new AiConversationSummaryResponse { Id = 1, Title = "Conv1" }],
                    Page = 1, PageSize = 20, TotalCount = 1
                });

            var result = await _controller.GetConversations(null, 1, 20);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = ok.Value?.GetType().GetProperty("data")?.GetValue(ok.Value) as PagedResult<AiConversationSummaryResponse>;
            data.Should().NotBeNull();
            data!.Items.Should().ContainSingle();
        }

        [Fact]
        public async Task Should_use_sessionId_for_guest()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns((int?)null);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns([]);
            _conversationMock.Setup(x => x.GetConversationsAsync(null, "sess-xyz", "Guest", 1, 20))
                .ReturnsAsync(new PagedResult<AiConversationSummaryResponse>
                {
                    Items = [], Page = 1, PageSize = 20, TotalCount = 0
                });

            var result = await _controller.GetConversations("sess-xyz", 1, 20);

            result.Should().BeOfType<OkObjectResult>();
        }
    }

    public class GetConversation : AiControllerTests
    {
        [Fact]
        public async Task Should_return_conversation_for_owner()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.GetConversationAsync(5, 1, null, "Customer"))
                .ReturnsAsync(new AiConversationDetailResponse { Id = 5, Messages = [] });

            var result = await _controller.GetConversation(5, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Should_return_not_found_for_non_owner()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.GetConversationAsync(5, 1, null, "Customer"))
                .ReturnsAsync((AiConversationDetailResponse?)null);

            var result = await _controller.GetConversation(5, null);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }

    public class DeleteConversation : AiControllerTests
    {
        [Fact]
        public async Task Should_delete_for_owner()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.DeleteConversationAsync(3, 1, null, "Customer"))
                .ReturnsAsync(true);

            var result = await _controller.DeleteConversation(3, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Should_return_not_found_for_non_owner()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.DeleteConversationAsync(3, 1, null, "Customer"))
                .ReturnsAsync(false);

            var result = await _controller.DeleteConversation(3, null);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }

    public class SendMessage : AiControllerTests
    {
        [Fact]
        public async Task Should_return_ok()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _assistantMock.Setup(x => x.SendMessageAsync(1, null, "Customer",
                It.IsAny<SendAiMessageRequest>()))
                .ReturnsAsync(new AiMessageResponse { Id = 1, Content = "Response" });

            var result = await _controller.SendMessage(1,
                new SendAiMessageRequest { Content = "Hello" });

            result.Should().BeOfType<OkObjectResult>();
        }
    }

    public class SearchConversations : AiControllerTests
    {
        [Fact]
        public async Task Should_return_filtered_results()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.SearchConversationsAsync(1, null, "Customer", "plumb", 1, 20))
                .ReturnsAsync(new PagedResult<AiConversationSummaryResponse>
                {
                    Items = [new AiConversationSummaryResponse { Id = 1, Title = "Plumbing help" }],
                    Page = 1, PageSize = 20, TotalCount = 1
                });

            var result = await _controller.SearchConversations("plumb", null, 1, 20);

            result.Should().BeOfType<OkObjectResult>();
        }
    }

    public class GetSuggestions : AiControllerTests
    {
        [Fact]
        public async Task Should_return_role_specific_prompts()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Admin"]);
            _assistantMock.Setup(x => x.GetSuggestedPromptsAsync("Admin"))
                .ReturnsAsync(new AiSuggestedPromptsResponse
                {
                    Prompts = ["كام مستخدم عندنا على المنصة؟"]
                });

            var result = await _controller.GetSuggestions();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Should_return_guest_prompts_for_anonymous()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns((int?)null);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns([]);
            _assistantMock.Setup(x => x.GetSuggestedPromptsAsync("Guest"))
                .ReturnsAsync(new AiSuggestedPromptsResponse
                {
                    Prompts = ["What services does 3OMAL offer?"]
                });

            var result = await _controller.GetSuggestions();

            result.Should().BeOfType<OkObjectResult>();
        }
    }

    public class GetMessages : AiControllerTests
    {
        [Fact]
        public async Task Should_return_paged_messages()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.GetConversationAsync(1, 1, null, "Customer"))
                .ReturnsAsync(new AiConversationDetailResponse
                {
                    Id = 1,
                    Messages =
                    [
                        new AiMessageResponse { Id = 1, Content = "Hi" },
                        new AiMessageResponse { Id = 2, Content = "Hello!" }
                    ]
                });

            var result = await _controller.GetMessages(1, null, 1, 50);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Should_return_not_found_for_invalid_conversation()
        {
            _currentUserMock.Setup(x => x.GetUserId()).Returns(1);
            _currentUserMock.Setup(x => x.GetUserRoles()).Returns(["Customer"]);
            _conversationMock.Setup(x => x.GetConversationAsync(999, 1, null, "Customer"))
                .ReturnsAsync((AiConversationDetailResponse?)null);

            var result = await _controller.GetMessages(999, null, 1, 50);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
