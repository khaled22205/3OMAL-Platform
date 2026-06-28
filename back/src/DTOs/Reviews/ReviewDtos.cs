namespace src.DTOs.Reviews;

public class CreateReviewRequest
{
    public int BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewResponse
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhoto { get; set; }
    public int WorkerProfileId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? WorkerReply { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkerReplyRequest
{
    public string Reply { get; set; } = string.Empty;
}