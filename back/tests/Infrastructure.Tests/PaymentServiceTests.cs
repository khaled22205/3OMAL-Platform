using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public class PaymentServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new PaymentService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private Booking CreateCompletedBooking()
    {
        var booking = new Booking
        {
            CustomerId = 1,
            WorkerProfileId = 1,
            Status = BookingStatus.Completed,
            ScheduledAt = DateTime.UtcNow.AddDays(-1),
            CompletedAt = DateTime.UtcNow,
            TotalPrice = 500m,
            CommissionAmount = 50m
        };
        return booking;
    }

    [Fact]
    public async Task GetByBookingIdAsync_Should_return_payment_when_exists()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = 500m,
            CommissionAmount = 50m,
            PaymentMethod = "Cash",
            Status = "Completed"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var result = await _service.GetByBookingIdAsync(booking.Id);

        result.Should().NotBeNull();
        result!.BookingId.Should().Be(booking.Id);
        result.Amount.Should().Be(500m);
    }

    [Fact]
    public async Task GetByBookingIdAsync_Should_return_null_when_not_found()
    {
        var result = await _service.GetByBookingIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_throw_when_booking_not_found()
    {
        var act = () => _service.ProcessPaymentAsync(999, "Cash");
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Booking not found");
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_throw_when_booking_not_completed()
    {
        var booking = new Booking
        {
            CustomerId = 1,
            WorkerProfileId = 1,
            Status = BookingStatus.Pending,
            TotalPrice = 500m,
            CommissionAmount = 50m
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var act = () => _service.ProcessPaymentAsync(booking.Id, "Cash");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Can only process payment for completed bookings");
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_throw_when_payment_already_exists()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var payment = new Payment { BookingId = booking.Id, Amount = 500m, PaymentMethod = "Cash" };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var act = () => _service.ProcessPaymentAsync(booking.Id, "Cash");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payment already processed for this booking");
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_throw_for_invalid_payment_method()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var act = () => _service.ProcessPaymentAsync(booking.Id, "Bitcoin");
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid payment method");
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_create_payment_and_invoice()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var result = await _service.ProcessPaymentAsync(booking.Id, "CreditCard");

        result.Should().NotBeNull();
        result.BookingId.Should().Be(booking.Id);
        result.Status.Should().Be("Completed");
        result.PaymentMethod.Should().Be("CreditCard");
        result.TransactionReference.Should().NotBeNullOrEmpty();

        var paymentInDb = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == booking.Id);
        paymentInDb.Should().NotBeNull();
        paymentInDb!.Status.Should().Be("Completed");

        var invoiceInDb = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == booking.Id);
        invoiceInDb.Should().NotBeNull();
        invoiceInDb!.InvoiceNumber.Should().StartWith("INV-");
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_use_cash_method()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var result = await _service.ProcessPaymentAsync(booking.Id, "Cash");
        result.PaymentMethod.Should().Be("Cash");
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_return_false_when_no_payment()
    {
        var result = await _service.ProcessRefundAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_return_false_when_already_refunded()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = 500m,
            Status = "Refunded"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var result = await _service.ProcessRefundAsync(booking.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessRefundAsync_Should_refund_and_return_true()
    {
        var booking = CreateCompletedBooking();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = 500m,
            CommissionAmount = 50m,
            PaymentMethod = "Cash",
            Status = "Completed"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var result = await _service.ProcessRefundAsync(booking.Id);
        result.Should().BeTrue();

        var refunded = await _context.Payments.FirstAsync(p => p.BookingId == booking.Id);
        refunded.Status.Should().Be("Refunded");
    }
}
