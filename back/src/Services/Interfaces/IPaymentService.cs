using src.DTOs.Payments;

namespace src.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse?> GetByBookingIdAsync(int bookingId);
    Task<PaymentResponse> ProcessPaymentAsync(int bookingId, string paymentMethod);
    Task<bool> ProcessRefundAsync(int bookingId);
}