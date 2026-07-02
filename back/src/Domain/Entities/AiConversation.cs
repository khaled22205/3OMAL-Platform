using Domain.Common;

namespace Domain.Entities;

public class AiConversation : BaseEntity, ISoftDelete
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<AiMessage> Messages { get; set; } = [];
}
