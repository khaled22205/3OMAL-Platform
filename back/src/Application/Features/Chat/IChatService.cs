using Application.Common.Models;

namespace Application.Features.Chat;

public interface IChatService
{
    Task<PagedResult<ConversationResponse>> GetConversationsAsync(int userId, int page, int pageSize);
    Task<ConversationResponse> GetConversationAsync(int conversationId, int userId);
    Task<ConversationResponse> GetOrCreateConversationAsync(int currentUserId, int participantUserId);
    Task<bool> IsConversationParticipantAsync(int conversationId, int userId);

    Task<PagedResult<MessageResponse>> GetMessagesAsync(int conversationId, int userId, int page, int pageSize);
    Task<MessageResponse> SendMessageAsync(int senderId, SendMessageRequest request);
    Task<MessageResponse> EditMessageAsync(int userId, int messageId, string newContent);
    Task<bool> DeleteMessageAsync(int userId, int messageId);
    Task<int?> GetMessageConversationIdAsync(int messageId);
    Task<bool> MarkAsReadAsync(int userId, int conversationId, List<int> messageIds);

    Task<PagedResult<ConversationResponse>> SearchConversationsAsync(int userId, string query, int page, int pageSize);
    Task<PagedResult<MessageResponse>> SearchMessagesAsync(int userId, string query, int page, int pageSize);
    Task<UnreadCountResponse> GetUnreadCountAsync(int userId);
}
