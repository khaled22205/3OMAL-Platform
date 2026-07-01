using Domain.Common;

namespace Domain.Entities;

public class Payment : BaseEntity
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime? CompletedAt { get; set; }
    public string? TransactionReference { get; set; }

    public void Complete(string transactionRef)
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
        TransactionReference = transactionRef;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        Status = "Refunded";
        UpdatedAt = DateTime.UtcNow;
    }
}
