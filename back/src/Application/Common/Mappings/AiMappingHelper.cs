using Domain.Entities;
using Application.Features.AiAssistant;

namespace Application.Common.Mappings;

public static class AiMappingHelper
{
    public static AiConversationSummaryResponse ToSummaryResponse(this AiConversation conversation, AiMessageResponse? lastMessage = null) => new()
    {
        Id = conversation.Id,
        UserId = conversation.UserId,
        SessionId = conversation.SessionId,
        UserRole = conversation.UserRole,
        Title = conversation.Title,
        Language = conversation.Language,
        IsArchived = conversation.IsArchived,
        IsHidden = conversation.IsHidden,
        LastMessage = lastMessage,
        CreatedAt = conversation.CreatedAt,
        UpdatedAt = conversation.UpdatedAt
    };

    public static AiConversationDetailResponse ToDetailResponse(this AiConversation conversation) => new()
    {
        Id = conversation.Id,
        UserId = conversation.UserId,
        SessionId = conversation.SessionId,
        UserRole = conversation.UserRole,
        Title = conversation.Title,
        Language = conversation.Language,
        IsArchived = conversation.IsArchived,
        IsHidden = conversation.IsHidden,
        Messages = conversation.Messages.Select(m => m.ToResponse()).ToList(),
        CreatedAt = conversation.CreatedAt,
        UpdatedAt = conversation.UpdatedAt
    };

    public static AiMessageResponse ToResponse(this AiMessage message) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        Role = message.Role.ToString(),
        Content = message.Content,
        Sources = message.ContextReferences?.Select(r => r.ToResponse()).ToList() ?? [],
        CreatedAt = message.CreatedAt
    };

    public static AiSourceReferenceResponse ToResponse(this AiContextReference reference) => new()
    {
        SourceType = reference.SourceType,
        SourceId = reference.SourceId,
        Title = reference.Title,
        Excerpt = reference.Excerpt,
        RelevanceScore = reference.RelevanceScore
    };
}
