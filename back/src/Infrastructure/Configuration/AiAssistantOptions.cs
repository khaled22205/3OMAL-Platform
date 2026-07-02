namespace Infrastructure.Configuration;

public class AiAssistantOptions
{
    public const string SectionName = "AiAssistant";

    public int MaxContextMessages { get; set; } = 10;
    public int MaxRetrievalResults { get; set; } = 5;
    public bool EnableStreaming { get; set; } = true;
    public int RateLimitPerMinute { get; set; } = 10;
    public int RateLimitPerDay { get; set; } = 100;
    public float Temperature { get; set; } = 0.3f;
    public int MaxTokens { get; set; } = 8192;
}

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public string FallbackModel { get; set; } = "gemini-2.0-flash";
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public int MaxTokens { get; set; } = 8192;
    public float Temperature { get; set; } = 0.3f;
    public float TopP { get; set; } = 0.9f;
    public int TopK { get; set; } = 40;
    public int TimeoutSeconds { get; set; } = 60;
    public int RetryCount { get; set; } = 2;
}
