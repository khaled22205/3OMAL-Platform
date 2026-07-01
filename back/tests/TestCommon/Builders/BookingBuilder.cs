using Domain.Entities;
using Domain.Enums;

namespace TestCommon.Builders;

public class BookingBuilder
{
    private int _customerId = 1;
    private int _workerProfileId = 1;
    private int? _workerServiceId;
    private BookingStatus _status = BookingStatus.Pending;
    private DateTime _scheduledAt = DateTime.UtcNow.AddDays(1);
    private string? _address = "123 Test St";
    private string? _notes;
    private decimal _totalPrice = 500m;
    private decimal _commissionAmount = 50m;
    private DateTime? _startedAt;
    private DateTime? _completedAt;
    private DateTime? _cancelledAt;
    private string? _cancellationReason;
    private string? _cancelledBy;
    private bool _isDeleted;
    private DateTime? _deletedAt;

    public BookingBuilder WithCustomerId(int id) { _customerId = id; return this; }
    public BookingBuilder WithWorkerProfileId(int id) { _workerProfileId = id; return this; }
    public BookingBuilder WithWorkerServiceId(int? id) { _workerServiceId = id; return this; }
    public BookingBuilder WithStatus(BookingStatus status) { _status = status; return this; }
    public BookingBuilder WithScheduledAt(DateTime dt) { _scheduledAt = dt; return this; }
    public BookingBuilder WithAddress(string? addr) { _address = addr; return this; }
    public BookingBuilder WithNotes(string? notes) { _notes = notes; return this; }
    public BookingBuilder WithTotalPrice(decimal price) { _totalPrice = price; return this; }
    public BookingBuilder WithCommissionAmount(decimal amount) { _commissionAmount = amount; return this; }
    public BookingBuilder WithStartedAt(DateTime? dt) { _startedAt = dt; return this; }
    public BookingBuilder WithCompletedAt(DateTime? dt) { _completedAt = dt; return this; }
    public BookingBuilder WithCancelled(DateTime? at, string? reason = null, string? by = null)
    {
        _cancelledAt = at; _cancellationReason = reason; _cancelledBy = by; return this;
    }
    public BookingBuilder Deleted()
    {
        _isDeleted = true; _deletedAt = DateTime.UtcNow; return this;
    }

    public BookingBuilder InAcceptedState()
    {
        _status = BookingStatus.Accepted; return this;
    }

    public BookingBuilder InScheduledState()
    {
        _status = BookingStatus.Scheduled; return this;
    }

    public Booking Build()
    {
        return new Booking
        {
            CustomerId = _customerId,
            WorkerProfileId = _workerProfileId,
            WorkerServiceId = _workerServiceId,
            Status = _status,
            ScheduledAt = _scheduledAt,
            Address = _address,
            Notes = _notes,
            TotalPrice = _totalPrice,
            CommissionAmount = _commissionAmount,
            StartedAt = _startedAt,
            CompletedAt = _completedAt,
            CancelledAt = _cancelledAt,
            CancellationReason = _cancellationReason,
            CancelledBy = _cancelledBy,
            IsDeleted = _isDeleted,
            DeletedAt = _deletedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Booking CreatePending(int customerId = 1, int workerProfileId = 1)
        => new BookingBuilder().WithCustomerId(customerId).WithWorkerProfileId(workerProfileId).Build();

    public static Booking CreateAccepted(int customerId = 1, int workerProfileId = 1)
        => new BookingBuilder().WithCustomerId(customerId).WithWorkerProfileId(workerProfileId).InAcceptedState().Build();
}
