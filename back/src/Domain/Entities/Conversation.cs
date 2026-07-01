using Domain.Common;

namespace Domain.Entities;

public class Conversation : BaseEntity, ISoftDelete
{
    public int? LastMessageId { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Message? LastMessage { get; set; }
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
