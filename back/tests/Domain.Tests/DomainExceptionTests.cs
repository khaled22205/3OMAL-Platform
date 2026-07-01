using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Tests;

public class DomainExceptionTests
{
    [Fact]
    public void Should_set_message()
    {
        var ex = new DomainException("Test error");
        ex.Message.Should().Be("Test error");
    }

    [Fact]
    public void Should_be_assignable_to_exception()
    {
        var ex = new DomainException("Test");
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Should_allow_empty_message()
    {
        var ex = new DomainException("");
        ex.Message.Should().Be("");
    }
}
