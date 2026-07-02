using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Common.Models;
using Application.Common.Mappings;
using Application.Features.AiAssistant;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class AiConversationService : IAiConversationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AiConversationService> _logger;

    public AiConversationService(AppDbContext context, ILogger<AiConversationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AiConversationSummaryResponse> CreateConversationAsync(int userId, string language, string? title, string? firstMessage)
    {
        var conversation = new AiConversation
        {
            UserId = userId,
            Title = title ?? GenerateTitle(firstMessage),
            Language = language,
            CreatedAt = DateTime.UtcNow
        };

        _context.AiConversations.Add(conversation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("AI conversation {ConversationId} created for user {UserId}",
            conversation.Id, userId);

        return conversation.ToSummaryResponse();
    }

    public async Task<PagedResult<AiConversationSummaryResponse>> GetConversationsAsync(int userId, int page, int pageSize)
    {
        var query = _context.AiConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);

        var totalCount = await query.CountAsync();

        var conversations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ToListAsync();

        var items = conversations.Select(c =>
        {
            var lastMessage = c.Messages.FirstOrDefault();
            return c.ToSummaryResponse(lastMessage?.ToResponse());
        }).ToList();

        return new PagedResult<AiConversationSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AiConversationDetailResponse?> GetConversationAsync(int conversationId, int userId)
    {
        var conversation = await _context.AiConversations
            .Include(c => c.Messages.Where(m => !m.IsDeleted)
                .OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.ContextReferences)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId && !c.IsDeleted);

        if (conversation == null) return null;

        return conversation.ToDetailResponse();
    }

    public async Task<bool> DeleteConversationAsync(int conversationId, int userId)
    {
        var conversation = await _context.AiConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId && !c.IsDeleted);

        if (conversation == null) return false;

        conversation.IsDeleted = true;
        conversation.DeletedAt = DateTime.UtcNow;

        foreach (var message in await _context.AiMessages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .ToListAsync())
        {
            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("AI conversation {ConversationId} deleted by user {UserId}",
            conversationId, userId);

        return true;
    }

    public async Task<PagedResult<AiConversationSummaryResponse>> SearchConversationsAsync(int userId, string query, int page, int pageSize)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        var queryable = _context.AiConversations
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Where(c => EF.Functions.Like(c.Title, $"%{normalizedQuery}%"))
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);

        var totalCount = await queryable.CountAsync();

        var conversations = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ToListAsync();

        var items = conversations.Select(c =>
        {
            var lastMessage = c.Messages.FirstOrDefault();
            return c.ToSummaryResponse(lastMessage?.ToResponse());
        }).ToList();

        return new PagedResult<AiConversationSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AiMessageResponse> AddMessageAsync(int conversationId, int userId, string role, string content, List<SearchResult>? sources = null)
    {
        if (!Enum.TryParse<AiMessageRole>(role, true, out var messageRole))
            messageRole = AiMessageRole.User;

        var message = new AiMessage
        {
            ConversationId = conversationId,
            Role = messageRole,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.AiMessages.Add(message);

        if (sources?.Count > 0)
        {
            var references = sources.Select(s => new AiContextReference
            {
                MessageId = message.Id,
                SourceType = s.SourceType,
                SourceId = s.SourceId,
                Title = s.Title,
                Excerpt = s.Excerpt,
                RelevanceScore = s.RelevanceScore,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            message.SourcesJson = System.Text.Json.JsonSerializer.Serialize(
                sources.Select(s => new { s.SourceType, s.SourceId, s.Title, s.RelevanceScore }));
        }

        var conversation = await _context.AiConversations.FindAsync(conversationId);
        if (conversation != null)
        {
            if (string.IsNullOrEmpty(conversation.Title) && role == "User")
                conversation.Title = GenerateTitle(content);

            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return message.ToResponse();
    }

    public async Task<int> GetMessageCountAsync(int conversationId)
    {
        return await _context.AiMessages
            .CountAsync(m => m.ConversationId == conversationId && !m.IsDeleted);
    }

    private static string GenerateTitle(string? firstMessage)
    {
        if (string.IsNullOrWhiteSpace(firstMessage))
            return "New Conversation";

        return firstMessage.Length > 100
            ? firstMessage[..100] + "..."
            : firstMessage;
    }
}
