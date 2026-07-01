using Domain.Entities;
using Domain.Enums;

namespace TestCommon.Builders;

public class MessageBuilder
{
    private int _conversationId;
    private int _senderId = 1;
    private MessageType _messageType = MessageType.Text;
    private string? _content = "Test message";
    private int? _replyToMessageId;
    private DateTime? _deliveredAt;
    private DateTime? _readAt;
    private DateTime? _editedAt;
    private bool _isEdited;
    private readonly List<MessageAttachment> _attachments = [];
    private bool _isDeleted;
    private DateTime? _deletedAt;

    public MessageBuilder WithConversationId(int id) { _conversationId = id; return this; }
    public MessageBuilder WithSenderId(int id) { _senderId = id; return this; }
    public MessageBuilder WithMessageType(MessageType type) { _messageType = type; return this; }
    public MessageBuilder WithContent(string? content) { _content = content; return this; }
    public MessageBuilder ReplyTo(int replyToId) { _replyToMessageId = replyToId; return this; }
    public MessageBuilder Delivered() { _deliveredAt = DateTime.UtcNow; return this; }
    public MessageBuilder Read() { _readAt = DateTime.UtcNow; return this; }
    public MessageBuilder Edited()
    {
        _isEdited = true; _editedAt = DateTime.UtcNow; return this;
    }
    public MessageBuilder WithAttachment(string fileName = "test.pdf", string contentType = "application/pdf", long fileSize = 1024, string attachmentType = "File")
    {
        _attachments.Add(new MessageAttachment
        {
            FileName = fileName,
            FilePath = $"/uploads/{fileName}",
            ContentType = contentType,
            FileSize = fileSize,
            AttachmentType = attachmentType
        });
        return this;
    }
    public MessageBuilder Deleted()
    {
        _isDeleted = true; _deletedAt = DateTime.UtcNow; return this;
    }

    public Message Build()
    {
        var msg = new Message
        {
            ConversationId = _conversationId,
            SenderId = _senderId,
            MessageType = _messageType,
            Content = _content,
            ReplyToMessageId = _replyToMessageId,
            DeliveredAt = _deliveredAt,
            ReadAt = _readAt,
            EditedAt = _editedAt,
            IsEdited = _isEdited,
            IsDeleted = _isDeleted,
            DeletedAt = _deletedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        foreach (var a in _attachments)
        {
            a.Message = msg;
            msg.Attachments.Add(a);
        }
        return msg;
    }

    public static Message CreateText(int conversationId, int senderId, string content = "Test message")
        => new MessageBuilder().WithConversationId(conversationId).WithSenderId(senderId).WithContent(content).Build();

    public static Message CreateDelivered(int conversationId, int senderId, string content = "Test message")
        => new MessageBuilder().WithConversationId(conversationId).WithSenderId(senderId).WithContent(content).Delivered().Build();

    public static Message CreateRead(int conversationId, int senderId, string content = "Test message")
        => new MessageBuilder().WithConversationId(conversationId).WithSenderId(senderId).WithContent(content).Delivered().Read().Build();
}
