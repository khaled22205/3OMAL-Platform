using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Message : BaseEntity, ISoftDelete
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string? Content { get; set; }
    public int? ReplyToMessageId { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public Message? ReplyToMessage { get; set; }
    public ICollection<MessageAttachment> Attachments { get; set; } = [];
}
