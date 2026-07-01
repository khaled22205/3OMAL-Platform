using Domain.Common;

namespace Domain.Entities;

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

    public void UpdateRating(int newRating, string? comment)
    {
        if (newRating < 1 || newRating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        Rating = newRating;
        Comment = comment;
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reply(string reply)
    {
        WorkerReply = reply;
        UpdatedAt = DateTime.UtcNow;
    }
}
