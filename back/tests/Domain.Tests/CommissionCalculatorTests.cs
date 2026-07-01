using Domain.DomainServices;
using FluentAssertions;

namespace Domain.Tests;

public class CommissionCalculatorTests
{
    public class Calculate
    {
        [Theory]
        [InlineData(100, 15, 15)]
        [InlineData(100, 10, 10)]
        [InlineData(200, 15, 30)]
        [InlineData(1000, 5, 50)]
        public void Should_calculate_commission_correctly(decimal price, double percentage, decimal expectedCommission)
        {
            var commission = CommissionCalculator.Calculate(price, percentage);
            commission.Should().Be(expectedCommission);
        }

        [Theory]
        [InlineData(0, 15)]
        [InlineData(0, 0)]
        [InlineData(100, 0)]
        public void Should_return_zero_for_zero_percentage(decimal price, double percentage)
        {
            var commission = CommissionCalculator.Calculate(price, percentage);
            commission.Should().Be(0);
        }

        [Theory]
        [InlineData(100, 100, 100)]
        [InlineData(200, 50, 100)]
        public void Should_never_exceed_price(decimal price, double percentage, decimal expected)
        {
            var commission = CommissionCalculator.Calculate(price, percentage);
            commission.Should().Be(expected);
        }

        [Fact]
        public void Should_handle_large_values()
        {
            var commission = CommissionCalculator.Calculate(1000000, 10);
            commission.Should().Be(100000);
        }

        [Fact]
        public void Should_handle_fractional_percentages()
        {
            var commission = CommissionCalculator.Calculate(200, 2.5);
            commission.Should().Be(5);
        }

        [Fact]
        public void Should_handle_decimal_rounding()
        {
            var commission = CommissionCalculator.Calculate(10, 33.33);
            commission.Should().Be(3.333m);
        }
    }
}
