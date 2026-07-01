using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Booking : BaseEntity, ISoftDelete
{
    private static readonly Dictionary<BookingStatus, BookingStatus[]> AllowedTransitions = new()
    {
        [BookingStatus.Pending] = [BookingStatus.Accepted, BookingStatus.Rejected, BookingStatus.Cancelled, BookingStatus.Expired],
        [BookingStatus.Accepted] = [BookingStatus.Scheduled, BookingStatus.Cancelled, BookingStatus.Rejected],
        [BookingStatus.Scheduled] = [BookingStatus.OnTheWay, BookingStatus.Cancelled],
        [BookingStatus.OnTheWay] = [BookingStatus.Started, BookingStatus.Cancelled],
        [BookingStatus.Started] = [BookingStatus.Paused, BookingStatus.Completed, BookingStatus.Cancelled],
        [BookingStatus.Paused] = [BookingStatus.Started, BookingStatus.Cancelled]
    };

    private static readonly BookingStatus[] CancellableStatuses =
        [BookingStatus.Pending, BookingStatus.Accepted, BookingStatus.Scheduled];

    private static readonly BookingStatus[] ReschedulableStatuses =
        [BookingStatus.Pending, BookingStatus.Accepted, BookingStatus.Scheduled];

    public int CustomerId { get; set; }
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public int? WorkerServiceId { get; set; }
    public WorkerService? WorkerService { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
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

    public void TransitionTo(BookingStatus next)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var validNext) || !validNext.Contains(next))
            throw new InvalidOperationException($"Cannot transition from '{Status}' to '{next}'");

        Status = next;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        TransitionTo(BookingStatus.Accepted);
    }

    public void Reject(string? reason = null)
    {
        TransitionTo(BookingStatus.Rejected);
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void Cancel(int cancelledByUserId, string? reason = null)
    {
        if (!CancellableStatuses.Contains(Status))
            throw new InvalidOperationException($"Cannot cancel booking in status '{Status}'");

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        CancelledBy = cancelledByUserId.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reschedule(DateTime newScheduledAt)
    {
        if (!ReschedulableStatuses.Contains(Status))
            throw new InvalidOperationException($"Cannot reschedule booking in status '{Status}'");

        ScheduledAt = newScheduledAt;
        Status = BookingStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkOnTheWay()
    {
        TransitionTo(BookingStatus.OnTheWay);
    }

    public void StartJob()
    {
        TransitionTo(BookingStatus.Started);
        StartedAt = DateTime.UtcNow;
    }

    public void PauseJob()
    {
        TransitionTo(BookingStatus.Paused);
    }

    public void CompleteJob()
    {
        TransitionTo(BookingStatus.Completed);
        CompletedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = BookingStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}
