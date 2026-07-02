using Domain.Common;

namespace Domain.Entities;

public class AiContextReference : BaseEntity
{
    public int MessageId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string? Title { get; set; }
    public string? Excerpt { get; set; }
    public double RelevanceScore { get; set; }

    public AiMessage Message { get; set; } = null!;
}
