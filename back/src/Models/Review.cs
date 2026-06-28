using src.Models.Common;

namespace src.Models;

public class Review : BaseEntity
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int CustomerId { get; set; }
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? WorkerReply { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
}