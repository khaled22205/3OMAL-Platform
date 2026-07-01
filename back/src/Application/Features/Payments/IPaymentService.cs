namespace Application.Features.Payments;

public interface IPaymentService
{
    Task<PaymentResponse?> GetByBookingIdAsync(int bookingId);
    Task<PaymentResponse> ProcessPaymentAsync(int bookingId, string paymentMethod);
    Task<bool> ProcessRefundAsync(int bookingId);
}
