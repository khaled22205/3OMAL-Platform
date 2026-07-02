using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class AiMessage : BaseEntity, ISoftDelete
{
    public int ConversationId { get; set; }
    public AiMessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SourcesJson { get; set; }
    public int? PromptTokens { get; set; }
    public int? ResponseTokens { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public AiConversation Conversation { get; set; } = null!;
    public ICollection<AiContextReference> ContextReferences { get; set; } = [];
}
