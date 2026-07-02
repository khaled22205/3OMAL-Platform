namespace Application.Features.AiAssistant;

public class AiRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public List<AiHistoryMessage> History { get; set; } = [];
    public float Temperature { get; set; } = 0.3f;
    public int MaxTokens { get; set; } = 8192;
}

public class AiHistoryMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class AiResponse
{
    public string Content { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int ResponseTokens { get; set; }
}

public interface IAiProvider
{
    string ProviderName { get; }
    Task<AiResponse> GenerateAsync(AiRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GenerateStreamAsync(AiRequest request, CancellationToken cancellationToken = default);
}
