using Domain.DomainServices;
using FluentAssertions;

namespace Domain.Tests;

public class StringHelperTests
{
    public class ToSeoUrl
    {
        [Theory]
        [InlineData("Hello World", "hello-world")]
        [InlineData("Pipe Repair & Maintenance", "pipe-repair-maintenance")]
        [InlineData("  Extra   Spaces  ", "extra-spaces")]
        [InlineData("Special!@#$%^&*()Chars", "specialchars")]
        [InlineData("UPPERCASE", "uppercase")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        public void Should_generate_correct_slug(string input, string expected)
        {
            var slug = StringHelper.ToSeoUrl(input);
            slug.Should().Be(expected);
        }

        [Fact]
        public void Should_trim_leading_and_trailing_hyphens()
        {
            var slug = StringHelper.ToSeoUrl("  hello  ");
            slug.Should().Be("hello");
        }

        [Fact]
        public void Should_replace_multiple_spaces_with_single_hyphen()
        {
            var slug = StringHelper.ToSeoUrl("a   b");
            slug.Should().Be("a-b");
        }

        [Fact]
        public void Should_remove_special_characters()
        {
            var slug = StringHelper.ToSeoUrl("Hello!@#$%^&*()World");
            slug.Should().Be("helloworld");
        }
    }

    public class GenerateInvoiceNumber
    {
        [Fact]
        public void Should_generate_invoice_number_with_correct_format()
        {
            var result = StringHelper.GenerateInvoiceNumber(123);
            result.Should().Match("INV-????????-00123");
        }

        [Fact]
        public void Should_pad_booking_id_to_five_digits()
        {
            var result = StringHelper.GenerateInvoiceNumber(5);
            result.Should().Match("INV-????????-00005");
        }

        [Fact]
        public void Should_handle_large_booking_id()
        {
            var result = StringHelper.GenerateInvoiceNumber(99999);
            result.Should().Match("INV-????????-99999");
        }

        [Fact]
        public void Should_include_todays_date()
        {
            var now = DateTime.UtcNow;
            var result = StringHelper.GenerateInvoiceNumber(1);
            result.Should().Contain($"INV-{now:yyyyMMdd}-");
        }
    }
}
