namespace src.DTOs.Payments;

public class PaymentResponse
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string? TransactionReference { get; set; }
}

public class ProcessPaymentRequest
{
    public int BookingId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class WithdrawalRequest
{
    public decimal Amount { get; set; }
    public string BankAccount { get; set; } = string.Empty;
}