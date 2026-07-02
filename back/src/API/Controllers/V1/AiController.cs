using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.AiAssistant;

namespace API.Controllers.V1;

[AllowAnonymous]
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
        var (userId, sessionId, role) = ResolveContext(request.SessionId);
        var result = await _assistantService.StartConversationAsync(userId, sessionId, role, request);
        return CreatedResult(result, "Conversation started");
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(
        [FromQuery] string? sessionId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (userId, resolvedSession, role) = ResolveContext(sessionId);
        var result = await _conversationService.GetConversationsAsync(userId, resolvedSession, role, page, pageSize);
        return OkResult(result);
    }

    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> GetConversation(int id, [FromQuery] string? sessionId = null)
    {
        var (userId, resolvedSession, role) = ResolveContext(sessionId);
        var conversation = await _conversationService.GetConversationAsync(id, userId, resolvedSession, role);
        if (conversation == null)
            return NotFoundResult("Conversation not found");
        return OkResult(conversation);
    }

    [HttpDelete("conversations/{id:int}")]
    public async Task<IActionResult> DeleteConversation(int id, [FromQuery] string? sessionId = null)
    {
        var (userId, resolvedSession, role) = ResolveContext(sessionId);
        var deleted = await _conversationService.DeleteConversationAsync(id, userId, resolvedSession, role);
        if (!deleted)
            return NotFoundResult("Conversation not found");
        return OkResult(true, "Conversation deleted");
    }

    [HttpGet("conversations/search")]
    public async Task<IActionResult> SearchConversations(
        [FromQuery] string q,
        [FromQuery] string? sessionId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (userId, resolvedSession, role) = ResolveContext(sessionId);
        var result = await _conversationService.SearchConversationsAsync(userId, resolvedSession, role, q, page, pageSize);
        return OkResult(result);
    }

    [HttpPost("conversations/{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendAiMessageRequest request)
    {
        request.ConversationId = id;
        var (userId, sessionId, role) = ResolveContext(request.SessionId);
        var result = await _assistantService.SendMessageAsync(userId, sessionId, role, request);
        return OkResult(result);
    }

    [HttpGet("conversations/{id:int}/messages")]
    public async Task<IActionResult> GetMessages(
        int id,
        [FromQuery] string? sessionId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var (userId, resolvedSession, role) = ResolveContext(sessionId);
        var conversation = await _conversationService.GetConversationAsync(id, userId, resolvedSession, role);
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

    /// <summary>
    /// Resolves the caller's identity context: authenticated user ID + role, or guest session ID.
    /// </summary>
    private (int? userId, string? sessionId, string role) ResolveContext(string? clientSessionId)
    {
        var userId = _currentUser.GetUserId();
        var role = GetUserRole();

        if (userId.HasValue)
        {
            // Authenticated user — ignore any session ID from client
            return (userId, null, role);
        }

        // Guest — use client-supplied session ID
        var sessionId = !string.IsNullOrWhiteSpace(clientSessionId)
            ? clientSessionId
            : Request.Headers.TryGetValue("X-Session-Id", out var header)
                ? header.ToString()
                : null;

        return (null, sessionId, "Guest");
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
