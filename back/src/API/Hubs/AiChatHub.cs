using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Application.Features.AiAssistant;
using Application.Common.Interfaces;

namespace API.Hubs;

[Authorize]
public class AiChatHub : Hub
{
    private readonly IAiAssistantService _assistantService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AiChatHub> _logger;

    public AiChatHub(
        IAiAssistantService assistantService,
        ICurrentUserService currentUser,
        ILogger<AiChatHub> logger)
    {
        _assistantService = assistantService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _logger.LogInformation("User {UserId} connected to AI chat hub", userId.Value);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ai_user_{userId.Value}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _logger.LogInformation("User {UserId} disconnected from AI chat hub", userId.Value);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(int conversationId, string content)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("AiResponseError", "Not authenticated");
            return;
        }

        var role = GetUserRole();

        try
        {
            var request = new SendAiMessageRequest
            {
                ConversationId = conversationId,
                Content = content
            };

            await foreach (var chunk in _assistantService.SendMessageStreamAsync(userId.Value, role, request, Context.ConnectionAborted))
            {
                await Clients.Caller.SendAsync("AiResponseChunk", chunk, Context.ConnectionAborted);

                if (chunk.IsComplete)
                {
                    _logger.LogInformation("AI streaming completed for conversation {ConvId}, user {UserId}",
                        conversationId, userId.Value);
                }
            }
        }
        catch (OperationCanceledException)
        {
            await Clients.Caller.SendAsync("AiResponseError", "Request cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI streaming error for conversation {ConvId}, user {UserId}",
                conversationId, userId.Value);
            await Clients.Caller.SendAsync("AiResponseError", "An error occurred processing your request");
        }
    }

    public async Task StartConversation(string? title, string? firstMessage)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("AiResponseError", "Not authenticated");
            return;
        }

        var role = GetUserRole();
        var request = new StartConversationRequest
        {
            Title = title,
            FirstMessage = firstMessage
        };

        var conversation = await _assistantService.StartConversationAsync(userId.Value, role, request);
        await Clients.Caller.SendAsync("AiConversationCreated", conversation);
    }

    public async Task DeleteConversation(int conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        await _assistantService.StartConversationAsync(userId.Value, GetUserRole(), new());
        await Clients.Caller.SendAsync("AiConversationDeleted", conversationId);
    }

    private int? GetUserId()
    {
        return _currentUser.GetUserId();
    }

    private string GetUserRole()
    {
        var roles = _currentUser.GetUserRoles();
        if (roles.Contains("Admin")) return "Admin";
        if (roles.Contains("Worker")) return "Worker";
        if (roles.Contains("Customer")) return "Customer";
        return "Guest";
    }
}
