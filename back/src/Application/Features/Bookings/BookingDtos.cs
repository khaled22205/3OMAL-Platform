namespace Application.Features.Bookings;

public class CreateBookingRequest
{
    public int WorkerProfileId { get; set; }
    public int? WorkerServiceId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public class BookingResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int WorkerProfileId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int? WorkerServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal CommissionAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

public class UpdateBookingStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
}

public class BookingFilterRequest
{
    public string? Status { get; set; }
    public int? WorkerProfileId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
