using Application.Common.Models;

namespace Application.Features.AiAssistant;

public interface IAiConversationService
{
    Task<AiConversationSummaryResponse> CreateConversationAsync(int userId, string language, string? title, string? firstMessage);
    Task<PagedResult<AiConversationSummaryResponse>> GetConversationsAsync(int userId, int page, int pageSize);
    Task<AiConversationDetailResponse?> GetConversationAsync(int conversationId, int userId);
    Task<bool> DeleteConversationAsync(int conversationId, int userId);
    Task<PagedResult<AiConversationSummaryResponse>> SearchConversationsAsync(int userId, string query, int page, int pageSize);
    Task<AiMessageResponse> AddMessageAsync(int conversationId, int userId, string role, string content, List<SearchResult>? sources = null);
    Task<int> GetMessageCountAsync(int conversationId);
}
