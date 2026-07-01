using Domain.Common;

namespace Domain.Entities;

public class Invoice : BaseEntity
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal WorkerAmount { get; set; }
    public string? FileUrl { get; set; }
}
