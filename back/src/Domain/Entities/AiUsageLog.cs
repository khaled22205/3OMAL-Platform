using Domain.Common;

namespace Domain.Entities;

public class AiUsageLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? Role { get; set; }
    public int PromptTokens { get; set; }
    public int ResponseTokens { get; set; }
    public int RetrievalDurationMs { get; set; }
    public int TotalDurationMs { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}
