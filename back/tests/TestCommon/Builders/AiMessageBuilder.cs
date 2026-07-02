using Domain.Entities;
using Domain.Enums;

namespace TestCommon.Builders;

public class AiMessageBuilder
{
    private int _id = 1;
    private int _conversationId = 1;
    private AiMessageRole _role = AiMessageRole.User;
    private string _content = "Test message content";
    private string? _sourcesJson;
    private int? _promptTokens;
    private int? _responseTokens;
    private bool _isDeleted;
    private DateTime? _deletedAt;

    public AiMessageBuilder WithId(int id) { _id = id; return this; }
    public AiMessageBuilder WithConversationId(int id) { _conversationId = id; return this; }
    public AiMessageBuilder WithRole(AiMessageRole role) { _role = role; return this; }
    public AiMessageBuilder WithContent(string content) { _content = content; return this; }
    public AiMessageBuilder WithSourcesJson(string json) { _sourcesJson = json; return this; }
    public AiMessageBuilder WithTokens(int prompt, int response) { _promptTokens = prompt; _responseTokens = response; return this; }
    public AiMessageBuilder Deleted() { _isDeleted = true; _deletedAt = DateTime.UtcNow; return this; }

    public AiMessage Build()
    {
        return new AiMessage
        {
            Id = _id,
            ConversationId = _conversationId,
            Role = _role,
            Content = _content,
            SourcesJson = _sourcesJson,
            PromptTokens = _promptTokens,
            ResponseTokens = _responseTokens,
            IsDeleted = _isDeleted,
            DeletedAt = _deletedAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static AiMessage CreateUserMessage(int conversationId = 1, string content = "User question", int id = 1)
        => new AiMessageBuilder().WithId(id).WithConversationId(conversationId)
            .WithRole(AiMessageRole.User).WithContent(content).Build();

    public static AiMessage CreateAssistantMessage(int conversationId = 1, string content = "AI response", int id = 2)
        => new AiMessageBuilder().WithId(id).WithConversationId(conversationId)
            .WithRole(AiMessageRole.Assistant).WithContent(content).Build();
}
