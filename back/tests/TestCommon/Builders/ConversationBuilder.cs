using Domain.Entities;

namespace TestCommon.Builders;

public class ConversationBuilder
{
    private int? _lastMessageId;
    private string? _lastMessageContent;
    private DateTime? _lastMessageAt;
    private readonly List<ConversationParticipant> _participants = [];
    private readonly List<Message> _messages = [];
    private bool _isDeleted;
    private DateTime? _deletedAt;

    public ConversationBuilder WithLastMessage(int? messageId, string? content = null, DateTime? at = null)
    {
        _lastMessageId = messageId; _lastMessageContent = content; _lastMessageAt = at; return this;
    }

    public ConversationBuilder WithParticipant(int userId, DateTime? lastReadAt = null)
    {
        _participants.Add(new ConversationParticipant
        {
            ConversationId = 0,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            LastReadAt = lastReadAt
        });
        return this;
    }

    public ConversationBuilder WithMessage(Message message)
    {
        _messages.Add(message); return this;
    }

    public ConversationBuilder Deleted()
    {
        _isDeleted = true; _deletedAt = DateTime.UtcNow; return this;
    }

    public Conversation Build()
    {
        var conv = new Conversation
        {
            LastMessageId = _lastMessageId,
            LastMessageContent = _lastMessageContent,
            LastMessageAt = _lastMessageAt,
            IsDeleted = _isDeleted,
            DeletedAt = _deletedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        foreach (var p in _participants)
        {
            p.Conversation = conv;
            conv.Participants.Add(p);
        }
        foreach (var m in _messages)
        {
            m.Conversation = conv;
            conv.Messages.Add(m);
        }
        return conv;
    }

    public static Conversation Create(int participant1Id = 1, int participant2Id = 2)
        => new ConversationBuilder()
            .WithParticipant(participant1Id)
            .WithParticipant(participant2Id)
            .Build();
}
