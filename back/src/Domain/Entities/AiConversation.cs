using Domain.Common;

namespace Domain.Entities;

public class AiConversation : BaseEntity, ISoftDelete
{
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public string UserRole { get; set; } = "Guest";
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public bool IsArchived { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<AiMessage> Messages { get; set; } = [];
}
