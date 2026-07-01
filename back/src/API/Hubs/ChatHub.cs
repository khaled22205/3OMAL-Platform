using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Application.Features.Chat;
using Infrastructure.Services;

namespace API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ConnectionManager _connectionManager;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IChatService chatService,
        ConnectionManager connectionManager,
        ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _connectionManager.AddConnection(userId.Value, Context.ConnectionId);
            _logger.LogInformation("User {UserId} connected to chat hub (ConnectionId: {ConnectionId})",
                userId.Value, Context.ConnectionId);
            await Clients.Others.SendAsync("UserOnline", userId.Value);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _connectionManager.GetUserId(Context.ConnectionId);
        _connectionManager.RemoveConnection(Context.ConnectionId);

        if (userId.HasValue)
        {
            _logger.LogInformation("User {UserId} disconnected from chat hub (ConnectionId: {ConnectionId})",
                userId.Value, Context.ConnectionId);
            if (!_connectionManager.IsUserOnline(userId.Value))
            {
                _ = Task.Delay(5000).ContinueWith(async _ =>
                {
                    if (!_connectionManager.IsUserOnline(userId.Value))
                    {
                        await Clients.All.SendAsync("UserOffline", userId.Value);
                    }
                });
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversationGroup(int conversationId)
    {
        var userId = GetUserId();
        if (userId == null) throw new HubException("Not authenticated");

        if (!await _chatService.IsConversationParticipantAsync(conversationId, userId.Value))
            throw new HubException("Not a participant");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
        _logger.LogInformation("User {UserId} joined conversation group {ConversationId}",
            userId.Value, conversationId);
    }

    public async Task LeaveConversationGroup(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = GetUserId();
        if (userId == null) throw new HubException("Not authenticated");

        var message = await _chatService.SendMessageAsync(userId.Value, request);
        await Clients.Group($"conv_{request.ConversationId}").SendAsync("NewMessage", message);
    }

    public async Task EditMessage(int messageId, EditMessageRequest request)
    {
        var userId = GetUserId();
        if (userId == null) throw new HubException("Not authenticated");

        var message = await _chatService.EditMessageAsync(userId.Value, messageId, request.Content);
        await Clients.Group($"conv_{message.ConversationId}").SendAsync("MessageEdited", message);
    }

    public async Task DeleteMessage(int messageId)
    {
        var userId = GetUserId();
        if (userId == null) throw new HubException("Not authenticated");

        var conversationId = await _chatService.GetMessageConversationIdAsync(messageId);
        if (conversationId == null) throw new HubException("Message not found");

        await _chatService.DeleteMessageAsync(userId.Value, messageId);
        await Clients.Group($"conv_{conversationId}").SendAsync("MessageDeleted", messageId, userId.Value);
    }

    public async Task MarkAsRead(MarkAsReadRequest request)
    {
        var userId = GetUserId();
        if (userId == null) throw new HubException("Not authenticated");

        await _chatService.MarkAsReadAsync(userId.Value, request.ConversationId, request.MessageIds);
        await Clients.Group($"conv_{request.ConversationId}").SendAsync("MessagesRead",
            request.ConversationId, userId.Value, request.MessageIds);
    }

    public async Task StartTyping(int conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        await Clients.OthersInGroup($"conv_{conversationId}").SendAsync("UserTyping", conversationId, userId.Value);
    }

    public async Task StopTyping(int conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        await Clients.OthersInGroup($"conv_{conversationId}").SendAsync("UserStoppedTyping", conversationId, userId.Value);
    }

    private int? GetUserId()
    {
        var claim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var id))
            return id;
        return null;
    }
}
