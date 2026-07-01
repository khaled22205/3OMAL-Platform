using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Chat;

namespace API.Controllers.V1;

[Authorize]
public class ChatController : BaseApiController
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _chatService.GetConversationsAsync(userId, page, pageSize);
        return OkResult(result);
    }

    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        var userId = GetUserId();
        var result = await _chatService.GetConversationAsync(id, userId);
        return OkResult(result);
    }

    [HttpPost("conversations")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetUserId();
        var result = await _chatService.GetOrCreateConversationAsync(userId, request.ParticipantUserId);
        return OkResult(result);
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        var result = await _chatService.GetMessagesAsync(id, userId, page, pageSize);
        return OkResult(result);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        var result = await _chatService.SendMessageAsync(userId, request);
        return OkResult(result);
    }

    [HttpPut("messages/{id}")]
    public async Task<IActionResult> EditMessage(int id, [FromBody] EditMessageRequest request)
    {
        var userId = GetUserId();
        var result = await _chatService.EditMessageAsync(userId, id, request.Content);
        return OkResult(result);
    }

    [HttpDelete("messages/{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = GetUserId();
        var result = await _chatService.DeleteMessageAsync(userId, id);
        if (!result) return NotFoundResult("Message not found");
        return OkResult(new { deleted = true });
    }

    [HttpPost("messages/read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        var userId = GetUserId();
        var result = await _chatService.MarkAsReadAsync(userId, request.ConversationId, request.MessageIds);
        return OkResult(new { success = result });
    }

    [HttpGet("conversations/search")]
    public async Task<IActionResult> SearchConversations([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _chatService.SearchConversationsAsync(userId, query, page, pageSize);
        return OkResult(result);
    }

    [HttpGet("messages/search")]
    public async Task<IActionResult> SearchMessages([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _chatService.SearchMessagesAsync(userId, query, page, pageSize);
        return OkResult(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var result = await _chatService.GetUnreadCountAsync(userId);
        return OkResult(result);
    }
}
