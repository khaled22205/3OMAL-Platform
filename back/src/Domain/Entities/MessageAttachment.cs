using Domain.Common;

namespace Domain.Entities;

public class MessageAttachment : BaseEntity
{
    public int MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string AttachmentType { get; set; } = string.Empty;

    public Message Message { get; set; } = null!;
}
