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

    public async Task<AiConversationSummaryResponse> CreateConversationAsync(
        int? userId, string? sessionId, string userRole, string language, string? title, string? firstMessage)
    {
        var conversation = new AiConversation
        {
            UserId = userId,
            SessionId = sessionId,
            UserRole = userRole,
            Title = title ?? GenerateTitle(firstMessage),
            Language = language,
            CreatedAt = DateTime.UtcNow
        };

        _context.AiConversations.Add(conversation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "AI conversation {ConversationId} created for user {UserId} / session {SessionId} with role {Role}",
            conversation.Id, userId, sessionId, userRole);

        return conversation.ToSummaryResponse();
    }

    public async Task<PagedResult<AiConversationSummaryResponse>> GetConversationsAsync(
        int? userId, string? sessionId, string userRole, int page, int pageSize)
    {
        IQueryable<AiConversation> query;

        if (userId.HasValue)
        {
            // Authenticated: show only their own conversations (by userId AND matching role so Admin can't see Customer convs)
            query = _context.AiConversations
                .Where(c => c.UserId == userId.Value && c.UserRole == userRole)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            // Guest: show only conversations from this session
            query = _context.AiConversations
                .Where(c => c.SessionId == sessionId && c.UserId == null)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
        }
        else
        {
            return new PagedResult<AiConversationSummaryResponse>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

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

    public async Task<AiConversationDetailResponse?> GetConversationAsync(
        int conversationId, int? userId, string? sessionId, string userRole)
    {
        AiConversation? conversation;

        if (userId.HasValue)
        {
            conversation = await _context.AiConversations
                .Include(c => c.Messages.Where(m => !m.IsDeleted).OrderBy(m => m.CreatedAt))
                    .ThenInclude(m => m.ContextReferences)
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    c.UserId == userId.Value &&
                    c.UserRole == userRole &&
                    !c.IsDeleted);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            conversation = await _context.AiConversations
                .Include(c => c.Messages.Where(m => !m.IsDeleted).OrderBy(m => m.CreatedAt))
                    .ThenInclude(m => m.ContextReferences)
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    c.SessionId == sessionId &&
                    c.UserId == null &&
                    !c.IsDeleted);
        }
        else
        {
            return null;
        }

        return conversation?.ToDetailResponse();
    }

    public async Task<bool> DeleteConversationAsync(
        int conversationId, int? userId, string? sessionId, string userRole)
    {
        AiConversation? conversation;

        if (userId.HasValue)
        {
            conversation = await _context.AiConversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    c.UserId == userId.Value &&
                    c.UserRole == userRole &&
                    !c.IsDeleted);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            conversation = await _context.AiConversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    c.SessionId == sessionId &&
                    c.UserId == null &&
                    !c.IsDeleted);
        }
        else
        {
            return false;
        }

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

        _logger.LogInformation(
            "AI conversation {ConversationId} deleted by user {UserId} / session {SessionId}",
            conversationId, userId, sessionId);

        return true;
    }

    public async Task<PagedResult<AiConversationSummaryResponse>> SearchConversationsAsync(
        int? userId, string? sessionId, string userRole, string query, int page, int pageSize)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        IQueryable<AiConversation> queryable;

        if (userId.HasValue)
        {
            queryable = _context.AiConversations
                .Where(c => c.UserId == userId.Value && c.UserRole == userRole && !c.IsDeleted)
                .Where(c => EF.Functions.Like(c.Title, $"%{normalizedQuery}%"))
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            queryable = _context.AiConversations
                .Where(c => c.SessionId == sessionId && c.UserId == null && !c.IsDeleted)
                .Where(c => EF.Functions.Like(c.Title, $"%{normalizedQuery}%"))
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
        }
        else
        {
            return new PagedResult<AiConversationSummaryResponse>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

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

    public async Task<AiMessageResponse> AddMessageAsync(
        int conversationId, int? userId, string? sessionId, string userRole,
        string role, string content, List<SearchResult>? sources = null)
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

            _context.AiContextReferences.AddRange(references);
            message.ContextReferences = references;

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
