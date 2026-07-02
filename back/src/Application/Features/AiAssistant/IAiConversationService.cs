using Application.Common.Models;

namespace Application.Features.AiAssistant;

public interface IAiConversationService
{
    Task<AiConversationSummaryResponse> CreateConversationAsync(int? userId, string? sessionId, string userRole, string language, string? title, string? firstMessage);
    Task<PagedResult<AiConversationSummaryResponse>> GetConversationsAsync(int? userId, string? sessionId, string userRole, int page, int pageSize);
    Task<AiConversationDetailResponse?> GetConversationAsync(int conversationId, int? userId, string? sessionId, string userRole);
    Task<bool> DeleteConversationAsync(int conversationId, int? userId, string? sessionId, string userRole);
    Task<PagedResult<AiConversationSummaryResponse>> SearchConversationsAsync(int? userId, string? sessionId, string userRole, string query, int page, int pageSize);
    Task<AiMessageResponse> AddMessageAsync(int conversationId, int? userId, string? sessionId, string userRole, string role, string content, List<SearchResult>? sources = null);
    Task<int> GetMessageCountAsync(int conversationId);
}
