using src.Models.Common;

namespace src.Models;

public class WorkerAvailability : BaseEntity
{
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
}