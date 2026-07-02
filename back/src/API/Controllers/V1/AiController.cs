using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.AiAssistant;

namespace API.Controllers.V1;

public class AiController : BaseApiController
{
    private readonly IAiAssistantService _assistantService;
    private readonly IAiConversationService _conversationService;
    private readonly ICurrentUserService _currentUser;

    public AiController(
        IAiAssistantService assistantService,
        IAiConversationService conversationService,
        ICurrentUserService currentUser)
    {
        _assistantService = assistantService;
        _conversationService = conversationService;
        _currentUser = currentUser;
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var result = await _assistantService.StartConversationAsync(userId, role, request);
        return CreatedResult(result, "Conversation started");
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _conversationService.GetConversationsAsync(userId, page, pageSize);
        return OkResult(result);
    }

    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        var userId = GetUserId();
        var conversation = await _conversationService.GetConversationAsync(id, userId);
        if (conversation == null)
            return NotFoundResult("Conversation not found");
        return OkResult(conversation);
    }

    [HttpDelete("conversations/{id:int}")]
    public async Task<IActionResult> DeleteConversation(int id)
    {
        var userId = GetUserId();
        var deleted = await _conversationService.DeleteConversationAsync(id, userId);
        if (!deleted)
            return NotFoundResult("Conversation not found");
        return OkResult(true, "Conversation deleted");
    }

    [HttpGet("conversations/search")]
    public async Task<IActionResult> SearchConversations([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _conversationService.SearchConversationsAsync(userId, q, page, pageSize);
        return OkResult(result);
    }

    [HttpPost("conversations/{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendAiMessageRequest request)
    {
        request.ConversationId = id;
        var userId = GetUserId();
        var role = GetUserRole();
        var result = await _assistantService.SendMessageAsync(userId, role, request);
        return OkResult(result);
    }

    [HttpGet("conversations/{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        var conversation = await _conversationService.GetConversationAsync(id, userId);
        if (conversation == null)
            return NotFoundResult("Conversation not found");

        var totalCount = conversation.Messages.Count;
        var messages = conversation.Messages
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return OkResult(new PagedResult<AiMessageResponse>
        {
            Items = messages,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions()
    {
        var role = GetUserRole();
        var result = await _assistantService.GetSuggestedPromptsAsync(role);
        return OkResult(result);
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
