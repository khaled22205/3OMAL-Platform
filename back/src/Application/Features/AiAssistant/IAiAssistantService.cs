namespace Application.Features.AiAssistant;

public interface IAiAssistantService
{
    Task<AiMessageResponse> SendMessageAsync(int userId, string userRole, SendAiMessageRequest request);
    IAsyncEnumerable<AiStreamChunkResponse> SendMessageStreamAsync(int userId, string userRole, SendAiMessageRequest request, CancellationToken cancellationToken = default);
    Task<AiConversationSummaryResponse> StartConversationAsync(int userId, string userRole, StartConversationRequest request);
    Task<AiSuggestedPromptsResponse> GetSuggestedPromptsAsync(string userRole);
}
