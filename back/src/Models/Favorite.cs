using src.Models.Common;

namespace src.Models;

public class Favorite : BaseEntity
{
    public int CustomerId { get; set; }
    public int? WorkerProfileId { get; set; }
    public WorkerProfile? WorkerProfile { get; set; }
    public int? WorkerServiceId { get; set; }
    public WorkerService? WorkerService { get; set; }
}