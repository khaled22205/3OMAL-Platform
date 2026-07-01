using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class PaymentTests
{
    private static Payment CreatePayment() => new()
    {
        BookingId = 1,
        Amount = 500,
        CommissionAmount = 50,
        PaymentMethod = "Credit Card"
    };

    public class Complete
    {
        [Fact]
        public void Should_set_status_to_Completed()
        {
            var payment = CreatePayment();
            payment.Complete("TXN-123");
            payment.Status.Should().Be("Completed");
            payment.TransactionReference.Should().Be("TXN-123");
            payment.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_update_UpdatedAt()
        {
            var payment = CreatePayment();
            payment.Complete("TXN-123");
            payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }

    public class Refund
    {
        [Fact]
        public void Should_set_status_to_Refunded()
        {
            var payment = CreatePayment();
            payment.Complete("TXN-123");
            payment.Refund();
            payment.Status.Should().Be("Refunded");
        }

        [Fact]
        public void Should_update_UpdatedAt()
        {
            var payment = CreatePayment();
            payment.Complete("TXN-123");
            payment.Refund();
            payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_work_from_Pending_status()
        {
            var payment = CreatePayment();
            payment.Refund();
            payment.Status.Should().Be("Refunded");
        }
    }

    public class InitialState
    {
        [Fact]
        public void Should_default_to_Pending()
        {
            var payment = CreatePayment();
            payment.Status.Should().Be("Pending");
        }
    }
}
