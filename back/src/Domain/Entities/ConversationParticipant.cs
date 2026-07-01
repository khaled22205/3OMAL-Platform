using Domain.Common;

namespace Domain.Entities;

public class ConversationParticipant : BaseEntity
{
    public int ConversationId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
