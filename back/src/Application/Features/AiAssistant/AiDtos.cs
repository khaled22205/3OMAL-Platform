namespace Application.Features.AiAssistant;

public class StartConversationRequest
{
    public string? Title { get; set; }
    public string? FirstMessage { get; set; }
}

public class SendAiMessageRequest
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class AiConversationSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public AiMessageResponse? LastMessage { get; set; }
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AiConversationDetailResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public List<AiMessageResponse> Messages { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AiMessageResponse
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<AiSourceReferenceResponse> Sources { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class AiSourceReferenceResponse
{
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string? Title { get; set; }
    public string? Excerpt { get; set; }
    public double RelevanceScore { get; set; }
}

public class AiSuggestedPromptsResponse
{
    public List<string> Prompts { get; set; } = [];
}

public class AiStreamChunkResponse
{
    public int ConversationId { get; set; }
    public int? MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public List<AiSourceReferenceResponse>? Sources { get; set; }
    public string? Error { get; set; }
}
