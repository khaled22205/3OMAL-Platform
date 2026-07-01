namespace Application.Features.Chat;

public class SendMessageRequest
{
    public int ConversationId { get; set; }
    public string MessageType { get; set; } = "Text";
    public string? Content { get; set; }
    public int? ReplyToMessageId { get; set; }
}

public class CreateConversationRequest
{
    public int ParticipantUserId { get; set; }
}

public class EditMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class MarkAsReadRequest
{
    public int ConversationId { get; set; }
    public List<int> MessageIds { get; set; } = [];
}

public class ConversationResponse
{
    public int Id { get; set; }
    public UserBriefResponse OtherUser { get; set; } = null!;
    public MessageResponse? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastMessageAt { get; set; }
}

public class MessageResponse
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int? ReplyToMessageId { get; set; }
    public string? ReplyToContent { get; set; }
    public List<AttachmentResponse> Attachments { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
}

public class UserBriefResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Photo { get; set; }
}

public class AttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
}

public class UnreadCountResponse
{
    public int Count { get; set; }
}
