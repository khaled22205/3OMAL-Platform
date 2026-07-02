using Microsoft.AspNetCore.SignalR;
using Application.Features.AiAssistant;
using Application.Common.Interfaces;

namespace API.Hubs;

// No [Authorize] — guests can connect too; auth is handled per-method
public class AiChatHub : Hub
{
    private readonly IAiAssistantService _assistantService;
    private readonly IAiConversationService _conversationService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AiChatHub> _logger;

    public AiChatHub(
        IAiAssistantService assistantService,
        IAiConversationService conversationService,
        ICurrentUserService currentUser,
        ILogger<AiChatHub> logger)
    {
        _assistantService = assistantService;
        _conversationService = conversationService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var sessionId = GetSessionId();

        if (userId.HasValue)
        {
            var role = GetUserRole();
            _logger.LogInformation(
                "Authenticated user {UserId} ({Role}) connected to AI hub", userId.Value, role);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ai_user_{userId.Value}");
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            _logger.LogInformation("Guest session {SessionId} connected to AI hub", sessionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ai_session_{sessionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
            _logger.LogInformation("User {UserId} disconnected from AI hub", userId.Value);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(int conversationId, string content, string? sessionId = null)
    {
        var userId = GetUserId();
        var resolvedSessionId = sessionId ?? GetSessionId();
        var role = GetUserRole();

        try
        {
            var request = new SendAiMessageRequest
            {
                ConversationId = conversationId,
                Content = content,
                SessionId = resolvedSessionId
            };

            await foreach (var chunk in _assistantService.SendMessageStreamAsync(
                userId, resolvedSessionId, role, request, Context.ConnectionAborted))
            {
                await Clients.Caller.SendAsync("AiResponseChunk", chunk);

                if (chunk.IsComplete)
                {
                    _logger.LogInformation(
                        "AI streaming completed for conversation {ConvId}, user {UserId}, session {SessionId}",
                        conversationId, userId, resolvedSessionId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            await Clients.Caller.SendAsync("AiResponseError", "Request cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AI streaming error for conversation {ConvId}, user {UserId}, session {SessionId}",
                conversationId, userId, resolvedSessionId);
            await Clients.Caller.SendAsync("AiResponseError", "An error occurred processing your request");
        }
    }

    public async Task StartConversation(string? title, string? firstMessage, string? sessionId = null)
    {
        var userId = GetUserId();
        var resolvedSessionId = sessionId ?? GetSessionId();
        var role = GetUserRole();

        var request = new StartConversationRequest
        {
            Title = title,
            FirstMessage = firstMessage,
            SessionId = resolvedSessionId
        };

        var conversation = await _assistantService.StartConversationAsync(
            userId, resolvedSessionId, role, request);
        await Clients.Caller.SendAsync("AiConversationCreated", conversation);
    }

    public async Task DeleteConversation(int conversationId, string? sessionId = null)
    {
        var userId = GetUserId();
        var resolvedSessionId = sessionId ?? GetSessionId();
        var role = GetUserRole();

        await _conversationService.DeleteConversationAsync(
            conversationId, userId, resolvedSessionId, role);
        await Clients.Caller.SendAsync("AiConversationDeleted", conversationId);
    }

    private int? GetUserId() => _currentUser.GetUserId();

    private string? GetSessionId()
    {
        // Client can pass session ID as a query param on connection
        if (Context.GetHttpContext()?.Request.Query.TryGetValue("sessionId", out var sid) == true)
            return sid.ToString();
        return null;
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
