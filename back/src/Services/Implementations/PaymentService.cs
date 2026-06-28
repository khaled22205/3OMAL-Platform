using Microsoft.EntityFrameworkCore;
using src.Data;
using src.DTOs.Payments;
using src.Helpers;
using src.Models;
using src.Services.Interfaces;

namespace src.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponse?> GetByBookingIdAsync(int bookingId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        return payment?.ToResponse();
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(int bookingId, string paymentMethod)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
            ?? throw new ArgumentException("Booking not found");

        if (booking.Status != "Completed")
            throw new InvalidOperationException("Can only process payment for completed bookings");

        var existingPayment = await _context.Payments
            .AnyAsync(p => p.BookingId == bookingId);
        if (existingPayment)
            throw new InvalidOperationException("Payment already processed for this booking");

        var validMethods = new[] { "Cash", "CreditCard" };
        if (!validMethods.Contains(paymentMethod))
            throw new ArgumentException("Invalid payment method");

        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = booking.TotalPrice,
            CommissionAmount = booking.CommissionAmount,
            PaymentMethod = paymentMethod,
            Status = "Completed",
            CompletedAt = DateTime.UtcNow,
            TransactionReference = Guid.NewGuid().ToString("N")[..12].ToUpper()
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var invoice = new Invoice
        {
            BookingId = bookingId,
            InvoiceNumber = StringHelper.GenerateInvoiceNumber(bookingId),
            Amount = booking.TotalPrice,
            CommissionAmount = booking.CommissionAmount,
            WorkerAmount = booking.TotalPrice - booking.CommissionAmount
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return payment.ToResponse();
    }

    public async Task<bool> ProcessRefundAsync(int bookingId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment == null || payment.Status == "Refunded")
            return false;

        payment.Status = "Refunded";
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}