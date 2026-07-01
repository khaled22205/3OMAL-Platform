using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Common.Models;
using Application.Common.Mappings;
using Application.Features.Chat;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(AppDbContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ConversationResponse>> GetConversationsAsync(int userId, int page, int pageSize)
    {
        var query = _context.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.LastMessage)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Participants)
            .Select(cp => cp.Conversation)
            .Where(c => c.LastMessageAt != null)
            .OrderByDescending(c => c.LastMessageAt);

        var totalCount = await query.CountAsync();
        var conversations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = conversations.Select(c =>
        {
            var otherParticipant = c.Participants.First(p => p.UserId != userId);
            var otherUser = GetUserBrief(otherParticipant.UserId);
            var unread = c.Messages.Count(m =>
                m.SenderId != userId &&
                (m.ReadAt == null) &&
                !m.IsDeleted);
            return c.ToResponse(otherUser, unread);
        }).ToList();

        return new PagedResult<ConversationResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ConversationResponse> GetConversationAsync(int conversationId, int userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Participants)
            .Include(c => c.LastMessage)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted)
            ?? throw new KeyNotFoundException("Conversation not found");

        if (!conversation.Participants.Any(p => p.UserId == userId))
            throw new UnauthorizedAccessException("Not a participant");

        var otherParticipant = conversation.Participants.First(p => p.UserId != userId);
        var otherUser = GetUserBrief(otherParticipant.UserId);
        var unread = await _context.Messages.CountAsync(m =>
            m.ConversationId == conversationId &&
            m.SenderId != userId &&
            m.ReadAt == null &&
            !m.IsDeleted);

        return conversation.ToResponse(otherUser, unread);
    }

    public async Task<ConversationResponse> GetOrCreateConversationAsync(int currentUserId, int participantUserId)
    {
        if (currentUserId == participantUserId)
            throw new ArgumentException("Cannot create conversation with yourself");

        var existing = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.Participants)
            .Include(cp => cp.Conversation)
                .ThenInclude(c => c.LastMessage)
            .Select(cp => cp.Conversation)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c =>
                c.Participants.Any(p => p.UserId == currentUserId) &&
                c.Participants.Any(p => p.UserId == participantUserId));

        if (existing != null)
            return await GetConversationAsync(existing.Id, currentUserId);

        var conversation = new Conversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        _context.ConversationParticipants.AddRange(
            new ConversationParticipant { ConversationId = conversation.Id, UserId = currentUserId, JoinedAt = DateTime.UtcNow },
            new ConversationParticipant { ConversationId = conversation.Id, UserId = participantUserId, JoinedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        _logger.LogInformation("Conversation {ConversationId} created between user {UserA} and user {UserB}",
            conversation.Id, currentUserId, participantUserId);

        return await GetConversationAsync(conversation.Id, currentUserId);
    }

    public async Task<bool> IsConversationParticipantAsync(int conversationId, int userId)
    {
        return await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
    }

    public async Task<PagedResult<MessageResponse>> GetMessagesAsync(int conversationId, int userId, int page, int pageSize)
    {
        if (!await IsConversationParticipantAsync(conversationId, userId))
            throw new UnauthorizedAccessException("Not a participant");

        var query = _context.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .Include(m => m.ReplyToMessage)
            .Include(m => m.Attachments)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var senderIds = messages.Select(m => m.SenderId).Distinct();
        var senderNames = await _context.Set<Microsoft.AspNetCore.Identity.IdentityUser<int>>()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? $"User#{u.Id}");

        var items = messages.Select(m =>
        {
            var name = senderNames.GetValueOrDefault(m.SenderId, $"User#{m.SenderId}");
            return m.ToResponse(name);
        }).ToList();

        return new PagedResult<MessageResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MessageResponse> SendMessageAsync(int senderId, SendMessageRequest request)
    {
        if (!await IsConversationParticipantAsync(request.ConversationId, senderId))
            throw new UnauthorizedAccessException("Not a participant");

        if (!Enum.TryParse<MessageType>(request.MessageType, true, out var messageType))
            throw new ArgumentException($"Invalid message type: {request.MessageType}");

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = senderId,
            MessageType = messageType,
            Content = request.Content,
            ReplyToMessageId = request.ReplyToMessageId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        var conversation = await _context.Conversations.FindAsync(request.ConversationId);
        if (conversation != null)
        {
            conversation.LastMessageId = message.Id;
            conversation.LastMessageContent = request.Content;
            conversation.LastMessageAt = message.CreatedAt;
            conversation.UpdatedAt = message.CreatedAt;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Message {MessageId} sent in conversation {ConversationId} by user {SenderId}",
            message.Id, request.ConversationId, senderId);

        var senderUser = await _context.Set<Microsoft.AspNetCore.Identity.IdentityUser<int>>()
            .FindAsync(senderId);
        var senderName = senderUser?.UserName ?? $"User#{senderId}";

        return message.ToResponse(senderName);
    }

    public async Task<MessageResponse> EditMessageAsync(int userId, int messageId, string newContent)
    {
        var message = await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.ReplyToMessage)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Message not found");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("Cannot edit another user's message");

        message.Content = newContent;
        message.EditedAt = DateTime.UtcNow;
        message.IsEdited = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Message {MessageId} edited by user {UserId}", messageId, userId);

        var senderUser = await _context.Set<Microsoft.AspNetCore.Identity.IdentityUser<int>>()
            .FindAsync(userId);
        return message.ToResponse(senderUser?.UserName ?? $"User#{userId}");
    }

    public async Task<bool> DeleteMessageAsync(int userId, int messageId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted)
            ?? throw new KeyNotFoundException("Message not found");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("Cannot delete another user's message");

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Message {MessageId} deleted by user {UserId}", messageId, userId);
        return true;
    }

    public async Task<int?> GetMessageConversationIdAsync(int messageId)
    {
        var message = await _context.Messages
            .Where(m => m.Id == messageId && !m.IsDeleted)
            .Select(m => new { m.ConversationId })
            .FirstOrDefaultAsync();
        return message?.ConversationId;
    }

    public async Task<bool> MarkAsReadAsync(int userId, int conversationId, List<int> messageIds)
    {
        if (!await IsConversationParticipantAsync(conversationId, userId))
            throw new UnauthorizedAccessException("Not a participant");

        var messages = await _context.Messages
            .Where(m => messageIds.Contains(m.Id) && m.ConversationId == conversationId && m.ReadAt == null)
            .ToListAsync();

        foreach (var msg in messages)
        {
            msg.ReadAt = DateTime.UtcNow;
        }

        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
        if (participant != null)
        {
            participant.LastReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("{Count} messages marked as read in conversation {ConversationId} by user {UserId}",
            messages.Count, conversationId, userId);

        return true;
    }

    public async Task<PagedResult<ConversationResponse>> SearchConversationsAsync(int userId, string query, int page, int pageSize)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        var conversationIds = _context.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.ConversationId);

        var matching = await _context.Conversations
            .Where(c => conversationIds.Contains(c.Id) && !c.IsDeleted)
            .Where(c => c.LastMessageContent != null &&
                EF.Functions.Like(c.LastMessageContent, $"%{normalizedQuery}%"))
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Participants)
            .Include(c => c.LastMessage)
            .ToListAsync();

        var totalCount = await _context.Conversations
            .Where(c => conversationIds.Contains(c.Id) && !c.IsDeleted)
            .Where(c => c.LastMessageContent != null &&
                EF.Functions.Like(c.LastMessageContent, $"%{normalizedQuery}%"))
            .CountAsync();

        var items = matching.Select(c =>
        {
            var otherParticipant = c.Participants.First(p => p.UserId != userId);
            var otherUser = GetUserBrief(otherParticipant.UserId);
            var unread = c.Messages.Count(m =>
                m.SenderId != userId && m.ReadAt == null && !m.IsDeleted);
            return c.ToResponse(otherUser, unread);
        }).ToList();

        return new PagedResult<ConversationResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<MessageResponse>> SearchMessagesAsync(int userId, string query, int page, int pageSize)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        var userConversationIds = _context.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.ConversationId);

        var messagesQuery = _context.Messages
            .Where(m => userConversationIds.Contains(m.ConversationId) && !m.IsDeleted)
            .Where(m => m.Content != null && EF.Functions.Like(m.Content, $"%{normalizedQuery}%"))
            .Include(m => m.ReplyToMessage)
            .Include(m => m.Attachments)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await messagesQuery.CountAsync();
        var messages = await messagesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var senderIds = messages.Select(m => m.SenderId).Distinct();
        var senderNames = await _context.Set<Microsoft.AspNetCore.Identity.IdentityUser<int>>()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? $"User#{u.Id}");

        var items = messages.Select(m =>
        {
            var name = senderNames.GetValueOrDefault(m.SenderId, $"User#{m.SenderId}");
            return m.ToResponse(name);
        }).ToList();

        return new PagedResult<MessageResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(int userId)
    {
        var count = await _context.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .SelectMany(cp => _context.Messages
                .Where(m => m.ConversationId == cp.ConversationId
                    && m.SenderId != userId
                    && m.ReadAt == null
                    && !m.IsDeleted))
            .CountAsync();

        return new UnreadCountResponse { Count = count };
    }

    private UserBriefResponse GetUserBrief(int userId)
    {
        var user = _context.Set<Microsoft.AspNetCore.Identity.IdentityUser<int>>()
            .Find(userId);
        return (userId, user?.UserName ?? $"User#{userId}", "", (string?)null).ToBriefResponse();
    }
}
