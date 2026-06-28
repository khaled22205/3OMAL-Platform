using src.Models.Common;

namespace src.Models;

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
}