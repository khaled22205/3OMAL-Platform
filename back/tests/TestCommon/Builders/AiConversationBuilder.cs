using Domain.Entities;
using Domain.Enums;

namespace TestCommon.Builders;

public class AiConversationBuilder
{
    private int _id = 1;
    private int? _userId;
    private string? _sessionId;
    private string _userRole = "Guest";
    private string _title = "Test Conversation";
    private string _language = "en";
    private bool _isArchived;
    private bool _isHidden;
    private bool _isDeleted;
    private DateTime? _deletedAt;
    private readonly List<AiMessage> _messages = [];

    public AiConversationBuilder WithId(int id) { _id = id; return this; }
    public AiConversationBuilder WithUserId(int userId) { _userId = userId; _sessionId = null; return this; }
    public AiConversationBuilder WithSessionId(string sessionId) { _sessionId = sessionId; _userId = null; return this; }
    public AiConversationBuilder WithUserRole(string role) { _userRole = role; return this; }
    public AiConversationBuilder WithTitle(string title) { _title = title; return this; }
    public AiConversationBuilder WithLanguage(string lang) { _language = lang; return this; }
    public AiConversationBuilder Archived() { _isArchived = true; return this; }
    public AiConversationBuilder Hidden() { _isHidden = true; return this; }
    public AiConversationBuilder Deleted() { _isDeleted = true; _deletedAt = DateTime.UtcNow; return this; }
    public AiConversationBuilder WithMessage(AiMessage message) { _messages.Add(message); return this; }

    public AiConversation Build()
    {
        return new AiConversation
        {
            Id = _id,
            UserId = _userId,
            SessionId = _sessionId,
            UserRole = _userRole,
            Title = _title,
            Language = _language,
            IsArchived = _isArchived,
            IsHidden = _isHidden,
            IsDeleted = _isDeleted,
            DeletedAt = _deletedAt,
            Messages = _messages,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static AiConversation CreateUserConversation(int userId = 1, string role = "Customer", int id = 1)
        => new AiConversationBuilder().WithId(id).WithUserId(userId).WithUserRole(role).Build();

    public static AiConversation CreateGuestConversation(string sessionId = "sess-abc-123", int id = 1)
        => new AiConversationBuilder().WithId(id).WithSessionId(sessionId).Build();

    public static AiConversation CreateAdminConversation(int userId = 1, int id = 1)
        => new AiConversationBuilder().WithId(id).WithUserId(userId).WithUserRole("Admin").Build();
}
