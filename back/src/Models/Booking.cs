using src.Models.Common;

namespace src.Models;

public class Booking : BaseEntity, ISoftDelete
{
    public int CustomerId { get; set; }
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public int? WorkerServiceId { get; set; }
    public WorkerService? WorkerService { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime ScheduledAt { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal CommissionAmount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}