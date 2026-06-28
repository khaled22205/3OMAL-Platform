using src.Models.Common;

namespace src.Models;

public class WorkerPortfolioItem : BaseEntity
{
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public string MediaType { get; set; } = "Image";
    public string MediaUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
}